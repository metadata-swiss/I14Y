import {Component, EventEmitter, inject, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, ViewChild} from '@angular/core';
import {AbstractControl, UntypedFormArray, UntypedFormGroup} from '@angular/forms';
import {
	DcatCatalog,
	DcatCatalogInputClient,
	DcatCatalogRecordInput,
	DcatCatalogResource,
	IActiveDirectoryUser,
	IAgent,
	IPerson,
	MultiLanguage,
	PublicationLevelInfoModel,
	RegistrationStatusInfoModel,
	VocabularyClient,
	VocabularyEntry,
	PublicationLevel,
	RegistrationStatus,
	AllowActionResourceType,
	AllowActionType,
	DcatDatasetModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, of, Subject} from 'rxjs';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {IDialogConfig} from 'src/app/shared/dialog/dialog.component';
import {map, shareReplay, startWith, takeUntil} from 'rxjs/operators';
import {MatAccordion} from '@angular/material/expansion';
import {Languages} from 'src/app/shared/ApplicationLanguage.enum';
import {ModalDialogComponent} from 'src/app/shared/modal-dialog/modal-dialog.component';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {ObNotificationService} from '@oblique/oblique';
import {AllowActionService} from '../../../services/allow.action.service';
import {VocabularyConfigService} from 'src/app/services/vocabulary-config.service';

@Component({
	selector: 'app-description-edit-form',
	templateUrl: './description-edit-form.component.html',
	styleUrls: ['./description-edit-form.component.scss'],
	standalone: false
})
export class DescriptionEditFormComponent implements OnInit, OnChanges, OnDestroy {
	@Output() cancel: EventEmitter<void> = new EventEmitter();
	@Output() saveAndClose: EventEmitter<void> = new EventEmitter();
	@Output() save: EventEmitter<void> = new EventEmitter();
	@Output() formChanges: EventEmitter<SimpleChanges> = new EventEmitter();
	@Input() datasetId: string | undefined;
	@Input() dto: DcatDatasetModel = new DcatDatasetModel();
	@Input() previousVersion: DcatDatasetModel | undefined = undefined;
	@Input() form!: UntypedFormGroup;
	@Input() cancelDialogConfig!: IDialogConfig;
	@Input() isEditMode!: boolean;
	@Input() isVersionMode: boolean = false;
	@Input() registrationStatusInfo: RegistrationStatusInfoModel | undefined;
	@Input() publicationLevelInfo: PublicationLevelInfoModel | undefined;
	cannotEdit$: Observable<boolean> = of<boolean>(true);
	allowActionEditMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionEditMessage$: Observable<string> = of('');
	cannotDelete$: Observable<boolean> = of<boolean>(true);
	allowActionDeleteMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionDeleteMessage$: Observable<string> = of('');
	cannotLinkCatalogue$: Observable<boolean> = of<boolean>(true);
	allowActionCreateMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionCreateMessage$: Observable<string> = of('');

	@Input()
	public set agents(input: IAgent[]) {
		this._agents.splice(0, this._agents.length, ...input);
	}

	@ViewChild(MatAccordion) accordion!: MatAccordion;
	@ViewChild(ModalDialogComponent) modalDialog!: ModalDialogComponent;

	showAllLanguages: boolean;
	filteredAgents$!: Observable<IAgent[]>;

	currentLanguage: string;
	vocabulary$: Observable<VocabularyEntry[]>;
	geoIvIdCodes$: Observable<VocabularyEntry[]>;
	catalogsList$: Observable<DcatCatalog[]> | undefined;
	catalogsByDatasetId: DcatCatalogRecordInput[] | undefined;
	vocabularyFrequency$: Observable<VocabularyEntry[]>;
	vocabularyConfidentiality$: Observable<VocabularyEntry[]>;
	themes$: Observable<VocabularyEntry[]>;
	formatThemes$: Observable<VocabularyEntry[]>;
	contentLanguages: readonly string[] = Languages.ContentLanguagesRm;
	themesConceptPageIri: string | undefined = undefined;
	accessRightsConceptPageIri: string | undefined = undefined;
	readonly maxEntries: number = 3;
	readonly publicationLevelEnum = PublicationLevel;
	readonly registrationStatusEnum = RegistrationStatus;

	COLUMN_TITLE = 'title';
	COLUMN_IDENTIFIER = 'identifier';
	COLUMN_VERSION = 'version';
	COLUMN_REGISTRATIONSTATUS = 'registrationStatus';
	COLUMN_PUBLICATIONLEVEL = 'publicationLevel';

	displayedColumns: string[] = [this.COLUMN_TITLE, this.COLUMN_IDENTIFIER, this.COLUMN_VERSION, this.COLUMN_REGISTRATIONSTATUS, this.COLUMN_PUBLICATIONLEVEL];

	private readonly accessRights = 'RightsStatement_ACCESS_RIGHTS';
	private readonly geoIV = 'VOCAB_GEOBASISDATEN';
	private readonly vocabularyFrequency = 'VOCAB_EU_FREQUENCY';
	private readonly datasetTheme = 'Concept_DATASET_THEME';
	private readonly vocabularyConfidentiality = 'VOCAB_I14Y_CONFIDENTIALITY_PERSON';
	private readonly editingChildren: string[] = [];
	private readonly _agents: IAgent[] = [];
	private readonly unsubscribe$ = new Subject();
	private hasUpdatedControls: boolean = false;

