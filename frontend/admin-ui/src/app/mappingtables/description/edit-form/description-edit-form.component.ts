import {Component, EventEmitter, inject, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, ViewChild} from '@angular/core';
import {AbstractControl, UntypedFormArray, UntypedFormGroup} from '@angular/forms';
import {
	FileParameter,
	IActiveDirectoryUser,
	IAgent,
	IPerson,
	MappingRelationModel,
	MappingRelationsDataFormat,
	MappingTableModel,
	MappingTablesClient,
	PublicationLevel,
	PublicationLevelInfoModel,
	RegistrationStatus,
	RegistrationStatusInfoModel,
	VocabularyClient,
	VocabularyEntry
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, Subject} from 'rxjs';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {DialogComponent, DialogType, IDialogConfig} from 'src/app/shared/dialog/dialog.component';
import {map, startWith, takeUntil} from 'rxjs/operators';
import {MatAccordion} from '@angular/material/expansion';
import {ModalDialogComponent} from 'src/app/shared/modal-dialog/modal-dialog.component';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {Languages} from 'src/app/shared/ApplicationLanguage.enum';
import {ObNotificationService} from '@oblique/oblique';
import {PageEvent} from '@angular/material/paginator';
import {MappingTableService} from '../../services/mappingtable.service';
import {SearchResultPagingInfo} from 'src/app/shared/searchResultPagingInfo';
import {ComponentMode, DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY} from 'src/app/app-constants';
import {MatDialog} from '@angular/material/dialog';

@Component({
	selector: 'app-description-edit-form',
	templateUrl: './description-edit-form.component.html',
	styleUrls: ['./description-edit-form.component.scss'],
	standalone: false
})
export class DescriptionEditFormComponent implements OnInit, OnDestroy, OnChanges {
	@Output() cancel: EventEmitter<void> = new EventEmitter();
	@Output() saveAndClose: EventEmitter<void> = new EventEmitter();
	@Output() save: EventEmitter<void> = new EventEmitter();
	@Output() formChanges: EventEmitter<SimpleChanges> = new EventEmitter();
	@Input() dto: MappingTableModel = new MappingTableModel();
	@Input() mappingTableId: string | undefined;
	@Input() form!: UntypedFormGroup;
	@Input() cancelDialogConfig!: IDialogConfig;
	@Input() mode: ComponentMode = ComponentMode.Create;
	@Input() publicationLevelInfo: PublicationLevelInfoModel | undefined;
	@Input() registrationStatusInfo: RegistrationStatusInfoModel | undefined;
	@Input()
	public set agents(input: IAgent[]) {
		this.allAgents.splice(0, this.allAgents.length, ...input);
	}

	@ViewChild(MatAccordion) accordion!: MatAccordion;
	@ViewChild(ModalDialogComponent) modalDialog!: ModalDialogComponent;

	showAllLanguages: boolean;
	filteredAgents$!: Observable<IAgent[]>;

	currentLanguage: string;
	vocabulary$: Observable<VocabularyEntry[]>;
	themes$: Observable<VocabularyEntry[]>;
	formatThemes$: Observable<VocabularyEntry[]>;
	contentLanguages: readonly string[] = Languages.ContentLanguagesRm;
	mappingRelations: MappingRelationModel[] = [];
	pagingInfo: SearchResultPagingInfo = new SearchResultPagingInfo(undefined);

	COLUMN_TITLE = 'title';
	COLUMN_ENDPPOINTURLS = 'endpointUrls';
	COLUMN_VERSION = 'version';
	COLUMN_REGISTRATIONSTATUS = 'registrationStatus';
	COLUMN_PUBLICATIONLEVEL = 'publicationLevel';
	readonly publicationLevelEnum = PublicationLevel;
	readonly registrationStatusEnum = RegistrationStatus;
	readonly mappingRelationsDataFormatEnum = MappingRelationsDataFormat;

	displayedColumns: string[] = [
		this.COLUMN_TITLE,
		this.COLUMN_ENDPPOINTURLS,
		this.COLUMN_VERSION,
		this.COLUMN_REGISTRATIONSTATUS,
		this.COLUMN_PUBLICATIONLEVEL
	];

