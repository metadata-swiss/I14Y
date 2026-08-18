import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {forkJoin, of, Subject} from 'rxjs';
import {catchError, map, takeUntil} from 'rxjs/operators';
import {DcatDatasetService} from '../services/dcat-dataset.service';
import {PublisherContextService} from 'src/app/shared/services/publisher-context/publisher-context.service';
import {ViewType} from 'src/app/shared/templates/viewtype';
import {
	DataServiceModel,
	Dataset,
	DatasetClient,
	DcatCatalogsClient,
	DcatCatalogInputClient,
	DcatCatalogRecordInput,
	DcatVocabularyEntry
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
	catalogsAndThemes: DcatCatalogRecordInput[] = [];
	publisherIdentifier: string | undefined;
	currentLanguage: string;
	readonly emptyPlaceHolder: string = '-';
	readonly viewTypeEnum = ViewType;

	private readonly unsubscribe$ = new Subject();

	private readonly datasetClient = inject(DatasetClient);
	private readonly dcatCatalogInputClient = inject(DcatCatalogInputClient);
	private readonly dcatCatalogsClient = inject(DcatCatalogsClient);
	private readonly dcatDatasetService = inject(DcatDatasetService);
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
					this.catalogsAndThemes = response.result;
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

						this.catalogsAndThemes = response.result.map(
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

	getDisplayableCatalogThemes(themes: DcatVocabularyEntry[] | undefined): DcatVocabularyEntry[] {
		return (themes ?? []).filter(theme => Object.values(theme.name ?? {}).some(name => typeof name === 'string' && name.length > 0));
	}
}
