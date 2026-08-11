import {ValidationErrors, ValidatorFn, AbstractControl} from '@angular/forms';

export function createCoverageValidator(): ValidatorFn {
	return (control: AbstractControl): ValidationErrors | null => {
		const coverageFromControl = control.get('coverageFrom');
		const coverageToControl = control.get('coverageTo');

		let errors: ValidationErrors | null = null;

		if (coverageFromControl?.value && coverageToControl?.value) {
			const coverageFromDate = new Date(coverageFromControl.value);
			const coverageToDate = new Date(coverageToControl.value);
			if (coverageFromDate > coverageToDate) {
				errors = {coverage: true};
				coverageFromControl?.setErrors(errors);
				coverageFromControl?.markAsTouched();
				coverageToControl?.setErrors(errors);
				coverageToControl?.markAsTouched();
			} else {
				coverageFromControl?.setErrors(null);
				coverageToControl?.setErrors(null);
			}
		} else {
			coverageFromControl?.setErrors(null);
			coverageToControl?.setErrors(null);
		}

		return errors;
	};
}
