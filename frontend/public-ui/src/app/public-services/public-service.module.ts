import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {PublicServiceRoutingModule} from './public-service-routing.module';
import {PublicServicesComponent} from './public-services.component';
import {PublicServiceDescriptionComponent} from './public-service-description/public-service-description.component';
import {TranslateModule} from '@ngx-translate/core';
import {SharedModule} from '../shared/shared.module';
import {DcatPublicServiceService} from './services/dcat-publicservice.service';
import {ChannelDetailComponent} from './channel/channel-detail.component';
import {ChannelsComponent} from './channel/channels.component';
import {ChannelsOverviewComponent} from './channel/channels-overview.component';

@NgModule({
	declarations: [PublicServicesComponent, PublicServiceDescriptionComponent, ChannelDetailComponent, ChannelsComponent, ChannelsOverviewComponent],
	imports: [CommonModule, PublicServiceRoutingModule, TranslateModule, SharedModule, CommonModule],
	exports: [PublicServicesComponent, PublicServiceDescriptionComponent, ChannelDetailComponent, ChannelsComponent, ChannelsOverviewComponent],
	providers: [DcatPublicServiceService]
})
export class PublicServiceModule {}
