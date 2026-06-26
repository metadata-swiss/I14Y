import {Validators, ValidatorFn, AbstractControl} from '@angular/forms';

export class IsIncludedValidators extends Validators {
	static isIncluded(values: any[]): ValidatorFn {
		return (control: AbstractControl): {[key: string]: boolean} | null => {
			if (control.value && !values.includes(control.value)) {
				return {valueIsIncluded: true};
			}
			return null;
		};
	}
	static isEquivalentValueIncluded(values: any[], equals: (first: any, second: any) => boolean): ValidatorFn {
		return (control: AbstractControl): {[key: string]: boolean} | null => {
			if (control.value && !values.find(x => equals(x, control.value))) {
				return {valueIsIncluded: true};
			}
			return null;
		};
	}
}
