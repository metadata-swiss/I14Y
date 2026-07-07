import {AfterViewInit, Component, inject, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {DeactivationGuarded} from '../../shared/deactivationguarded.interface';
import {ModalDialogComponent} from '../../shared/modal-dialog/modal-dialog.component';
import {
	ActiveDirectoryUser,
	AgentClient,
	ConceptInputClient,
	ConceptInputCreateVersion,
	ConceptReferenceModel,
	ConceptType,
	ConceptView,
	IAgent,
	MultiLanguage,
	Person,
	PublicationLevel,
	PublicationLevelInfoModel,
	SwaggerResponse,
	VocabularyEntryModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {AbstractControl, UntypedFormArray, UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {DialogComponent, DialogType, IDialogConfig} from '../../shared/dialog/dialog.component';
import {Observable, Subject, takeUntil} from 'rxjs';
import {Languages} from '../../shared/ApplicationLanguage.enum';
import {ActivatedRoute, Router} from '@angular/router';
import {ObNotificationService} from '@oblique/oblique';
import {MatDialog} from '@angular/material/dialog';
import {TranslateService} from '@ngx-translate/core';
import {
	ComponentMode,
	DEFAULT_VERSION,
	FORM_FIELD_VERSION,
	NAV_PARAM_FROM,
	NAV_VALUE_DATAELEMENT,
	NAV_VALUE_DETAIL,
	NAV_VALUE_EDIT,
	NAV_VALUE_VERSION,
	DIALOG_CANCEL_BUTTON_KEY,
	DIALOG_DISCARD_CHANGES_BUTTON_KEY,
	DIALOG_SAVE_CHANGES_BUTTON_KEY,
	VERSION_PATTERN,
	FORM_FIELD_IDENTIFIER,
	FORM_FIELD_IDENTIFIERS
} from '../../app-constants';
import {createMultilangValidator} from '../../shared/validators/multilang.validator';
import {ResourceMapper} from 'src/app/shared/mappers/resourcemapper';
import {createPersonPickerValidator} from 'src/app/shared/validators/person-picker.validator';
import {createValidToNotEarlierThanValidFromValidator} from 'src/app/shared/validators/valid-to-not-earlier-than-valid-from.validator';
import {createDeputyNotSameAsPersonValidator} from 'src/app/shared/validators/deputy-not-same-as-person.validator';
import {CodelistIdentifierValidator} from 'src/app/shared/validators/codelist-identifier.validator';
import {ConceptVersionValidator} from 'src/app/shared/validators/concept-version.validator';
import {KeywordMapper} from 'src/app/shared/mappers/keywordmapper';
import {ConceptService} from '../services/concept.service';
import {CodeInputModelMapper} from 'src/app/shared/mappers/codeinputmodelmapper';
import {ConceptInputMapper} from 'src/app/shared/mappers/conceptinputmapper';
import {IdentifierMapper} from 'src/app/shared/mappers/identifiermapper';
import {MultiIdentifiersValidator} from 'src/app/shared/validators/identifier-validator/multi-Identifiers.validator';

@Component({
	selector: 'app-concept-edit',
	templateUrl: './concept-edit.component.html',
	styleUrls: [],
	standalone: false
})
export class ConceptEditComponent implements OnInit, AfterViewInit, OnDestroy, DeactivationGuarded {
	@ViewChild(ModalDialogComponent) modalDialog!: ModalDialogComponent;
	from = '';
	mode: ComponentMode = ComponentMode.Create;
	saveAndCloseText = '';
	saveAndCloseTooltip = '';
	conceptId: string = '';
	// Read model loaded from the GET (ConceptView); converted to the ConceptInput write model via ConceptInputMapper.mapToInputModel(...) on POST/PUT.
	dto: ConceptView = new ConceptView();
	initialConceptType: ConceptType | undefined;
	form: UntypedFormGroup;
	conceptType = ConceptType;
	agents: IAgent[] = [];
	publicationLevelInfo: PublicationLevelInfoModel | undefined;

	cancelDialogConfig: IDialogConfig = {
		showHeader: true,
		enableSave: true,
		headerText: '',
		bodyText: '',
		dialogType: DialogType.save,
		okButtonText: '',
		cancelButtonText: '',
		confirmButtonText: '',
		discardChangesButtonText: '',
		saveChangesButtonText: ''
	};

	private saveDisabled: boolean = false;
	private readonly unsubscribe$ = new Subject();
	private readonly contentLanguages: readonly string[] = Languages.ContentLanguages;

	private readonly agentClient = inject(AgentClient);
	private readonly codelistIdentifierValidator = inject(CodelistIdentifierValidator);
	private readonly conceptInputClient = inject(ConceptInputClient);
	private readonly conceptMultiIdentifiersValidator = inject(MultiIdentifiersValidator);
	private readonly conceptService = inject(ConceptService);
	private readonly conceptVersionValidator = inject(ConceptVersionValidator);
	private readonly dialog = inject(MatDialog);
	private readonly notification = inject(ObNotificationService);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.form = this.createEmptyForm();
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe(() => {
			this.UpdateModalDialog();
			this.updateSaveAndCloseTextKeys();
		});

		this.from = this.route.snapshot.queryParams[NAV_PARAM_FROM];
		this.mode = this.resolveMode();
		this.updateSaveAndCloseTextKeys();

		this.getAgents().then(_ => {
			if (this.isVersionMode()) {
				this.initVersionMode();
			} else if (this.isEditMode()) {
				const id = this.route.snapshot.params.conceptId;
				if (id) {
					this.conceptId = id;
					this.conceptService.load(id);
					this.conceptService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
						this.dto = x;
						this.initialConceptType = this.dto?.conceptType;

						this.conceptVersionValidator.initialValue = this.dto?.version;
						this.conceptMultiIdentifiersValidator.setInitalValue(this.dto?.identifiers);

						if (this.dto.conceptType === this.conceptType.CodeList) {
							this.codelistIdentifierValidator.conceptId = this.dto.id;
						}
						this.mapDataToForm();
					});
					this.conceptService.publicationLevelInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(info => {
						this.publicationLevelInfo = info;
					});
				}
			} else {
				this.conceptService.resetCodeListEntries();
				this.conceptService.resetMappingTables();
				this.dto = this.createEmptyDto();
				this.conceptVersionValidator.initialValue = undefined;
				this.conceptMultiIdentifiersValidator.setInitalValue(undefined);
			}

			this.form
				.get(FORM_FIELD_VERSION)
				?.valueChanges.pipe(takeUntil(this.unsubscribe$))
				.subscribe(() => {
					((this.form.get(FORM_FIELD_IDENTIFIERS)?.get(FORM_FIELD_IDENTIFIERS) as UntypedFormArray | null)?.controls ?? [])
						.filter(control => control?.get(FORM_FIELD_IDENTIFIER) !== null)
						.map(control => control?.get(FORM_FIELD_IDENTIFIER) as AbstractControl)
						.forEach(control => {
							control.markAsTouched();
							control.updateValueAndValidity({emitEvent: false});
						});
				});
		});
	}

	ngAfterViewInit() {
		this.UpdateModalDialog();
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	canDeactivate(): boolean | Observable<boolean> | Promise<boolean> {
		return this.isNavigationAllowed();
	}

	hasChanges(): boolean {
		return this.form.dirty;
	}

	isCreateMode(): boolean {
		return this.mode === ComponentMode.Create;
	}

	isEditMode(): boolean {
		return this.mode === ComponentMode.Edit;
	}

	isVersionMode(): boolean {
		return this.mode === ComponentMode.Version;
	}

	isFirstIdentifierLocked(): boolean {
		return this.publicationLevelInfo?.level === PublicationLevel.Public;
	}

	onDisableSave(disableSave: boolean): void {
		this.saveDisabled = disableSave;
	}

	onCancel(): void {
		this.navigateBack(undefined, this.isCreateMode());
	}

	onDiscard(): void {
		this.form.markAsPristine();
		this.navigateBack(undefined, this.isCreateMode());
	}

	onSave(): void {
		if (this.isFormValid) {
			this.save();
		}
	}

	onSaveAndClose(): void {
		if (this.isFormValid) {
			this.mapFormToData();
			if (this.isVersionMode()) {
				this.conceptInputClient.postCreateVersionByBody(this.mapToConceptInputCreateVersion(this.dto)).subscribe(response => {
					this.showSuccessNotification();
					this.form.markAsPristine();
					const id = response.headers.location?.split('/').pop();
					this.navigateBack(id);
				});
			} else if (this.isEditMode()) {
				this.conceptInputClient.putByBody(ConceptInputMapper.mapToInputModel(this.dto)).subscribe((_: any) => {
					this.showSuccessNotification();
					this.form.markAsPristine();
					this.navigateBack(this.dto.id);
				});
			} else {
				this.dto = this.createEmptyDto();
				this.mapFormToData();
				this.conceptInputClient.postByBody(ConceptInputMapper.mapToInputModel(this.dto)).subscribe((response: SwaggerResponse<void>) => {
					this.showSuccessNotification();
					this.form.markAsPristine();
					let id: string | undefined = '';
					if (response.headers.location) {
						id = response.headers.location.split('/').pop();
					}
					this.navigateBack(id);
				});
			}
		}
	}

	onBeforeOpenDialog(): void {
		this.cancelDialogConfig.enableSave = !this.isSaveDisabled;

		this.modalDialog.openDialog();
	}

	get isSaveDisabled(): boolean {
		return this.saveDisabled;
	}

	private resolveMode(): ComponentMode {
		const lastSegment = this.route.snapshot.url[this.route.snapshot.url.length - 1].path;
		if (lastSegment === NAV_VALUE_VERSION) return ComponentMode.Version;
		if (lastSegment === NAV_VALUE_EDIT) return ComponentMode.Edit;
		return ComponentMode.Create;
	}

	private initVersionMode(): void {
		[
			'publisher',
			'conceptType',
			'conformsTo',
			'theme',
			'keywords',
			'pattern',
			'minValue',
			'maxValue',
			'minLength',
			'maxLength',
			'measurementUnit',
			'nbDecimal',
			'codeListEntryValueType',
			'codeListEntryDefaultSortProperty',
			'codelistEntryValueMaxLength'
		].forEach(ctrl => this.form.get(ctrl)?.disable());

		const id = this.route.snapshot.params.conceptId;
		if (id) {
			this.conceptId = id;
			this.conceptService.load(id);
			this.conceptService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
				this.dto = x;
				this.conceptVersionValidator.initialValue = undefined;
				this.conceptMultiIdentifiersValidator.setInitalValue(this.dto?.identifiers);
				this.mapDataToForm();
			});
		}
	}

	private mapToConceptInputCreateVersion(dto: ConceptView): ConceptInputCreateVersion {
		return new ConceptInputCreateVersion({
			description: dto.description,
			identifiers: dto.identifiers ?? [],
			name: dto.name,
			previousVersionId: dto.id,
			responsibleDeputy: dto.responsibleDeputy,
			responsiblePerson: dto.responsiblePerson,
			validFrom: dto.validFrom,
			validTo: dto.validTo,
			version: dto.version
		});
	}

	private getAgents(): Promise<void> {
		return new Promise<void>(resolve => {
			this.agentClient.getUser().subscribe(response => {
				this.agents = response.result;
				resolve();
			});
		});
	}

	private createEmptyDto(): ConceptView {
		return new ConceptView({
			codeListEntryValueType: undefined,
			codeListEntryDefaultSortProperty: undefined,
			codelistEntryValueMaxLength: undefined,
			conceptType: undefined,
			conformsTo: [],
			description: new MultiLanguage(),
			identifiers: [],
			keywords: [],
			name: new MultiLanguage(),
			publisher: undefined,
			responsibleDeputy: undefined,
			responsiblePerson: undefined,
			themes: [],
			validFrom: undefined,
			validTo: undefined,
			version: DEFAULT_VERSION
		});
	}

	private isNavigationAllowed(beforeunloadEvent = false): Promise<boolean> {
		return new Promise<boolean>(resolve => {
			if (!this.hasChanges()) {
				resolve(true);
			} else {
				if (beforeunloadEvent) {
					resolve(false);
				} else {
					this.cancelDialogConfig.enableSave = !this.isSaveDisabled;

					const dialogRef = this.dialog.open(DialogComponent, {
						data: this.cancelDialogConfig,
						disableClose: true
					});
					const dialogCancel = dialogRef.componentInstance.cancel.subscribe(() => {
						resolve(false);
					});
					const dialogDiscardChanges = dialogRef.componentInstance.discardChanges.subscribe(() => {
						resolve(true);
					});
					const dialogSaveChanges = dialogRef.componentInstance.saveChanges.subscribe(() => {
						if (this.isFormValid) {
							this.save().then(() => {
								resolve(true);
							});
						} else {
							resolve(false);
						}
					});
					dialogRef.afterClosed().subscribe(() => {
						dialogCancel.unsubscribe();
						dialogDiscardChanges.unsubscribe();
						dialogSaveChanges.unsubscribe();
					});
				}
			}
		});
	}

	private mapDataToForm(): void {
		if (this.isVersionMode()) {
			this.mapDataToFormForVersion();
		} else {
			this.mapDataToFormForCreateEdit();
		}
	}

	private mapDataToFormForCreateEdit(): void {
		this.form.patchValue({
			conceptType: this.dto?.conceptType,
			description: new MultiLanguage(this.dto?.description),
			identifiers: this.dto?.identifiers,
			name: new MultiLanguage(this.dto?.name),
			publisher: this.dto?.publisher,
			responsibleDeputy: this.dto?.responsibleDeputy,
			responsiblePerson: this.dto?.responsiblePerson,
			validFrom: this.dto?.validFrom,
			validTo: this.dto?.validTo,
			version: this.dto?.version,
			pattern: this.dto?.pattern,
			minValue: this.dto?.minValue,
			maxValue: this.dto?.maxValue,
			minLength: this.dto?.minLength,
			maxLength: this.dto?.maxLength,
			measurementUnit: this.dto?.measurementUnit,
			nbDecimal: this.dto?.nbDecimal,
			codeListEntryValueType: this.dto?.codeListEntryValueType,
			codeListEntryDefaultSortProperty: this.dto?.codeListEntryDefaultSortProperty,
			codelistEntryValueMaxLength: this.dto?.codelistEntryValueMaxLength,
			theme: this.dto?.themes?.map(x => x.code),
			conformsTo: this.dto?.conformsTo,
			replaces: this.dto?.replaces ?? []
		});
	}

	private mapDataToFormForVersion(): void {
		this.form.patchValue({
			identifiers: this.dto?.identifiers,
			name: new MultiLanguage(this.dto?.name),
			description: new MultiLanguage(this.dto?.description),
			version: this.dto?.version,
			responsibleDeputy: this.dto?.responsibleDeputy,
			responsiblePerson: this.dto?.responsiblePerson,
			validFrom: this.dto?.validFrom,
			validTo: this.dto?.validTo
		});

		setTimeout(() => {
			this.form.get(FORM_FIELD_VERSION)?.updateValueAndValidity();
			this.form.get(FORM_FIELD_VERSION)?.markAsTouched();
		}, 200);
	}

	private mapFormToData(): void {
		this.contentLanguages.forEach(l => {
			this.dto.name![l as keyof MultiLanguage] = this.form.value.name[l] || undefined;
			this.dto.description![l as keyof MultiLanguage] = this.form.value.description[l] || undefined;
		});
		this.dto.identifiers = IdentifierMapper.mapElements(this.form.value.identifiers?.identifiers);
		this.dto.responsibleDeputy = this.CreateNewPerson(this.form.value.responsibleDeputy);
		this.dto.responsiblePerson = this.CreateNewPerson(this.form.value.responsiblePerson);
		this.dto.validFrom = this.form.value.validFrom;
		this.dto.validTo = this.form.value.validTo;
		this.dto.version = this.form.value.version;

		if (!this.isVersionMode()) {
			this.dto.conceptType = this.form.value.conceptType;
			this.dto.publisher = this.form.value.publisher;
			this.dto.themes = this.form.value.theme
				? CodeInputModelMapper.mapElements(this.form.value.theme)?.map(x => new VocabularyEntryModel({code: x.code}))
				: [];
			this.dto.keywords = KeywordMapper.mapElements(this.form.value.keywords);
			this.dto.conformsTo = ResourceMapper.mapElements(this.form.value.conformsTo);
			this.dto.replaces = (this.form.value.replaces ?? []) as ConceptReferenceModel[];
			this.dto.pattern = this.form.value.pattern || undefined;

			if (this.form.value.conceptType === ConceptType.Numeric) {
				this.dto.minValue = this.form.value.minValue;
				this.dto.maxValue = this.form.value.maxValue;
				this.dto.measurementUnit = this.form.value.measurementUnit;
				this.dto.nbDecimal = this.form.value.nbDecimal;
			}

			if (this.form.value.conceptType === ConceptType.String) {
				this.dto.minLength = this.form.value.minLength;
				this.dto.maxLength = this.form.value.maxLength;
			}

			if (this.form.value.conceptType === ConceptType.CodeList) {
				this.dto.codeListEntryDefaultSortProperty = this.form.value.codeListEntryDefaultSortProperty;
				this.dto.codeListEntryValueType = this.form.value.codeListEntryValueType;
				this.dto.codelistEntryValueMaxLength = this.form.value.codelistEntryValueMaxLength;
			}
		}
	}

	private navigateBack(id: string | undefined = undefined, cancelCreate: boolean = false): void {
		if (this.isVersionMode()) {
			this.navigateFromVersionMode(id);
		} else if (this.isCreateMode()) {
			this.navigateFromCreateMode(cancelCreate, id);
		} else {
			this.navigateFromEditMode(id);
		}
	}

	private navigateFromVersionMode(id: string | undefined): void {
		if (id) {
			this.router.navigate(['../../', id], {relativeTo: this.route});
		} else {
			this.router.navigate(['../'], {relativeTo: this.route});
		}
	}

	private navigateFromCreateMode(cancelCreate: boolean, id: string | undefined) {
		if (this.route?.parent?.snapshot.url[1].path === 'concepts') {
			this.router.navigate(cancelCreate ? ['../../'] : [`../${id}`], {
				relativeTo: this.route
			});
		} else {
			this.router.navigate(['../../concept-search'], {
				relativeTo: this.route,
				queryParams: id ? {conceptId: id} : {}
			});
		}
	}

	private navigateFromEditMode(id: string | undefined) {
		if (this.isEditMode() && this.from === NAV_VALUE_DATAELEMENT) {
			this.router.navigate(['../../../'], {relativeTo: this.route});
		} else if (this.isEditMode() && this.from === NAV_VALUE_DETAIL) {
			this.router.navigate(['../'], {relativeTo: this.route});
		} else {
			this.router.navigate(['../../../concept-search'], {relativeTo: this.route, queryParams: id ? {conceptId: id} : {}});
		}
	}

	private get isFormValid(): boolean {
		this.form.markAllAsTouched();
		this.form.updateValueAndValidity();
		return this.form.valid;
	}

	private save(): Promise<void> {
		return new Promise<void>(resolve => {
			this.mapFormToData();
			if (this.isVersionMode()) {
				this.conceptInputClient.postCreateVersionByBody(this.mapToConceptInputCreateVersion(this.dto)).subscribe(_ => {
					this.showSuccessNotification();
					this.form.markAsPristine();
					this.conceptMultiIdentifiersValidator.setInitalValue(this.dto?.identifiers);
					resolve();
				});
			} else if (this.isEditMode()) {
				this.conceptInputClient.putByBody(ConceptInputMapper.mapToInputModel(this.dto)).subscribe(_ => {
					this.showSuccessNotification();
					this.form.markAsPristine();
					this.conceptMultiIdentifiersValidator.setInitalValue(this.dto?.identifiers);
					resolve();
				});
			} else {
				this.conceptInputClient.postByBody(ConceptInputMapper.mapToInputModel(this.dto)).subscribe(_ => {
					this.showSuccessNotification();
					this.form.markAsPristine();
					this.conceptMultiIdentifiersValidator.setInitalValue(this.dto?.identifiers);
					resolve();
				});
			}
		});
	}

	private CreateNewPerson(person: ActiveDirectoryUser): Person | undefined {
		if (!person || typeof person === 'string') {
			return undefined;
		}
		// @ts-ignore
		if (person.name) {
			return person as Person;
		}
		return new Person({
			name: person.displayName,
			identifier: person.email,
			firstName: person.firstname,
			lastName: person.lastname
		});
	}

	private showSuccessNotification() {
		this.notification.success('i18n.notification.save_succeeded');
	}

	private UpdateModalDialog(): void {
		const headertextKey = 'i18n.edit.cancel_dialog.headertext';
		const bodytextKey = 'i18n.edit.cancel_dialog.bodytext';
		const saveChangesButtonTextKey = this.isEditMode() ? 'i18n.button.save_and_close' : 'i18n.datasets.concept.create.concept.button';

		this.translate // eslint-disable-next-line max-len
			.get([headertextKey, bodytextKey, DIALOG_CANCEL_BUTTON_KEY, DIALOG_DISCARD_CHANGES_BUTTON_KEY, DIALOG_SAVE_CHANGES_BUTTON_KEY, saveChangesButtonTextKey])
			.subscribe(result => {
				this.cancelDialogConfig = {
					showHeader: true,
					enableSave: true,
					headerText: result[headertextKey],
					bodyText: result[bodytextKey],
					dialogType: DialogType.save,
					okButtonText: '',
					cancelButtonText: result[DIALOG_CANCEL_BUTTON_KEY],
					confirmButtonText: '',
					discardChangesButtonText: result[DIALOG_DISCARD_CHANGES_BUTTON_KEY],
					saveChangesButtonText: result[saveChangesButtonTextKey]
				};
			});
	}

	private updateSaveAndCloseTextKeys() {
		const textCreateMode = 'i18n.datasets.concept.edit.concept.button';
		const tooltipCreateMode = 'i18n.datasets.concept.edit.concept.button.tooltip';
		const textEditMode = 'i18n.button.save_and_close';
		const tooltipEditMode = 'i18n.button.save_and_close.tooltip';
		this.translate.get([textCreateMode, textEditMode, tooltipCreateMode, tooltipEditMode]).subscribe(result => {
			this.saveAndCloseText = this.isEditMode() ? result[textEditMode] : result[textCreateMode];
			this.saveAndCloseTooltip = this.isEditMode() ? result[tooltipEditMode] : result[tooltipCreateMode];
		});
	}

	private createEmptyForm() {
		return new UntypedFormGroup(
			{
				description: new UntypedFormGroup(
					this.getObjectFromKeys(this.contentLanguages, () => new UntypedFormControl('')),
					{
						validators: [createMultilangValidator([...this.contentLanguages])]
					}
				),
				identifiers: new UntypedFormControl([]),
				conceptType: new UntypedFormControl('', [Validators.required]),
				name: new UntypedFormGroup(
					this.getObjectFromKeys(this.contentLanguages, () => new UntypedFormControl('')),
					{
						validators: [createMultilangValidator([...this.contentLanguages])]
					}
				),
				publisher: new UntypedFormControl('', [Validators.required]),
				responsibleDeputy: new UntypedFormControl('', [createPersonPickerValidator()]),
				responsiblePerson: new UntypedFormControl('', [Validators.required, createPersonPickerValidator()]),
				validFrom: new UntypedFormControl(''),
				validTo: new UntypedFormControl(''),
				version: new UntypedFormControl(DEFAULT_VERSION, {
					validators: [Validators.required, Validators.pattern(VERSION_PATTERN)],
					asyncValidators: [this.conceptVersionValidator.validate.bind(this.conceptVersionValidator)],
					updateOn: 'blur'
				}),
				theme: new UntypedFormControl(''),
				keywords: new UntypedFormControl(''),
				conformsTo: new UntypedFormControl(''),
				replaces: new UntypedFormControl([]),
				pattern: new UntypedFormControl(''),
				maxLength: new UntypedFormControl(''),
				minLength: new UntypedFormControl(''),
				measurementUnit: new UntypedFormControl(''),
				minValue: new UntypedFormControl(''),
				maxValue: new UntypedFormControl(''),
				nbDecimal: new UntypedFormControl(undefined),
				codeListEntryValueType: new UntypedFormControl(''),
				codeListEntryDefaultSortProperty: new UntypedFormControl(null),
				codelistEntryValueMaxLength: new UntypedFormControl('')
			},
			{
				validators: [createValidToNotEarlierThanValidFromValidator(), createDeputyNotSameAsPersonValidator()]
			}
		);
	}

	private getObjectFromKeys<Type>(keys: readonly string[], initialValue: (key: string) => Type) {
		return Object.assign({}, ...keys.map(x => ({[x]: initialValue(x)})));
	}
}
