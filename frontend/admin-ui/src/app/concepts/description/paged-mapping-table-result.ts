import {MappingTableModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {SearchResultPagingInfo} from 'src/app/shared/searchResultPagingInfo';

export class PagedMappingTableResult {
	mappingTables: MappingTableModel[];
	pagingInfo: SearchResultPagingInfo;

	constructor(mappingTables: MappingTableModel[], pagingInfo: SearchResultPagingInfo) {
		this.mappingTables = mappingTables;
		this.pagingInfo = pagingInfo;
	}
}
