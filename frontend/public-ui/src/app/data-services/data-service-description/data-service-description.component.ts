import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {ViewType} from 'src/app/shared/templates/viewtype';
import {DataServiceService} from '../services/dataservice.service';
import {PublisherContextService} from 'src/app/shared/services/publisher-context/publisher-context.service';
import {catchError, forkJoin, map, of, Subject, takeUntil} from 'rxjs';
import {DataService, DcatCatalogInputClient, DcatCatalogRecordInput, DcatCatalogsClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';

@Component({
	selector: 'app-data-service-description',
	templateUrl: './data-service-description.component.html',
	standalone: false
})
export class DataServiceDescriptionComponent implements OnInit, OnDestroy {
	dataService: DataService;
	catalogues: DcatCatalogRecordInput[] = [];
	publisherIdentifier: string | undefined;
	currentLanguage: string;
	readonly emptyPlaceHolder: string = '-';
	readonly viewTypeEnum = ViewType;

	private readonly unsubscribe$ = new Subject();

	private readonly dcatCatalogInputClient = inject(DcatCatalogInputClient);
	private readonly dcatCatalogsClient = inject(DcatCatalogsClient);
	private readonly dcatDataServiceService = inject(DataServiceService);
	private readonly publisherContextService = inject(PublisherContextService);
	private readonly translate = inject(TranslateService);

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
		this.dcatDataServiceService.dataService$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.dataService = x;
			this.getDcatCatalogRecordInput();
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(null);
		this.unsubscribe$.complete();
	}

	getDcatCatalogRecordInput(): void {
		if (!this.dataService.id) {
			return;
		}

		this.dcatCatalogInputClient
			.getRecordsByResourceByResourceId(this.dataService.id)
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