	private readonly INVALID = 'INVALID';

	private readonly allowActionService = inject(AllowActionService);
	private readonly dcatCatalogInputClient = inject(DcatCatalogInputClient);
	private readonly fallback = inject(FallbackPipe);
	private readonly notification = inject(ObNotificationService);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyClient = inject(VocabularyClient);
	private readonly vocabularyConfigService = inject(VocabularyConfigService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.vocabulary$ = this.vocabularyClient.getByIdentifier(this.accessRights).pipe(map(response => response.result));
		this.geoIvIdCodes$ = this.vocabularyClient.getByIdentifier(this.geoIV).pipe(map(response => response.result));
		this.catalogsList$ = this.dcatCatalogInputClient.getUser().pipe(map(response => response.result));
		this.vocabularyFrequency$ = this.vocabularyClient.getByIdentifier(this.vocabularyFrequency).pipe(map(response => response.result));
		this.vocabularyConfidentiality$ = this.vocabularyClient.getByIdentifier(this.vocabularyConfidentiality).pipe(map(response => response.result));
		this.themes$ = this.vocabularyClient.getByIdentifier(this.datasetTheme).pipe(map(response => response.result));
		this.formatThemes$ = this.themes$.pipe(
			map(x =>
				[...x].sort((a, b) => {
					const aName = this.fallback.transform(a.name, this.currentLanguage);
					const bName = this.fallback.transform(b.name, this.currentLanguage);
					return aName!.localeCompare(bName!);
				})
			),
			shareReplay(1)
		);
		this.showAllLanguages = true;
	}

	ngOnChanges(changes: SimpleChanges): void {
		this.formChanges.emit(changes);
		if (changes.datasetId) {
			const previousDtoId = changes.datasetId.previousValue;
			const currentDtoId = changes.datasetId.currentValue;
			if (currentDtoId && currentDtoId !== previousDtoId) {
				this.getCatalogs();
			}
		}
		if (changes.dto && this.isEditMode && this._agents.length > 0) {
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
				map(value => (typeof value === 'string' ? value : value?.name[this.currentLanguage])),
				map(term => (term ? this.filterAgents(term) : this._agents))
			);
		}

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
			this.showAllLanguages = false;
		});

		this.vocabularyConfigService
			.resolveConceptPageIris([this.datasetTheme, this.accessRights])
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(iris => {
				this.themesConceptPageIri       = iris[this.datasetTheme];
				this.accessRightsConceptPageIri = iris[this.accessRights];
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
		if (this.datasetId) {
			this.dcatCatalogInputClient.getRecordsByResourceByResourceId(this.datasetId).subscribe(x => {
				this.catalogsByDatasetId = x.result;
				this.catalogsByDatasetId.forEach((response: DcatCatalogRecordInput) => {
					if (response.id) {
						this.allowActionService.loadChild(response.id, AllowActionResourceType.DcatCatalogRecord, true);
					}
				});
			});
		}
	}

	addNewCatalogFromMenu(catalog: DcatCatalogRecordInput) {
		let payload = new DcatCatalogRecordInput({
			catalogId: catalog.id,
			themes: [],
			catalogTitle: new MultiLanguage({}),
			primaryTopic: new DcatCatalogResource({
				resourceId: this.datasetId,
				resourceType: 'Dataset'
			})
		});

		this.dcatCatalogInputClient.postRecordsByBody(payload).subscribe(_ => {
			this.showSuccessNotification('i18n.notification.catalogue_added');
			this.getCatalogs();
		});
	}

	deleteCatalog(catalog: DcatCatalogRecordInput): void {
		this.dcatCatalogInputClient.deleteRecordsByIdAndRecordId(catalog.catalogId!, catalog.id!).subscribe(_ => {
			this.showSuccessNotification('i18n.notification.catalogue_removed');
			this.getCatalogs();
		});
	}

	updateCatalogs() {
		this.getCatalogs();
	}

	canSave(): boolean {
		return this.editingChildren.length === 0;
	}

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

	displayPersonFn = (user: IPerson & IActiveDirectoryUser): string => {
		if (user) {
			return (user?.displayName ? user?.displayName : (user?.name as string)) ?? user.email;
		}
		return '';
	};

	hasError(key: string): boolean {
		return this.form!.get(key)?.errors ? true : false;
	}

	getIdentifiers(): UntypedFormArray {
		let controls = this.form.controls.identifiers as UntypedFormArray;
		return controls;
	}

	private filterAgents(term: string): IAgent[] {
		return this._agents.filter((option: IAgent) => this.getName(option).toLowerCase().includes(term.toLowerCase()));
	}

	private getName(option: IAgent): string {
		return this.fallback.transform(option.name, this.currentLanguage) ?? '';
	}

	private get isFormValid(): boolean {
		this.form.markAllAsTouched();
		this.form.updateValueAndValidity();
		return this.form.valid;
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
		formArray.controls.forEach((control) => {
			if (control.status === this.INVALID) {
				control.updateValueAndValidity();
			}
		});
	}
}
