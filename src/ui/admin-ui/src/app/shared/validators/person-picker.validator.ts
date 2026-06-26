import {ValidationErrors, ValidatorFn, AbstractControl} from '@angular/forms';

export function createPersonPickerValidator(): ValidatorFn {
	return (control: AbstractControl): ValidationErrors | null => {
		const selection: any = control.value;
		if (typeof selection === 'string') {
			if (selection.length === 0) {
				return null;
			}
			return selection.length > 2 ? {notSelected: true} : {minSearchLength: true};
		}
		return null;
	};
}
