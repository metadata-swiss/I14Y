import {AbstractControl, ValidationErrors, ValidatorFn} from '@angular/forms';

export const DEPUTY_SAME_AS_PERSON_ERROR = 'deputySameAsPerson';
export const RESPONSIBLE_PERSON_CONTROL_NAME = 'responsiblePerson';
export const RESPONSIBLE_DEPUTY_CONTROL_NAME = 'responsibleDeputy';
export const RESPONSIBLE_PERSON_DEPUTY_CONTROL_NAME = 'responsiblePersonDeputy';

/**
 * Extracts a normalized, case-insensitive identifier from either an ActiveDirectoryUser
 * (which carries `.email`) or a Person model (which carries `.identifier`).
 * Returns undefined when the value is absent or still a raw search string.
 */
function getPersonIdentifier(person: any): string | undefined {
	return (person?.email ?? person?.identifier)?.toLowerCase();
}

export function createDeputyNotSameAsPersonValidator(
	personControlName = RESPONSIBLE_PERSON_CONTROL_NAME,
	deputyControlName = RESPONSIBLE_DEPUTY_CONTROL_NAME
): ValidatorFn {
	return (group: AbstractControl): ValidationErrors | null => {
		const personControl = group.get(personControlName);
		const deputyControl = group.get(deputyControlName);

		if (!personControl || !deputyControl) {
			return null;
		}

		const person = personControl.value;
		const deputy = deputyControl.value;

		const existingErrors = deputyControl.errors ?? {};
		const {[DEPUTY_SAME_AS_PERSON_ERROR]: _removed, ...otherErrors} = existingErrors;

		const personId = getPersonIdentifier(person);
		const deputyId = getPersonIdentifier(deputy);

		if (personId && deputyId && personId === deputyId) {
			deputyControl.setErrors({...otherErrors, [DEPUTY_SAME_AS_PERSON_ERROR]: true});
			if (!deputyControl.touched) {
				deputyControl.markAsTouched({onlySelf: true});
			}
			return {[DEPUTY_SAME_AS_PERSON_ERROR]: true};
		}

		const remainingErrors = Object.keys(otherErrors).length > 0 ? otherErrors : null;
		if (deputyControl.errors && DEPUTY_SAME_AS_PERSON_ERROR in deputyControl.errors) {
			deputyControl.setErrors(remainingErrors);
		}
		return null;
	};
}
