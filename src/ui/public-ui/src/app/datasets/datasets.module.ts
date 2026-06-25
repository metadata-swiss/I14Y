import {NgModule} from '@angular/core';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {DateFormatService} from 'src/app/shared/services/date-format/date-format.service';
import {SharedModule} from 'src/app/shared/shared.module';
import {DcatDatasetService} from './services/dcat-dataset.service';
import {DatasetQualityInformationDataService} from './services/dataset-quality-information-data.service';
import {DatasetsComponent} from './datasets.component';
import {DescriptionComponent} from './description/description.component';
import {DistributionDetailComponent} from './distributions/distribution-detail.component';
import {DistributionsOverviewComponent} from './distributions/distributions-overview.component';
import {DistributionComponent} from './distributions/distribution.component';
import {DatasetsRoutingModule} from './datasets-routing.module';
import {QualityInfoComponent} from './qualityinfo/qualityinfo.component';
import {StructureComponent} from './structure/structure.component';
import {StructureGraphComponent} from './structure/graph/structure-graph.component';
import {StructureGraphSidebarComponent} from './structure/graph/graph-sidebar/structure-graph-sidebar.component';
import {StructureGraphTableComponent} from './structure/graph/graph-table/structure-graph-table.component';
import {FFlowModule} from '@foblex/flow';
import {MatListModule} from '@angular/material/list';
import {MatAutocomplete} from '@angular/material/autocomplete';
import {ClassTableComponent} from './structure/class-table/class-table.component';

@NgModule({
	imports: [TranslateModule, SharedModule, DatasetsRoutingModule, FFlowModule, MatListModule, MatAutocomplete],
	declarations: [
		ClassTableComponent,
		DatasetsComponent,
		DescriptionComponent,
		DistributionDetailComponent,
		DistributionsOverviewComponent,
		DistributionComponent,
		QualityInfoComponent,
		StructureComponent,
		StructureGraphComponent,
		StructureGraphSidebarComponent,
		StructureGraphTableComponent
	],
	exports: [DatasetsComponent],
	providers: [DateFormatService, TranslateService, DatasetQualityInformationDataService, DcatDatasetService]
})
export class DatasetsModule {}
