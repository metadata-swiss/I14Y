import {Component, EventEmitter, inject, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, ViewChild} from '@angular/core';
import {AbstractControl, UntypedFormArray, UntypedFormGroup} from '@angular/forms';
import {
	AllowActionResourceType,
	AllowActionType,
	DataServiceModel,
	DcatCatalog,
	DcatCatalogInputClient,
	DcatCatalogRecordInput,
	DcatCatalogResource,
	IActiveDirectoryUser,
	IAgent,
	IPerson,
	MultiLanguage,
	PublicationLevel,
	PublicationLevelInfoModel,
	RegistrationStatus,
	RegistrationStatusInfoModel,
	VocabularyClient,
	VocabularyEntry
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, of, Subject} from 'rxjs';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {IDialogConfig} from 'src/app/shared/dialog/dialog.component';
import {map, startWith, takeUntil} from 'rxjs/operators';
import {MatAccordion} from '@angular/material/expansion';
import {ModalDialogComponent} from 'src/app/shared/modal-dialog/modal-dialog.component';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {Languages} from 'src/app/shared/ApplicationLanguage.enum';
import {ObNotificationService} from '@oblique/oblique';
import {AllowActionService} from '../../../services/allow.action.service';

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
	@Input() dto: DataServiceModel = new DataServiceModel();
	@Input() previousVersion: DataServiceModel | undefined = undefined;
	@Input() form!: UntypedFormGroup;
	@Input() cancelDialogConfig!: IDialogConfig;
	@Input() isEditMode!: boolean;
	@Input() isVersionMode: boolean = false;
	@Input() publicationLevelInfo: PublicationLevelInfoModel | undefined;
	@Input() licenses: VocabularyEntry[] = [];
	@Input() registrationStatusInfo: RegistrationStatusInfoModel | undefined;
	@Input()
	public set agents(input: IAgent[]) {
		this.allAgents.splice(0, this.allAgents.length, ...input);
	}
	cannotEdit$: Observable<boolean> = of<boolean>(true);
	allowActionEditMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionEditMessage$: Observable<string> = of('');
	cannotDelete$: Observable<boolean> = of<boolean>(true);
	allowActionDeleteMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionDeleteMessage$: Observable<string> = of('');
	cannotLinkCatalogue$: Observable<boolean> = of<boolean>(true);
	allowActionCreateMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionCreateMessage$: Observable<string> = of('');

	@ViewChild(MatAccordion) accordion!: MatAccordion;
	@ViewChild(ModalDialogComponent) modalDialog!: ModalDialogComponent;

	showAllLanguages: boolean;
	filteredAgents$!: Observable<IAgent[]>;

	currentLanguage: string;
	vocabulary$: Observable<VocabularyEntry[]>;
	themes$: Observable<VocabularyEntry[]>;
	formatThemes$: Observable<VocabularyEntry[]>;
	contentLanguages: readonly string[] = Languages.ContentLanguages;

	catalogsList$: Observable<DcatCatalog[]> | undefined;
	catalogsByDatasetId: DcatCatalogRecordInput[] | undefined;

	COLUMN_TITLE = 'title';
	COLUMN_ENDPPOINTURLS = 'endpointUrls';
	COLUMN_VERSION = 'version';
	COLUMN_REGISTRATIONSTATUS = 'registrationStatus';
	COLUMN_PUBLICATIONLEVEL = 'publicationLevel';
	readonly publicationLevelEnum = PublicationLevel;
	readonly registrationStatusEnum = RegistrationStatus;

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

	private readonly allowActionService = inject(AllowActionService);
	private readonly dcatCatalogInputClient = inject(DcatCatalogInputClient);
	private readonly fallback = inject(FallbackPipe);
	private readonly notification = inject(ObNotificationService);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyClient = inject(VocabularyClient);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.vocabulary$ = this.vocabularyClient.getByIdentifier(this.accessRights).pipe(map(response => response.result));
		this.themes$ = this.vocabularyClient.getByIdentifier(this.datasetTheme).pipe(map(response => response.result));
		this.catalogsList$ = this.dcatCatalogInputClient.getUser().pipe(map(response => response.result));
		this.formatThemes$ = this.themes$;
		this.showAllLanguages = true;
		this.formatThemes();
	}

	ngOnChanges(changes: SimpleChanges): void {
		this.formChanges.emit(changes);
		if (changes.dto) {
			const previousDtoId = changes.dto.previousValue?.id;
			const currentDtoId = changes.dto.currentValue?.id;
			if (currentDtoId && currentDtoId !== previousDtoId) {
				this.getCatalogs();
			}
		}
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

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
			this.showAllLanguages = false;
			this.formatThemes();
		});

		this.cannotEdit$ = this.allowActionService.childAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.actionType === AllowActionType.Edit)?.value),
			startWith(true)
		);
		this.allowActionEditMessageDetailCode$ = this.allowActionService.childAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => result.find(x => x.actionType === AllowActionType.Edit)?.messageDetailsCode?.toString() ?? undefined),
			startWith('')
		);
		this.defaultAllowActionEditMessage$ = this.allowActionService.childAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => result.find(x => x.actionType === AllowActionType.Edit)?.message ?? ''),
			startWith('')
		);
		this.cannotDelete$ = this.allowActionService.childAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.actionType === AllowActionType.Delete)?.value),
			startWith(true)
		);
		this.allowActionEditMessageDetailCode$ = this.allowActionService.childAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => result.find(x => x.actionType === AllowActionType.Edit)?.messageDetailsCode?.toString() ?? undefined),
			startWith('')
		);
		this.defaultAllowActionEditMessage$ = this.allowActionService.childAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => result.find(x => x.actionType === AllowActionType.Edit)?.message ?? ''),
			startWith('')
		);

		this.cannotLinkCatalogue$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.resourceType === AllowActionResourceType.DcatCatalogRecord && x.actionType === AllowActionType.Create)?.value),
			startWith(true)
		);
		this.allowActionCreateMessageDetailCode$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(
				result =>
					result
						.find(x => x.resourceType === AllowActionResourceType.DcatCatalogRecord && x.actionType === AllowActionType.Create)
						?.messageDetailsCode?.toString() ?? undefined
			),
			startWith('')
		);
		this.defaultAllowActionCreateMessage$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			// eslint-disable-next-line max-len
			map(result => result.find(x => x.resourceType === AllowActionResourceType.DcatCatalogRecord && x.actionType === AllowActionType.Create)?.message ?? ''),
			startWith('')
		);
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	getCatalogs() {
		this.dcatCatalogInputClient.getRecordsByResourceByResourceId(this.dto.id!).subscribe(x => {
			this.catalogsByDatasetId = x.result;

			this.catalogsByDatasetId.forEach((response: DcatCatalogRecordInput) => {
				if (response.id) {
					this.allowActionService.loadChild(response.id, AllowActionResourceType.DcatCatalogRecord, true);
				}
			});
		});
	}

	addNewCatalogFromMenu(catalog: DcatCatalogRecordInput) {
		let payload = new DcatCatalogRecordInput({
			catalogId: catalog.id,
			themes: [],
			catalogTitle: new MultiLanguage({}),
			primaryTopic: new DcatCatalogResource({
				resourceId: this.dto.id,
				resourceType: 'DataService'
			})
		});

		this.dcatCatalogInputClient.postRecordsByBody(payload).subscribe(_ => {
			this.showSuccessNotification('i18n.datasets.description.new.catalogue.created');
			this.getCatalogs();
		});
	}

	deleteCatalog(catalog: DcatCatalogRecordInput): void {
		this.dcatCatalogInputClient.deleteRecordsByIdAndRecordId(catalog.catalogId!, catalog.id!).subscribe(_ => {
			this.showSuccessNotification('i18n.datasets.description.new.catalogue.delete');
			this.getCatalogs();
		});
	}

	updateCatalogs() {
		this.getCatalogs();
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

	private showSuccessNotification(translation: string): void {
		this.notification.success(translation);
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
		formArray.controls.forEach((control, index) => {
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
