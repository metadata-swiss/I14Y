import {NgModule} from '@angular/core';
import {RouterModule} from '@angular/router';
import {TranslateModule} from '@ngx-translate/core';
import {SharedModule} from '../shared/shared.module';
import {OrganisationsComponent} from './organisations.component';
import {DescriptionComponent} from './description/description.component';

@NgModule({
	declarations: [OrganisationsComponent, DescriptionComponent],
	imports: [
		SharedModule,
		TranslateModule,
		RouterModule,
		RouterModule.forChild([
			{
				path: '',
				component: OrganisationsComponent
			},
			{
				path: ':agentId',
				component: DescriptionComponent
			}
		])
	]
})
export class OrganisationsModule {}
