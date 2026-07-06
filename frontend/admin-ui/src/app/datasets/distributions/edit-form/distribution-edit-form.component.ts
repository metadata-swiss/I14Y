import {Component, EventEmitter, inject, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, ViewChild} from '@angular/core';
import {AbstractControl, UntypedFormArray, UntypedFormGroup} from '@angular/forms';
import {DcatDistributionModel, PublicationLevel, PublicationLevelInfoModel, VocabularyEntry} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Observable, Subject} from 'rxjs';
import {IDialogConfig} from 'src/app/shared/dialog/dialog.component';
import {MatAccordion} from '@angular/material/expansion';
import {map, startWith, takeUntil} from 'rxjs/operators';
import {Languages} from 'src/app/shared/ApplicationLanguage.enum';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {ModalDialogComponent} from 'src/app/shared/modal-dialog/modal-dialog.component';
import {IDENTIFIER_PATTERN, VOCAB_ID_FILE_TYPE} from 'src/app/app-constants';
import {FormatFunctions} from 'src/app/shared/format-functions';
import {VocabularyConfigService} from 'src/app/services/vocabulary-config.service';

@Component({
	selector: 'app-distribution-edit-form',
	templateUrl: './distribution-edit-form.component.html',
	styleUrls: ['./distribution-edit-form.component.scss'],
	standalone: false
})
export class DistributionEditFormComponent implements OnInit, OnDestroy, OnChanges {
	@Output() cancel: EventEmitter<void> = new EventEmitter();
	@Output() saveAndClose: EventEmitter<void> = new EventEmitter();
	@Output() save: EventEmitter<void> = new EventEmitter();
	@Output() formChanges: EventEmitter<SimpleChanges> = new EventEmitter();
	@Input() dto: DcatDistributionModel = new DcatDistributionModel();
	@Input() form!: UntypedFormGroup;
	@Input() cancelDialogConfig!: IDialogConfig;
	@Input() isEditMode!: boolean;
	@Input() publicationLevelInfo: PublicationLevelInfoModel | undefined;

	@Input() public set formats(input: VocabularyEntry[]) {
		this.allFormats.splice(0, this.allFormats.length, ...input);
	}
	@Input() public set mediaTypes(input: VocabularyEntry[]) {
		this.allMediaTypes.splice(0, this.allMediaTypes.length, ...input);
	}
	@Input() public set licenses(input: VocabularyEntry[]) {
		this.allLicenses.splice(0, this.allLicenses.length, ...input);
	}
	@Input() public set checksumAlgoriths(input: VocabularyEntry[]) {
		this.allChecksumAlgoriths.splice(0, this.allChecksumAlgoriths.length, ...input);
	}
	@Input() public set packagingFormats(input: VocabularyEntry[]) {
		this.allPackagingFormats.splice(0, this.allPackagingFormats.length, ...input);
	}
	@Input() public set availabilities(input: VocabularyEntry[]) {
		this.allAvailabilities.splice(0, this.allAvailabilities.length, ...input);
	}

	@ViewChild(MatAccordion) accordion!: MatAccordion;
	@ViewChild(ModalDialogComponent) modalDialog!: ModalDialogComponent;

	filteredFormats$!: Observable<VocabularyEntry[]>;
	filteredMediaTypes$!: Observable<VocabularyEntry[]>;
	filteredLicenses$!: Observable<VocabularyEntry[]>;
	filteredChecksumAlgoriths$!: Observable<VocabularyEntry[]>;
	filteredPackagingFormats$!: Observable<VocabularyEntry[]>;
	filteredAvailabilities$!: Observable<VocabularyEntry[]>;

	formatConceptPageIri: string | undefined = undefined;
	showAllLanguages: boolean;
	currentLanguage: string;
	languages: readonly string[] = Languages.ContentLanguagesRm;
	contentLanguages: readonly string[] = Languages.ContentLanguages;
	readonly maxEntries: number = 3;
	readonly publicationLevelEnum = PublicationLevel;

	private readonly allFormats: VocabularyEntry[] = [];
	private readonly allMediaTypes: VocabularyEntry[] = [];
	private readonly allLicenses: VocabularyEntry[] = [];
	private readonly allChecksumAlgoriths: VocabularyEntry[] = [];
	private readonly allPackagingFormats: VocabularyEntry[] = [];
	private readonly allAvailabilities: VocabularyEntry[] = [];
	private readonly editingChildren: string[] = [];
	private readonly unsubscribe$ = new Subject();
	private hasUpdatedControls: boolean = false;
	private readonly INVALID = 'INVALID';

