import {NgModule} from '@angular/core';
import {RouterModule} from '@angular/router';
import {TranslateModule} from '@ngx-translate/core';
import {SharedModule} from '../../shared/shared.module';
import {GeocatSearchComponent} from './geocat-search.component';
import {GeocatSearchResultsComponent} from './geocat-search-results.component';

@NgModule({
	declarations: [GeocatSearchComponent, GeocatSearchResultsComponent],
	imports: [
		SharedModule,
		TranslateModule,
		RouterModule,
		RouterModule.forChild([
			{
				path: '',
				component: GeocatSearchComponent
			}
		])
	],
	exports: [GeocatSearchComponent]
})
export class GeocatModule {}
