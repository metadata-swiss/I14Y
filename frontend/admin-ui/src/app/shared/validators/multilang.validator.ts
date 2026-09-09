import {ValidationErrors, ValidatorFn, AbstractControl} from '@angular/forms';

export function createMultilangValidator(languagesToValidate: string[]): ValidatorFn {
	return (control: AbstractControl): ValidationErrors | null => {
		const controls = languagesToValidate.map(x => control.get(x));

		if (controls.every(x => x)) {
			let errors: ValidationErrors | null = controls.some(x => x?.value) ? null : {multiLangError: true};
			controls.forEach(x => x!.setErrors(errors));
			return errors;
		}

		return null;
	};
}
