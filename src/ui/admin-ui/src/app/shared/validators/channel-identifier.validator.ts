import {inject, Injectable} from '@angular/core';
import {AbstractControl, AsyncValidator, ValidationErrors} from '@angular/forms';
import {PublicServicesClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, of} from 'rxjs';
import {catchError, map} from 'rxjs/operators';

@Injectable({providedIn: 'root'})
export class ChannelIdentifierValidator implements AsyncValidator {
	private _initialValue: string | undefined;

	private readonly client = inject(PublicServicesClient);

	setInitialValue(initialValue: string | undefined) {
		this._initialValue = initialValue;
	}

	validate(control: AbstractControl): Observable<ValidationErrors | null> {
		if (control.value === this._initialValue || !control.value) {
			return of(null);
		}

		return this.client.getChannelsIdentifierExistsByIdentifier(control.value).pipe(
			map(exists => (exists.result ? {identifierExists: true} : null)),
			catchError(() => of(null))
		);
	}
}
