export class SearchResultPagingInfo {
	page = 1;
	pageSize = 10;
	totalPages = 1;
	totalRows = 0;
	pageSizeOptions = [10, 25, 50, 100];

	constructor(headers: {[key: string]: any} | undefined, pageSize?: number) {
		if (pageSize) {
			this.pageSize = pageSize;
		}
		if (headers) {
			this.page = Number(headers['x-paging-page']);
			this.pageSize = Number(headers['x-paging-pagesize']);
			this.totalPages = Number(headers['x-paging-totalpages']);
			this.totalRows = Number(headers['x-paging-totalrows']);
			this.pageSizeOptions = [10, 25, 50, 100];
		}
	}
}
