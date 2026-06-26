import {inject, Pipe, PipeTransform} from '@angular/core';
import {ActiveDirectoryUser, PersonsClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, of} from 'rxjs';
import {catchError, debounceTime, distinctUntilChanged, filter, startWith, switchMap, map} from 'rxjs/operators';

/*
 * Load the persons from the active directory as the input changes, optionally defining a debounce interval
 * Usage:
 *   value$ | personsearch
 *   value$ | personsearch:debounceMs
 * Example:
 *   {{ input$ | personsearch:1000 }}
 */
@Pipe({
	name: 'personsearch',
	pure: true,
	standalone: false
})
export class PersonSearchPipe implements PipeTransform {
	private readonly personsClient = inject(PersonsClient);

	transform(input: Observable<any> | undefined, debounceMs: number = 300): Observable<ActiveDirectoryUser[]> {
		if (input) {
			return input.pipe(
				startWith(''),
				debounceTime(debounceMs),
				distinctUntilChanged(),
				filter((value: string) => value?.length > 2),
				switchMap(value => this.personsClient.getByQuery(value)),
				map(x => x.result),
				catchError(_ => [])
			);
		}
		return of([]);
	}
}
