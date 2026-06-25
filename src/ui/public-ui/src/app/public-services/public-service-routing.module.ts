import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {PublicServiceDescriptionComponent} from './public-service-description/public-service-description.component';
import {PublicServicesComponent} from './public-services.component';

const routes: Routes = [
	{
		path: '',
		component: PublicServicesComponent,
		children: [
			{path: '', redirectTo: 'description', pathMatch: 'full'},
			{
				path: 'description',
				component: PublicServiceDescriptionComponent
			}
		]
	}
];

@NgModule({
	imports: [RouterModule.forChild(routes)],
	exports: [RouterModule]
})
export class PublicServiceRoutingModule {}
