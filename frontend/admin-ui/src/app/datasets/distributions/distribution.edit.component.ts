import {AfterViewInit, Component, inject, OnDestroy, OnInit, SimpleChanges} from '@angular/core';
import {AbstractControl, UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {MatDialog} from '@angular/material/dialog';
import {ActivatedRoute, Router} from '@angular/router';
import {
	ChecksumInputModel,
	ChecksumModel,
	DatasetInputClient,
	DatasetsClient,
	DcatDatasetModel,
	DcatDistributionInputModel,
	DcatDistributionModel,
	MultiLanguage,
	PeriodOfTimeModel,
	PublicationLevelInfoModel,
	ResourceModel,
	VocabularyClient,
	VocabularyEntry,
	VocabularyEntryModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {Observable, Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {
	NAV_PARAM_FROM,
	NAV_VALUE_CREATE,
	NAV_VALUE_DETAIL,
	NAV_VALUE_EDIT,
	DIALOG_CANCEL_BUTTON_KEY,
	DIALOG_CREATE_BUTTON_KEY,
	DIALOG_DISCARD_CHANGES_BUTTON_KEY,
	DIALOG_SAVE_CHANGES_BUTTON_KEY,
	URI_PATTERN,
	IDENTIFIER_PATTERN
} from 'src/app/app-constants';
import {DeactivationGuarded} from 'src/app/shared/deactivationguarded.interface';
import {DialogComponent, DialogType, IDialogConfig} from 'src/app/shared/dialog/dialog.component';
import {IsIncludedValidators} from 'src/app/shared/validators/is-included-validators';
import {createMultilangValidator} from 'src/app/shared/validators/multilang.validator';
import {Languages} from 'src/app/shared/ApplicationLanguage.enum';
import {createChecksumValidator} from 'src/app/shared/validators/checksum.validator';
import {createWhitespaceValidator} from 'src/app/shared/validators/whitespace.validator';
import {ArrayHelper} from 'src/app/shared/helper/array-helper';
import {MultiLanguageMapper} from 'src/app/shared/mappers/multilanguagemapper';
import {ResourceModelMapper} from 'src/app/shared/mappers/resourcemodelmapper';
import {DcatDatasetInputModelMapper} from 'src/app/shared/mappers/dcatdatasetinputmodelmapper';
import {IdModelMapper} from 'src/app/shared/mappers/idmodelmapper';
import {DatasetService} from '../services/dataset.service';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';

@Component({
	selector: 'app-distribution.edit',
	templateUrl: './distribution.edit.component.html',
	styleUrls: [],
	standalone: false
})
export class DistributionEditComponent implements OnInit, AfterViewInit, OnDestroy, DeactivationGuarded {
	datasetId: string;
	currentLanguage: string;
	dto: DcatDistributionModel = new DcatDistributionModel();
	dataset: DcatDatasetModel = new DcatDatasetModel();
	formats: VocabularyEntry[] = [];
	mediaTypes: VocabularyEntry[] = [];
	licenses: VocabularyEntry[] = [];
	checksumAlgoriths: VocabularyEntry[] = [];
	packagingFormats: VocabularyEntry[] = [];
	availabilities: VocabularyEntry[] = [];
	from = '';
	publicationLevelInfo: PublicationLevelInfoModel | undefined;
	mode = '';
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

	private readonly formatIdentifier = 'VOCAB_I14Y_FILE_TYPE';
	private readonly fileTypeIdentifier = 'VOCAB_I14Y_MEDIA_TYPE';
	private readonly licenseIdentifier = 'VOCAB_I14Y_LICENSE';
	private readonly checksumAlgorithIdentifier = 'VOCAB_I14Y_CHECKSUM_ALGORITHM';
	private readonly packagingFormatIdentifier = 'VOCAB_I14Y_PACKAGING_FORMAT';
	private readonly plannedAvailabilityIdentifier = 'VOCAB_EU_PLANNED_AVAILABILITY';
	private readonly unsubscribe$ = new Subject();
	private readonly contentLanguages: readonly string[] = Languages.ContentLanguagesRm;

	private readonly datasetsClient = inject(DatasetsClient);
	private readonly datasetInputClient = inject(DatasetInputClient);
	private readonly datasetService = inject(DatasetService);
	private readonly dialog = inject(MatDialog);
	private readonly fallback = inject(FallbackPipe);
	private readonly notification = inject(ObNotificationService);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyClient = inject(VocabularyClient);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.datasetId = this.route.snapshot.params.id;
		this.form = this.createEmptyForm();
	}

	ngOnInit(): void {
		this.from = this.route.snapshot.queryParams[NAV_PARAM_FROM];
		this.mode = this.route.snapshot.url[this.route.snapshot.url.length - 1].path;
		this.updateTitle();
		this.getFormats();
		this.getMediaTyes();
		this.getLicenses();
		this.getChecksumAlgorithms();
		this.getPackagingFormats();
		this.getAvailabilities();

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
			this.UpdateModalDialog();
			this.updateTitle();
		});

		this.datasetsClient.getById(this.datasetId).subscribe(response => {
			this.dataset = response.result;

			if (this.mode === NAV_VALUE_EDIT) {
				const distributionId = this.route.snapshot.params.distributionId;
				this.dto = this.dataset.distributions?.find(d => d.id === distributionId)!;
			} else {
				this.dto = this.createEmptyDto();
			}
		});

		this.datasetService.load(this.datasetId);
		this.datasetService.publicationLevelInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.publicationLevelInfo = x));
	}

	ngAfterViewInit() {
		this.form.valueChanges.subscribe(() => {
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
		if (!this.isEditMode()) {
			this.dataset.distributions?.push(this.dto);
		}
		// eslint-disable-next-line max-len
		this.datasetInputClient.putByIdAndBody(this.datasetId, DcatDatasetInputModelMapper.mapToInputModel(this.dataset)).subscribe(_ => {
			this.updateAndNavigateBack();
		});
	}

	canDeactivate(): boolean | Observable<boolean> | Promise<boolean> {
		return this.isNavigationAllowed();
	}

	isEditMode(): boolean {
		return this.mode === NAV_VALUE_EDIT;
	}

	private updateAndNavigateBack() {
		this.updateAfterSave();
		this.navigateBack();
	}

	private isNavigationAllowed(beforeunloadEvent = false): Promise<boolean> {
		return new Promise<boolean>(resolve => {
			if (!this.form.dirty) {
				resolve(true);
			} else {
				if (beforeunloadEvent) {
					resolve(false);
				} else {
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
			if (!this.isEditMode()) {
				this.dataset.distributions?.push(this.dto);
			}
			this.datasetInputClient.putByIdAndBody(this.datasetId, DcatDatasetInputModelMapper.mapToInputModel(this.dataset)).subscribe(_ => {
				this.updateAfterSave();
				resolve();
			});
		});
	}

	private updateAfterSave() {
		this.showSuccessNotification();
		this.form.markAsPristine();
	}

	private get isFormValid(): boolean {
		this.form.markAllAsTouched();
		this.form.updateValueAndValidity();
		return this.form.valid;
	}

	private navigateBack(): void {
		if (this.isCreateMode() || (this.isEditMode() && this.fromDetail())) {
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

	private isCreateMode(): boolean {
		return this.mode === NAV_VALUE_CREATE;
	}

	private UpdateModalDialog(): void {
		const headertextKey: string = this.isEditMode() ? 'i18n.dialog.cancel_edit.header_text' : 'i18n.dialog.cancel_create.header_text';
		const bodytextKey: string = this.isEditMode() ? 'i18n.dialog.cancel_edit.body_text' : 'i18n.dialog.cancel_create.body_text';

		this.translate // eslint-disable-next-line max-len
			.get([headertextKey, bodytextKey, DIALOG_CANCEL_BUTTON_KEY, DIALOG_DISCARD_CHANGES_BUTTON_KEY, DIALOG_SAVE_CHANGES_BUTTON_KEY, DIALOG_CREATE_BUTTON_KEY])
			.subscribe(result => {
				this.cancelDialogConfig = {
					showHeader: true,
					enableSave: this.form.valid,
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

	private mapDataToForm(): void {
		this.form.patchValue(
			{
				title: new MultiLanguage(this.dto?.title),
				description: new MultiLanguage(this.dto?.description),
				identifier: this.dto.identifier,
				modified: this.dto.modified,
				issued: this.dto.issued,
				languages: this.dto.languages?.map(x => x.code),
				filesize: this.dto.byteSize,
				license: this.dto.license,
				rights: this.dto.rights,
				format: this.dto.format,
				mediaType: this.dto.mediaType,
				packagingFormat: this.dto.packagingFormat,
				coverageFrom: ArrayHelper.hasElements<PeriodOfTimeModel>(this.dto?.coverage) ? this.dto.coverage![0].start : undefined,
				coverageTo: ArrayHelper.hasElements<PeriodOfTimeModel>(this.dto?.coverage) ? this.dto.coverage![0].end : undefined,
				checksum: new ChecksumModel({algorithm: this.dto.checksum?.algorithm, checksumValue: this.dto.checksum?.checksumValue}),
				temporalResolution: this.dto.temporalResolution,
				availability: this.dto.availability,
				accessServices: this.dto.accessServices
			},
			{emitEvent: false, onlySelf: true}
		);
		this.form.controls.accessUrl.patchValue(
			{
				href: this.dto.accessUrl?.uri,
				isDownload: this.dto.downloadUrl !== undefined,
				label: this.dto.accessUrl?.label
			},
			{emitEvent: false, onlySelf: true}
		);

		this.form.markAsUntouched();
	}

	private mapFormToData() {
		this.contentLanguages.forEach(l => {
			this.dto.title![l as keyof MultiLanguage] = this.form.value.title[l] || undefined;
			this.dto.description![l as keyof MultiLanguage] = this.form.value.description[l] || undefined;
		});
		this.dto.identifier = this.form.value.identifier || undefined;
		this.dto.modified = this.form.value.modified;
		this.dto.issued = this.form.value.issued;
		this.dto.accessUrl = new ResourceModel({
			uri: this.form.value.accessUrl.href,
			label: this.form.value.accessUrl.label ? MultiLanguageMapper.clone(this.form.value.accessUrl.label) : undefined
		});
		this.dto.downloadUrl = this.form.value.accessUrl?.isDownload
			? new ResourceModel({
					uri: this.form.value.accessUrl.href,
					label: this.form.value.accessUrl.label ? MultiLanguageMapper.clone(this.form.value.accessUrl.label) : undefined
			  })
			: undefined;
		// eslint-disable-next-line max-len
		this.dto.languages = this.form.value.languages ? (this.form.value.languages as string[]).map(lang => new VocabularyEntryModel({code: lang})) : undefined;
		this.dto.byteSize = this.form.value.filesize > 0 ? this.form.value.filesize : undefined;
		this.dto.license = this.form.value.license;
		this.dto.rights = this.form.value.rights || undefined;
		this.dto.format = this.form.value.format;
		this.dto.mediaType = this.form.value.mediaType;
		this.dto.packagingFormat = this.form.value.packagingFormat;
		this.dto.checksum = this.mapChecksum(this.form.get('checksum'));
		this.dto.availability = this.form.value.availability;
		this.dto.temporalResolution = this.form.value.temporalResolution;
		this.dto.conformsTo = ResourceModelMapper.mapElements(this.form.value.conformsTo);
		this.dto.documentation = ResourceModelMapper.mapElements(this.form.value.documentation);
		this.dto.images = ResourceModelMapper.mapElements(this.form.value.images);


		this.dto.accessServices = IdModelMapper.mapElements(this.form.value.accessServices);

		const coverageItems = (this.form.value.coverage?.coverages ?? []) as {coverageFrom?: Date; coverageTo?: Date}[];
		this.dto.coverage = coverageItems
			.filter(x => x.coverageFrom || x.coverageTo)
			.map(x => new PeriodOfTimeModel({start: x.coverageFrom ?? undefined, end: x.coverageTo ?? undefined}));
	}

	private createEmptyDto(): DcatDistributionInputModel {
		return new DcatDistributionInputModel({
			accessUrl: new ResourceModel({
				uri: undefined,
				label: new MultiLanguage()
			}),
			availability: undefined,
			byteSize: undefined,
			checksum: undefined,
			conformsTo: [],
			coverage: [],
			description: new MultiLanguage(),
			documentation: [],
			downloadUrl: undefined,
			format: undefined,
			id: undefined,
			identifier: undefined,
			images: [],
			languages: [],
			modified: undefined,
			license: undefined,
			rights: undefined,
			mediaType: undefined,
			packagingFormat: undefined,
			issued: undefined,
			temporalResolution: undefined,
			title: new MultiLanguage(),
			accessServices: []
		});
	}

	private updateTitle(): void {
		if (this.isEditMode()) {
			this.title = this.fallback.transform(this.dto.title, this.currentLanguage) ?? '';
		} else {
			this.translate.get('i18n.title.create_distribution').subscribe(result => {
				this.title = result;
			});
		}
	}

	private createEmptyForm(): UntypedFormGroup {
		return new UntypedFormGroup({
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
			identifier: new UntypedFormControl('', [Validators.pattern(IDENTIFIER_PATTERN), createWhitespaceValidator()]),
			issued: new UntypedFormControl(''),
			modified: new UntypedFormControl(''),
			accessUrl: new UntypedFormGroup({
				href: new UntypedFormControl('', [Validators.required, Validators.pattern(URI_PATTERN)]),
				isDownload: new UntypedFormControl(''),
				label: new UntypedFormGroup(this.getObjectFromKeys(this.contentLanguages, () => new UntypedFormControl('')))
			}),
			languages: new UntypedFormControl(''),
			filesize: new UntypedFormControl('', [Validators.pattern(/^-?(0|[1-9]\d*)?$/)]),
			format: new UntypedFormControl(''),
			mediaType: new UntypedFormControl(''),
			packagingFormat: new UntypedFormControl(''),
			checksum: new UntypedFormGroup(
				{
					algorithm: new UntypedFormControl(''),
					checksumValue: new UntypedFormControl('')
				},
				{validators: [createChecksumValidator()]}
			),
			license: new UntypedFormControl(''),
			rights: new UntypedFormControl(''),
			availability: new UntypedFormControl(''),
			coverage: new UntypedFormControl([]),
			temporalResolution: new UntypedFormControl(''),
			conformsTo: new UntypedFormControl(''),
			documentation: new UntypedFormControl(''),
			images: new UntypedFormControl(''),
			accessServices: new UntypedFormControl('')
		});
	}

	private getObjectFromKeys<Type>(keys: readonly string[], initialValue: (key: string) => Type) {
		return Object.assign({}, ...keys.map(x => ({[x]: initialValue(x)})));
	}

	private mapChecksum(checksum: AbstractControl | null): ChecksumInputModel | undefined {
		if (checksum) {
			let algorithm = checksum.get('algorithm')?.value;
			let checksumValue = checksum.get('checksumValue')?.value;

			if (algorithm && checksumValue) {
				return new ChecksumInputModel({algorithm: algorithm, checksumValue: checksumValue});
			}
		}
		return undefined;
	}

	private getFormats(): void {
		this.vocabularyClient.getByIdentifier(this.formatIdentifier).subscribe(response => {
			this.formats = response.result;
			this.form.get('format')?.setValidators([IsIncludedValidators.isEquivalentValueIncluded(this.formats, (x: any, y: any) => x.code === y.code)]);
		});
	}

	private getMediaTyes(): void {
		this.vocabularyClient.getByIdentifier(this.fileTypeIdentifier).subscribe(response => {
			this.mediaTypes = response.result;
			this.form.get('mediaType')?.setValidators([IsIncludedValidators.isEquivalentValueIncluded(this.mediaTypes, (x: any, y: any) => x.code === y.code)]);
		});
	}

	private getLicenses() {
		this.vocabularyClient.getByIdentifier(this.licenseIdentifier).subscribe(response => {
			this.licenses = response.result;
			this.form.get('license')?.setValidators([IsIncludedValidators.isEquivalentValueIncluded(this.licenses, (x: any, y: any) => x.code === y.code)]);
		});
	}

	private getAvailabilities() {
		this.vocabularyClient.getByIdentifier(this.plannedAvailabilityIdentifier).subscribe(response => {
			this.availabilities = response.result;
			this.form
				.get('availability')
				?.setValidators([IsIncludedValidators.isEquivalentValueIncluded(this.availabilities, (x: any, y: any) => x.code === y.code)]);
		});
	}

	private getPackagingFormats() {
		this.vocabularyClient.getByIdentifier(this.packagingFormatIdentifier).subscribe(response => {
			this.packagingFormats = response.result;
			this.form
				.get('packagingFormat')
				?.setValidators([IsIncludedValidators.isEquivalentValueIncluded(this.packagingFormats, (x: any, y: any) => x.code === y.code)]);
		});
	}

	private getChecksumAlgorithms() {
		this.vocabularyClient.getByIdentifier(this.checksumAlgorithIdentifier).subscribe(response => {
			this.checksumAlgoriths = response.result;
			this.form
				.get('checksum')
				?.get('algorithm')
				?.setValidators([IsIncludedValidators.isEquivalentValueIncluded(this.checksumAlgoriths, (x: any, y: any) => x.code === y.code)]);
		});
	}
}
