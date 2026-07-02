import {NgModule} from '@angular/core';
import {RouterModule} from '@angular/router';
import {TranslateModule} from '@ngx-translate/core';
import {SharedModule} from '../../shared/shared.module';
import {OpendataSearchComponent} from './opendata-search.component';
import {OpendataSearchResultsComponent} from './opendata-search-results.component';

@NgModule({
	declarations: [OpendataSearchComponent, OpendataSearchResultsComponent],
	imports: [
		SharedModule,
		TranslateModule,
		RouterModule,
		RouterModule.forChild([
			{
				path: '',
				component: OpendataSearchComponent
			}
		])
	],
	exports: [OpendataSearchComponent]
})
export class OpendataModule {}
