export class SearchResultPagingInfo {
	page: number;
	pageSize: number;
	totalPages: number;
	totalRows: number;
	pageSizeOptions: number[];

	constructor(headers: {[key: string]: any}, pagpageSizeOptions: number[] = [10, 25, 50, 100]) {
		if (headers) {
			this.page = Number(headers['x-paging-page']);
			this.pageSize = Number(headers['x-paging-pagesize']);
			this.totalPages = Number(headers['x-paging-totalpages']);
			this.totalRows = Number(headers['x-paging-totalrows']);
		}
		this.pageSizeOptions = pagpageSizeOptions;
	}
}
