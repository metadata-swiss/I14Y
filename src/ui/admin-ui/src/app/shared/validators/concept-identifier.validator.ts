import {inject, Injectable} from '@angular/core';
import {AbstractControl, AsyncValidator, ValidationErrors} from '@angular/forms';
import {ConceptInputClient, IdentifierVersionExistsResult, IdentifierVersionExistsResultMessage} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, of} from 'rxjs';
import {catchError, map} from 'rxjs/operators';

@Injectable({providedIn: 'root'})
export class ConceptIdentifierValidator implements AsyncValidator {
	private _initialValue: string | undefined;
	private _versionControl: AbstractControl<any, any> | null = null;

	set versionControl(value: AbstractControl<any, any> | null) {
		this._versionControl = value;
	}

	set initialValue(initialValue: string | undefined) {
		this._initialValue = initialValue;
	}

	private readonly client = inject(ConceptInputClient);

	validate(control: AbstractControl): Observable<ValidationErrors | null> {
		if (control.value === this._initialValue || !control.value) {
			return of(null);
		}

		if (this._versionControl?.value) {
			return this.client.getIdentifierExistsByIdentifierAndVersion(control.value, this._versionControl.value).pipe(
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
			this._versionControl?.setErrors({identifierAndVersionExists: true});
			this._versionControl?.markAsTouched();

			return {identifierAndVersionExists: true};
		} else {
			this._versionControl?.setErrors({identifierAndVersionExists: null});
			this._versionControl?.markAsTouched();
		}

		if (message === IdentifierVersionExistsResultMessage.IdentifierFromAnotherPublisherAlreadyExists) {
			return {anotherPublisher: true};
		}

		return null;
	}
}
