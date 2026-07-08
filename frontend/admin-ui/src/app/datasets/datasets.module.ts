import {DeactivationGuarded} from '../shared/deactivationguarded.interface';
import {ContactpointComponent} from './description/edit-form/contactpoint/contactpoint.component';
import {DatasetService} from './services/dataset.service';
import {QualityInfoComponent} from './qualityinfo/qualityinfo.component';
import {DatasetsViewComponent} from './datasets.view.component';
import {DescriptionComponent} from './description/description.component';
import {DescriptionEditComponent} from './description/description.edit.component';
import {DescriptionEditFormComponent} from './description/edit-form/description-edit-form.component';
import {DistributionEditComponent} from './distributions/distribution.edit.component';
import {DistributionEditFormComponent} from './distributions/edit-form/distribution-edit-form.component';
import {DistributionsComponent} from './distributions/distributions.component';
import {DistributionsTableComponent} from './distributions/distributions-table/distributions-table.component';
import {DistributionsDetailComponent} from './distributions/distributions-detail/distributions-detail.component';
import {NgModule} from '@angular/core';
import {QualityInfoEditComponent} from './qualityinfo/qualityinfo.edit.component';
import {QualityInfoEditFormComponent} from './qualityinfo/edit-form/qualityinfo-edit-form.component';
import {RouterModule} from '@angular/router';
import {SharedModule} from '../shared/shared.module';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {NAV_VALUE_CREATE, NAV_VALUE_EDIT} from 'src/app/app-constants';
import {DatasetCreateGuard, DatasetCreateVersionGuard, DatasetEditGuard} from '../services/guards';
import {LinkedDataModelComponent} from './linked-data-model/linked-data-model.component';
import {LinkedDataGraphComponent} from './linked-data-model/linked-data-graph/linked-data-graph.component';
import {FFlowModule} from '@foblex/flow';
import {MatListModule} from '@angular/material/list';
import {LinkedDataGraphTableComponent} from './linked-data-model/linked-data-graph/linked-data-graph-table/linked-data-graph-table.component';
import {LinkedDataSidebarComponent} from './linked-data-model/linked-data-graph/linked-data-sidebar/linked-data-sidebar.component';
import {DatasetMultiIdentifiersValidator} from '../shared/validators/identifier-validator/dataset-multi-identifiers.validator';
import {MultiIdentifiersValidator} from '../shared/validators/identifier-validator/multi-Identifiers.validator';
import {StructureDetailViewComponent} from './linked-data-model/linked-data-graph/linked-data-sidebar/structure-detail-view/structure-detail-view.component';
import {StructureDetailEditComponent} from './linked-data-model/linked-data-graph/linked-data-sidebar/structure-detail-edit/structure-detail-edit.component';
import {StructureDetailEditFormComponent} from './linked-data-model/linked-data-graph/linked-data-sidebar/structure-detail-edit/structure-detail-edit-form/structure-detail-edit-form.component';
import {LinkedDataClassTableComponent} from './linked-data-model/linked-data-class-table/linked-data-class-table.component';
import {SpatialComponent} from './description/edit-form/spatial/spatial.component';
import {TemporalCoverageComponent} from './description/edit-form/temporal-coverage/temporal-coverage.component';

@NgModule({
	imports: [
		RouterModule.forChild([
			{
				path: NAV_VALUE_CREATE,
				component: DescriptionEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [DatasetCreateGuard]
			},
			{
				path: ':id/description/' + NAV_VALUE_EDIT,
				component: DescriptionEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [DatasetEditGuard]
			},
			{
				path: ':id/distributions/' + NAV_VALUE_CREATE,
				component: DistributionEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [DatasetEditGuard]
			},
			{
				path: ':id/distributions/:distributionId/' + NAV_VALUE_EDIT,
				component: DistributionEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [DatasetEditGuard]
			},
			{
				path: ':id/qualityinfo/' + NAV_VALUE_CREATE,
				component: QualityInfoEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [DatasetEditGuard]
			},
			{
				path: ':id/qualityinfo/' + NAV_VALUE_EDIT,
				component: QualityInfoEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [DatasetEditGuard]
			},
			{
				path: ':id/newversion',
				component: DescriptionEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [DatasetCreateVersionGuard]
			},
			{
				path: ':id',
				component: DatasetsViewComponent,
				children: [
					{path: '', redirectTo: 'description', pathMatch: 'full'},
					{path: 'description', component: DescriptionComponent},
					{path: 'distributions', component: DistributionsComponent},
					{
						path: 'distributions/:distributionId',
						component: DistributionsDetailComponent
					},
					{
						path: 'structure',
						component: LinkedDataModelComponent
					},
					{path: 'qualityinfo', component: QualityInfoComponent}
				]
			}
		]),
		SharedModule,
		TranslateModule,
		FFlowModule,
		MatListModule
	],
	declarations: [
		ContactpointComponent,
		DatasetsViewComponent,
		DescriptionComponent,
		DescriptionEditComponent,
		DescriptionEditFormComponent,
		DistributionEditComponent,
		DistributionEditFormComponent,
		DistributionsComponent,
		DistributionsDetailComponent,
		DistributionsTableComponent,
		LinkedDataClassTableComponent,
		LinkedDataModelComponent,
		LinkedDataGraphComponent,
		LinkedDataGraphTableComponent,
		LinkedDataSidebarComponent,
		SpatialComponent,
		StructureDetailViewComponent,
		StructureDetailEditComponent,
		StructureDetailEditFormComponent,
		TemporalCoverageComponent,
		QualityInfoComponent,
		QualityInfoEditComponent,
		QualityInfoEditFormComponent
	],
	exports: [],
	providers: [
		TranslateService,
		DatasetService,
		DatasetMultiIdentifiersValidator,
		{provide: MultiIdentifiersValidator, useClass: DatasetMultiIdentifiersValidator}
	]
})
export class DatasetsModule {}