	private readonly allAgents: IAgent[] = [];
	private readonly accessRights = 'RightsStatement_ACCESS_RIGHTS';
	private readonly datasetTheme = 'Concept_DATASET_THEME';
	private readonly editingChildren: string[] = [];
	private readonly unsubscribe$ = new Subject();
	private hasUpdatedControls: boolean = false;
	private readonly INVALID = 'INVALID';
	private readonly defaultPage = 1;

	private readonly dialog = inject(MatDialog);
	private readonly fallback = inject(FallbackPipe);
	private readonly mappingTableClient = inject(MappingTablesClient);
	private readonly mappingTableService = inject(MappingTableService);
	private readonly notification = inject(ObNotificationService);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyClient = inject(VocabularyClient);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.vocabulary$ = this.vocabularyClient.getByIdentifier(this.accessRights).pipe(map(response => response.result));
		this.themes$ = this.vocabularyClient.getByIdentifier(this.datasetTheme).pipe(map(response => response.result));
		this.formatThemes$ = this.themes$;
		this.showAllLanguages = true;
		this.formatThemes();
	}

	get isEditMode(): boolean {
		return this.mode === ComponentMode.Edit;
	}

	isVersionMode(): boolean {
		return this.mode === ComponentMode.Version;
	}

	ngOnChanges(changes: SimpleChanges): void {
		this.formChanges.emit(changes);
		if (this.dto.id && this.allAgents.length > 0) {
			setTimeout(() => {
				this.checkIfFormChangesIsInvalid();
			}, 100);
		}
	}

	ngOnInit() {
		let publisher = this.form.get('publisher');
		if (publisher) {
			this.filteredAgents$ = publisher.valueChanges.pipe(
				startWith(''),
				map(value => this.mapValue(value)),
				map(term => (term ? this.filterAgents(term) : this.allAgents))
			);
		}

		if (!this.isVersionMode()) {
			this.mappingTableService.pagedMappingRelationResult$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
				this.mappingRelations = x?.relations || [];
				this.pagingInfo = x?.pagingInfo || new SearchResultPagingInfo(undefined);
			});
		}

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
			this.showAllLanguages = false;
			this.formatThemes();
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	onChangePage(pageEvent: PageEvent) {
		this.mappingTableService.updateMappingRelations(this.mappingTableId!, pageEvent.pageIndex + 1, pageEvent.pageSize);
	}

	canSave(): boolean {
		return this.editingChildren.length === 0;
	}

	displayVocabularyEntry = (format: VocabularyEntry) => {
		return this.fallback.transform(format?.name, this.currentLanguage) ?? '';
	};

	onBeforeOpenDialog(): void {
		this.cancelDialogConfig.enableSave = this.canSave();

		this.modalDialog.openDialog();
	}

	onChildIsEditing(name: string, isEditing: boolean): void {
		if (isEditing) {
			this.editingChildren.push(name);
		} else {
			const index = this.editingChildren.indexOf(name, 0);
			if (index > -1) {
				this.editingChildren.splice(index, 1);
			}
		}
	}

	onCancel(): void {
		this.cancel.emit();
	}

	onSave(): void {
		if (this.isFormValid) {
			this.save.emit();
		}
	}

	onSaveAndClose(): void {
		if (this.isFormValid) {
			this.saveAndClose.emit();
		}
	}

	displayValue = (user: any) => {
		return this.fallback.transform(user?.name, this.currentLanguage) ?? '';
	};

	displayPersonName = (user: IPerson & IActiveDirectoryUser): string => {
		if (user) {
			return (user?.displayName ? user?.displayName : (user?.name as string)) ?? user.email;
		}
		return '';
	};

	isIdentifierEditionDisabled(): boolean {
		return (
			this.registrationStatusInfo !== undefined &&
			this.registrationStatusInfo?.status !== this.registrationStatusEnum.Incomplete &&
			this.registrationStatusInfo?.status !== this.registrationStatusEnum.Candidate
		);
	}

	isFirstIdentifierLocked(): boolean {
		return this.publicationLevelInfo?.level === this.publicationLevelEnum.Public;
	}

	canCreateMappingRelations(): boolean {
		return this.isEditMode;
	}

	updateMappingRelations(): void {
		if (this.dto.id) {
			this.mappingTableService.updateMappingRelations(this.dto.id, this.pagingInfo.page, this.pagingInfo.pageSize);
		}
	}

	canImport(): boolean {
		return this.isEditMode && this.mappingRelations.length === 0;
	}

	handleImport(files: FileList, input: HTMLInputElement, format: MappingRelationsDataFormat): void {
		const file: File = files[0];
		if (file && this.dto.id) {
			this.notification.info({
				title: 'i18n.datasets.content.import.notifications.title',
				message: 'i18n.datasets.content.import.notifications.started',
				messageParams: {fileName: file.name}
			});
			const fileParameter: FileParameter = {fileName: file.name, data: file};

			this.mappingTableClient.postRelationsImportsByIdAndFormatAndBody(this.dto.id, format, fileParameter).subscribe({
				next: _ => {
					this.notification.success({
						title: 'i18n.datasets.content.import.notifications.title',
						message: 'i18n.datasets.content.import.notifications.success',
						messageParams: {fileName: file.name}
					});
					this.updateRelations();
				},
				error: error => {
					this.notification.error({
						title: 'i18n.datasets.content.import.notifications.title',
						message: 'i18n.datasets.content.import.notifications.error',
						messageParams: {error: error.detail, fileName: file.name},
						sticky: true
					});
				},
				complete: () => {
					input.value = '';
				}
			});
		}
	}

	deleteAllRelations(): void {
		const headertextKey = 'i18n.mapping_relations.delete.dialog.header';
		const bodytextKey = 'i18n.delete_dialog.body';
		const confirmButtontextKey = 'i18n.delete_dialog.confirmbutton';

		this.translate.get([headertextKey, bodytextKey, confirmButtontextKey, DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY]).subscribe(result => {
			const dialogRef = this.dialog.open(DialogComponent, {
				data: {
					showHeader: true,
					headerText: result[headertextKey],
					bodyText: result[bodytextKey],
					dialogType: DialogType.confirm,
					cancelButtonText: result[DIALOG_CANCEL_BUTTON_KEY],
					confirmButtonText: result[DIALOG_CONFIRM_BUTTON_KEY]
				},
				disableClose: true
			});
			const dialogConfirm = dialogRef.componentInstance.confirm.subscribe(() => {
				this.mappingTableClient.deleteRelationsById(this.dto.id as string).subscribe(() => {
					this.notification.success('i18n.notification.deleted');
					this.updateRelations();
				});
			});
			dialogRef.afterClosed().subscribe(() => {
				dialogConfirm.unsubscribe();
			});
		});
	}

	updateRelations(): void {
		this.mappingTableService.updateMappingRelations(this.mappingTableId!, this.defaultPage, this.pagingInfo.pageSize);
	}

	private filterAgents(term: string): IAgent[] {
		return this.allAgents.filter((option: IAgent) => this.getName(option).toLowerCase().includes(term.toLowerCase()));
	}

	private getName(option: IAgent): string {
		return this.fallback.transform(option.name, this.currentLanguage) ?? '';
	}

	private get isFormValid(): boolean {
		this.form.markAllAsTouched();
		this.form.updateValueAndValidity();
		return this.form.valid;
	}

	private formatThemes(): void {
		this.formatThemes$ = this.themes$.pipe(
			map(x =>
				[...x].sort((a, b) => {
					const aName = this.fallback.transform(a.name, this.currentLanguage);
					const bName = this.fallback.transform(b.name, this.currentLanguage);
					return aName!.localeCompare(bName!);
				})
			)
		);
	}

	private checkIfFormChangesIsInvalid(): void {
		if (this.form.status === this.INVALID && !this.hasUpdatedControls) {
			this.hasUpdatedControls = true;
			Object.keys(this.form.controls).forEach(controlName => {
				const control = this.form.get(controlName) as AbstractControl;
				if (control && control.status === this.INVALID) {
					if (control instanceof UntypedFormArray) {
						this.markFormArrayControlsAsInvalid(control);
					} else {
						control.updateValueAndValidity();
						control.markAsTouched();
					}
				}
			});
		}
	}

	private markFormArrayControlsAsInvalid(formArray: UntypedFormArray): void {
		formArray.controls.forEach((control) => {
			if (control.status === this.INVALID) {
				control.updateValueAndValidity();
			}
		});
	}

	private mapValue(value: any): any {
		if (value) {
			return typeof value === 'string' ? value : value.name[this.currentLanguage];
		}
		return '';
	}
}