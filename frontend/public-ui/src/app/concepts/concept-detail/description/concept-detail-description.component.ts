import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {
	ConceptType,
	ConceptVersionView,
	ConceptView,
	ConceptViewClient,
	LindasClient,
	LindasResourceType,
	IopConceptStructureReferenceModel,
	MappingTableModel,
	MappingTablesClient
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {SearchResultPagingInfo} from 'src/app/shared/search/SearchResultPagingInfo';
import {BackgroundRequestService} from 'src/app/shared/interceptors/background-request';
import {catchError, forkJoin, map, of, Subject, switchMap, takeUntil} from 'rxjs';
import {ConceptService} from '../../services/concept.service';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ViewType} from 'src/app/shared/templates/viewtype';
import {DateFormatService} from 'src/app/shared/services/date-format/date-format.service';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {SearchEngineOptimizationService} from 'src/app/shared/services/search-engine-optimization/search-engine-optimization.service';
import {buildConceptIri} from 'src/app/shared/helper/iri-helpers';

@Component({
	selector: 'app-concept-detail-description',
	templateUrl: './concept-detail-description.component.html',
	styleUrls: ['./concept-detail-description.component.scss'],
	standalone: false
})
export class ConceptDetailDescriptionComponent implements OnInit, OnDestroy {
	conceptView: ConceptView;
	versions: ConceptVersionView[] = [];
	mappingTables: MappingTableModel[] = [];
	conceptReferencesCount: number = 0;
	structureReferences: IopConceptStructureReferenceModel[] = [];
	structureReferencesPagingInfo: SearchResultPagingInfo = new SearchResultPagingInfo(null);
	structureReferencesLoading = false;
	lindasRdfUrl: string | undefined;
	lindasLdUri: string | undefined;
	currentLang: string;
	columnsToDisplay = ['name', 'type', 'version', 'validFrom', 'validTo', 'status'];
	mappingTableColumns = ['name', 'version', 'validFrom', 'validTo', 'publisher', 'status'];
	readonly viewTypeEnum = ViewType;

	private readonly unsubscribe$ = new Subject();

	private readonly conceptViewClient = inject(ConceptViewClient);
	private readonly lindasClient = inject(LindasClient);
	private readonly conceptViewService = inject(ConceptService);
	private readonly dateFormatService = inject(DateFormatService);
	private readonly mappingTablesClient = inject(MappingTablesClient);
	private readonly translate = inject(TranslateService);
	private readonly fallbackPipe = inject(FallbackPipe);
	private readonly searchEngineOptimizationService = inject(SearchEngineOptimizationService);
	private readonly backgroundRequests = inject(BackgroundRequestService);

	constructor() {
		this.currentLang = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLang = language.lang;
		});

		this.conceptViewService.conceptView$
			.pipe(
				switchMap(concept => {
					this.lindasRdfUrl = undefined;
					this.lindasLdUri = undefined;
					return this.getLindasLinks(concept);
				}),
				takeUntil(this.unsubscribe$)
			)
			.subscribe(links => {
				this.lindasRdfUrl = links.rdfUrl;
				this.lindasLdUri = links.ldUri;
			});

		this.conceptViewService.conceptView$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.conceptView = x;
			this.mappingTables = [];
			this.versions = [];
			this.conceptViewClient.getAllVersionsById(x.id).subscribe(response => {
				this.versions = [...response.result].sort((a, b) => (b.version as string).localeCompare(a.version as string));
			});
			this.updateMetaData(x);

			if (x.conceptType === ConceptType.CodeList) {
				this.loadMappingTables(x.identifiers?.[0], x.version);
			}
			// Fetching the first page directly also yields the total row count in the paging headers,
			// which is all the count endpoint gave us. The page is handed to the relation table so it
			// does not request the very same rows again.
			this.structureReferencesLoading = true;
			this.backgroundRequests
				.withoutGlobalSpinner(this.conceptViewClient.getStructureReferencesByIdAndPageAndPageSize(x.id, 1, 10))
				.pipe(takeUntil(this.unsubscribe$))
				.subscribe({
					next: response => {
						this.structureReferencesPagingInfo = new SearchResultPagingInfo(response.headers);
						this.conceptReferencesCount = this.structureReferencesPagingInfo.totalRows;
						this.structureReferences = response.result;
						this.structureReferencesLoading = false;
					},
					error: () => {
						this.structureReferencesLoading = false;
					}
				});
		});
	}

	getType(conceptType: ConceptType): string {
		const conceptTypeEnum = ConceptType;
		let type: string = null;
		if (conceptType) {
			type = conceptTypeEnum[conceptType].toLocaleLowerCase();
		}
		return type;
	}

	getFormattedDate(date: Date): string | undefined {
		return date ? this.dateFormatService.formatShortDate(date) : undefined;
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	private getLindasLinks(concept: ConceptView) {
		const [identifier] = concept.identifiers ?? [];
		const version = concept.version;

		if (!identifier || !version) {
			return of({rdfUrl: undefined, ldUri: undefined});
		}

		// Only feeds the export links, so the page must not wait on it. forkJoin subscribes to both
		// calls synchronously, so both are covered.
		return this.backgroundRequests.withoutGlobalSpinner(
			forkJoin({
				rdfUrl: this.lindasClient.getRdfLinkByTypeAndIdentifierAndVersion(LindasResourceType.Concept, identifier, version).pipe(
					map(response => response.result ?? undefined),
					catchError(() => of(undefined))
				),
				ldUri: this.lindasClient.getLdUriByTypeAndIdentifierAndVersion(LindasResourceType.Concept, identifier, version).pipe(
					map(response => response.result ?? undefined),
					catchError(() => of(undefined))
				)
			})
		);
	}

	private loadMappingTables(identifier: string | undefined, version: string | undefined): void {
		if (!identifier || !version) {
			return;
		}
		const codeSystemUri = buildConceptIri(identifier, version);

		this.mappingTablesClient
			.getByMappingTableIdentifierAndPublisherIdentifierAndVersionAndCodeSystemUriAndPublicationLevelAndRegistrationStatusAndPageAndPageSize(
				undefined,
				undefined,
				undefined,
				codeSystemUri,
				undefined,
				undefined,
				undefined,
				undefined
			)
			.subscribe(response => {
				this.mappingTables = response.result;
			});
	}

	private updateMetaData(concept: ConceptView) {
		let titleText = this.fallbackPipe.transform(concept.name, this.translate.getCurrentLang()) ?? '';
		this.searchEngineOptimizationService.UpdateMetaTitle(titleText);
		let descriptionText = this.fallbackPipe.transform(concept.description, this.translate.getCurrentLang()) ?? '';
		this.searchEngineOptimizationService.UpdateMetaDescrition(descriptionText);
	}
}
