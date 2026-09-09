import {Component, ContentChild, inject, Input, OnDestroy, OnInit, Output, TemplateRef} from '@angular/core';
import {Subject} from 'rxjs';
import {filter, takeUntil} from 'rxjs/operators';
import {ActivatedRoute, NavigationEnd, Router} from '@angular/router';
import {SearchResultPagingInfo} from 'src/app/shared/search/SearchResultPagingInfo';
import {PageEvent} from '@angular/material/paginator';
import {TranslateService} from '@ngx-translate/core';

@Component({
	templateUrl: './search-results.component.html',
	standalone: false
})
export abstract class SearchResultsComponent implements OnInit, OnDestroy {
	@Input() displayTitle = true;
	@Input() displayViewSwitcher = false;
	@Output() results: object[];
	pagingInfo: SearchResultPagingInfo;
	showTableView: boolean = false;

	@ContentChild('listResult', {static: false}) listResultTemplateRef: TemplateRef<any>;
	@ContentChild('tableResult', {static: false}) tableResultTemplateRef: TemplateRef<any>;

	protected readonly defaultPage: number = 1;
	protected readonly defaultPageSize: number = 25;

	protected readonly router = inject(Router);
	protected readonly route = inject(ActivatedRoute);
	protected readonly translate = inject(TranslateService);

	protected readonly unsubscribe$ = new Subject();

	ngOnInit() {
		this.loadFilters().then(() => {
			this.router.events
				.pipe(
					takeUntil(this.unsubscribe$),
					filter((e): e is NavigationEnd => e instanceof NavigationEnd)
				)
				.subscribe(_ => this.search());
			this.search();
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	onChangePage(pageEvent: PageEvent) {
		const queryParams = {
			page: pageEvent.pageIndex + 1 === this.defaultPage ? null : pageEvent.pageIndex + 1,
			pageSize: pageEvent.pageSize === this.defaultPageSize ? null : pageEvent.pageSize
		};
		this.router.navigate([], {
			queryParams,
			queryParamsHandling: 'merge'
		});
	}

	protected abstract loadFilters(): Promise<void>;

	protected abstract search();
}
