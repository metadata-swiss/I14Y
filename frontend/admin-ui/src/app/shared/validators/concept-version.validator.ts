import {inject, Injectable} from '@angular/core';
import {AbstractControl, AsyncValidator, UntypedFormArray, ValidationErrors} from '@angular/forms';
import {ConceptInputClient, IdentifierVersionExistsResult, IdentifierVersionExistsResultMessage} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, of} from 'rxjs';
import {catchError, map} from 'rxjs/operators';

@Injectable({providedIn: 'root'})
export class ConceptVersionValidator implements AsyncValidator {
	private _initialValue: string | undefined;

	set initialValue(initialValue: string | undefined) {
		this._initialValue = initialValue;
	}

	private readonly client = inject(ConceptInputClient);

	validate(control: AbstractControl): Observable<ValidationErrors | null> {
		if (control.value === this._initialValue || !control.value) {
			return of(null);
		}

		const firstIdentifierControl = (control.parent?.get('identifiers')?.get('identifiers') as UntypedFormArray)?.controls[0]?.get('identifier');
		if (firstIdentifierControl?.value) {
			return this.client.getIdentifierExistsByIdentifierAndVersion(firstIdentifierControl.value, control.value).pipe(
				map(response => this.checkValidationResult(response.result)),
				catchError(() => of(null))
			);
		}

		return of(null);
	}

	private checkValidationResult(existsResult: IdentifierVersionExistsResult): ValidationErrors | null {
		if (existsResult.result) {
			return this.getErrorFromMessage(existsResult.message);
		}

		return null;
	}

	private getErrorFromMessage(message: IdentifierVersionExistsResultMessage | undefined): ValidationErrors | null {
		if (message === IdentifierVersionExistsResultMessage.IdentifierAndVersionAlreadyExists) {
			return {identifierAndVersionExists: true};
		} 

		return null;
	}
}
