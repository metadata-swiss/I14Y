import {Component, inject, OnDestroy, OnInit, signal} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {
	IMappingRelationModel,
	MappingRelationModel,
	MappingRelationsDataFormat,
	MappingTableModel,
	MappingTablesClient
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {takeUntil} from 'rxjs/operators';
import {NAV_VALUE_EDIT} from 'src/app/app-constants';
import {Subject} from 'rxjs';
import {ViewType} from 'src/app/shared/templates/viewtype';
import {MappingTableService} from '../services/mappingtable.service';
import {MatTableDataSource} from '@angular/material/table';
import {SearchResultPagingInfo} from 'src/app/shared/searchResultPagingInfo';
import {PageEvent} from '@angular/material/paginator';
import {isLocalIri} from 'src/app/shared/iri-helpers';

@Component({
	selector: 'app-description',
	templateUrl: './description.component.html',
	styleUrls: ['./description.component.scss'],
	standalone: false
})
export class DescriptionComponent implements OnInit, OnDestroy {
	mappingTable: MappingTableModel = new MappingTableModel();
	currentLanguage: string;
	mappingTableId: string = '';
	mappingRelations = new MatTableDataSource<MappingRelationModel>([]);
	pagingInfo: SearchResultPagingInfo = new SearchResultPagingInfo(undefined);

	COLUMN_ADDITIONALINFO = 'additionalInfo';
	COLUMN_RELATIONTYPE = 'relationType';
	COLUMN_SOURCECODE = 'sourceCode';
	COLUMN_TARGETCODE = 'tragetCode';

	displayedColumns = [this.COLUMN_SOURCECODE, this.COLUMN_RELATIONTYPE, this.COLUMN_TARGETCODE];
	expandedElement = signal<IMappingRelationModel | null>(null);
	readonly nav_value_edit: string = NAV_VALUE_EDIT;
	readonly viewTypeEnum = ViewType;
	readonly downloadFormat = MappingRelationsDataFormat;

	private readonly unsubscribe$ = new Subject();

	private readonly mappingTableClient = inject(MappingTablesClient);
	private readonly mappingTableService = inject(MappingTableService);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.route.parent?.params.pipe(takeUntil(this.unsubscribe$)).subscribe(params => (this.mappingTableId = params.id));

		this.mappingTableService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.mappingTable = x));

		this.mappingTableService.pagedMappingRelationResult$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.mappingRelations.data = x?.relations || [];
			this.pagingInfo = x?.pagingInfo || new SearchResultPagingInfo(undefined);
		});

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	/**
	 * Returns true when the URI belongs to the current environment AND the backend
	 * resolved a code for it. Used to gate the "code | label" display in the
	 * Mapping relations table; cross-environment URIs always show the raw IRI.
	 */
	showResolvedCode(entry: {uri?: string; code?: string} | undefined): boolean {
		return !!entry?.uri && !!entry.code && isLocalIri(entry.uri);
	}

	onChangePage(pageEvent: PageEvent) {
		this.mappingTableService.updateMappingRelations(this.mappingTableId, pageEvent.pageIndex + 1, pageEvent.pageSize);
	}

	downloadCodelistEntries(format: MappingRelationsDataFormat): void {
		this.mappingTableClient.getRelationsExportsByIdAndFormat(this.mappingTableId, format).subscribe(response => {
			const a = document.createElement('a');
			const objectUrl = URL.createObjectURL(response.result.data);

			const fileName = `MappingRelations_${
				this.mappingTable?.identifiers && this.mappingTable?.identifiers.length > 0 ? this.mappingTable?.identifiers[0] : this.mappingTable.id
			}-${this.mappingTable?.version}.${format}`;

			a.href = objectUrl;
			a.download = response.result.fileName ?? fileName;
			a.click();

			URL.revokeObjectURL(objectUrl);
			a.remove();
		});
	}

	toggleDetail(element: IMappingRelationModel) {
		this.expandedElement.update(e => (e !== element ? element : null));
	}
}
