import {HttpErrorResponse, HttpParams, HttpStatusCode} from '@angular/common/http';
import {Component, inject, OnDestroy} from '@angular/core';
import {ParamMap} from '@angular/router';
import {ConceptViewClient, FilterConfigurationModel, FilterConfigurationsClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {AppConfig} from 'src/app/app.config';
import {IAppConfig} from 'src/app/app.config.interface';
import {SearchResultsComponent} from 'src/app/shared/search/search-results/search-results.component';
import {SearchResultPagingInfo} from 'src/app/shared/search/SearchResultPagingInfo';
import {OffCanvasService} from 'src/app/shared/services/off-canvas/off-canvas.service';
import {ConceptService} from '../../services/concept.service';

@Component({
	selector: 'app-content-search-result',
	templateUrl: './content-search-result.component.html',
	styleUrls: ['./content-search-result.component.scss'],
	standalone: false
})
export class ContentSearchResultComponent extends SearchResultsComponent implements OnDestroy {
	filterLanguage: string | null = null;
	filterConfiguration: FilterConfigurationModel | undefined;

	private searchJsonUrl: string | undefined;
	private searchCsvUrl: string | undefined;

	private readonly languages: string[] = ['de', 'fr', 'it', 'en', 'rm'];

	private readonly conceptViewClient = inject(ConceptViewClient);
	private readonly filterConfigurationsClient = inject(FilterConfigurationsClient);
	private readonly offCanvasService = inject(OffCanvasService);
	private readonly conceptService = inject(ConceptService);

	ngOnDestroy() {
		this.offCanvasService.reset();
		this.conceptService.setSearchExport(null);
		super.ngOnDestroy();
	}

	getFilters(queryParams: ParamMap): string[] | undefined {
		const filters: string[] = [];

		if (this.filterConfiguration) {
			this.filterConfiguration.filters?.forEach(item => {
				let param = queryParams.get(item.filterIdentifier);

				if (param) {
					filters.push(`{"filterIdentifier": "${item.filterIdentifier}","values": ["${param}"] }`);
				}
			});
		}

		return filters.length > 0 ? filters : undefined;
	}

	protected loadFilters(): Promise<void> {
		return new Promise(resolve => {
			this.loadFilterConfiguration().then(() => {
				this.offCanvasService.filterConfiguration$.pipe().subscribe(x => {
					this.filterConfiguration = x;
					resolve();
				});
			});
		});
	}

	protected search() {
		const conceptId = this.route.parent.parent.parent.snapshot.params.conceptId;

		const queryParams = this.route.snapshot.queryParamMap;
		const query = queryParams.get('query') ?? undefined;
		this.filterLanguage = queryParams.get('lang');
		const lang = this.filterLanguage ?? this.translate.getCurrentLang();
		const page = queryParams.has('page') ? Number(queryParams.get('page')) : this.defaultPage;
		const pageSize = queryParams.has('pageSize') ? Number(queryParams.get('pageSize')) : this.defaultPageSize;

		let filters: string[] | undefined = this.getFilters(queryParams);

		this.conceptService.setSearchExport(null);
		this.buildSearchExportUrls(conceptId, lang, query, filters);

		this.conceptViewClient
			.getCodelistEntriesSearchByIdAndLanguageAndQueryAndFiltersAndPageAndPageSize(conceptId, lang, query, filters, page, pageSize)
			.subscribe({
				next: response => {
					this.pagingInfo = new SearchResultPagingInfo(response.headers);
					this.results = response.result;
					this.conceptService.setSearchExport({
						jsonUrl: this.searchJsonUrl,
						csvUrl: this.searchCsvUrl,
						totalRows: this.pagingInfo.totalRows
					});
				},
				error: () => {
					this.conceptService.setSearchExport(null);
				}
			});
	}

	private buildSearchExportUrls(conceptId: string, lang: string, query: string | undefined, filters: string[] | undefined): void {
		const baseUrl = AppConfig.getConfig<IAppConfig>().PUBLIC_API_BASE_URL;
		if (!baseUrl) {
			return;
		}

		let params = new HttpParams().set('language', lang);
		if (query) {
			params = params.set('query', query);
		}
		if (filters) {
			filters.forEach(f => {
				params = params.append('filters', f);
			});
		}

		const exportBase = `${baseUrl}/concepts/${conceptId}/codelist-entries/search/exports`;
		const queryString = params.toString();
		this.searchJsonUrl = `${exportBase}/json?${queryString}`;
		this.searchCsvUrl = `${exportBase}/csv?${queryString}`;
	}

	private loadFilterConfiguration(): Promise<void> {
		return new Promise(resolve => {
			const conceptId = this.route.parent.parent.parent.snapshot.params.conceptId;
			const titleKey = 'i18n.offcanvas.filter.title';
			this.filterConfigurationsClient.getById(conceptId).subscribe(
				res => {
					this.offCanvasService.setConfiguration(res.result, this.languages, titleKey, this.route);
					resolve();
				},
				(error: HttpErrorResponse) => {
					if (error.status === HttpStatusCode.NoContent) {
						this.offCanvasService.setConfiguration(new FilterConfigurationModel({filters: []}), this.languages, titleKey, this.route);
						resolve();
					}
				}
			);
		});
	}
}
