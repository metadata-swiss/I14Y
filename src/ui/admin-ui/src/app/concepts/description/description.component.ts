import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {PageEvent} from '@angular/material/paginator';
import {ActivatedRoute, Params} from '@angular/router';
import {
	CodeListEntriesDataFormat,
	CodeListEntryDetail,
	ConceptType,
	ConceptVersionView,
	ConceptView,
	ConceptViewClient,
	IopConceptStructureReferenceModel,
	MappingTableModel,
	MultiLanguage,
	RegistrationStatusInfoModel,
	VocabularyEntry
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Observable, of, Subject, takeUntil} from 'rxjs';
import {FormatFunctions} from 'src/app/shared/format-functions';
import {SearchResultPagingInfo} from 'src/app/shared/searchResultPagingInfo';
import {ConceptService} from '../services/concept.service';
import {ViewType} from 'src/app/shared/templates/viewtype';

@Component({
	selector: 'app-description',
	templateUrl: './description.component.html',
	styleUrls: ['./description.component.scss'],
	standalone: false
})
export class DescriptionComponent implements OnInit, OnDestroy {
	concept: ConceptView | undefined;
	currentLanguage: string;
	versions: ConceptVersionView[] = [];
	deleteState$: Observable<string> = of('');
	pagingInfo: SearchResultPagingInfo = new SearchResultPagingInfo(undefined);
	codeListEntries: CodeListEntryDetail[] = [];
	backParams: Params = {};
	newversion: boolean = false;
	conceptId = '';
	structureReferences: IopConceptStructureReferenceModel[] = [];
	registrationStatusInfo: RegistrationStatusInfoModel | undefined;
	count: number | undefined;
	columnsToDisplay = ['name', 'type', 'version', 'validFrom', 'validTo', 'status', 'publication'];

	// Mapping tables
	mappingTables: MappingTableModel[] = [];
	mappingTablesPagingInfo: SearchResultPagingInfo = new SearchResultPagingInfo(undefined);
	mappingTableColumns = ['name', 'version', 'validFrom', 'validTo', 'publisher', 'status'];

	readonly conceptTypeEnum = ConceptType;
	readonly viewTypeEnum = ViewType;
	readonly downloadFormat = CodeListEntriesDataFormat;

	private readonly unsubscribe$ = new Subject();

	private readonly conceptService = inject(ConceptService);
	private readonly conceptViewClient = inject(ConceptViewClient);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.conceptId = this.route.parent?.snapshot.params.conceptId;
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});

		this.route.parent?.params.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
			this.conceptId = params.conceptId;
		});

		this.conceptService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
			this.concept = result;
		});

		this.conceptService.pagedCodelistEntryResult$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.codeListEntries = x?.codeListEntries || [];
			this.pagingInfo = x?.pagingInfo || new SearchResultPagingInfo(undefined);
		});

		this.conceptService.mappingTables$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
			this.mappingTables = result?.mappingTables || [];
			this.mappingTablesPagingInfo = result?.pagingInfo || new SearchResultPagingInfo(undefined);
		});

		this.conceptService.registrationStatusInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(status => (this.registrationStatusInfo = status));
		this.conceptService.versions$
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(result => (this.versions = [...result].sort((a, b) => (b.version as string).localeCompare(a.version as string))));

		this.conceptService.structureReferences$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
			this.structureReferences = result ?? [];
		});
	}

	onChangePage(pageEvent: PageEvent) {
		this.conceptService.updateCodeListEntries(this.conceptId, pageEvent.pageIndex + 1, pageEvent.pageSize);
	}

	onChangeMappingTablesPage(pageEvent: PageEvent) {
		this.conceptService.updateMappingTables(pageEvent.pageIndex + 1, pageEvent.pageSize);
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	getFormattedDate(date: Date | undefined): string | null {
		return FormatFunctions.getFormattedDate(date, this.translate);
	}

	getThemes(themes: VocabularyEntry[] | undefined): string[] | undefined {
		return FormatFunctions.getTranslatedVocabularyEntries(themes, this.currentLanguage);
	}

	getKeyWords(keyWords: MultiLanguage[] | undefined): string[] | undefined {
		return FormatFunctions.getTranslatedMultiLanguages(keyWords, this.currentLanguage);
	}

	downloadCodelistEntries(format: CodeListEntriesDataFormat, withoutAnnotations: boolean = true): void {
		this.conceptViewClient.getCodelistEntriesExportsByIdAndFormatAndWithAnnotations(this.conceptId, format, withoutAnnotations).subscribe(response => {
			const a = document.createElement('a');
			const objectUrl = URL.createObjectURL(response.result.data);

			const fileName = `CodelistEntries_${this.concept?.identifiers?.[0]}-${this.concept?.version}.${format}`;

			a.href = objectUrl;
			a.download = response.result.fileName ?? fileName;
			a.click();

			URL.revokeObjectURL(objectUrl);
			a.remove();
		});
	}
}
