import {inject, Injectable} from '@angular/core';
import {AbstractControl, AsyncValidator, ValidationErrors} from '@angular/forms';
import {ConceptViewClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, of} from 'rxjs';
import {catchError, map} from 'rxjs/operators';

@Injectable({providedIn: 'root'})
export class CodelistEntryCodeValidator implements AsyncValidator {
	private _initalValue: string | undefined;
	private _conceptId: string | undefined;

	get conceptId(): string | undefined {
		return this._conceptId;
	}

	set conceptId(value: string | undefined) {
		this._conceptId = value;
	}

	private readonly client = inject(ConceptViewClient);

	setInitalValue(initalValue: string | undefined) {
		this._initalValue = initalValue;
	}

	validate(control: AbstractControl): Observable<ValidationErrors | null> {
		if (control.value === this._initalValue || !control.value) {
			return of(null);
		}

		if (this.conceptId) {
			return this.client.getCodelistEntriesExistsByIdAndCode(this.conceptId, control.value).pipe(
				map(exists => (exists.result ? {codeExists: true} : null)),
				catchError(() => of(null))
			);
		}

		return of(null);
	}
}
