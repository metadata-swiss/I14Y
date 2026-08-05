import {AfterViewInit, Component, inject, OnDestroy, OnInit, SimpleChanges, ViewChild} from '@angular/core';
import {UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {MatDialog} from '@angular/material/dialog';
import {ActivatedRoute, Router} from '@angular/router';
import {
	ActiveDirectoryUser,
	AgentClient,
	DataServiceInputClient,
	DataServiceModel,
	DataServicesClient,
	IAgent,
	IdModel,
	MultiLanguage,
	Person,
	PublicationLevelInfoModel,
	RegistrationStatusInfoModel,
	VocabularyClient,
	VocabularyEntry,
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
import {createMultilangValidator} from 'src/app/shared/validators/multilang.validator';
import {DescriptionEditFormComponent} from './edit-form/description-edit-form.component';
import {Languages} from 'src/app/shared/ApplicationLanguage.enum';
import {IsIncludedValidators} from 'src/app/shared/validators/is-included-validators';
import {createPersonPickerValidator} from 'src/app/shared/validators/person-picker.validator';
import {createDeputyNotSameAsPersonValidator} from 'src/app/shared/validators/deputy-not-same-as-person.validator';
import {DataserviceService} from '../services/dataservice.service';
import {DataServiceInputMapper} from 'src/app/shared/mappers/dataservicemodelmapper';
import {VCardModelMapper} from 'src/app/shared/mappers/vcardmodelmapper';
import {ResourceModelMapper} from 'src/app/shared/mappers/resourcemodelmapper';
import {IdModelMapper} from 'src/app/shared/mappers/idmodelmapper';
import {MultiIdentifiersValidator} from 'src/app/shared/validators/identifier-validator/multi-Identifiers.validator';
import {IdentifierMapper} from 'src/app/shared/mappers/identifiermapper';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {CodeInputModelMapper} from 'src/app/shared/mappers/codeinputmodelmapper';
import {KeywordMapper} from 'src/app/shared/mappers/keywordmapper';

@Component({
	selector: 'app-description-edit',
	templateUrl: './description.edit.component.html',
	styleUrls: [],
	standalone: false
})
export class DescriptionEditComponent implements OnInit, AfterViewInit, OnDestroy, DeactivationGuarded {
	@ViewChild(DescriptionEditFormComponent, {static: true}) descriptionEditForm!: DescriptionEditFormComponent;
	currentLanguage: string;
	dto: DataServiceModel = new DataServiceModel();
	agents: IAgent[] = [];
	licenses: VocabularyEntry[] = [];
	previousVersion: DataServiceModel | undefined = undefined;
	responsiblePerson: Person | undefined;
	responsibleDeputy: Person | undefined;
	from = '';
	publicationLevelInfo: PublicationLevelInfoModel | undefined;
	registrationStatusInfo: RegistrationStatusInfoModel | undefined;
	mode: ComponentMode = ComponentMode.Create;
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

	readonly licenseIdentifier = 'VOCAB_I14Y_LICENSE';
	private readonly unsubscribe$ = new Subject();
	private readonly contentLanguages: readonly string[] = Languages.ContentLanguagesRm;

	private readonly agentClient = inject(AgentClient);
	private readonly dataServiceInputClient = inject(DataServiceInputClient);
	private readonly dataserviceMultiIdentifiersValidator = inject(MultiIdentifiersValidator);
	private readonly dataServicesClient = inject(DataServicesClient);
	private readonly dataServiceService = inject(DataserviceService);
	private readonly dialog = inject(MatDialog);
	private readonly fallback = inject(FallbackPipe);
	private readonly notification = inject(ObNotificationService);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyClient = inject(VocabularyClient);

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

		this.dataServiceService.registrationStatusInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.registrationStatusInfo = x));

		this.getAgents().then(_ => {
			if (this.isVersionMode()) {
				this.initVersionMode();
			} else if (this.isEditMode()) {
				const id = this.route.snapshot.params.id;
				if (id) {
					this.dataServiceService.load(id);
					this.dataServiceService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
						this.dto = x;
						this.responsiblePerson = x.responsiblePerson;
						this.responsibleDeputy = x.responsibleDeputy;
						this.dataserviceMultiIdentifiersValidator.setInitalValue(this.dto?.identifiers);
						let previousVersionId = this.dto.previousVersion?.id;
						if (previousVersionId) {
							this.dataServicesClient.getById(previousVersionId).subscribe(r => {
								this.previousVersion = r.result;
							});
						}

						this.updateTitle();
					});
					this.dataServiceService.publicationLevelInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.publicationLevelInfo = x));
				}
			} else {
				this.dto = this.createEmptyDto();
				this.dataserviceMultiIdentifiersValidator.setInitalValue(undefined);
			}
		});

		this.getLicenses();
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
			this.dataServiceInputClient.postByBody(DataServiceInputMapper.mapToInputModel(this.dto)).subscribe(response => {
				let id: string | undefined = '';
				if (response.headers.location) {
					id = response.headers.location.split('/').pop();
				}
				this.updateAndNavigateBack(id);
			});
		} else if (this.isEditMode()) {
			this.dataServiceInputClient.putByBody(DataServiceInputMapper.mapToInputModel(this.dto)).subscribe(_ => this.updateAndNavigateBack());
		} else {
			this.dataServiceInputClient.postByBody(DataServiceInputMapper.mapToInputModel(this.dto)).subscribe(response => {
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
		this.dataServiceService.load(id);
		this.dataServiceService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.dto = x;
			this.dto.previousVersion = new IdModel({id});
			this.previousVersion = x;
			this.responsiblePerson = x.responsiblePerson;
			this.responsibleDeputy = x.responsibleDeputy;
			this.dataserviceMultiIdentifiersValidator.setInitalValue(undefined);
			this.updateTitle();

			const keepEnabled = [FORM_FIELD_TITLE, FORM_FIELD_DESCRIPTION, FORM_FIELD_IDENTIFIERS, FORM_FIELD_VERSION, FORM_FIELD_VERSION_NOTES];
			Object.keys(this.form.controls)
				.filter(k => !keepEnabled.includes(k))
				.forEach(k => this.form.get(k)?.disable());

			this.form.get(FORM_FIELD_VERSION)?.setValidators([Validators.required]);
			this.form.get(FORM_FIELD_VERSION)?.updateValueAndValidity();
		});
		this.dataServiceService.publicationLevelInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.publicationLevelInfo = x));
	}

	getLicenses(): void {
		this.vocabularyClient.getByIdentifier(this.licenseIdentifier).subscribe(response => {
			this.licenses = response.result;
			this.form.get('license')!.setValidators([IsIncludedValidators.isEquivalentValueIncluded(this.licenses, (x: any, y: any) => x.code === y.code)]);
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
				this.dataServiceInputClient.postByBody(DataServiceInputMapper.mapToInputModel(this.dto)).subscribe(_ => {
					this.updateAfterSave();
					resolve();
				});
			} else if (this.isEditMode()) {
				this.dataServiceInputClient.putByBody(DataServiceInputMapper.mapToInputModel(this.dto)).subscribe(_ => {
					this.updateAfterSave();
					resolve();
				});
			} else {
				this.dataServiceInputClient.postByBody(DataServiceInputMapper.mapToInputModel(this.dto)).subscribe(_ => {
					this.updateAfterSave();
					resolve();
				});
			}
		});
	}

	private updateAfterSave() {
		this.showSuccessNotification();
		this.form.markAsPristine();
		this.dataserviceMultiIdentifiersValidator.setInitalValue(this.dto?.identifiers);
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
		if (this.fromDetail()) {
			this.router.navigate(['../'], {relativeTo: this.route});
		} else {
			this.router.navigate(['../../'], {relativeTo: this.route});
		}
	}

	private showSuccessNotification(): void {
		this.notification.success('i18n.notification.save_succeeded');
	}

	private fromDetail(): boolean {
		return this.from === NAV_VALUE_DETAIL;
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
		if (this.isEditMode() || this.isVersionMode()) {
			this.title = this.fallback.transform(this.dto.title, this.currentLanguage) ?? '';
		} else {
			this.translate.get('i18n.title.create_dataservice').subscribe(result => {
				this.title = result;
			});
		}
	}

	private mapDataToForm(): void {
		this.form.patchValue({
			title: new MultiLanguage(this.dto?.title),
			description: new MultiLanguage(this.dto?.description),
			accessRights: this.dto?.accessRights?.code,
			license: this.dto?.license,
			conformsTo: this.dto?.conformsTo,
			contactPoint: this.dto?.contactPoints,
			documentation: this.dto?.documentation,
			endpointDescriptions: this.dto?.endpointDescriptions,
			endpointUrls: this.dto?.endpointUrls,
			landingPages: this.dto?.landingPages,
			publisher: this.dto?.publisher,
			themes: this.dto?.themes?.map(x => x.code),
			version: this.dto?.version,
			versionNotes: new MultiLanguage(this.dto?.versionNotes),
			responsiblePerson: this.createNewActiveDirectoryUser(this.responsiblePerson),
			responsibleDeputy: this.createNewActiveDirectoryUser(this.responsibleDeputy),
			issued: this.dto?.issued,
			modified: this.dto?.modified,
			servesDatasets: this.dto.servesDatasets,
			identifiers: this.dto.identifiers
		});
	}

	private mapFormToData(): void {
		if (this.isVersionMode()) {
			if (!this.dto.versionNotes) {
				this.dto.versionNotes = new MultiLanguage();
			}
			this.contentLanguages.forEach(l => {
				this.dto.title![l as keyof MultiLanguage] = this.form.value.title[l] || undefined;
				this.dto.description![l as keyof MultiLanguage] = this.form.value.description[l] || undefined;
				this.dto.versionNotes![l as keyof MultiLanguage] = this.form.value.versionNotes[l] || undefined;
			});
			this.dto.version = this.form.value.version;
			this.dto.identifiers = IdentifierMapper.mapElements(this.form.getRawValue().identifiers?.identifiers);
			return;
		}
		this.dto.accessRights = this.form.value.accessRights ? new VocabularyEntryModel({code: this.form.value.accessRights}) : undefined;

		if (!this.dto.versionNotes) {
			this.dto.versionNotes = new MultiLanguage();
		}

		this.contentLanguages.forEach(l => {
			this.dto.title![l as keyof MultiLanguage] = this.form.value.title[l] || undefined;
			this.dto.description![l as keyof MultiLanguage] = this.form.value.description[l] || undefined;
			this.dto.versionNotes![l as keyof MultiLanguage] = this.form.value.versionNotes[l] || undefined;
		});

		this.dto.conformsTo = ResourceModelMapper.mapElements(this.form.value.conformsTo);
		this.dto.contactPoints = VCardModelMapper.mapElements(this.form.value.contactPoint.contactPoint);
		this.dto.documentation = ResourceModelMapper.mapElements(this.form.value.documentation);
		this.dto.endpointDescriptions = ResourceModelMapper.mapElements(this.form.value.endpointDescriptions);
		this.dto.endpointUrls = ResourceModelMapper.mapElements(this.form.value.endpointUrls);
		this.dto.keywords = KeywordMapper.mapElements(this.form.value.keywords);
		this.dto.landingPages = ResourceModelMapper.mapElements(this.form.value.landingPages);
		this.dto.license = this.form.value.license ? new VocabularyEntryModel(this.form.value.license) : undefined;
		this.dto.publisher = this.form.value.publisher;
		this.dto.themes = CodeInputModelMapper.mapElements(this.form.value.themes)?.map(x => new VocabularyEntryModel({code: x.code}));
		this.dto.version = this.form.value.version;
		this.dto.responsiblePerson = this.form.value.responsiblePerson && typeof this.form.value.responsiblePerson !== 'string' ? this.form.value.responsiblePerson : undefined;
		this.dto.responsibleDeputy = this.form.value.responsibleDeputy && typeof this.form.value.responsibleDeputy !== 'string' ? this.form.value.responsibleDeputy : undefined;
		this.dto.modified = this.form.value.modified;
		this.dto.issued = this.form.value.issued;
		this.dto.servesDatasets = IdModelMapper.mapElements(this.form.value.servesDatasets);
		this.dto.identifiers = IdentifierMapper.mapElements(this.form.value.identifiers.identifiers);
	}

	private createNewActiveDirectoryUser(person: ActiveDirectoryUser | Person | undefined): ActiveDirectoryUser | undefined {
		if ((person as Person)?.identifier) {
			return new ActiveDirectoryUser({
				displayName: (person as Person).name,
				email: (person as Person).identifier,
				firstname: (person as Person).firstName,
				lastname: (person as Person).firstName
			});
		}
		if ((person as ActiveDirectoryUser)?.email) {
			return person as ActiveDirectoryUser;
		}
		return undefined;
	}

	private createEmptyDto(): DataServiceModel {
		return new DataServiceModel({
			accessRights: undefined,
			license: undefined,
			conformsTo: [],
			contactPoints: [],
			description: new MultiLanguage(),
			documentation: [],
			id: undefined,
			endpointUrls: [],
			endpointDescriptions: [],
			landingPages: [],
			keywords: [],
			publisher: undefined,
			responsiblePerson: undefined,
			responsibleDeputy: undefined,
			themes: [],
			title: new MultiLanguage(),
			version: undefined,
			versionNotes: new MultiLanguage(),
			issued: undefined,
			modified: undefined,
			servesDatasets: [],
			system: undefined,
			identifiers: []
		});
	}

	private createEmptyForm(): UntypedFormGroup {
		return new UntypedFormGroup({
			contactPoint: new UntypedFormControl([]),

			title: new UntypedFormGroup(
				this.getObjectFromKeys(this.contentLanguages, () => new UntypedFormControl('')),
				{
					validators: [createMultilangValidator([...this.contentLanguages])]
				}
			),
			description: new UntypedFormGroup(
				this.getObjectFromKeys(this.contentLanguages, () => new UntypedFormControl('')),
				{
					validators: [createMultilangValidator([...this.contentLanguages])]
				}
			),
			accessRights: new UntypedFormControl('', [Validators.required]),
			license: new UntypedFormControl(''),
			publisher: new UntypedFormControl('', [Validators.required]),
			endpointUrls: new UntypedFormControl('', [Validators.required]),
			endpointDescriptions: new UntypedFormControl(''),
			themes: new UntypedFormControl(''),
			keywords: new UntypedFormControl(''),
			landingPages: new UntypedFormControl(''),
			conformsTo: new UntypedFormControl(''),
			documentation: new UntypedFormControl(''),
			version: new UntypedFormControl(''),
			versionNotes: new UntypedFormGroup(this.getObjectFromKeys(this.contentLanguages, () => new UntypedFormControl(''))),
			responsiblePerson: new UntypedFormControl('', [createPersonPickerValidator()]),
			responsibleDeputy: new UntypedFormControl('', [createPersonPickerValidator()]),
			issued: new UntypedFormControl(''),
			modified: new UntypedFormControl(''),
			servesDatasets: new UntypedFormControl('')
		}, {
			validators: [createDeputyNotSameAsPersonValidator()]
		});
	}

	private getObjectFromKeys<Type>(keys: readonly string[], initialValue: (key: string) => Type) {
		return Object.assign({}, ...keys.map(x => ({[x]: initialValue(x)})));
	}
}
