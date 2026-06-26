import {Pipe, PipeTransform} from '@angular/core';
import {AbstractControl} from '@angular/forms';
import {Observable, of} from 'rxjs';
import {catchError, delay, distinctUntilChanged, map} from 'rxjs/operators';

/*
 * Return true if control is invalid and untouched
 * Usage:
 *   control | isinvalidanduntouched
 */
@Pipe({
	name: 'isinvalidanduntouched',
	standalone: false
})
export class IsInvalidAndUntouchedPipe implements PipeTransform {
	transform(source: AbstractControl | null | undefined): Observable<boolean> {
		if (source) {
			return source.statusChanges.pipe(
				distinctUntilChanged(),
				delay(0),
				map(_ => source.invalid && source.untouched),
				catchError(_ => [])
			);
		}

		return of();
	}
}
