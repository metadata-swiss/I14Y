import {NgModule} from '@angular/core';
import {RouterModule} from '@angular/router';
import {NewsComponent} from './news.component';
import {SharedModule} from '../shared/shared.module';
import {TranslateModule} from '@ngx-translate/core';

@NgModule({
	declarations: [NewsComponent],
	imports: [
		RouterModule.forChild([
			{
				path: '',
				component: NewsComponent
			}
		]),
		SharedModule,
		TranslateModule
	]
})
export class NewsModule {}
