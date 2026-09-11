import {Component, inject, Input, OnDestroy, OnInit} from '@angular/core';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {takeUntil} from 'rxjs/operators';
import {Subject} from 'rxjs';
import {ConceptViewClient, IopConceptStructureReferenceModel, MultiLanguageModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {SearchResultPagingInfo} from '../searchResultPagingInfo';
import {BackgroundRequestService} from '../interceptors/background-request';
import {PageEvent} from '@angular/material/paginator';
import {MatTableDataSource} from '@angular/material/table';

interface ConceptStructureReferenceViewModel {
	datasetUri?: string;
	datasetUrl?: string;
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
	@Input() conceptId: string | undefined;

	/** First page, already fetched by ConceptService: reusing it avoids requesting the same rows twice. */
	@Input()
	set references(value: IopConceptStructureReferenceModel[] | undefined) {
		this.setRows(value ?? []);
	}

	@Input()
	set referencesPagingInfo(value: SearchResultPagingInfo | undefined) {
		if (value) {
			this.pagingInfo = value;
		}
	}

	dataSource = new MatTableDataSource<ConceptStructureReferenceViewModel>([]);

	/** Only covers paginating; the initial load is shown by the section spinner. */
	isLoadingPage = false;
	/** Bound while paginating so the header stays put and only the rows are replaced by the spinner. */
	readonly noRows: ConceptStructureReferenceViewModel[] = [];

	currentLanguage: string;

	COLUMN_DATASET = 'dataset';
	COLUMN_ATTRIBUTE = 'attribute';
	COLUMN_PUBLISHER = 'publisher';

	displayedColumns: string[] = [this.COLUMN_DATASET, this.COLUMN_ATTRIBUTE, this.COLUMN_PUBLISHER];
	pagingInfo: SearchResultPagingInfo = new SearchResultPagingInfo(undefined);

	private readonly unsubscribe$ = new Subject<void>();

	private readonly conceptViewClient = inject(ConceptViewClient);
	private readonly translate = inject(TranslateService);
	private readonly backgroundRequests = inject(BackgroundRequestService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	private loadStructureReferences(conceptId: string, page: number, pageSize: number) {
		this.isLoadingPage = true;

		this.backgroundRequests
			.withoutGlobalSpinner(this.conceptViewClient.getStructureReferencesByIdAndPageAndPageSize(conceptId, page, pageSize))
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe({
				next: response => {
					this.isLoadingPage = false;
					this.pagingInfo = new SearchResultPagingInfo(response.headers);
					this.setRows(response.result);
				},
				error: () => (this.isLoadingPage = false)
			});
	}

	private setRows(references: IopConceptStructureReferenceModel[]) {
		this.dataSource.data = references.map(item => ({
			datasetUri: item.datasetUri,
			datasetUrl: item.dataset?.datasetId ? `/catalog/datasets/${item.dataset.datasetId}/description` : undefined,
			attributeUri: item.attributeUri,
			datasetName: item.dataset?.title,
			attributeName: this.getAttributeName(item.attributeUri),
			publisherName: item.dataset?.publisherName
		}));
	}

	onChangePage(pageEvent: PageEvent) {
		if (!this.conceptId) {
			return;
		}

		this.loadStructureReferences(this.conceptId, pageEvent.pageIndex + 1, pageEvent.pageSize);
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
