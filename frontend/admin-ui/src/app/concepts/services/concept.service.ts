import {PagedCodelistEntryResult} from './../description/paged-codelistentry-result';
import {PagedMappingTableResult} from './../description/paged-mapping-table-result';
import {BackgroundRequestService} from 'src/app/shared/interceptors/background-request';
import {
	ConceptType,
	ConceptVersionView,
	ConceptView,
	ConceptViewClient,
	IopConceptStructureReferenceModel,
	MappingTablesClient,
	PublicationLevelInfoModel,
	RegistrationStatusInfoModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {inject, Injectable} from '@angular/core';
import {Observable} from 'rxjs/internal/Observable';
import {ReplaySubject, Subject} from 'rxjs';
import {SearchResultPagingInfo} from 'src/app/shared/searchResultPagingInfo';
import {buildConceptIri} from 'src/app/shared/helper/iri-helpers';

export type PagedStructureReferenceResult = {
	structureReferences: IopConceptStructureReferenceModel[];
	pagingInfo: SearchResultPagingInfo;
};

@Injectable()
export class ConceptService {
	readonly data$: Observable<ConceptView>;
	readonly mappingTables$: Observable<PagedMappingTableResult | undefined>;
	readonly pagedCodelistEntryResult$: Observable<PagedCodelistEntryResult | undefined>;
	readonly publicationLevelInfo$: Observable<PublicationLevelInfoModel>;
	readonly registrationStatusInfo$: Observable<RegistrationStatusInfoModel>;
	readonly versions$: Observable<ConceptVersionView[]>;
	readonly structureReferences$: Observable<PagedStructureReferenceResult | undefined>;

	private last: ConceptView | undefined;
	private readonly defaultPage = 1;
	private readonly defaultPageSize = 10;

	private readonly data: Subject<ConceptView> = new ReplaySubject<ConceptView>();
	private readonly mappingTables: Subject<PagedMappingTableResult | undefined> = new ReplaySubject<PagedMappingTableResult | undefined>();
	private readonly pagedCodelistEntryResult: Subject<PagedCodelistEntryResult | undefined> = new ReplaySubject<PagedCodelistEntryResult | undefined>();
	private readonly publicationLevelInfo: Subject<PublicationLevelInfoModel> = new ReplaySubject<PublicationLevelInfoModel>();
	private readonly registrationStatusInfo: Subject<RegistrationStatusInfoModel> = new ReplaySubject<RegistrationStatusInfoModel>();
	private readonly versions: Subject<ConceptVersionView[]> = new ReplaySubject<ConceptVersionView[]>();
	private readonly structureReferences: Subject<PagedStructureReferenceResult | undefined> = new ReplaySubject<
		PagedStructureReferenceResult | undefined
	>();

	private readonly conceptViewClient = inject(ConceptViewClient);
	private readonly mappingTablesClient = inject(MappingTablesClient);
	private readonly backgroundRequests = inject(BackgroundRequestService);

	constructor() {
		this.data$ = this.data.asObservable();
		this.mappingTables$ = this.mappingTables.asObservable();
		this.pagedCodelistEntryResult$ = this.pagedCodelistEntryResult.asObservable();
		this.publicationLevelInfo$ = this.publicationLevelInfo.asObservable();
		this.registrationStatusInfo$ = this.registrationStatusInfo.asObservable();
		this.versions$ = this.versions.asObservable();
		this.structureReferences$ = this.structureReferences.asObservable();
	}

	load(id: string, force: boolean = false): void {
		if (force || !this.last || this.last.id !== id) {
			this.versions.next([]);
			this.pagedCodelistEntryResult.next(undefined);
			this.mappingTables.next(undefined);
			this.structureReferences.next(undefined);

			this.conceptViewClient.getById(id).subscribe(response => {
				this.last = response.result;
				this.data.next(response.result);

				if (response?.result.conceptType === ConceptType.CodeList) {
					this.conceptViewClient.getCodelistEntriesByIdAndPageAndPageSize(id, this.defaultPage, this.defaultPageSize).subscribe(res => {
						this.pagedCodelistEntryResult.next(new PagedCodelistEntryResult(res.result, new SearchResultPagingInfo(res.headers)));
					});

					const identifier = response.result.identifiers?.[0];
					const codeSystemUri = this.buildConceptUri(identifier, response.result.version);
					if (codeSystemUri) {
						this.fetchMappingTables(codeSystemUri, this.defaultPage, this.defaultPageSize);
					}
				}
			});

			this.conceptViewClient.getAllVersionsById(id).subscribe(response => {
				this.versions.next(response.result);
			});

			this.conceptViewClient.getPublicationLevelById(id).subscribe(response => this.publicationLevelInfo.next(response.result));
			this.conceptViewClient.getRegistrationStatusById(id).subscribe(response => this.registrationStatusInfo.next(response.result));
			// Background enrichment for the relations table: it renders its own spinner, so it must not
			// raise the global Oblique master loader over the whole concept page.
			this.backgroundRequests
				.withoutGlobalSpinner(this.conceptViewClient.getStructureReferencesByIdAndPageAndPageSize(id, this.defaultPage, this.defaultPageSize))
				.subscribe({
					next: response => {
						this.structureReferences.next({structureReferences: response.result, pagingInfo: new SearchResultPagingInfo(response.headers)});
					},
					error: () => {
						this.structureReferences.next({structureReferences: [], pagingInfo: new SearchResultPagingInfo(undefined)});
					}
				});
		}
	}

	updateCodeListEntries(id: string, page: number, pageSize: number): void {
		if (id === this.last?.id) {
			this.conceptViewClient.getCodelistEntriesByIdAndPageAndPageSize(id, page, pageSize).subscribe(response => {
				this.pagedCodelistEntryResult.next(new PagedCodelistEntryResult(response.result, new SearchResultPagingInfo(response.headers)));
			});
		}
	}

	updateMappingTables(page: number, pageSize: number): void {
		if (this.last?.identifiers?.[0] && this.last?.version) {
			const codeSystemUri = this.buildConceptUri(this.last.identifiers[0], this.last.version);
			if (codeSystemUri) {
				this.fetchMappingTables(codeSystemUri, page, pageSize);
			}
		}
	}

	resetCodeListEntries(): void {
		this.pagedCodelistEntryResult.next(undefined);
	}

	resetMappingTables(): void {
		this.mappingTables.next(undefined);
	}

	private buildConceptUri(identifier: string | undefined, version: string | undefined): string | undefined {
		if (!identifier || !version) {
			return undefined;
		}
		return buildConceptIri(identifier, version);
	}

	private fetchMappingTables(codeSystemUri: string, page: number, pageSize: number): void {
		this.mappingTablesClient
			.getByMappingTableIdentifierAndPublisherIdentifierAndVersionAndCodeSystemUriAndPublicationLevelAndRegistrationStatusAndPageAndPageSize(
				undefined,
				undefined,
				undefined,
				codeSystemUri,
				undefined,
				undefined,
				page,
				pageSize
			)
			.subscribe(response => {
				this.mappingTables.next(new PagedMappingTableResult(response.result, new SearchResultPagingInfo(response.headers)));
			});
	}
}
