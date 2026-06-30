import {Component, Input, OnChanges, OnDestroy, signal, SimpleChanges} from '@angular/core';
import {MatTableDataSource} from '@angular/material/table';
import {CodeListEntryDetail, ConceptView, ICodeListEntryDetail, MultiLanguage} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';
import {SortableListViewComponent} from 'src/app/shared/sortable-list-view/sortable-list-view.component';
import {IFilterModel} from './filter/filter-model';
import {FormatFunctions} from '../format-functions';
import {buildConceptCodeIri} from 'src/app/shared/iri-helpers';

type SortableKeys = 'value' | 'parentCode' | 'name';

@Component({
	selector: 'app-codevalue-table',
	templateUrl: './codevalue-table.component.html',
	styleUrls: ['./codevalue-table.component.scss'],
	standalone: false
})
export class CodevalueTableComponent extends SortableListViewComponent<ICodeListEntryDetail, SortableKeys> implements OnChanges, OnDestroy {
	@Input() dto!: CodeListEntryDetail[];
	@Input() concept: ConceptView | undefined;
	@Input() showHeader: boolean = true;
	@Input() setFilter: IFilterModel | undefined;
	dataSource = new MatTableDataSource<ICodeListEntryDetail>([]);

	COLUMN_VALUE = 'code';
	COLUMN_PARENTCODE = 'parentCode';
	COLUMN_NAME = 'name';
	COLUMN_VALIDFROM = 'validFrom';
	COLUMN_VALIDTO = 'validTo';
	COLUMN_ADDITIONALINFO = 'additionalInfo';

	currentLanguage: string;

	displayedColumns = [this.COLUMN_VALUE, this.COLUMN_NAME, this.COLUMN_VALIDFROM, this.COLUMN_VALIDTO, this.COLUMN_PARENTCODE, this.COLUMN_ADDITIONALINFO];
	expandedElement = signal<ICodeListEntryDetail | null>(null);

	private readonly unsubscribe$ = new Subject();

	constructor() {
		super();
		this.currentLanguage = this.translate.getCurrentLang();
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => (this.currentLanguage = language.lang));
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	ngOnChanges(changes: SimpleChanges) {
		const change = changes.dto;
		if (change?.currentValue !== undefined) {
			this.data = this.dto;
		}
	}

	toggleDetail(element: ICodeListEntryDetail) {
		this.expandedElement.update(e => (e !== element ? element : null));
	}

	hasAdditionalInfo(entry: ICodeListEntryDetail): boolean {
		return (
			((entry && entry.description && Object.keys(entry.description).some(x => entry.description![x as keyof MultiLanguage])) ||
				(entry && entry.annotations && entry.annotations.length > 0)) ??
			false
		);
	}

	getFormattedDate(date: Date | undefined): string | null {
		return FormatFunctions.getFormattedDate(date);
	}

	getFormattedConceptCodeIriPattern(entry: ICodeListEntryDetail): string | undefined {
		const identifier = this.concept?.identifiers?.[0];
		const version = this.concept?.version;
		const code = encodeURIComponent(entry.value ?? '');

		if (!identifier || !version) {
			return undefined;
		}

		return buildConceptCodeIri(identifier, code, version);
	}
}
