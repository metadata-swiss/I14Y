import {Component, OnDestroy, AfterViewInit, inject} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {ActivatedRoute, Params} from '@angular/router';
import {SearchResultPagingInfo} from '../shared/search/SearchResultPagingInfo';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {SearchEngineOptimizationService} from '../shared/services/search-engine-optimization/search-engine-optimization.service';
import {CatalogEntry, SearchResourceType, FilterCountResult, FilterCountResultItem} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {SearchType} from '../shared/search-filters/search-filters';
import {SearchFilterService} from '../shared/search-filters/search-filters.service';

@Component({
	selector: 'app-catalog',
	templateUrl: './catalog.component.html',
	styleUrls: ['./catalog.component.scss'],
	standalone: false
})
export class CatalogComponent implements AfterViewInit, OnDestroy {
	hideFilters: boolean = true;
	query: string | undefined;
	searchResult: CatalogEntry[];
	pagingInfo: SearchResultPagingInfo = new SearchResultPagingInfo(null);
	allCount: number = 0;
	countResults: FilterCountResultItem[] | undefined;
	catalogType = SearchResourceType;
	searchTypeEnum = SearchType;

	updateFilterSubject: Subject<void> = new Subject<void>();

	private readonly unsubscribe$ = new Subject();

	private readonly route = inject(ActivatedRoute);
	private readonly searchEngineOptimizationService = inject(SearchEngineOptimizationService);
	private readonly translate = inject(TranslateService);

	ngAfterViewInit(): void {
		this.updateMetaData();
		this.updateFilterSubject.next();
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	onQueryChange(query: string | undefined) {
		this.query = query;
	}

	onCountResultChange(countResult: FilterCountResult) {
		this.countResults = countResult.types;
		this.allCount = countResult.totalDocCount;
	}

	getCountResult(type: SearchResourceType): number {
		return this.countResults?.find(r => r.reference === type)?.count ?? 0;
	}

	getQueryParams(searchType: SearchType | undefined = undefined): Params {
		if (searchType) {
			let params: Params = {};
			const keys = SearchFilterService.getOrderedKeys(searchType);

			keys.forEach(k => {
				params[k] = this.route.snapshot.queryParams[k];
			});

			return {query: this.route.snapshot.queryParams.query, ...params};
		}

		return {query: this.route.snapshot.queryParams.query};
	}

	toggleFilterVisibility() {
		this.hideFilters = !this.hideFilters;
	}

	getTranslateKeyFragmentFromUrl(): string {
		return this.route.parent.snapshot.url[1].path;
	}

	get lang() {
		return this.translate.getCurrentLang();
	}

	get searchType(): string {
		switch (this.route.parent.snapshot.url[1].path) {
			case 'datasets':
				return SearchType.Dataset;
			case 'publicservices':
				return SearchType.Publicservice;
			case 'dataservices':
				return SearchType.Dataservice;
			case 'concepts':
				return SearchType.Concept;
			case 'mappingtables':
				return SearchType.MappingTable;
			case 'all':
			default:
				return SearchType.All;
		}
	}

	private updateMetaData() {
		const titleKey = 'i18n.title.catalog.all';
		const descriptionKey = 'i18n.meta_description.catalog.all';

		this.translate
			.stream([titleKey, descriptionKey])
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(result => {
				this.searchEngineOptimizationService.UpdateMetaTitle(result[titleKey]);
				this.searchEngineOptimizationService.UpdateMetaDescrition(result[descriptionKey]);
			});
	}
}
