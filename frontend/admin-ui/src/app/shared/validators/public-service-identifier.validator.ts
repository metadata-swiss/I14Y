import {inject, Injectable} from '@angular/core';
import {AbstractControl, AsyncValidator, ValidationErrors} from '@angular/forms';
import {PublicServiceInputClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, of} from 'rxjs';
import {catchError, map} from 'rxjs/operators';

@Injectable({providedIn: 'root'})
export class PublicServiceIdentifierValidator implements AsyncValidator {
	private _initalValue: string | undefined;

	private readonly client = inject(PublicServiceInputClient);

	setInitalValue(initalValue: string | undefined) {
		this._initalValue = initalValue;
	}

	validate(control: AbstractControl): Observable<ValidationErrors | null> {
		if (control.value === this._initalValue || !control.value) {
			return of(null);
		}

		return this.client.getIdentifierExistsByIdentifier(control.value).pipe(
			map(exists => (exists.result ? {identifierExists: true} : null)),
			catchError(() => of(null))
		);
	}
}
