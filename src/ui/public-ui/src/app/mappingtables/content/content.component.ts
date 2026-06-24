import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {PageEvent} from '@angular/material/paginator';
import {MatTableDataSource} from '@angular/material/table';
import {
	IMappingRelationModel,
	MappingRelationModel,
	MappingRelationsDataFormat,
	MappingTableModel,
	MappingTablesClient
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {SearchResultPagingInfo} from 'src/app/shared/search/SearchResultPagingInfo';
import {MappingTableService} from '../services/mappingtable.service';
import {Subject, takeUntil} from 'rxjs';
import {animate, state, style, transition, trigger} from '@angular/animations';

@Component({
	selector: 'app-content',
	templateUrl: './content.component.html',
	styleUrls: ['./content.component.scss'],
	animations: [
		trigger('detailExpand', [
			state('collapsed', style({height: '0px', minHeight: '0'})),
			state(
				'expanded',
				style({
					height: '*',
					minHeight: ''
				})
			),
			transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)'))
		])
	],
	standalone: false
})
export class ContentComponent implements OnInit, OnDestroy {
	mappingTable: MappingTableModel;
	mappingRelations = new MatTableDataSource<MappingRelationModel>([]);
	pagingInfo: SearchResultPagingInfo = new SearchResultPagingInfo(undefined);

	COLUMN_ADDITIONALINFO = 'additionalInfo';
	COLUMN_RELATIONTYPE = 'relationType';
	COLUMN_SOURCECODE = 'sourceCode';
	COLUMN_TARGETCODE = 'tragetCode';

	displayedColumns = [this.COLUMN_SOURCECODE, this.COLUMN_RELATIONTYPE, this.COLUMN_TARGETCODE];
	expandedElement: IMappingRelationModel | undefined;

	readonly downloadFormat = MappingRelationsDataFormat;

	private readonly defaultPage = 1;
	private readonly defaultPageSize = 10;
	private readonly unsubscribe$ = new Subject();

	private readonly mappingTableClient = inject(MappingTablesClient);
	private readonly mappingTableService = inject(MappingTableService);

	ngOnInit() {
		this.mappingTableService.mappingtable$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.mappingTable = x;
			this.mappingTableClient.getRelationsByIdAndPageAndPageSize(x.id, this.defaultPage, this.defaultPageSize).subscribe(response => {
				this.mappingRelations.data = response.result;
				this.pagingInfo = new SearchResultPagingInfo(response.headers);
			});
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(null);
		this.unsubscribe$.complete();
	}

	onChangePage(pageEvent: PageEvent) {
		this.mappingTableClient.getRelationsByIdAndPageAndPageSize(this.mappingTable.id, pageEvent.pageIndex + 1, pageEvent.pageSize).subscribe(response => {
			this.mappingRelations.data = response.result;
			this.pagingInfo = new SearchResultPagingInfo(response.headers);
		});
	}

	downloadMappingRelations(format: MappingRelationsDataFormat): void {
		this.mappingTableClient.getRelationsExportsByIdAndFormat(this.mappingTable.id, format).subscribe(response => {
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
}
