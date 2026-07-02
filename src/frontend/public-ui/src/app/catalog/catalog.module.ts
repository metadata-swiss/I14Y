import {NgModule} from '@angular/core';
import {RouterModule} from '@angular/router';
import {TranslateModule} from '@ngx-translate/core';
import {SharedModule} from '../shared/shared.module';
import {CatalogComponent} from './catalog.component';
import {CatalogSearchResultsComponent} from './search/catalog-search-results.component';
import {CatalogSearchResultItemComponent} from './search/search-result-item/search-result-item.component';
import {SearchResultTableComponent} from './search/table/search-result-table.component';

@NgModule({
	declarations: [CatalogComponent, CatalogSearchResultsComponent, CatalogSearchResultItemComponent, SearchResultTableComponent],
	imports: [
		SharedModule,
		TranslateModule,
		RouterModule,
		RouterModule.forChild([
			{
				path: '',
				component: CatalogComponent
			}
		])
	],
	exports: [CatalogComponent]
})
export class CatalogModule {}
