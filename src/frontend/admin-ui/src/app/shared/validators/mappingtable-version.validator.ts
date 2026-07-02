import {inject, Injectable} from '@angular/core';
import {AbstractControl, AsyncValidator, UntypedFormArray, ValidationErrors} from '@angular/forms';
import {IdentifierVersionExistsResult, IdentifierVersionExistsResultMessage, MappingTablesClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, of} from 'rxjs';
import {catchError, map} from 'rxjs/operators';
import {FORM_FIELD_IDENTIFIER, FORM_FIELD_IDENTIFIERS} from 'src/app/app-constants';

@Injectable({providedIn: 'root'})
export class MappingTableVersionValidator implements AsyncValidator {
	private _initialValue: string | undefined;

	set initialValue(initialValue: string | undefined) {
		this._initialValue = initialValue;
	}

	private readonly client = inject(MappingTablesClient);

	validate(control: AbstractControl): Observable<ValidationErrors | null> {
		const identfierControls: AbstractControl[] = (
			(control.parent?.get(FORM_FIELD_IDENTIFIERS)?.get(FORM_FIELD_IDENTIFIERS) as UntypedFormArray | null)?.controls ?? []
		)
			.filter(control => control?.get(FORM_FIELD_IDENTIFIER) !== null)
			.map(control => control?.get(FORM_FIELD_IDENTIFIER) as AbstractControl);

		if (control.value === this._initialValue || !control.value) {
			return of(null);
		}

		if (identfierControls?.length > 0) {
			const firstIdentifierControl = identfierControls[0];
			if (firstIdentifierControl?.value) {
				return this.client.getIdentifierExistsByIdentifierAndVersion(firstIdentifierControl.value, control.value).pipe(
					map(response => this.checkValidationResult(response.result)),
					catchError(() => of(null))
				);
			}
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
