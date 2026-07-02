import {inject, Injectable} from '@angular/core';
import {AbstractControl, AsyncValidator, ValidationErrors} from '@angular/forms';
import {DatasetInputClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, of} from 'rxjs';
import {catchError, map} from 'rxjs/operators';

@Injectable({providedIn: 'root'})
export class NewVersionDatasetIdentifierValidator implements AsyncValidator {
	private readonly client = inject(DatasetInputClient);

	validate(control: AbstractControl): Observable<ValidationErrors | null> {
		if (!control.value) {
			return of(null);
		}

		return this.client.getIdentifierExistsByIdentifier(control.value).pipe(
			map(exists => (exists.result ? {identifierExists: true} : null)),
			catchError(() => of(null))
		);
	}
}
