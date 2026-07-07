import {ValidationErrors, ValidatorFn, AbstractControl} from '@angular/forms';

export function createWhitespaceValidator(): ValidatorFn {
	return (control: AbstractControl): ValidationErrors | null => {
		if (!control.value) {
			return null;
		}
		return new RegExp(/^\S+$/).test(control.value) ? null : {whitespace: true};
	};
}
