import {MappingRelationModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {SearchResultPagingInfo} from 'src/app/shared/searchResultPagingInfo';

export class PagedMappingRelationResult {
	relations: MappingRelationModel[];
	pagingInfo: SearchResultPagingInfo;

	constructor(relations: MappingRelationModel[], pagingInfo: SearchResultPagingInfo) {
		this.relations = relations;
		this.pagingInfo = pagingInfo;
	}
}
