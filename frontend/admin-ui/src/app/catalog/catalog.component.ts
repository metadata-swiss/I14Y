import {AfterViewInit, Component, inject, OnDestroy, OnInit} from '@angular/core';
import {NAV_VALUE_CREATE} from '../app-constants';
import {
	AllowActionResourceType,
	AllowActionType,
	CatalogClient,
	CatalogEntry,
	SearchStructureOption,
	SearchResourceType,
	ConceptType,
	DatasetInputClient,
	FileParameter,
	FilterCountResult,
	FilterCountResultItem,
	PublicationLevel,
	RegistrationStatus,
	RelationsCountModel,
	RelationsCountRequestItem,
	SwaggerResponse
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {SearchResultPagingInfo} from '../shared/searchResultPagingInfo';
import {PageEvent} from '@angular/material/paginator';
import {catchError, filter, map, Observable, of, startWith, Subject, takeUntil} from 'rxjs';
import {ActivatedRoute, NavigationEnd, Params, Router} from '@angular/router';
import {AllowActionService} from '../services/allow.action.service';
import {SearchFilters, SearchType} from '../shared/search-filters/search-filters';
import {SearchFilterService} from '../shared/search-filters/search-filters.service';
import {MatDialog} from '@angular/material/dialog';
import {ImportDialogComponent} from '../shared/importdialog/importdialog.component';
import {ObHttpApiInterceptorEvents, ObINotification, ObIUploadEvent, ObNotificationService} from '@oblique/oblique';
import {HttpErrorResponse} from '@angular/common/http';

@Component({
	selector: 'app-catalog',
	templateUrl: './catalog.component.html',
	styleUrls: ['./catalog.component.scss'],
	standalone: false
})
export class CatalogComponent implements AfterViewInit, OnInit, OnDestroy {
	readonly nav_value_create: string = NAV_VALUE_CREATE;
	hideFilters: boolean = true;
	queryInput: string;
	query: string | undefined;
	catalogEntry: CatalogEntry[] = [];
	cannotCreateDataset$: Observable<boolean> = of(true);
	allowActionCreateDatasetMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionCreateDatasetMessage$: Observable<string> = of('');
	cannotCreateDataService$: Observable<boolean> = of(true);
	allowActionCreateDataServiceMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionCreateDataServiceMessage$: Observable<string> = of('');
	cannotCreatePublicService$: Observable<boolean> = of(true);
	allowActionCreatePublicServiceMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionCreatePublicServiceMessage$: Observable<string> = of('');
	cannotCreateConcept$: Observable<boolean> = of(true);
	allowActionCreateConceptMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionCreateConceptMessage$: Observable<string> = of('');
	cannotCreateMappingTable$: Observable<boolean> = of(true);
	allowActionCreateMappingTableMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionCreateMappingTableMessage$: Observable<string> = of('');

	defaultPageSize: number = 50;
	defaultPage: number = 1;
	pagingInfo: SearchResultPagingInfo = new SearchResultPagingInfo(undefined, this.defaultPageSize);
	allCount: number = 0;
	countResults: FilterCountResultItem[] | undefined;
	catalogType = SearchResourceType;
	searchTypeEnum = SearchType;

	updateFilterSubject: Subject<void> = new Subject<void>();

	private readonly unsubscribe$ = new Subject();

	private readonly allowActionService = inject(AllowActionService);
	private readonly catalogClient = inject(CatalogClient);
	private readonly datasetInputClient = inject(DatasetInputClient);
	private readonly dialog = inject(MatDialog);
	private readonly notification = inject(ObNotificationService);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly searchFilterService = inject(SearchFilterService);

	constructor() {
		this.queryInput = this.route.snapshot.queryParamMap.get('query') ?? '';
	}

	ngOnInit(): void {
		this.router.events
			.pipe(
				takeUntil(this.unsubscribe$),
				filter((e): e is NavigationEnd => e instanceof NavigationEnd)
			)
			.subscribe(_ => {
				this.queryInput = this.route.snapshot.queryParamMap.get('query') ?? '';
				this.searchCatalog();
			});
		this.searchCatalog();
		this.cannotCreateDataset$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.resourceType === AllowActionResourceType.Dataset && x.actionType === AllowActionType.Create)?.value),
			startWith(true)
		);
		this.allowActionCreateDatasetMessageDetailCode$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(
				result =>
					// eslint-disable-next-line max-len
					result.find(x => x.resourceType === AllowActionResourceType.Dataset && x.actionType === AllowActionType.Create)?.messageDetailsCode?.toString() ??
					undefined
			),
			startWith('')
		);
		this.defaultAllowActionCreateDatasetMessage$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => result.find(x => x.resourceType === AllowActionResourceType.Dataset && x.actionType === AllowActionType.Create)?.message ?? ''),
			startWith('')
		);
		this.cannotCreateDataService$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.resourceType === AllowActionResourceType.DataService && x.actionType === AllowActionType.Create)?.value),
			startWith(true)
		);
		this.allowActionCreateDataServiceMessageDetailCode$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(
				result =>
					// eslint-disable-next-line max-len
					result.find(x => x.resourceType === AllowActionResourceType.DataService && x.actionType === AllowActionType.Create)?.messageDetailsCode?.toString() ??
					undefined
			),
			startWith('')
		);
		this.defaultAllowActionCreateDataServiceMessage$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => result.find(x => x.resourceType === AllowActionResourceType.DataService && x.actionType === AllowActionType.Create)?.message ?? ''),
			startWith('')
		);
		this.cannotCreatePublicService$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.resourceType === AllowActionResourceType.PublicService && x.actionType === AllowActionType.Create)?.value),
			startWith(true)
		);
		this.allowActionCreatePublicServiceMessageDetailCode$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(
				result =>
					result
						.find(x => x.resourceType === AllowActionResourceType.PublicService && x.actionType === AllowActionType.Create)
						?.messageDetailsCode?.toString() ?? undefined
			),
			startWith('')
		);
		this.defaultAllowActionCreatePublicServiceMessage$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => result.find(x => x.resourceType === AllowActionResourceType.PublicService && x.actionType === AllowActionType.Create)?.message ?? ''),
			startWith('')
		);
		this.cannotCreateConcept$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.resourceType === AllowActionResourceType.Concept && x.actionType === AllowActionType.Create)?.value),
			startWith(true)
		);
		this.allowActionCreateConceptMessageDetailCode$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(
				result =>
					// eslint-disable-next-line max-len
					result.find(x => x.resourceType === AllowActionResourceType.Concept && x.actionType === AllowActionType.Create)?.messageDetailsCode?.toString() ??
					undefined
			),
			startWith('')
		);
		this.defaultAllowActionCreateConceptMessage$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => result.find(x => x.resourceType === AllowActionResourceType.Concept && x.actionType === AllowActionType.Create)?.message ?? ''),
			startWith('')
		);
		this.cannotCreateMappingTable$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.resourceType === AllowActionResourceType.MappingTable && x.actionType === AllowActionType.Create)?.value),
			startWith(true)
		);
		this.allowActionCreateMappingTableMessageDetailCode$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(
				result =>
					// eslint-disable-next-line max-len
					result
						.find(x => x.resourceType === AllowActionResourceType.MappingTable && x.actionType === AllowActionType.Create)
						?.messageDetailsCode?.toString() ?? undefined
			),
			startWith('')
		);
		this.defaultAllowActionCreateMappingTableMessage$ = this.allowActionService.globalAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => result.find(x => x.resourceType === AllowActionResourceType.MappingTable && x.actionType === AllowActionType.Create)?.message ?? ''),
			startWith('')
		);
	}

	ngAfterViewInit(): void {
		this.updateFilterSubject.next();
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

	search(): void {
		if (this.queryInput) {
			const queryParams = {query: this.queryInput ?? null};
			this.router.navigate([], {
				relativeTo: this.route,
				queryParams: {...queryParams, page: null},
				queryParamsHandling: 'merge'
			});
		}
	}

	onResetQuery(): void {
		this.queryInput = '';
		this.query = undefined;
		this.router.navigate(['./'], {
			relativeTo: this.route,
			queryParams: {query: null, page: null, pageSize: null},
			queryParamsHandling: 'merge'
		});
	}

	get searchType(): string {
		switch (this.route.snapshot.url[0].path) {
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

	getQueryParams(searchType: SearchType | undefined): Params {
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

	onCountResultChange(countResult: FilterCountResult) {
		this.countResults = countResult.types;
		this.allCount = countResult.totalDocCount!;
	}

	getCountResult(type: SearchResourceType): number {
		return this.countResults?.find(r => r.reference === type)?.count ?? 0;
	}

	handleImport(): void {
		this.importFile().then(result => {
			if (result) {
				this.router.navigate([`./catalog/datasets/${result}`]);
			}
		});
	}

	private importFile(): Promise<string | undefined> {
		return new Promise<string | undefined>(resolve => {
			const dialogRef = this.dialog.open(ImportDialogComponent, {
				data: {accept: ['application/json', '.json'], headerTextKey: 'i18n.dialog.import.header_text'}
			});
			const dialogCancel = dialogRef.componentInstance.cancel.subscribe(() => {
				resolve(undefined);
			});
			const dialogUpload = dialogRef.componentInstance.upload.subscribe((event: ObIUploadEvent) => {
				const skippedErrorNotifications = 1;
				this.obHttpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(skippedErrorNotifications);
				const fileParameter: FileParameter = {fileName: (event.files[0] as File).name, data: event.files[0] as File};
				this.datasetInputClient
					.postImportByBody(fileParameter)
					.pipe(
						catchError((error: HttpErrorResponse) => {
							this.notification.error(this.getErrorMessage(error));
							return of();
						})
					)
					.subscribe(response => {
						resolve(response.result);
						dialogRef.close();
					});
			});
			dialogRef.afterClosed().subscribe(() => {
				dialogCancel.unsubscribe();
				dialogUpload.unsubscribe();
			});
		});
	}

	private searchCatalog() {
		const queryParams = this.route.snapshot.queryParamMap;
		const filters = this.searchFilterService.getSelectedFilters(this.searchType);
		const query = this.route.snapshot.queryParamMap.get('query');
		const page = queryParams.has('page') ? Number(queryParams.get('page')) : this.defaultPage;
		const pageSize = queryParams.has('pageSize') ? Number(queryParams.get('pageSize')) : this.defaultPageSize;

		this.catalogClient // eslint-disable-next-line max-len
			.getSearchByQueryAndAccessRightsAndConceptValueTypesAndFormatsAndBusinessEventsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypesAndPageAndPageSize(
				query ?? undefined,
				filters.accessRights ?? undefined,
				filters.conceptTypes.map(t => t as ConceptType) ?? undefined,
				filters.formats ?? undefined,
				filters.businessEvents ?? undefined,
				filters.levels.map(l => l as PublicationLevel) ?? undefined,
				filters.levelProposals.map(l => l as PublicationLevel) ?? undefined,
				filters.lifeEvents ?? undefined,
				filters.publishers ?? undefined,
				filters.statuses.map(s => s as RegistrationStatus) ?? undefined,
				filters.statusProposals.map(s => s as RegistrationStatus) ?? undefined,
				(filters.structure as SearchStructureOption) ?? undefined,
				filters.themes ?? undefined,
				this.getCatalogTypes(filters) ?? undefined,
				page ?? this.defaultPage,
				pageSize ?? this.defaultPageSize
			)
			.subscribe((response: SwaggerResponse<CatalogEntry[]>) => {
				this.pagingInfo = new SearchResultPagingInfo(response.headers);
				this.catalogEntry = response.result;
				this.loadRelatedByCounts(response.result);
			});
	}

	/**
	 * Fetches the related-by counts for the rendered page asynchronously and merges them onto
	 * the rows. Rows are flagged `relationsLoading` until the counts arrive so the table can show
	 * a spinner in the Relations cell.
	 */
	private loadRelatedByCounts(entries: CatalogEntry[]) {
		if (!entries?.length) {
			return;
		}

		const rows = entries as Array<CatalogEntry & {relationsLoading?: boolean; relationsCount?: number; relations?: RelationsCountModel}>;
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
		// per-row spinner shows. `deactivateSpinnerOnNextAPICalls(1)` disables it for the next request
		this.obHttpApiInterceptorEvents.deactivateSpinnerOnNextAPICalls(1);
		this.catalogClient
			.postRelationsCountByBody(items)
			.pipe(
				map(response => response.result ?? []),
				takeUntil(this.unsubscribe$),
				catchError(() => {
					relevant.forEach(row => (row.relationsLoading = false));
					return of([] as RelationsCountModel[]);
				})
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
				return filters.types.map(t => t as SearchResourceType) ?? undefined;
		}
	}

	private getErrorMessage(error: HttpErrorResponse): ObINotification {
		let message: string = '';
		let title: string = '';
		switch (error.status) {
			case 400:
			case 403:
			case 500:
			case 501:
			case 502:
			case 503:
			case 504:
				title = `i18n.http_error.${error.status}.title`;
				message = `i18n.http_error.${error.status}.import`;
				break;
			default:
				title = 'i18n.oblique.notification.type.error';
				message = error.message;
				break;
		}

		return {message: message, title: title};
	}
}
