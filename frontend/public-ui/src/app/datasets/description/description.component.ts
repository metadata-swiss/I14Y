import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {forkJoin, of, Subject} from 'rxjs';
import {catchError, map, switchMap, takeUntil} from 'rxjs/operators';
import {DcatDatasetService} from '../services/dcat-dataset.service';
import {PublisherContextService} from 'src/app/shared/services/publisher-context/publisher-context.service';
import {BackgroundRequestService} from 'src/app/shared/interceptors/background-request';
import {ViewType} from 'src/app/shared/templates/viewtype';
import {
	DataServiceModel,
	Dataset,
	DatasetClient,
	LindasClient,
	LindasResourceType,
	DcatCatalogsClient,
	DcatCatalogInputClient,
	DcatCatalogRecordInput
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';

@Component({
	selector: 'app-description',
	templateUrl: './description.component.html',
	styleUrls: ['./description.component.scss'],
	standalone: false
})
export class DescriptionComponent implements OnInit, OnDestroy {
	dataset: Dataset;
	isServedBy: DataServiceModel[] = [];
	catalogues: DcatCatalogRecordInput[] = [];
	publisherIdentifier: string | undefined;
	currentLanguage: string;
	lindasRdfUrl: string | undefined;
	lindasLinksLoading = false;
	readonly emptyPlaceHolder: string = '-';
	readonly viewTypeEnum = ViewType;

	private readonly unsubscribe$ = new Subject();

	private readonly datasetClient = inject(DatasetClient);
	private readonly lindasClient = inject(LindasClient);
	private readonly dcatCatalogInputClient = inject(DcatCatalogInputClient);
	private readonly dcatCatalogsClient = inject(DcatCatalogsClient);
	private readonly dcatDatasetService = inject(DcatDatasetService);
	private readonly publisherContextService = inject(PublisherContextService);
	private readonly translate = inject(TranslateService);
	private readonly backgroundRequests = inject(BackgroundRequestService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
		this.publisherContextService.identifier$.pipe(takeUntil(this.unsubscribe$)).subscribe(id => {
			this.publisherIdentifier = id;
		});
		this.dcatDatasetService.dataset$
			.pipe(
				switchMap(dataset => {
					this.lindasRdfUrl = undefined;
					this.lindasLinksLoading = true;
					const [identifier] = dataset.identifiers ?? [];

					if (!identifier) {
						return of(undefined);
					}

					return this.backgroundRequests
						.withoutGlobalSpinner(this.lindasClient.getRdfLinkByTypeAndIdentifierAndVersion(LindasResourceType.Dataset, identifier, undefined))
						.pipe(
							map(response => response.result ?? undefined),
							catchError(() => of(undefined))
						);
				}),
				takeUntil(this.unsubscribe$)
			)
			.subscribe(url => {
				this.lindasRdfUrl = url;
				this.lindasLinksLoading = false;
			});

		this.dcatDatasetService.dataset$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.dataset = x;
			this.datasetClient.getIsServedByById(x.id).subscribe(response => {
				this.isServedBy = response.result;
			});
			this.getDcatCatalogRecordInput();
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(null);
		this.unsubscribe$.complete();
	}

	getDcatCatalogRecordInput(): void {
		if (!this.dataset.id) {
			return;
		}

		this.dcatCatalogInputClient
			.getRecordsByResourceByResourceId(this.dataset.id)
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(response => {
				const catalogIds = [...new Set(response.result.map(record => record.catalogId).filter((id): id is string => !!id))];
				if (catalogIds.length === 0) {
					this.catalogues = response.result;
					return;
				}

				const catalogTitleRequests = catalogIds.map(catalogId =>
					this.dcatCatalogsClient.getById(catalogId).pipe(
						map(catalogResponse => [catalogId, catalogResponse.result.title] as const),
						catchError(() => of([catalogId, undefined] as const))
					)
				);

				// Resolve catalog titles in parallel and update the view once all requests have completed.
				forkJoin(catalogTitleRequests)
					.pipe(takeUntil(this.unsubscribe$))
					.subscribe(catalogTitles => {
						const titlesByCatalogId = new Map(catalogTitles);

						this.catalogues = response.result.map(
							catalogRecord =>
								new DcatCatalogRecordInput({
									...catalogRecord,
									catalogTitle: catalogRecord.catalogId
										? (titlesByCatalogId.get(catalogRecord.catalogId) ?? catalogRecord.catalogTitle)
										: catalogRecord.catalogTitle
								})
						);
					});
			});
	}
}
