import {AfterViewInit, Component, inject, OnDestroy, OnInit, SimpleChanges, ViewChild} from '@angular/core';
import {AbstractControl, UntypedFormArray, UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {MatDialog} from '@angular/material/dialog';
import {ActivatedRoute, Router} from '@angular/router';
import {
	ActiveDirectoryUser,
	AgentClient,
	IAgent,
	MappingTableModel,
	MappingTablesClient,
	MultiLanguage,
	Person,
	PublicationLevelInfoModel,
	RegistrationStatusInfoModel,
	UriInputModel,
	VocabularyEntryModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {Observable, Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {
	ComponentMode,
	FORM_FIELD_IDENTIFIERS,
	NAV_PARAM_FROM,
	NAV_VALUE_DETAIL,
	NAV_VALUE_EDIT,
	NAV_VALUE_VERSION,
	DIALOG_CANCEL_BUTTON_KEY,
	DIALOG_CREATE_BUTTON_KEY,
	DIALOG_DISCARD_CHANGES_BUTTON_KEY,
	DIALOG_SAVE_CHANGES_BUTTON_KEY,
	URI_PATTERN,
	DEFAULT_VERSION,
	VERSION_PATTERN,
	FORM_FIELD_VERSION,
	FORM_FIELD_IDENTIFIER
} from 'src/app/app-constants';
import {DeactivationGuarded} from 'src/app/shared/deactivationguarded.interface';
import {DialogComponent, DialogType, IDialogConfig} from 'src/app/shared/dialog/dialog.component';
import {createMultilangValidator} from 'src/app/shared/validators/multilang.validator';
import {DescriptionEditFormComponent} from './edit-form/description-edit-form.component';
import {Languages} from 'src/app/shared/ApplicationLanguage.enum';
import {createPersonPickerValidator} from 'src/app/shared/validators/person-picker.validator';
import {createValidToNotEarlierThanValidFromValidator} from 'src/app/shared/validators/valid-to-not-earlier-than-valid-from.validator';
import {createDeputyNotSameAsPersonValidator} from 'src/app/shared/validators/deputy-not-same-as-person.validator';
import {ResourceModelMapper} from 'src/app/shared/mappers/resourcemodelmapper';
import {IdentifierMapper} from 'src/app/shared/mappers/identifiermapper';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {CodeInputModelMapper} from 'src/app/shared/mappers/codeinputmodelmapper';
import {KeywordMapper} from 'src/app/shared/mappers/keywordmapper';
import {MappingTableService} from '../services/mappingtable.service';
import {MappingTableInputMapper} from 'src/app/shared/mappers/mappingtableinputmapper';
import {MappingTableVersionValidator} from 'src/app/shared/validators/mappingtable-version.validator';
import {MultiIdentifiersValidator} from 'src/app/shared/validators/identifier-validator/multi-Identifiers.validator';

@Component({
	selector: 'app-description-edit',
	templateUrl: './description.edit.component.html',
	styleUrls: [],
	standalone: false
})
export class DescriptionEditComponent implements OnInit, AfterViewInit, OnDestroy, DeactivationGuarded {
	@ViewChild(DescriptionEditFormComponent, {static: true}) descriptionEditForm!: DescriptionEditFormComponent;
	currentLanguage: string;
	mappingTableId: string = '';
	dto: MappingTableModel = new MappingTableModel();
	agents: IAgent[] = [];
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
	private readonly dialog = inject(MatDialog);
	private readonly fallback = inject(FallbackPipe);
	private readonly mappingTableClient = inject(MappingTablesClient);
	private readonly mappingTableMultiIdentifiersValidator = inject(MultiIdentifiersValidator);
	private readonly mappingTableVersionValidator = inject(MappingTableVersionValidator);
	private readonly mappingTableService = inject(MappingTableService);
	private readonly notification = inject(ObNotificationService);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly translate = inject(TranslateService);

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

		this.mappingTableService.registrationStatusInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.registrationStatusInfo = x));

		this.getAgents().then(_ => {
			if (this.isVersionMode()) {
				this.initVersionMode();
			} else if (this.isEditMode()) {
				this.initEditMode();
			} else {
				this.mappingTableService.resetMappingRelations();
				this.dto = this.createEmptyDto();
				this.mappingTableMultiIdentifiersValidator.setInitalValue(undefined);
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
			this.mappingTableClient.postVersionsByIdAndBody(this.mappingTableId, MappingTableInputMapper.mapToInputModel(this.dto)).subscribe(response => {
				let id: string | undefined;
				if (response.headers.location) {
					id = response.headers.location.split('/').pop();
				}
				this.updateAndNavigateBack(id);
			});
		} else if (this.isEditMode()) {
			this.mappingTableClient
				.putByIdAndBody(this.mappingTableId, MappingTableInputMapper.mapToInputModel(this.dto))
				.subscribe(_ => this.updateAndNavigateBack());
		} else {
			this.mappingTableClient.postByBody(MappingTableInputMapper.mapToInputModel(this.dto)).subscribe(response => {
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

	isCreateMode(): boolean {
		return this.mode === ComponentMode.Create;
	}

	private resolveMode(): ComponentMode {
		const lastSegment = this.route.snapshot.url[this.route.snapshot.url.length - 1].path;
		if (lastSegment === NAV_VALUE_VERSION) {
			return ComponentMode.Version;
		}
		if (lastSegment === NAV_VALUE_EDIT) {
			return ComponentMode.Edit;
		}
		return ComponentMode.Create;
	}

	private initVersionMode(): void {
		// Disable controls not used in version mode so their validators don't block save
		this.form.get('publisher')?.disable();
		this.form.get('sourceUri')?.disable();
		this.form.get('targetUri')?.disable();
		this.form.get(FORM_FIELD_IDENTIFIERS)?.disable();
		this.form.get('conformsTo')?.disable();
		this.form.get('keywords')?.disable();
		this.form.get('themes')?.disable();

		const id = this.route.snapshot.params.id;
		if (id) {
			this.mappingTableId = id;
			this.mappingTableClient.getById(id).subscribe(response => {
				this.dto = response.result;
				this.responsiblePerson = response.result.responsiblePerson;
				this.responsibleDeputy = response.result.responsibleDeputy;
				this.mappingTableVersionValidator.initialValue = undefined;
				this.mapDataToForm();
				this.updateTitle();
			});
		}
	}

	private initEditMode(): void {
		const id = this.route.snapshot.params.id;
		if (id) {
			this.mappingTableId = id;
			this.mappingTableService.load(id);
			this.mappingTableService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
				this.dto = x;
				this.responsiblePerson = x.responsiblePerson;
				this.responsibleDeputy = x.responsibleDeputy;
				this.mappingTableMultiIdentifiersValidator.setInitalValue(this.dto.identifiers);
				this.mappingTableVersionValidator.initialValue = this.dto.version;
				this.updateTitle();
			});
			this.mappingTableService.publicationLevelInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.publicationLevelInfo = x));
		}
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
				this.mappingTableClient.postVersionsByIdAndBody(this.mappingTableId, MappingTableInputMapper.mapToInputModel(this.dto)).subscribe(_ => {
					this.updateAfterSave();
					resolve();
				});
			} else if (this.isEditMode()) {
				this.mappingTableClient.putByIdAndBody(this.mappingTableId, MappingTableInputMapper.mapToInputModel(this.dto)).subscribe(_ => {
					this.updateAfterSave();
					resolve();
				});
			} else {
				this.mappingTableClient.postByBody(MappingTableInputMapper.mapToInputModel(this.dto)).subscribe(_ => {
					this.updateAfterSave();
					resolve();
				});
			}
		});
	}

	private updateAfterSave() {
		this.showSuccessNotification();
		this.form.markAsPristine();
		this.mappingTableMultiIdentifiersValidator.setInitalValue(this.dto?.identifiers);
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

	private navigateFromVersionMode(id: string | undefined) {
		if (id) {
			this.router.navigate(['../../', id], {relativeTo: this.route});
		} else {
			this.router.navigate(['../'], {relativeTo: this.route});
		}
	}

	private showSuccessNotification(): void {
		this.notification.success('i18n.notification.save_succeeded');
	}

	private fromDetail(): boolean {
		return this.from === NAV_VALUE_DETAIL;
	}

	private UpdateModalDialog(): void {
		const isEdit = this.isEditMode();
		const headertextKey: string = isEdit ? 'i18n.edit.cancel_dialog.headertext' : 'i18n.create.cancel_dialog.headertext';
		const bodytextKey: string = isEdit ? 'i18n.edit.cancel_dialog.bodytext' : 'i18n.create.cancel_dialog.bodytext';

		this.translate
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
					saveChangesButtonText: isEdit ? result[DIALOG_SAVE_CHANGES_BUTTON_KEY] : result[DIALOG_CREATE_BUTTON_KEY]
				};
			});
	}

	private updateTitle(): void {
		if (this.isVersionMode()) {
			this.translate.get('i18n.mapping_table.create.new.version.title').subscribe(result => {
				this.title = result;
			});
		} else if (this.isEditMode()) {
			this.title = this.fallback.transform(this.dto.name, this.currentLanguage) ?? '';
		} else {
			this.translate.get('i18n.mapping_table.create.title').subscribe(result => {
				this.title = result;
			});
		}
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
			conformsTo: this.dto?.conformsTo,
			description: new MultiLanguage(this.dto?.description),
			identifiers: this.dto.identifiers,
			name: new MultiLanguage(this.dto?.name),
			publisher: this.dto?.publisher,
			responsiblePerson: this.createNewActiveDirectoryUser(this.responsiblePerson),
			responsibleDeputy: this.createNewActiveDirectoryUser(this.responsibleDeputy),
			sourceUri: this.dto?.source?.uri,
			targetUri: this.dto?.target?.uri,
			themes: this.dto?.themes?.map(x => x.code),
			validFrom: this.dto?.validFrom,
			validTo: this.dto?.validTo,
			version: this.dto?.version
		});
	}

	private mapDataToFormForVersion(): void {
		this.form.patchValue({
			description: new MultiLanguage(this.dto?.description),
			name: new MultiLanguage(this.dto?.name),
			responsiblePerson: this.createNewActiveDirectoryUser(this.responsiblePerson),
			responsibleDeputy: this.createNewActiveDirectoryUser(this.responsibleDeputy),
			validFrom: this.dto?.validFrom,
			validTo: this.dto?.validTo,
			version: this.dto?.version
		});
		this.form.setControl(
			FORM_FIELD_IDENTIFIERS,
			new UntypedFormGroup({
				identifiers: new UntypedFormArray([])
			})
		);
		let identifierItems: UntypedFormArray = this.form.get(FORM_FIELD_IDENTIFIERS)?.get(FORM_FIELD_IDENTIFIERS) as UntypedFormArray;
		this.dto.identifiers?.forEach(identfier =>
			identifierItems?.push(
				new UntypedFormGroup({
					identifier: new UntypedFormControl(identfier)
				})
			)
		);

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

		if (this.isVersionMode() == false) {
			this.dto.conformsTo = ResourceModelMapper.mapElements(this.form.value.conformsTo);
			this.dto.keywords = KeywordMapper.mapElements(this.form.value.keywords);
			this.dto.publisher = this.form.value.publisher;
			this.dto.responsiblePerson =
				this.form.value.responsiblePerson && typeof this.form.value.responsiblePerson !== 'string' ? this.form.value.responsiblePerson : undefined;
			this.dto.responsibleDeputy =
				this.form.value.responsibleDeputy && typeof this.form.value.responsibleDeputy !== 'string' ? this.form.value.responsibleDeputy : undefined;
			this.dto.source = new UriInputModel({uri: this.form.value.sourceUri});
			this.dto.target = new UriInputModel({uri: this.form.value.targetUri});
			this.dto.themes = CodeInputModelMapper.mapElements(this.form.value.themes)?.map(x => new VocabularyEntryModel({code: x.code}));
			this.dto.identifiers = IdentifierMapper.mapElements(this.form.value.identifiers.identifiers);
		}

		this.dto.validFrom = this.form.value.validFrom || undefined;
		this.dto.validTo = this.form.value.validTo || undefined;
		this.dto.version = this.form.value.version;
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

	private createEmptyDto(): MappingTableModel {
		return new MappingTableModel({
			conformsTo: [],
			description: new MultiLanguage(),
			id: undefined,
			identifiers: [],
			keywords: [],
			name: new MultiLanguage(),
			publisher: undefined,
			responsiblePerson: undefined,
			responsibleDeputy: undefined,
			source: undefined,
			target: undefined,
			themes: [],
			validFrom: undefined,
			validTo: undefined,
			system: undefined,
			version: DEFAULT_VERSION
		});
	}

	private createEmptyForm(): UntypedFormGroup {
		return new UntypedFormGroup(
			{
				conformsTo: new UntypedFormControl(''),
				description: new UntypedFormGroup(
					this.getObjectFromKeys(this.contentLanguages, () => new UntypedFormControl('')),
					{
						validators: [createMultilangValidator([...this.contentLanguages])]
					}
				),
				identifiers: new UntypedFormControl([]),
				keywords: new UntypedFormControl(''),
				name: new UntypedFormGroup(
					this.getObjectFromKeys(this.contentLanguages, () => new UntypedFormControl('')),
					{
						validators: [createMultilangValidator([...this.contentLanguages])]
					}
				),
				publisher: new UntypedFormControl('', [Validators.required]),
				responsiblePerson: new UntypedFormControl('', [Validators.required, createPersonPickerValidator()]),
				responsibleDeputy: new UntypedFormControl('', [createPersonPickerValidator()]),
				sourceUri: new UntypedFormControl('', [Validators.required, Validators.pattern(URI_PATTERN)]),
				targetUri: new UntypedFormControl('', [Validators.required, Validators.pattern(URI_PATTERN)]),
				themes: new UntypedFormControl(''),
				validFrom: new UntypedFormControl(''),
				validTo: new UntypedFormControl(''),
				version: new UntypedFormControl(DEFAULT_VERSION, {
					validators: [Validators.required, Validators.pattern(VERSION_PATTERN)],
					asyncValidators: [this.mappingTableVersionValidator.validate.bind(this.mappingTableVersionValidator)],
					updateOn: 'blur'
				})
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
