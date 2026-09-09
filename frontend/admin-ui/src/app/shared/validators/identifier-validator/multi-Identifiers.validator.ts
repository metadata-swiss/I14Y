import {Injectable} from '@angular/core';
import {AbstractControl, AsyncValidator, UntypedFormArray, UntypedFormControl, ValidationErrors} from '@angular/forms';
import {Observable} from 'rxjs';

@Injectable()
export abstract class MultiIdentifiersValidator implements AsyncValidator {
	protected _initalValue: string[] | undefined;
	protected _formGroupToValidate: UntypedFormArray | undefined;
	protected _versionFormControl: UntypedFormControl | undefined;

	setInitalValue(initalValue: string[] | undefined) {
		this._initalValue = initalValue;
	}

	setFormGroup(formArray: UntypedFormArray | undefined) {
		this._formGroupToValidate = formArray;
	}

	setVersionFormControl(versionControl: UntypedFormControl | undefined) {
		this._versionFormControl = versionControl;
	}

	abstract validate(control: AbstractControl): Observable<ValidationErrors | null>;
}
