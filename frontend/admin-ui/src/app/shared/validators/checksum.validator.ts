import {ValidationErrors, ValidatorFn, AbstractControl} from '@angular/forms';

export function createChecksumValidator(): ValidatorFn {
	return (control: AbstractControl): ValidationErrors | null => {
		const algorithmControl = control.get('algorithm');
		const checksumValueControl = control.get('checksumValue');

		let errors: ValidationErrors | null = null;
		if (algorithmControl?.value || checksumValueControl?.value) {
			if (!algorithmControl?.value) {
				errors = {checksum: true};
				algorithmControl?.setErrors(errors);
			}

			if (!checksumValueControl?.value) {
				errors = {checksum: true};
				checksumValueControl?.setErrors(errors);
			}
		} else {
			algorithmControl?.setErrors(null);
			checksumValueControl?.setErrors(null);
		}

		return errors;
	};
}
