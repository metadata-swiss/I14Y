import {NgModule} from '@angular/core';
import {DescriptionEditComponent} from './description/description.edit.component';
import {ContactpointComponent} from './description/edit-form/contactpoint/contactpoint.component';
import {DescriptionEditFormComponent} from './description/edit-form/description-edit-form.component';
import {SharedModule} from '../shared/shared.module';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {RouterModule} from '@angular/router';
import {DeactivationGuarded} from '../shared/deactivationguarded.interface';
import {DataservicesViewComponent} from './dataservices.view.component';
import {DescriptionComponent} from './description/description.component';
import {DataserviceService} from './services/dataservice.service';
import {DataServiceCreateGuard, DataServiceCreateVersionGuard, DataServiceEditGuard} from '../services/guards';
import {MultiIdentifiersValidator} from '../shared/validators/identifier-validator/multi-Identifiers.validator';
import {DataServiceMultiIdentifiersValidator} from '../shared/validators/identifier-validator/dataservice-multi-identifiers.validator';

@NgModule({
	imports: [
		RouterModule.forChild([
			{
				path: 'create',
				component: DescriptionEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [DataServiceCreateGuard]
			},
			{
				path: ':id/description/edit',
				component: DescriptionEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [DataServiceEditGuard]
			},
			{
				path: ':id/newversion',
				component: DescriptionEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [DataServiceCreateVersionGuard]
			},
			{
				path: ':id',
				component: DataservicesViewComponent,
				children: [
					{path: '', redirectTo: 'description', pathMatch: 'full'},
					{path: 'description', component: DescriptionComponent}
				]
			}
		]),
		SharedModule,
		TranslateModule
	],
	declarations: [
		DescriptionEditComponent,
		ContactpointComponent,
		DescriptionEditFormComponent,
		DataservicesViewComponent,
		DescriptionComponent
	],
	providers: [
		TranslateService,
		DataserviceService,
		DataServiceMultiIdentifiersValidator,
		{provide: MultiIdentifiersValidator, useClass: DataServiceMultiIdentifiersValidator}
	]
})
export class DataservicesModule {}
