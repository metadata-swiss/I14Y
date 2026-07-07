import {Component, EventEmitter, inject, Input, Output} from '@angular/core';
import {SearchResultsComponent} from 'src/app/shared/search/search-results/search-results.component';
import {SearchResultPagingInfo} from 'src/app/shared/search/SearchResultPagingInfo';
import {
	CatalogClient,
	CatalogEntry,
	SearchStructureOption,
	SearchResourceType,
	ConceptType,
	RelationsCountModel,
	RelationsCountRequestItem
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {SearchFilterService} from 'src/app/shared/search-filters/search-filters.service';
import {SearchFilters, SearchType} from 'src/app/shared/search-filters/search-filters';
import {ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {catchError, merge, of, Subject} from 'rxjs';
import {map, takeUntil} from 'rxjs/operators';
import {WithRelations} from './relations-count.model';
import {quoteQueryIfEmail} from 'src/app/shared/search/email-query.util';

@Component({
	selector: 'app-catalog-search-results',
	templateUrl: '../../shared/search/search-results/search-results.component.html',
	styleUrls: ['./catalog-search-results.component.scss'],
	standalone: false
})
export class CatalogSearchResultsComponent extends SearchResultsComponent {
	@Input() searchType: SearchType | undefined;
	@Output() update: EventEmitter<void> = new EventEmitter();

	private readonly catalogClient = inject(CatalogClient);
	private readonly searchFilterService = inject(SearchFilterService);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);

	/** Emits at the start of each search so a prior in-flight relations-count fetch is cancelled. */
	private readonly searchTrigger$ = new Subject<void>();

	protected loadFilters(): Promise<void> {
		return new Promise(resolve => resolve());
	}

	protected search() {
		const queryParams = this.route.snapshot.queryParamMap;
		const query = queryParams.get('query');
		const page = queryParams.has('page') ? Number(queryParams.get('page')) : this.defaultPage;
		const pageSize = queryParams.has('pageSize') ? Number(queryParams.get('pageSize')) : this.defaultPageSize;

		const filters = this.searchFilterService.getSelectedFilters(this.searchType);

		this.searchTrigger$.next();

		this.catalogClient // eslint-disable-next-line max-len
			.getSearchByQueryAndAccessRightsAndConceptValueTypesAndFormatsAndBusinessEventsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypesAndPageAndPageSize(
				quoteQueryIfEmail(query),
				filters.accessRights ?? undefined,
				filters.conceptTypes.map(t => t as ConceptType) ?? undefined,
				filters.formats ?? undefined,
				filters.businessEvents ?? undefined,
				filters.levels ?? undefined,
				filters.levelProposals ?? undefined,
				filters.lifeEvents ?? undefined,
				filters.publishers ?? undefined,
				filters.statuses ?? undefined,
				filters.statusProposals ?? undefined,
				(filters.structure as SearchStructureOption) ?? undefined,
				filters.themes ?? undefined,
				this.getCatalogTypes(filters),
				page,
				pageSize
			)
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(sr => {
				this.pagingInfo = new SearchResultPagingInfo(sr.headers);
				this.results = sr.result;
				this.update.emit();
				this.loadRelationsCounts(sr.result);
			});
	}

	/**
	 * Fetches the relations counts for the rendered page asynchronously and merges them onto the
	 * rows. Rows are flagged `relationsLoading` until the counts arrive so the table/list can show
	 * a spinner in the Relations cell.
	 */
	private loadRelationsCounts(entries: CatalogEntry[]): void {
		if (!entries?.length) {
			return;
		}

		const rows = entries as Array<CatalogEntry & WithRelations>;
		// Mapping tables intentionally show no relations count (column stays empty).
		const relevant = rows.filter(row => row.type !== SearchResourceType.MappingTable);
		relevant.forEach(row => (row.relationsLoading = true));

		const items: RelationsCountRequestItem[] = relevant
			.filter(row => row.id && row.type)
			.map(row => new RelationsCountRequestItem({id: row.id!, type: row.type! as SearchResourceType}));

		if (!items.length) {
			relevant.forEach(row => (row.relationsLoading = false));
			return;
		}

		// Keep the global Oblique master loader down for this background enrichment call so only the
		this.obHttpApiInterceptorEvents.deactivateSpinnerOnNextAPICalls(1);
		this.catalogClient
			.postRelationsCountByBody(items)
			.pipe(
				map(response => response.result ?? []),
				catchError(() => {
					relevant.forEach(row => (row.relationsLoading = false));
					return of([] as RelationsCountModel[]);
				}),
				takeUntil(merge(this.searchTrigger$, this.unsubscribe$))
			)
			.subscribe(models => {
				const byId = new Map(models.map(model => [model.id, model]));
				relevant.forEach(row => {
					const model = row.id ? byId.get(row.id) : undefined;
					row.relations = model;
					row.relationsCount = model?.total ?? 0;
					row.relationsLoading = false;
				});
			});
	}

	private getCatalogTypes(filters: SearchFilters): SearchResourceType[] | undefined {
		switch (this.searchType) {
			case SearchType.Dataset:
				return [SearchResourceType.Dataset];
			case SearchType.Dataservice:
				return [SearchResourceType.DataService];
			case SearchType.Publicservice:
				return [SearchResourceType.PublicService];
			case SearchType.Concept:
				return [SearchResourceType.Concept];
			case SearchType.MappingTable:
				return [SearchResourceType.MappingTable];
			case SearchType.All:
			default:
				return filters?.types?.map(t => t as SearchResourceType) ?? undefined;
		}
	}
}
