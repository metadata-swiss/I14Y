import {Component, EventEmitter, inject, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {NGXLogger} from 'ngx-logger';
import {Subject} from 'rxjs';
import {filter, takeUntil} from 'rxjs/operators';
import {ActivatedRoute, NavigationEnd, Router} from '@angular/router';

@Component({
	selector: 'app-search-input',
	templateUrl: './search-input.component.html',
	standalone: false
})
export class SearchInputComponent implements OnInit, OnDestroy {
	@Input() searchPath = './';
	@Input() backPath = './';
	@Input() disabled = false;
	@Input() minQueryLength: number = 3;
	@Output() queryChange = new EventEmitter<string | undefined>();

	query = '';

	private readonly unsubscribe$ = new Subject();

	private readonly logger = inject(NGXLogger);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);

	ngOnInit(): void {
		this.router.events
			.pipe(
				takeUntil(this.unsubscribe$),
				filter((e): e is NavigationEnd => e instanceof NavigationEnd)
			)
			.subscribe(_ => {
				this.matchQueryFromUrl();
			});
		this.matchQueryFromUrl();
	}

	ngOnDestroy(): void {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	onSearchClearButtonPressed() {
		this.logger.log('onSearchClearButtonPressed()...');
		this.searchWithEmptyQuery();
	}

	onSearchButtonPressed() {
		this.logger.log('onSearchButtonPressed()...');
		if (this.isSearchInputValid()) {
			this.search();
		} else if (this.query?.trim().length === 0) {
			this.searchWithEmptyQuery();
		}
	}

	onSearchInputEnter() {
		this.logger.log('onSearchInputEnter()...');
		if (this.isSearchInputValid()) {
			this.search();
		} else if (this.query?.trim().length === 0) {
			this.searchWithEmptyQuery();
		}
	}

	shouldDisplayWarning(): boolean {
		return !this.disabled && this.query?.length && this.query?.trim().length < this.minQueryLength;
	}

	isSearchInputValid(): boolean {
		return (this.query ?? '').trim().length >= this.minQueryLength;
	}

	private search() {
		const queryParams = {query: this.query ?? null, page: null};
		this.queryChange.emit(this.query ?? undefined);
		this.router.navigate([this.searchPath], {
			relativeTo: this.route,
			queryParams,
			queryParamsHandling: 'merge'
		});
	}

	private searchWithEmptyQuery() {
		this.query = '';
		this.queryChange.emit(undefined);
		this.router.navigate([this.backPath], {
			relativeTo: this.route,
			queryParams: {query: null, page: null, pageSize: null},
			queryParamsHandling: 'merge'
		});
	}

	private matchQueryFromUrl() {
		const query = this.route.snapshot.queryParamMap.get('query');
		this.query = query ?? '';
	}
}
