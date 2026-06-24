import {CommonModule} from '@angular/common';
import {DataServicesComponent} from './data-services.component';
import {DataServiceDescriptionComponent} from './data-service-description/data-service-description.component';
import {NgModule} from '@angular/core';
import {SharedModule} from '../shared/shared.module';
import {TranslateModule} from '@ngx-translate/core';
import {DataServicesRoutingModule} from './data-services-routing.module';
import {DataServiceService} from './services/dataservice.service';

@NgModule({
	declarations: [DataServicesComponent, DataServiceDescriptionComponent],
	imports: [TranslateModule, SharedModule, DataServicesRoutingModule, CommonModule],
	exports: [DataServicesComponent, DataServiceDescriptionComponent],
	providers: [DataServiceService]
})
export class DataServicesModule {}
