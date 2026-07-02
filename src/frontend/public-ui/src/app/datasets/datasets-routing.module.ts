import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {DatasetsComponent} from './datasets.component';
import {DescriptionComponent} from './description/description.component';
import {DistributionComponent} from './distributions/distribution.component';
import {QualityInfoComponent} from './qualityinfo/qualityinfo.component';
import {StructureComponent} from './structure/structure.component';

const routes: Routes = [
	{
		path: '',
		component: DatasetsComponent,
		children: [
			{path: '', redirectTo: 'description', pathMatch: 'full'},
			{path: 'description', component: DescriptionComponent},
			{
				path: 'distributions/:distributionId',
				redirectTo: 'description/distributions/:distributionId',
				pathMatch: 'full'
			},
			{
				path: 'description/distributions/:distributionId',
				component: DistributionComponent
			},
			{path: 'structure/:classId/:propertyId', component: StructureComponent},
			{path: 'structure/:classId', component: StructureComponent},
			{path: 'structure', component: StructureComponent},
			{path: 'qualityinfo', component: QualityInfoComponent}
		]
	}
];
@NgModule({
	imports: [RouterModule.forChild(routes)],
	exports: [RouterModule]
})
export class DatasetsRoutingModule {}