	private readonly fallback = inject(FallbackPipe);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyConfigService = inject(VocabularyConfigService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.showAllLanguages = true;
	}

	ngOnChanges(changes: SimpleChanges): void {
		this.formChanges.emit(changes);
		if (this.dto.id) {
			setTimeout(() => {
				this.checkIfFormChangesIsInvalid();
			}, 100);
		}
	}

	ngOnInit(): void {
		this.initalizeFormatAutocomplete();
		this.initalizeMediaTypesAutocomplete();
		this.initalizeLicensesAutocomplete();
		this.initalizeChecksumAlgorithsAutocomplete();
		this.initalizePackagingFormatsAutocomplete();
		this.initalizeAvailabilitiesAutocomplete();

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
			this.showAllLanguages = false;
		});

		this.vocabularyConfigService
			.resolveConceptPageIris([VOCAB_ID_FILE_TYPE])
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(iris => {
				this.formatConceptPageIri = iris[VOCAB_ID_FILE_TYPE];
			});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
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

	getPatternErrorMassage(value: string): Observable<string> {
		return this.translate
			.get('i18n.error.pattern', {
				invalidChars: FormatFunctions.convertArrayToString(this.getInvalidCharaters(value), ', ')
			})
			.pipe(map(x => x as string));
	}

	private getInvalidCharaters(value: string) {
		const pattern = new RegExp(IDENTIFIER_PATTERN);
		let invalidCharacters: string[] = [];

		value.split('').forEach(char => {
			if (!pattern.test(char)) {
				invalidCharacters.push(char);
			}
		});
		return invalidCharacters;
	}

	private get isFormValid(): boolean {
		this.form.markAllAsTouched();
		this.form.updateValueAndValidity();
		return this.form.valid;
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

	private initalizeFormatAutocomplete() {
		let format = this.form.get('format');
		if (format) {
			this.filteredFormats$ = format.valueChanges.pipe(
				startWith(''),
				map(value => this.mapValue(value)),
				map(term => (term ? this.filterVocabulryEntry(this.allFormats, term) : this.allFormats))
			);
		}
	}

	private initalizeMediaTypesAutocomplete() {
		let mediaType = this.form.get('mediaType');
		if (mediaType) {
			this.filteredMediaTypes$ = mediaType.valueChanges.pipe(
				startWith(''),
				map(value => this.mapValue(value)),
				map(term => (term ? this.filterVocabulryEntry(this.allMediaTypes, term) : this.allMediaTypes))
			);
		}
	}

	private initalizeLicensesAutocomplete() {
		let license = this.form.get('license');
		if (license) {
			this.filteredLicenses$ = license.valueChanges.pipe(
				startWith(''),
				map(value => this.mapValue(value)),
				map(term => (term ? this.filterVocabulryEntry(this.allLicenses, term) : this.allLicenses))
			);
		}
	}

	private initalizeChecksumAlgorithsAutocomplete() {
		let algorithm = this.form.get('checksum')?.get('algorithm');
		if (algorithm) {
			this.filteredChecksumAlgoriths$ = algorithm.valueChanges.pipe(
				startWith(''),
				map(value => this.mapValue(value)),
				map(term => (term ? this.filterVocabulryEntry(this.allChecksumAlgoriths, term) : this.allChecksumAlgoriths))
			);
		}
	}

	private initalizePackagingFormatsAutocomplete() {
		let packagingFormat = this.form.get('packagingFormat');
		if (packagingFormat) {
			this.filteredPackagingFormats$ = packagingFormat.valueChanges.pipe(
				startWith(''),
				map(value => this.mapValue(value)),
				map(term => (term ? this.filterVocabulryEntry(this.allPackagingFormats, term) : this.allPackagingFormats))
			);
		}
	}

	private initalizeAvailabilitiesAutocomplete() {
		let availability = this.form.get('availability');
		if (availability) {
			this.filteredAvailabilities$ = availability.valueChanges.pipe(
				startWith(''),
				map(value => this.mapValue(value)),
				map(term => (term ? this.filterVocabulryEntry(this.allAvailabilities, term) : this.allAvailabilities))
			);
		}
	}

	private filterVocabulryEntry(list: VocabularyEntry[], term: string): VocabularyEntry[] {
		return list.filter((option: VocabularyEntry) => this.getName(option).toLowerCase().includes(term.toLowerCase()));
	}

	private getName(option: VocabularyEntry): string {
		return this.fallback.transform(option.name, this.currentLanguage) ?? '';
	}

	private mapValue(value: any): any {
		if (value) {
			return typeof value === 'string' ? value : value.name[this.currentLanguage];
		}
		return '';
	}
}
