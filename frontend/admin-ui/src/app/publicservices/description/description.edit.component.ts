import {AfterViewInit, Component, inject, OnDestroy, OnInit, SimpleChanges, ViewChild} from '@angular/core';
import {UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {MatDialog} from '@angular/material/dialog';
import {ActivatedRoute, Router} from '@angular/router';
import {
	ActiveDirectoryUser,
	AgentClient,
	IAgent,
	IopPersonModel,
	IVocabularyEntry,
	MultiLanguage,
	Person,
	PublicationLevelInfoModel,
	PublicServiceInputClient,
	PublicServiceModel,
	VocabularyClient,
	VocabularyEntry,
	VocabularyEntryModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {Observable, Subject} from 'rxjs';
import {map, takeUntil} from 'rxjs/operators';
import {
	NAV_PARAM_FROM,
	NAV_VALUE_DETAIL,
	NAV_VALUE_EDIT,
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
import {FormatFunctions} from 'src/app/shared/format-functions';
import {IsIncludedValidators} from 'src/app/shared/validators/is-included-validators';
import {PublicServiceService} from '../services/publicservice.service';
import {createPersonPickerValidator} from 'src/app/shared/validators/person-picker.validator';
import {createDeputyNotSameAsPersonValidator} from 'src/app/shared/validators/deputy-not-same-as-person.validator';
import {PublicServiceInputMapper} from 'src/app/shared/mappers/publicserviceinputmapper';
import {IdModelMapper} from 'src/app/shared/mappers/idmodelmapper';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {KeywordMapper} from 'src/app/shared/mappers/keywordmapper';
import {IdentifierMapper} from 'src/app/shared/mappers/identifiermapper';
import {MultiIdentifiersValidator} from 'src/app/shared/validators/identifier-validator/multi-Identifiers.validator';

@Component({
	selector: 'app-description-edit',
	templateUrl: './description.edit.component.html',
	styleUrls: ['./description.edit.component.scss'],
	standalone: false
})
export class DescriptionEditComponent implements OnInit, AfterViewInit, OnDestroy, DeactivationGuarded {
	@ViewChild(DescriptionEditFormComponent, {static: true}) descriptionEditForm!: DescriptionEditFormComponent;
	organisations: IAgent[] = [];
	currentLanguage: string;
	dto: PublicServiceModel = new PublicServiceModel();
	responsiblePerson: Person | undefined;
	responsibleDeputy: Person | undefined;
	from = '';
	publicationLevelInfo: PublicationLevelInfoModel | undefined;
	mode = '';
	title = '';
	spatialCHCodes: VocabularyEntry[] = [];

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
	private readonly spatialCh = 'DV_KT_BEZ_GDE_SNAP';

	private readonly agentClient = inject(AgentClient);
	private readonly dialog = inject(MatDialog);
	private readonly fallback = inject(FallbackPipe);
	private readonly notification = inject(ObNotificationService);
	private readonly publicServiceInputClient = inject(PublicServiceInputClient);
	private readonly publicServiceMultiIdentifiersValidator = inject(MultiIdentifiersValidator);
	private readonly publicServiceService = inject(PublicServiceService);
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
		this.mode = this.route.snapshot.url[this.route.snapshot.url.length - 1].path;
		this.updateTitle();

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
			this.UpdateModalDialog();
			this.updateTitle();
		});

		this.getOrganisations().then(_ => {
			if (this.mode === NAV_VALUE_EDIT) {
				const id = this.route.snapshot.params.id;
				if (id) {
					this.publicServiceService.load(id);
					this.publicServiceService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
						this.dto = x;
						this.responsiblePerson = x.responsiblePerson;
						this.responsibleDeputy = x.responsibleDeputy;
						this.publicServiceMultiIdentifiersValidator.setInitalValue(this.dto?.identifiers);
						this.updateTitle();
					});
					this.publicServiceService.publicationLevelInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.publicationLevelInfo = x));
				}
			} else {
				this.dto = this.createEmptyDto();
				this.publicServiceMultiIdentifiersValidator.setInitalValue(undefined);
			}
		});

		this.getSpatialCHCodes();
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
		if (this.isEditMode()) {
			this.publicServiceInputClient.putByBody(PublicServiceInputMapper.mapToInputModel(this.dto)).subscribe(_ => {
				this.updateAndNavigateBack();
			});
		} else {
			this.publicServiceInputClient.postByBody(PublicServiceInputMapper.mapToInputModel(this.dto)).subscribe(response => {
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
		return this.mode === NAV_VALUE_EDIT;
	}

	private getOrganisations(): Promise<void> {
		return new Promise<void>(resolve => {
			this.agentClient.getUser().subscribe(response => {
				this.organisations = response.result;
				if (this.organisations) {
					this.validatePublisher(this.organisations);
				}
				resolve();
			});
		});
	}

	private validatePublisher(organisations: IAgent[]): void {
		this.form
			.get('publisher')!
			.setValidators([Validators.required, IsIncludedValidators.isEquivalentValueIncluded(organisations, (x: any, y: any) => x.id === y.id)]);
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
			if (this.isEditMode()) {
				this.publicServiceInputClient.putByBody(PublicServiceInputMapper.mapToInputModel(this.dto)).subscribe(_ => {
					this.updateAfterSave();
					resolve();
				});
			} else {
				this.publicServiceInputClient.postByBody(PublicServiceInputMapper.mapToInputModel(this.dto)).subscribe(_ => {
					this.updateAfterSave();
					resolve();
				});
			}
		});
	}

	private updateAfterSave() {
		this.showSuccessNotification();
		this.form.markAsPristine();
		this.publicServiceMultiIdentifiersValidator.setInitalValue(this.dto?.identifiers);
	}

	private get isFormValid(): boolean {
		this.form.markAllAsTouched();
		this.form.updateValueAndValidity();
		return this.form.valid;
	}

	private navigateBack(id: string | undefined = undefined): void {
		if (this.isEditMode()) {
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

	private showSuccessNotification(): void {
		this.notification.success('i18n.notification.save_succeeded');
	}

	private fromDetail(): boolean {
		return this.from === NAV_VALUE_DETAIL;
	}

	private UpdateModalDialog(): void {
		const headertextKey: string = this.isEditMode() ? 'i18n.edit.cancel_dialog.headertext' : 'i18n.create.cancel_dialog.headertext';
		const bodytextKey: string = this.isEditMode() ? 'i18n.edit.cancel_dialog.bodytext' : 'i18n.create.cancel_dialog.bodytext';

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
			this.title = this.fallback.transform(this.dto.name, this.currentLanguage) ?? '';
		} else {
			this.translate.get('i18n.publicservices.description.create.title').subscribe(result => {
				this.title = result;
			});
		}
	}

	private mapDataToForm(): void {
		this.form.patchValue({
			name: new MultiLanguage(this.dto?.name),
			description: new MultiLanguage(this.dto?.description),
			identifiers: this.dto?.identifiers,
			languages: this.dto?.languages?.map(x => x.code),
			sectors: this.dto?.sectors?.map(x => x.code),
			spatial: FormatFunctions.convertArrayToString(this.dto?.spatial),
			spatialCH: this.dto?.spatialCH,
			thematicAreas: this.dto?.thematicAreas?.map(x => x.code),
			businessEvents: this.dto?.businessEvents?.map(x => x.code),
			lifeEvents: this.dto?.lifeEvents?.map(x => x.code),
			publisher: this.dto?.publisher,
			responsiblePerson: this.createNewActiveDirectoryUser(this.responsiblePerson),
			responsibleDeputy: this.createNewActiveDirectoryUser(this.responsibleDeputy),
			isDescribedAt: this.dto.isDescribedAt,
			relations: this.dto.relations,
			requires: this.dto.requires
		});
	}

	private mapFormToData(): void {
		this.contentLanguages.forEach(l => {
			this.dto.name![l as keyof MultiLanguage] = this.form.value.name[l] || undefined;
			this.dto.description![l as keyof MultiLanguage] = this.form.value.description[l] || undefined;
		});

		this.dto.identifiers = IdentifierMapper.mapElements(this.form.value.identifiers?.identifiers);
		this.dto.keywords = KeywordMapper.mapElements(this.form.value.keywords);
		this.dto.languages = this.form.value.languages ? (this.form.value.languages as string[]).map(x => new VocabularyEntryModel({code: x})) : undefined;
		this.dto.sectors = this.form.value.sectors ? (this.form.value.sectors as string[]).map(x => new VocabularyEntryModel({code: x})) : undefined;
		this.dto.spatial = FormatFunctions.convertStringToArray(this.form.value.spatial);
		this.dto.spatialCH = this.form.value.spatialCH;
		this.dto.thematicAreas = this.form.value.thematicAreas
			? (this.form.value.thematicAreas as string[]).map(x => new VocabularyEntryModel({code: x}))
			: undefined;
		this.dto.businessEvents = this.form.value.businessEvents
			? (this.form.value.businessEvents as string[]).map(x => new VocabularyEntryModel({code: x}))
			: undefined;
		this.dto.lifeEvents = this.form.value.lifeEvents ? (this.form.value.lifeEvents as string[]).map(x => new VocabularyEntryModel({code: x})) : undefined;
		this.dto.publisher = this.form.value.publisher;
		this.dto.responsiblePerson = this.form.value.responsiblePerson
			? new IopPersonModel({
					email: this.form.value.responsiblePerson.email,
					familyName: this.form.value.responsiblePerson.lastname,
					givenName: this.form.value.responsiblePerson.firstname
			  })
			: undefined;
		this.dto.responsibleDeputy = this.form.value.responsibleDeputy
			? new IopPersonModel({
					email: this.form.value.responsibleDeputy.email,
					familyName: this.form.value.responsibleDeputy.lastname,
					givenName: this.form.value.responsibleDeputy.firstname
			  })
			: undefined;

		this.dto.isDescribedAt = IdModelMapper.mapElements(this.form.value.isDescribedAt);
		this.dto.relations = IdModelMapper.mapElements(this.form.value.relations);
		this.dto.requires = IdModelMapper.mapElements(this.form.value.requires);
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

	private createEmptyDto(): PublicServiceModel {
		return new PublicServiceModel({
			id: undefined,
			description: new MultiLanguage(),
			name: new MultiLanguage(),
			identifiers: [],
			keywords: [],
			languages: [],
			sectors: [],
			spatial: [],
			spatialCH: [],
			thematicAreas: [],
			businessEvents: [],
			lifeEvents: [],
			publisher: undefined,
			responsiblePerson: undefined,
			responsibleDeputy: undefined,
			isDescribedAt: [],
			relations: [],
			requires: [],
			system: undefined
		});
	}

	private createEmptyForm(): UntypedFormGroup {
		return new UntypedFormGroup({
			name: new UntypedFormGroup(
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
			identifiers: new UntypedFormControl([]),
			keywords: new UntypedFormControl(''),
			languages: new UntypedFormControl(''),
			sectors: new UntypedFormControl(''),
			spatial: new UntypedFormControl(''),
			spatialCH: new UntypedFormControl(''),
			thematicAreas: new UntypedFormControl(''),
			businessEvents: new UntypedFormControl(''),
			lifeEvents: new UntypedFormControl(''),
			publisher: new UntypedFormControl('', [
				Validators.required,
				IsIncludedValidators.isEquivalentValueIncluded(this.organisations, (x: any, y: any) => x.id === y.id)
			]),
			responsiblePerson: new UntypedFormControl('', [createPersonPickerValidator()]),
			responsibleDeputy: new UntypedFormControl('', [createPersonPickerValidator()]),
			isDescribedAt: new UntypedFormControl(''),
			relations: new UntypedFormControl(''),
			requires: new UntypedFormControl('')
		}, {
			validators: [createDeputyNotSameAsPersonValidator()]
		});
	}

	private getObjectFromKeys<Type>(keys: readonly string[], initialValue: (key: string) => Type) {
		return Object.assign({}, ...keys.map(x => ({[x]: initialValue(x)})));
	}

	private getSpatialCHCodes() {
		this.vocabularyClient
			.getByIdentifier(this.spatialCh)
			.pipe(
				map(response => {
					const sortedEntries = response.result.sort((a: IVocabularyEntry, b: IVocabularyEntry) => {
						if (a.name?.en && b.name?.en) {
							return a.name.en.localeCompare(b.name.en);
						}
						return 0;
					});
					return sortedEntries;
				})
			)
			.pipe(map(response => (this.spatialCHCodes = response)))
			.subscribe();
	}
}
