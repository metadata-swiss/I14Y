import {Component, EventEmitter, inject, Inject, OnInit, Output} from '@angular/core';
import {UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import {ConceptViewClient, MultiLanguage} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {createMultilangValidator} from '../../validators/multilang.validator';
import {Observable, of} from 'rxjs';
import {createWhitespaceValidator} from '../../validators/whitespace.validator';
import {CodelistEntryCodeValidator} from '../../validators/codelistentry-code.validator';
import {CodelistEntryParentCodeValidator} from '../../validators/codelistentry-parentcode.validator';
import {CodelistEntryDialogData} from './codelistentry.dialog.data';

@Component({
	selector: 'app-modal-dialog-codelist',
	templateUrl: './modal-dialog.component.html',
	standalone: false
})
export class ModalDialogCodeListComponent implements OnInit {
	@Output() save: EventEmitter<CodelistEntryDialogData> = new EventEmitter();
	form!: UntypedFormGroup;

	searchCodeListEntries$!: Observable<string[]>;

	private readonly codelistEntryCodeValidator = inject(CodelistEntryCodeValidator);
	private readonly codelistEntryParentCodeValidator = inject(CodelistEntryParentCodeValidator);
	private readonly conceptViewClient = inject(ConceptViewClient);

	constructor(@Inject(MAT_DIALOG_DATA) public data: CodelistEntryDialogData) {}

	ngOnInit(): void {
		this.form = new UntypedFormGroup({
			value: new UntypedFormControl('', {
				validators: [Validators.required, createWhitespaceValidator()],
				asyncValidators: [this.codelistEntryCodeValidator.validate.bind(this.codelistEntryCodeValidator)],
				updateOn: 'blur'
			}),
			parentCode: new UntypedFormControl('', {
				asyncValidators: [this.codelistEntryParentCodeValidator.validate.bind(this.codelistEntryParentCodeValidator)],
				updateOn: 'change'
			}),
			validFrom: new UntypedFormControl(),
			validTo: new UntypedFormControl(),
			name: new UntypedFormGroup(
				this.getObjectFromKeys(this.data.contentLanguages, () => new UntypedFormControl()),
				{
					validators: [createMultilangValidator([...this.data.contentLanguages])]
				}
			),

			description: new UntypedFormGroup(this.getObjectFromKeys(this.data.contentLanguages, () => new UntypedFormControl()))
		});

		this.mapDataToForm();

		if (this.data.dto.value) {
			this.codelistEntryCodeValidator.setInitalValue(this.data.dto.value);
		}

		this.codelistEntryCodeValidator.conceptId = this.data.conceptId;
		this.codelistEntryParentCodeValidator.conceptId = this.data.conceptId;
	}

	get isSaveDisabled(): boolean {
		return !this.form.dirty;
	}

	onParentCodeInputChanged(query: string): void {
		if (this.data.conceptId && query.length > 0) {
			this.conceptViewClient
				.getCodelistEntriesAutoCompleteByIdAndCodePrefixAndSortPropertyAndSortOrderAndPageAndPageSize(
					this.data.conceptId,
					query,
					undefined,
					undefined,
					undefined,
					undefined
				)
				.subscribe(response => {
					// eslint-disable-next-line max-len
					this.searchCodeListEntries$ = of(response.result.filter(entry => entry.value !== this.data.dto.value).map(entry => entry.value ?? ''));
				});
		}
	}

	safeClick() {
		if (this.isFormValid) {
			this.mapFormToData();
			this.save.emit(this.data);
		}
	}

	private get isFormValid(): boolean {
		this.form.markAllAsTouched();
		this.form.updateValueAndValidity();
		return this.form.valid;
	}

	private getObjectFromKeys<Type>(keys: readonly string[], initialValue: (key: string) => Type) {
		return Object.assign({}, ...keys.map(x => ({[x]: initialValue(x)})));
	}

	private mapDataToForm(): void {
		this.form.patchValue({
			value: this.data.dto.value,
			parentCode: this.data.dto.parentCode,
			validFrom: this.data.dto.validFrom,
			validTo: this.data.dto.validTo,
			name: new MultiLanguage(this.data.dto.name),
			description: this.data.dto.description
		});
	}

	private mapFormToData(): void {
		this.data.dto.value = this.form.value.value;
		this.data.dto.parentCode = this.form.value.parentCode?.trim().length > 0 ? this.form.value.parentCode : undefined;
		this.data.dto.validFrom = this.form.value.validFrom ? this.form.value.validFrom : null;
		this.data.dto.validTo = this.form.value.validTo ? this.form.value.validTo : undefined;
		this.data.contentLanguages.forEach((l: string) => {
			this.data.dto.name![l as keyof MultiLanguage] = this.form.value.name[l] || undefined;
			this.data.dto.description![l as keyof MultiLanguage] = this.form.value.description[l] || undefined;
		});
	}
}
