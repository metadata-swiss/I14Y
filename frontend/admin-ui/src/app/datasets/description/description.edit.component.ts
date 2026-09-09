import {AfterViewInit, Component, inject, OnDestroy, OnInit, SimpleChanges, ViewChild} from '@angular/core';
import {UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {MatDialog} from '@angular/material/dialog';
import {ActivatedRoute, Router} from '@angular/router';
import {
	ActiveDirectoryUser,
	CodeInputModel,
	DatasetInputClient,
	DatasetsClient,
	DcatDatasetModel,
	IAgent,
	IdModel,
	IopPersonModel,
	MultiLanguage,
	PeriodOfTimeModel,
	Person,
	PublicationLevelInfoModel,
	RegistrationStatusInfoModel,
	UsersClient,
	VocabularyEntryModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {Observable, Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {
	ComponentMode,
	FORM_FIELD_DESCRIPTION,
	FORM_FIELD_IDENTIFIERS,
	FORM_FIELD_TITLE,
	FORM_FIELD_VERSION,
	FORM_FIELD_VERSION_NOTES,
	NAV_PARAM_FROM,
	NAV_VALUE_DETAIL,
	NAV_VALUE_EDIT,
	NAV_VALUE_VERSION,
	DIALOG_CANCEL_BUTTON_KEY,
	DIALOG_CREATE_BUTTON_KEY,
	DIALOG_DISCARD_CHANGES_BUTTON_KEY,
	DIALOG_SAVE_CHANGES_BUTTON_KEY
} from 'src/app/app-constants';
import {DeactivationGuarded} from 'src/app/shared/deactivationguarded.interface';
import {DialogComponent, DialogType, IDialogConfig} from 'src/app/shared/dialog/dialog.component';
import {IsIncludedValidators} from 'src/app/shared/validators/is-included-validators';
import {MultiLanguageMapper} from 'src/app/shared/mappers/multilanguagemapper';
import {ResourceModelMapper} from 'src/app/shared/mappers/resourcemodelmapper';
import {createMultilangValidator} from 'src/app/shared/validators/multilang.validator';
import {VCardModelMapper} from 'src/app/shared/mappers/vcardmodelmapper';
import {DescriptionEditFormComponent} from './edit-form/description-edit-form.component';
import {Languages} from 'src/app/shared/ApplicationLanguage.enum';
import {createPersonPickerValidator} from 'src/app/shared/validators/person-picker.validator';
import {createDeputyNotSameAsPersonValidator} from 'src/app/shared/validators/deputy-not-same-as-person.validator';
import {CodeInputModelMapper} from 'src/app/shared/mappers/codeinputmodelmapper';
import {DatasetService} from '../services/dataset.service';
import {IdentifierMapper} from 'src/app/shared/mappers/identifiermapper';
import {MultiIdentifiersValidator} from 'src/app/shared/validators/identifier-validator/multi-Identifiers.validator';
import {DcatDatasetInputModelMapper} from 'src/app/shared/mappers/dcatdatasetinputmodelmapper';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {KeywordMapper} from 'src/app/shared/mappers/keywordmapper';
import {SpatialMapper} from 'src/app/shared/mappers/spatialmapper';
import {AgentMapper} from 'src/app/shared/mappers/agentmapper';

@Component({
	selector: 'app-description-edit',
	templateUrl: './description.edit.component.html',
	standalone: false
})
export class DescriptionEditComponent implements OnInit, AfterViewInit, OnDestroy, DeactivationGuarded {
	@ViewChild(DescriptionEditFormComponent, {static: true}) descriptionEditForm!: DescriptionEditFormComponent;
	agents: IAgent[] = [];
	currentLanguage: string;
	datasetId: string | undefined;
	responsiblePerson: Person | undefined;
	responsiblePersonDeputy: Person | undefined;
	dto: DcatDatasetModel = new DcatDatasetModel();
	previousVersion: DcatDatasetModel | undefined = undefined;
	from = '';
	publicationLevelInfo: PublicationLevelInfoModel | undefined;
	mode: ComponentMode = ComponentMode.Create;
	registrationStatusInfo: RegistrationStatusInfoModel | undefined;
	title = '';

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

	form: UntypedFormGroup;

	private readonly unsubscribe$ = new Subject();
	private readonly contentLanguages: readonly string[] = Languages.ContentLanguagesRm;

	private readonly usersClient = inject(UsersClient);
	private readonly datasetInputClient = inject(DatasetInputClient);
	private readonly datasetMultiIdentifiersValidator = inject(MultiIdentifiersValidator);
	private readonly datasetsClient = inject(DatasetsClient);
	private readonly datasetService = inject(DatasetService);
	private readonly dialog = inject(MatDialog);
	private readonly notification = inject(ObNotificationService);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly translate = inject(TranslateService);
	private readonly fallback = inject(FallbackPipe);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.form = this.createEmptyForm();
	}

	ngOnInit() {
		this.from = this.route.snapshot.queryParams[NAV_PARAM_FROM];
		this.mode = this.resolveMode();
		this.updateTitle();

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
			this.UpdateModalDialog();
			this.updateTitle();
		});

		this.getAgents().then(_ => {
			if (this.isVersionMode()) {
				this.initVersionMode();
			} else if (this.isEditMode()) {
				this.datasetId = this.route.snapshot.params.id;
				if (this.datasetId) {
					this.datasetService.load(this.datasetId);
					this.datasetService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
						this.dto = x;
						if (x.previousVersion?.id) {
							this.datasetsClient.getById(x.previousVersion?.id).subscribe(res => {
								this.previousVersion = res.result;
							});
						}
						this.datasetMultiIdentifiersValidator.setInitalValue(this.dto?.identifiers);
						this.updateTitle();
						this.responsiblePerson = x.responsiblePerson;
						this.responsiblePersonDeputy = x.responsibleDeputy;
					});
					this.datasetService.publicationLevelInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.publicationLevelInfo = x));
					this.datasetService.registrationStatusInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.registrationStatusInfo = x));
				}
			} else {
				this.dto = this.createEmptyDto();
				this.datasetMultiIdentifiersValidator.setInitalValue(undefined);
			}
		});
	}

	ngAfterViewInit() {
		this.form.valueChanges.pipe(takeUntil(this.unsubscribe$)).subscribe(() => {
			this.UpdateModalDialog();
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	onFormChanges(changes: SimpleChanges): void {
		if (changes.dto) {
			const change = changes.dto;
			if (change.currentValue) {
				this.dto = Object.assign(change.currentValue);
				this.mapDataToForm();
			}
		}
	}

	onCancel(): void {
		this.form.markAsPristine();
		this.navigateBack();
	}

	onSave(): void {
		this.save();
	}

	onSaveAndClose(): void {
		this.mapFormToData();
		if (this.isVersionMode()) {
			this.datasetInputClient.postByBody(DcatDatasetInputModelMapper.mapToInputModel(this.dto)).subscribe(response => {
				let id: string | undefined = '';
				if (response.headers.location) {
					id = response.headers.location.split('/').pop();
				}
				this.updateAndNavigateBack(id);
			});
		} else if (this.isEditMode()) {
			if (this.datasetId) {
				this.datasetInputClient.putByIdAndBody(this.datasetId, DcatDatasetInputModelMapper.mapToInputModel(this.dto)).subscribe(_ => {
					this.updateAndNavigateBack();
				});
			}
		} else {
			this.datasetInputClient.postByBody(DcatDatasetInputModelMapper.mapToInputModel(this.dto)).subscribe(response => {
				let id: string | undefined = '';
				if (response.headers.location) {
					id = response.headers.location.split('/').pop();
				}
				this.updateAndNavigateBack(id);
			});
		}
	}

	canDeactivate(): boolean | Observable<boolean> | Promise<boolean> {
		return this.isNavigationAllowed();
	}

	isEditMode(): boolean {
		return this.mode === ComponentMode.Edit;
	}

	isVersionMode(): boolean {
		return this.mode === ComponentMode.Version;
	}

	private resolveMode(): ComponentMode {
		const segment = this.route.snapshot.url[this.route.snapshot.url.length - 1]?.path;
		if (segment === NAV_VALUE_VERSION) return ComponentMode.Version;
		if (segment === NAV_VALUE_EDIT) return ComponentMode.Edit;
		return ComponentMode.Create;
	}

	private initVersionMode(): void {
		const id = this.route.snapshot.params.id;
		this.datasetId = id;
		this.datasetService.load(id);
		this.datasetService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.dto = x;
			this.dto.previousVersion = new IdModel({id});
			this.datasetsClient.getById(id).subscribe(res => {
				this.previousVersion = res.result;
			});
			this.datasetMultiIdentifiersValidator.setInitalValue(undefined);
			this.updateTitle();
			this.responsiblePerson = x.responsiblePerson;
			this.responsiblePersonDeputy = x.responsibleDeputy;

			const keepEnabled = [FORM_FIELD_TITLE, FORM_FIELD_DESCRIPTION, FORM_FIELD_IDENTIFIERS, FORM_FIELD_VERSION, FORM_FIELD_VERSION_NOTES];
			Object.keys(this.form.controls)
				.filter(k => !keepEnabled.includes(k))
				.forEach(k => this.form.get(k)?.disable());

			this.form.get(FORM_FIELD_VERSION)?.setValidators([Validators.required]);
			this.form.get(FORM_FIELD_VERSION)?.updateValueAndValidity();
		});
	}

	private getAgents(): Promise<void> {
		return new Promise<void>(resolve => {
			this.usersClient.getUserInfo().subscribe({
				next: response => {
					this.agents = AgentMapper.mapUserAgents(response.result);
					this.validatePublisher(this.agents);
					resolve();
				},
				error: () => resolve()
			});
		});
	}

	private validatePublisher(agents: IAgent[]): void {
		this.form
			.get('publisher')!
			.setValidators([Validators.required, IsIncludedValidators.isEquivalentValueIncluded(agents, (x: any, y: any) => x.identifier === y.identifier)]);
	}

	private updateAndNavigateBack(id: string | undefined = undefined) {
		this.updateAfterSave();
		this.navigateBack(id);
	}

	private isNavigationAllowed(beforeunloadEvent = false): Promise<boolean> {
		return new Promise<boolean>(resolve => {
			if (!this.form.dirty) {
				resolve(true);
			} else {
				if (beforeunloadEvent) {
					resolve(false);
				} else {
					this.cancelDialogConfig.enableSave = this.descriptionEditForm.canSave();

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

	private save(): Promise<void> {
		return new Promise<void>(resolve => {
			this.mapFormToData();
			if (this.isVersionMode()) {
				this.datasetInputClient.postByBody(DcatDatasetInputModelMapper.mapToInputModel(this.dto)).subscribe(_ => {
					this.updateAfterSave();
					resolve();
				});
			} else if (this.isEditMode()) {
				if (this.datasetId) {
					this.datasetInputClient.putByIdAndBody(this.datasetId, DcatDatasetInputModelMapper.mapToInputModel(this.dto)).subscribe(_ => {
						this.updateAfterSave();
						resolve();
					});
				}
			} else {
				this.datasetInputClient.postByBody(DcatDatasetInputModelMapper.mapToInputModel(this.dto)).subscribe(_ => {
					this.updateAfterSave();
					resolve();
				});
			}
		});
	}

	private updateAfterSave() {
		this.showSuccessNotification();
		this.form.markAsPristine();
		this.datasetMultiIdentifiersValidator.setInitalValue(this.dto?.identifiers);
	}

	private get isFormValid(): boolean {
		this.form.markAllAsTouched();
		this.form.updateValueAndValidity();
		return this.form.valid;
	}

	private navigateBack(id: string | undefined = undefined): void {
		if (this.isVersionMode()) {
			this.navigateFromVersionMode(id);
		} else if (this.isEditMode()) {
			this.navigateFromEditMode();
		} else {
			this.navigateFromCreateMode(id);
		}
	}

	private navigateFromVersionMode(id: string | undefined = undefined): void {
		if (id) {
			this.router.navigate(['../../', id], {relativeTo: this.route});
		} else {
			this.router.navigate(['../'], {relativeTo: this.route});
		}
	}

	private navigateFromCreateMode(id: string | undefined) {
		if (id) {
			this.router.navigate([`../${id}`], {relativeTo: this.route});
		} else {
			this.router.navigate(['../../'], {relativeTo: this.route});
		}
	}

	private navigateFromEditMode() {
		if (this.from === NAV_VALUE_DETAIL) {
			this.router.navigate(['../'], {relativeTo: this.route});
		} else {
			this.router.navigate(['../../'], {relativeTo: this.route});
		}
	}

	private showSuccessNotification(): void {
		this.notification.success('i18n.notification.save_succeeded');
	}

	private UpdateModalDialog(): void {
		const headertextKey: string = this.isEditMode() ? 'i18n.dialog.cancel_edit.header_text' : 'i18n.dialog.cancel_create.header_text';
		const bodytextKey: string = this.isEditMode() ? 'i18n.dialog.cancel_edit.body_text' : 'i18n.dialog.cancel_create.body_text';

		this.translate // eslint-disable-next-line max-len
			.get([headertextKey, bodytextKey, DIALOG_CANCEL_BUTTON_KEY, DIALOG_DISCARD_CHANGES_BUTTON_KEY, DIALOG_SAVE_CHANGES_BUTTON_KEY, DIALOG_CREATE_BUTTON_KEY])
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
					saveChangesButtonText: this.isEditMode() ? result[DIALOG_SAVE_CHANGES_BUTTON_KEY] : result[DIALOG_CREATE_BUTTON_KEY]
				};
			});
	}

	private updateTitle(): void {
		if (this.isEditMode()) {
			this.title = this.fallback.transform(this.dto.title, this.currentLanguage) ?? '';
		} else if (this.isVersionMode()) {
			this.translate
				.get('i18n.title.create_dataset_version', {dataset_title: this.fallback.transform(this.dto.title, this.currentLanguage) ?? ''})
				.subscribe(result => {
					this.title = result;
				});
		} else {
			this.translate.get('i18n.title.create_dataset').subscribe(result => {
				this.title = result;
			});
		}
	}

	private mapDataToForm(): void {
		this.form.patchValue({
			accessRights: this.dto?.accessRights?.code,
			dataOwner: this.dto?.dataOwner,
			responsiblePerson: this.createNewActiveDirectoryUser(this.responsiblePerson),
			responsiblePersonDeputy: this.createNewActiveDirectoryUser(this.responsiblePersonDeputy),
			geoIvIdCodes: this.dto?.geoIvIds?.map(x => x.code),
			processId: this.dto?.processId,
			confidentialityPersonCode: this.dto?.confidentialityPerson?.code,
			description: new MultiLanguage(this.dto?.description),
			frequencyCode: this.dto?.frequency?.code,
			title: new MultiLanguage(this.dto?.title),
			images: this.dto?.images,
			isReferencedBy: this.dto?.isReferencedBy,
			languages: this.dto?.languages?.map(x => x.code),
			lastModificationDate: this.dto?.modified,
			publicationDate: this.dto?.issued,
			publisher: this.agents.find(agent => agent.identifier === this.dto?.publisher?.identifier),
			qualifiedAttributionComplement: new MultiLanguage(this.dto?.qualifiedAttributionComplement),
			relations: this.dto?.relations,
			retentionPeriod: this.dto?.retentionPeriod,
			retentionPeriodDescription: new MultiLanguage(this.dto?.retentionPeriodComplement),
			themeCodes: this.dto?.themes?.map(x => x.code),
			version: this.dto?.version,
			versionNotes: new MultiLanguage(this.dto?.versionNotes)
		});
	}

	private mapFormToData(): void {
		if (this.isVersionMode()) {
			this.dto.versionNotes = this.dto.versionNotes ?? new MultiLanguage();
			this.contentLanguages.forEach(l => {
				this.dto.title![l as keyof MultiLanguage] = this.form.value.title[l] || undefined;
				this.dto.description![l as keyof MultiLanguage] = this.form.value.description[l] || undefined;
				this.dto.versionNotes![l as keyof MultiLanguage] = this.form.value.versionNotes[l] || undefined;
			});
			this.dto.version = this.form.value.version;
			this.dto.identifiers = IdentifierMapper.mapElements(this.form.value.identifiers?.identifiers);
			this.dto.distributions?.forEach(d => (d.id = undefined));
			return;
		}
		this.dto.accessRights = this.form.value.accessRights ? new VocabularyEntryModel({code: this.form.value.accessRights}) : undefined;
		this.dto.confidentialityPerson = this.form.value.confidentialityPersonCode
			? new VocabularyEntryModel({code: this.form.value.confidentialityPersonCode})
			: undefined;
		this.dto.geoIvIds = CodeInputModelMapper.mapElements(this.form.value.geoIvIdCodes);
		this.dto.processId = this.form.value.processId;
		this.dto.dataOwner = (this.form.value.dataOwner as string)?.replace(/\s/g, '')?.length > 0 ? this.form.value.dataOwner : undefined;
		this.dto.responsiblePerson = this.form.value.responsiblePerson
			? new IopPersonModel({
					givenName: this.form.value.responsiblePerson.firstname,
					familyName: this.form.value.responsiblePerson.lastname,
					email: this.form.value.responsiblePerson.email
				})
			: undefined;
		this.dto.responsibleDeputy = this.form.value.responsiblePersonDeputy
			? new IopPersonModel({
					givenName: this.form.value.responsiblePersonDeputy.firstname,
					familyName: this.form.value.responsiblePersonDeputy.lastname,
					email: this.form.value.responsiblePersonDeputy.email
				})
			: undefined;
		this.dto.contactPoints = VCardModelMapper.mapElements(this.form.value.contactPoint.contactPoint);
		this.dto.conformsTo = ResourceModelMapper.mapElements(this.form.value.conformsTo);
		this.dto.documentation = ResourceModelMapper.mapElements(this.form.value.documentation);
		this.dto.frequency = this.form.value.frequencyCode ? new CodeInputModel({code: this.form.value.frequencyCode}) : undefined;
		this.dto.qualifiedAttributions = this.form.value.qualifiedAttributions;
		this.dto.qualifiedRelations = this.form.value.qualifiedRelations;
		this.dto.identifiers = IdentifierMapper.mapElements(this.form.value.identifiers.identifiers);
		this.dto.images = ResourceModelMapper.mapElements(this.form.value.images);
		this.dto.isReferencedBy = ResourceModelMapper.mapElements(this.form.value.isReferencedBy);
		this.dto.keywords = KeywordMapper.mapElements(this.form.value.keywords);
		this.dto.landingPages = ResourceModelMapper.mapElements(this.form.value.landingPages);
		// eslint-disable-next-line max-len
		this.dto.languages = this.form.value.languages ? (this.form.value.languages as string[]).map(lang => new VocabularyEntryModel({code: lang})) : undefined;
		this.dto.modified = this.form.value.lastModificationDate;
		this.dto.issued = this.form.value.publicationDate;
		this.dto.publisher = this.form.value.publisher;
		this.dto.relations = ResourceModelMapper.mapElements(this.form.value.relations);
		this.dto.retentionPeriod = this.form.value.retentionPeriod;
		this.dto.spatial = this.form.value.spatial?.spatials ? SpatialMapper.mapElements(this.form.value.spatial?.spatials) : undefined;
		this.dto.themes = CodeInputModelMapper.mapElements(this.form.value.themeCodes);
		this.dto.version = this.form.value.version;
		this.dto.qualifiedAttributionComplement = this.form.value.qualifiedAttributionComplement
			? MultiLanguageMapper.clone(this.form.value.qualifiedAttributionComplement)
			: undefined;
		this.dto.retentionPeriodComplement = this.form.value.retentionPeriodDescription
			? MultiLanguageMapper.clone(this.form.value.retentionPeriodDescription)
			: undefined;
		this.dto.versionNotes = this.form.value.versionNotes ? MultiLanguageMapper.clone(this.form.value.versionNotes) : undefined;

		this.contentLanguages.forEach(l => {
			this.dto.title![l as keyof MultiLanguage] = this.form.value.title[l] || undefined;
			this.dto.description![l as keyof MultiLanguage] = this.form.value.description[l] || undefined;
		});

		const temporalCoverageItems = (this.form.value.temporalCoverage?.temporalCoverages ?? []) as {coverageFrom?: Date; coverageTo?: Date}[];
		this.dto.temporalCoverage = temporalCoverageItems
			.filter(x => x.coverageFrom || x.coverageTo)
			.map(x => new PeriodOfTimeModel({start: x.coverageFrom ?? undefined, end: x.coverageTo ?? undefined}));
	}

	private createNewActiveDirectoryUser(person: ActiveDirectoryUser | Person | undefined): ActiveDirectoryUser | undefined {
		if ((person as Person)?.identifier) {
			return new ActiveDirectoryUser({
				displayName: (person as Person).name,
				email: (person as Person).identifier,
				firstname: (person as Person).firstName,
				lastname: (person as Person).lastName
			});
		}
		if ((person as ActiveDirectoryUser)?.email) {
			return person as ActiveDirectoryUser;
		}
		return undefined;
	}

	private createEmptyDto(): DcatDatasetModel {
		return new DcatDatasetModel({
			accessRights: undefined,
			dataOwner: undefined,
			responsiblePerson: undefined,
			responsibleDeputy: undefined,
			geoIvIds: undefined,
			processId: undefined,
			confidentialityPerson: undefined,
			conformsTo: [],
			contactPoints: [],
			description: new MultiLanguage(),
			documentation: [],
			frequency: undefined,
			images: [],
			identifiers: [],
			isReferencedBy: [],
			issued: undefined,
			keywords: [],
			landingPages: [],
			languages: [],
			modified: undefined,
			publisher: undefined,
			qualifiedAttributions: [],
			qualifiedAttributionComplement: new MultiLanguage(),
			qualifiedRelations: [],
			relations: [],
			retentionPeriod: undefined,
			retentionPeriodComplement: new MultiLanguage(),
			spatial: [],
			system: undefined,
			temporalCoverage: [],
			themes: [],
			title: new MultiLanguage(),
			version: undefined,
			versionNotes: new MultiLanguage()
		});
	}

	private createEmptyForm(): UntypedFormGroup {
		return new UntypedFormGroup(
			{
				accessRights: new UntypedFormControl('', [Validators.required]),
				confidentialityPersonCode: new UntypedFormControl(''),
				dataOwner: new UntypedFormControl(''),
				geoIvIdCodes: new UntypedFormControl(''),
				processId: new UntypedFormControl(''),
				responsiblePerson: new UntypedFormControl('', [createPersonPickerValidator()]),
				responsiblePersonDeputy: new UntypedFormControl('', [createPersonPickerValidator()]),
				conformsTo: new UntypedFormControl(''),
				contactPoint: new UntypedFormControl([]),
				description: new UntypedFormGroup(
					this.getObjectFromKeys(this.contentLanguages, () => new UntypedFormControl('')),
					{
						validators: [createMultilangValidator([...this.contentLanguages])]
					}
				),
				documentation: new UntypedFormControl(''),
				frequencyCode: new UntypedFormControl(''),
				identifiers: new UntypedFormControl([]),
				images: new UntypedFormControl(''),
				isReferencedBy: new UntypedFormControl(''),
				keywords: new UntypedFormControl(''),
				landingPages: new UntypedFormControl(''),
				languages: new UntypedFormControl(''),
				lastModificationDate: new UntypedFormControl(''),
				publicationDate: new UntypedFormControl(''),
				publisher: new UntypedFormControl('', [Validators.required]),
				qualifiedAttributions: new UntypedFormControl(''),
				qualifiedAttributionComplement: new UntypedFormGroup(this.getObjectFromKeys(this.contentLanguages, () => new UntypedFormControl(''))),
				qualifiedRelations: new UntypedFormControl(''),
				relations: new UntypedFormControl(''),
				retentionPeriod: new UntypedFormControl(''),
				retentionPeriodDescription: new UntypedFormGroup(this.getObjectFromKeys(this.contentLanguages, () => new UntypedFormControl(''))),
				spatial: new UntypedFormControl([]),
				temporalCoverage: new UntypedFormControl([]),
				themeCodes: new UntypedFormControl(''),
				title: new UntypedFormGroup(
					this.getObjectFromKeys(this.contentLanguages, () => new UntypedFormControl('')),
					{
						validators: [createMultilangValidator([...this.contentLanguages])]
					}
				),
				version: new UntypedFormControl(''),
				versionNotes: new UntypedFormGroup(this.getObjectFromKeys(this.contentLanguages, () => new UntypedFormControl('')))
			},
			{
				validators: [createDeputyNotSameAsPersonValidator('responsiblePerson', 'responsiblePersonDeputy')]
			}
		);
	}

	private getObjectFromKeys<Type>(keys: readonly string[], initialValue: (key: string) => Type) {
		return Object.assign({}, ...keys.map(x => ({[x]: initialValue(x)})));
	}
}
