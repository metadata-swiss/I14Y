import {NgModule} from '@angular/core';
import {RouterModule} from '@angular/router';
import {DataServicesComponent} from './data-services.component';
import {DataServiceDescriptionComponent} from './data-service-description/data-service-description.component';

@NgModule({
	imports: [
		RouterModule.forChild([
			{
				path: '',
				component: DataServicesComponent,
				children: [
					{path: '', redirectTo: 'description', pathMatch: 'full'},
					{path: 'description', component: DataServiceDescriptionComponent}
				]
			}
		])
	],
	exports: [RouterModule]
})
export class DataServicesRoutingModule {}
