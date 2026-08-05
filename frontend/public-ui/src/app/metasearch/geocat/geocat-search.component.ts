import {AfterViewInit, Component, inject, OnDestroy} from '@angular/core';
import {ActivatedRoute, Params} from '@angular/router';
import {CatalogClient, SearchResourceType, FilterCountResultItem} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';
import {SearchEngineOptimizationService} from 'src/app/shared/services/search-engine-optimization/search-engine-optimization.service';

@Component({
	selector: 'app-geocat-search',
	templateUrl: './geocat-search.component.html',
	styleUrls: ['./geocat-search.component.scss'],
	standalone: false
})
export class GeocatSearchComponent implements AfterViewInit, OnDestroy {
	allCount: number = 0;
	catalogCount: FilterCountResultItem[] | undefined;
	catalogType = SearchResourceType;

	private readonly unsubscribe$ = new Subject();

	private readonly catalogClient = inject(CatalogClient);
	private readonly route = inject(ActivatedRoute);
	private readonly searchEngineOptimizationService = inject(SearchEngineOptimizationService);
	private readonly translate = inject(TranslateService);

	ngAfterViewInit(): void {
		this.updateMetaData();
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	updateCounters(): void {
		this.catalogClient // eslint-disable-next-line max-len
			.getSearchcountByQueryAndAccessRightsAndConceptValueTypesAndBusinessEventsAndFormatsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypes(
				this.route.snapshot.queryParams.query ?? undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined
			)
			.subscribe(response => {
				this.catalogCount = response.result.types ?? [];
				this.allCount = response.result.totalDocCount;
			});
	}

	getCountResult(type: SearchResourceType): number {
		return this.catalogCount?.find(r => r.reference === type)?.count ?? 0;
	}

	get queryParams(): Params {
		return {query: this.route.snapshot.queryParams.query};
	}

	private updateMetaData() {
		const titleKey = 'i18n.title.catalog.metasearch.geocat';
		const descriptionKey = 'i18n.meta_description.catalog.metasearch.geocat';

		this.translate
			.stream([titleKey, descriptionKey])
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(result => {
				this.searchEngineOptimizationService.UpdateMetaTitle(result[titleKey]);
				this.searchEngineOptimizationService.UpdateMetaDescrition(result[descriptionKey]);
			});
	}
}
