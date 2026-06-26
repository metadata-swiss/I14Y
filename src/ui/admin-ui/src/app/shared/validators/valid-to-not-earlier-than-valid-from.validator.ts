import {AbstractControl, ValidationErrors, ValidatorFn} from '@angular/forms';

export const VALID_TO_BEFORE_VALID_FROM_ERROR = 'validToBeforeValidFrom';

export function createValidToNotEarlierThanValidFromValidator(validFromControlName = 'validFrom', validToControlName = 'validTo'): ValidatorFn {
	return (group: AbstractControl): ValidationErrors | null => {
		const validFromControl = group.get(validFromControlName);
		const validToControl = group.get(validToControlName);

		if (!validFromControl || !validToControl) {
			return null;
		}

		const validFrom = validFromControl.value ? new Date(validFromControl.value) : null;
		const validTo = validToControl.value ? new Date(validToControl.value) : null;

		const existingErrors = validToControl.errors ?? {};
		const {[VALID_TO_BEFORE_VALID_FROM_ERROR]: _removed, ...otherErrors} = existingErrors;

		if (validFrom && validTo && validTo.getTime() < validFrom.getTime()) {
			validToControl.setErrors({...otherErrors, [VALID_TO_BEFORE_VALID_FROM_ERROR]: true});
			if (!validToControl.touched) {
				validToControl.markAsTouched({onlySelf: true});
			}
			return {[VALID_TO_BEFORE_VALID_FROM_ERROR]: true};
		}

		const remainingErrors = Object.keys(otherErrors).length > 0 ? otherErrors : null;
		if (validToControl.errors && VALID_TO_BEFORE_VALID_FROM_ERROR in validToControl.errors) {
			validToControl.setErrors(remainingErrors);
		}
		return null;
	};
}
