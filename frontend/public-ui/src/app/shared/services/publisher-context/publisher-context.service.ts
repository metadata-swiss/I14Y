import {Injectable} from '@angular/core';
import {BehaviorSubject} from 'rxjs';

@Injectable({providedIn: 'root'})
export class PublisherContextService {
	private readonly _identifier = new BehaviorSubject<string | undefined>(undefined);
	readonly identifier$ = this._identifier.asObservable();

	set(identifier: string | undefined): void {
		this._identifier.next(identifier);
	}
}
