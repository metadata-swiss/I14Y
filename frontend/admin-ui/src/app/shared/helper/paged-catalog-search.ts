import {inject} from '@angular/core';
import {MatAutocomplete} from '@angular/material/autocomplete';
import {CatalogClient, CatalogEntry, SearchResourceType} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {BehaviorSubject, merge, Observable, of, Subject} from 'rxjs';
import {catchError, filter, map, switchMap, takeUntil, tap} from 'rxjs/operators';
import {BackgroundRequestService} from '../interceptors/background-request';

export interface PagedCatalogSearchState {
	/** The loaded entries without the excluded (already linked) ones. */
	items: CatalogEntry[];
	hasMore: boolean;
}

/**
 * Loads catalog entries page by page for an autocomplete: the first page is queried when the panel opens,
 * the next page when the panel is scrolled to the bottom. Must be created in an injection context.
 * Bind the template to `state$ | async`, so the results are also rendered inside OnPush components.
 */
export class PagedCatalogSearch {
	private items: CatalogEntry[] = [];
	private hasMore = true;
	private loading = false;
	private query: string | null = null;
	private page = 1;
	private panelElement: HTMLElement | null = null;

	private readonly stateSubject = new BehaviorSubject<PagedCatalogSearchState>({items: [], hasMore: true});
	private readonly search$ = new Subject<string | null>();
	private readonly loadMore$ = new Subject<void>();
	private readonly destroy$ = new Subject<void>();
	private readonly catalogClient = inject(CatalogClient);
	private readonly backgroundRequests = inject(BackgroundRequestService);

	readonly state$: Observable<PagedCatalogSearchState>;

	constructor(
		private readonly types: SearchResourceType[],
		private readonly excludedIds: () => (string | undefined)[] = () => [],
		private readonly pageSize = 20
	) {
		this.state$ = this.stateSubject.asObservable();

		merge(
			this.search$.pipe(
				tap(query => {
					this.query = query;
					this.page = 1;
					this.items = [];
					this.hasMore = true;
					this.emitState();
				})
			),
			this.loadMore$.pipe(filter(() => !this.loading && this.hasMore))
		)
			.pipe(
				tap(() => (this.loading = true)),
				// a new search cancels a pending next page request
				switchMap(() => this.fetch(this.query, this.page)),
				takeUntil(this.destroy$)
			)
			.subscribe(items => {
				this.items = [...this.items, ...items];
				this.page++;
				this.hasMore = items.length === this.pageSize;
				this.loading = false;
				this.emitState();

				// the panel can not be scrolled when most of the loaded entries are already linked
				if (this.hasMore && this.visibleItems.length < this.pageSize / 2) {
					this.loadMore$.next();
				}
			});
	}

	/** Queries the first page again, e.g. when the autocomplete panel opens. */
	search(query: string | null | undefined): void {
		this.search$.next(this.normalize(query));
	}

	/** Queries the first page only if the query has changed, e.g. while typing. */
	updateQuery(query: string | null | undefined): void {
		const normalized = this.normalize(query);
		if (normalized !== this.query) {
			this.search$.next(normalized);
		}
	}

	/** Listens to the scroll events of the autocomplete panel; call it from the (opened) event. */
	attachScroll(autocomplete: MatAutocomplete): void {
		// the panel is rendered after the opened event
		setTimeout(() => {
			const panel: HTMLElement | undefined = autocomplete.panel?.nativeElement;
			if (!panel || panel === this.panelElement) {
				return;
			}
			this.detachScroll();
			this.panelElement = panel;
			this.panelElement.addEventListener('scroll', this.onScroll);
		});
	}

	destroy(): void {
		this.detachScroll();
		this.destroy$.next();
		this.destroy$.complete();
		this.stateSubject.complete();
	}

	private readonly onScroll = (event: Event): void => {
		const panel = event.target as HTMLElement;
		const atBottom = panel.scrollHeight - panel.scrollTop <= panel.clientHeight + 10;

		if (atBottom) {
			this.loadMore$.next();
		}
	};

	private get visibleItems(): CatalogEntry[] {
		const excludedIds = this.excludedIds();
		return this.items.filter(x => !excludedIds.includes(x.id));
	}

	private emitState(): void {
		this.stateSubject.next({items: this.visibleItems, hasMore: this.hasMore});
	}

	private detachScroll(): void {
		this.panelElement?.removeEventListener('scroll', this.onScroll);
		this.panelElement = null;
	}

	private normalize(query: string | null | undefined): string | null {
		return query?.trim() || null;
	}

	private fetch(query: string | null, page: number): Observable<CatalogEntry[]> {
		// the panel shows its own loading option, so the global spinner must not block the whole form
		return this.backgroundRequests
			.withoutGlobalSpinner(
				this.catalogClient // eslint-disable-next-line max-len
					.getSearchByQueryAndAccessRightsAndAttributedAgentsAndConceptValueTypesAndFormatsAndBusinessEventsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypesAndPageAndPageSize(
						query ?? undefined,
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
						undefined,
						this.types,
						page,
						this.pageSize
					)
			)
			.pipe(
				map(response => response.result ?? []),
				catchError(() => of([]))
			);
	}
}
