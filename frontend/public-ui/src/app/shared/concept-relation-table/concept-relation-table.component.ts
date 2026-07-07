import {Component, inject, Input, OnDestroy, OnInit} from '@angular/core';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {takeUntil} from 'rxjs/operators';
import {Subject} from 'rxjs';
import {ConceptViewClient, DatasetsClient, DcatDatasetModel, MultiLanguageModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {SearchResultPagingInfo} from '../search/SearchResultPagingInfo';
import {PageEvent} from '@angular/material/paginator';
import {MatTableDataSource} from '@angular/material/table';

interface ConceptStructureReferenceViewModel {
	datasetUri?: string;
	attributeUri?: string;
	datasetName?: MultiLanguageModel;
	attributeName?: string;
	publisherName?: MultiLanguageModel;
}

@Component({
	selector: 'app-concept-relation-table',
	templateUrl: './concept-relation-table.component.html',
	standalone: false
})
export class ConceptRelationTableComponent implements OnInit, OnDestroy {
	private _conceptId: string | undefined;

	@Input()
	set conceptId(value: string | undefined) {
		this._conceptId = value;

		if (value) {
			this.loadStructureReferences(value, 1, 10);
		}
	}

	get conceptId(): string | undefined {
		return this._conceptId;
	}

	dataSource = new MatTableDataSource<ConceptStructureReferenceViewModel>([]);

	currentLanguage: string;

	COLUMN_DATASET = 'dataset';
	COLUMN_ATTRIBUTE = 'attribute';
	COLUMN_PUBLISHER = 'publisher';

	displayedColumns: string[] = [this.COLUMN_DATASET, this.COLUMN_ATTRIBUTE, this.COLUMN_PUBLISHER];
	pagingInfo: SearchResultPagingInfo = new SearchResultPagingInfo(null);

	private readonly unsubscribe$ = new Subject<void>();

	private readonly conceptViewClient = inject(ConceptViewClient);
	private readonly datasetClient = inject(DatasetsClient);
	private readonly translate = inject(TranslateService);

	private datasetUriDcatDatasetModelMap = new Map<string, DcatDatasetModel>();

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	private loadStructureReferences(conceptId: string, page: number, pageSize: number) {
		this.conceptViewClient
			.getStructureReferencesByIdAndPageAndPageSize(conceptId, page, pageSize)
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(response => {
				this.pagingInfo = new SearchResultPagingInfo(response.headers);

				this.dataSource.data = response.result.map(item => ({
					datasetUri: item.datasetUri,
					attributeUri: item.attributeUri,
					datasetName: undefined,
					attributeName: this.getAttributeName(item.attributeUri),
					publisherName: undefined
				}));

				this.dataSource.data.forEach(row => {
					if (row.datasetUri) {
						this.fetchDatasetForRow(row);
					}
				});
			});
	}

	onChangePage(pageEvent: PageEvent) {
		if (!this.conceptId) {
			return;
		}

		this.loadStructureReferences(this.conceptId, pageEvent.pageIndex + 1, pageEvent.pageSize);
	}

	private fetchDatasetForRow(row: ConceptStructureReferenceViewModel) {
		if (!row.datasetUri) {
			return;
		}

		const cachedDataset = this.datasetUriDcatDatasetModelMap.get(row.datasetUri);

		if (cachedDataset) {
			row.datasetName = cachedDataset.title;
			row.publisherName = cachedDataset.publisher?.name;
			this.dataSource.data = [...this.dataSource.data];
			return;
		}

		const datasetIdentifier = row.datasetUri.split('/').at(-1);

		if (!datasetIdentifier) {
			return;
		}

		this.datasetClient
			.getByAccessRightsAndDatasetIdentifierAndPublisherIdentifierAndPublicationLevelAndRegistrationStatusAndPageAndPageSize(
				undefined,
				datasetIdentifier,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined
			)
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(response => {
				const dataset = response.result?.[0];

				if (!dataset || !row.datasetUri) {
					return;
				}

				this.datasetUriDcatDatasetModelMap.set(row.datasetUri, dataset);

				row.datasetName = dataset.title;
				row.publisherName = dataset.publisher?.name;

				this.dataSource.data = [...this.dataSource.data];
			});
	}

	private getAttributeName(attributeUri?: string): string {
		if (!attributeUri) {
			return '';
		}

		return attributeUri.split('/').at(-1) ?? '';
	}

	ngOnDestroy() {
		this.unsubscribe$.next();
		this.unsubscribe$.complete();
	}
}
