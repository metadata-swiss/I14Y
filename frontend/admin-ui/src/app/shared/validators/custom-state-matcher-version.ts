import {AbstractControl, FormGroupDirective, NgForm} from '@angular/forms';
import {ErrorStateMatcher} from '@angular/material/core';

export class CustomErrorStateMatcherVersion implements ErrorStateMatcher {
	private _isNumber = true;
	isErrorState(control: AbstractControl | null, form: FormGroupDirective | NgForm | null): boolean {
		let isNumber = true;

		if (form?.dirty) {
			const value = control?.value;
			isNumber = this.isNumeric(value);

			if (!isNumber) {
				const split = value.split('.');
				if (split.length === 3) {
					const res = split.map((y: any) => this.isNumeric(y));
					isNumber = res.findIndex((x: boolean) => x === false) === -1 ? true : false;
					if (!isNumber && split[2] === '') {
						isNumber = true;
					}
				}
			}

			this._isNumber = isNumber;
		}
		return !isNumber;
	}

	get hasError(): boolean {
		return !this._isNumber;
	}

	private isNumeric(n: any): boolean {
		return !isNaN(parseFloat(n)) && isFinite(n);
	}
}
