import {Injectable} from '@angular/core';
import {Observable} from 'rxjs/internal/Observable';
import {ReplaySubject, Subject} from 'rxjs';
import {FilterConfigurationModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ActivatedRoute} from '@angular/router';

@Injectable({providedIn: 'root'})
export class OffCanvasService {
	readonly filterConfiguration$: Observable<FilterConfigurationModel>;
	readonly languages$: Observable<string[]>;
	readonly titleKey$: Observable<string>;
	readonly route$: Observable<ActivatedRoute>;
	readonly resetFilterSelection$: Observable<void>;
	readonly toList$: Observable<{[key: string]: string[]}>;
	readonly toSearch$: Observable<{[key: string]: string[]}>;

	private readonly filterConfiguration: Subject<FilterConfigurationModel> = new ReplaySubject<FilterConfigurationModel>();
	private readonly languages: Subject<string[]> = new ReplaySubject<string[]>();
	private readonly titleKey: Subject<string> = new ReplaySubject<string>();
	private readonly route: Subject<ActivatedRoute> = new ReplaySubject<ActivatedRoute>();
	private readonly resetFilterSelection: Subject<void> = new ReplaySubject<void>();
	private readonly toList: Subject<{[key: string]: string[]}> = new ReplaySubject<{[key: string]: string[]}>();
	private readonly toSearch: Subject<{[key: string]: string[]}> = new ReplaySubject<{[key: string]: string[]}>();

	constructor() {
		this.filterConfiguration$ = this.filterConfiguration.asObservable();
		this.languages$ = this.languages.asObservable();
		this.titleKey$ = this.titleKey.asObservable();
		this.route$ = this.route.asObservable();
		this.resetFilterSelection$ = this.resetFilterSelection.asObservable();
		this.toList$ = this.toList.asObservable();
		this.toSearch$ = this.toSearch.asObservable();
	}

	setConfiguration(filterConfiguration: FilterConfigurationModel, languages: string[], titleKey: string, route: ActivatedRoute) {
		this.filterConfiguration.next(filterConfiguration);
		this.languages.next(languages);
		this.titleKey.next(titleKey);
		this.route.next(route);
	}

	reset() {
		this.resetFilterSelection.next();
	}

	navigateToList(queryParams: {[key: string]: string[]}) {
		this.toList.next(queryParams);
	}

	navigateToSearch(queryParams: {[key: string]: string[]}) {
		this.toSearch.next(queryParams);
	}
}
