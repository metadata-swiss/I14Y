import {CodeListEntryDetail} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {SearchResultPagingInfo} from 'src/app/shared/searchResultPagingInfo';

export class PagedCodelistEntryResult {
	codeListEntries: CodeListEntryDetail[];
	pagingInfo: SearchResultPagingInfo;

	constructor(codeListEntries: CodeListEntryDetail[], pagingInfo: SearchResultPagingInfo) {
		this.codeListEntries = codeListEntries;
		this.pagingInfo = pagingInfo;
	}
}
