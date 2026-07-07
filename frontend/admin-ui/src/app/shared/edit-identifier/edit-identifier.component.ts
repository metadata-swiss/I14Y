import {Component, inject, Input, OnChanges, SimpleChanges} from '@angular/core';
import {UntypedFormArray, UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {createWhitespaceValidator} from 'src/app/shared/validators/whitespace.validator';
import {MultiIdentifiersValidator} from '../validators/identifier-validator/multi-Identifiers.validator';
import {FORM_FIELD_IDENTIFIERS, FORM_FIELD_VERSION, IDENTIFIER_PATTERN} from 'src/app/app-constants';
import {map, Observable} from 'rxjs';
import {FormatFunctions} from '../format-functions';
import {TranslateService} from '@ngx-translate/core';

@Component({
	selector: 'app-edit-identifier',
	templateUrl: './edit-identifier.component.html',
	styleUrls: ['./edit-identifier.component.scss'],
	standalone: false
})
export class EditIdentifierComponent implements OnChanges {
	@Input() form!: UntypedFormGroup;
	@Input() dto: any;
	@Input() isIdentifierEditionDisabled: boolean = true;
	@Input() isFirstIdentifierLocked: boolean = false;
	@Input() isIdentifierRequired: boolean = false;
	public identifierForm: UntypedFormGroup;

	private readonly multiIdentifiersValidator = inject(MultiIdentifiersValidator);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.identifierForm = new UntypedFormGroup({
			identifiers: new UntypedFormArray([])
		});
	}

	ngOnChanges(changes: SimpleChanges): void {
		if (changes.dto) {
			this.items().clear();
			if (this.dto.identifiers?.length) {
				this.initControls();
			} else {
				this.addControl();
			}
			if (this.form.dirty || this.form.pristine) {
				this.form.setControl(FORM_FIELD_IDENTIFIERS, this.identifierForm);
				this.multiIdentifiersValidator.setFormGroup(this.form.get(FORM_FIELD_IDENTIFIERS)?.get(FORM_FIELD_IDENTIFIERS) as UntypedFormArray);
				this.multiIdentifiersValidator.setVersionFormControl(this.form.get(FORM_FIELD_VERSION) as UntypedFormControl);
			}
		}
	}

	items(): UntypedFormArray {
		return this.identifierForm.get(FORM_FIELD_IDENTIFIERS) as UntypedFormArray;
	}

	newItem(item?: string): UntypedFormGroup {
		return new UntypedFormGroup({
			identifier: new UntypedFormControl(item ?? '', {
				validators: this.isIdentifierRequired
					? [Validators.required, Validators.pattern(IDENTIFIER_PATTERN), createWhitespaceValidator()]
					: [Validators.pattern(IDENTIFIER_PATTERN), createWhitespaceValidator()],
				asyncValidators: [this.multiIdentifiersValidator.validate.bind(this.multiIdentifiersValidator)],
				updateOn: 'blur'
			})
		});
	}

	initControls() {
		this.dto.identifiers?.forEach((item: string) => {
			this.items().push(this.newItem(item));
		});
	}

	addControl() {
		this.items().push(this.newItem());
	}

	removeControl(i: number) {
		this.items().removeAt(i);
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
}
