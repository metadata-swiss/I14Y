import {Component, EventEmitter, inject, Output} from '@angular/core';
import {MetasearchClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {first} from 'rxjs/operators';
import {SearchResultsComponent} from 'src/app/shared/search/search-results/search-results.component';
import {SearchResultPagingInfo} from 'src/app/shared/search/SearchResultPagingInfo';

@Component({
	selector: 'app-opendata-search-results',
	templateUrl: '../../shared/search/search-results/search-results.component.html',
	styleUrls: ['./opendata-search.component.scss'],
	standalone: false
})
export class OpendataSearchResultsComponent extends SearchResultsComponent {
	@Output() updateCounters: EventEmitter<number> = new EventEmitter();

	private readonly metasearchClient = inject(MetasearchClient);

	protected loadFilters(): Promise<void> {
		return new Promise(resolve => resolve());
	}

	protected search() {
		const queryParams = this.route.snapshot.queryParamMap;
		const query = queryParams.get('query');
		const lang = queryParams.get('lang') ?? this.translate.getCurrentLang();
		const page = queryParams.has('page') ? Number(queryParams.get('page')) : this.defaultPage;
		const pageSize = queryParams.has('pageSize') ? Number(queryParams.get('pageSize')) : this.defaultPageSize;

		this.metasearchClient
			.getOpendataSearchByLanguageAndQueryAndPageAndPageSize(lang, query ?? '', page, pageSize)
			.pipe(first())
			.subscribe(response => {
				this.pagingInfo = new SearchResultPagingInfo(response.headers);
				this.results = response.result;
				this.updateCounters.emit(Number(response.headers['x-paging-totalrows']));
			});
	}
}
