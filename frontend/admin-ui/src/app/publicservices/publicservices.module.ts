import {NgModule} from '@angular/core';
import {RouterModule} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {ConceptSharedModule} from '../concepts/concept-shared.module';
import {DeactivationGuarded} from '../shared/deactivationguarded.interface';
import {SharedModule} from '../shared/shared.module';
import {PublicservicesViewComponent} from './publicservices.view/publicservices.view.component';
import {PublicServiceService} from './services/publicservice.service';
import {DescriptionComponent} from './description/description.component';
import {DescriptionEditComponent} from './description/description.edit.component';
import {DescriptionEditFormComponent} from './description/edit-form/description-edit-form.component';
import {NAV_VALUE_CREATE, NAV_VALUE_EDIT} from '../app-constants';
import {EditTableChannelComponent} from './description/edit-form/edit-table-channel/edit-table-channel.component';
import {ChannelDetailComponent} from './channel/channel-detail.component';
import {ChannelsOverviewComponent} from './channel/channels-overview.component';
import {ChannelsComponent} from './channel/channels.component';
import {EditTableLinkIsDescribedComponent} from './description/edit-form/edit-table-link-is-described/edit-table-link-is-described.component';
import {EditTableLinkIsLinkedWithComponent} from './description/edit-form/edit-table-link-is-linked-with/edit-table-link-is-linked-with.component';
import {EditTableLinkRequiresComponent} from './description/edit-form/edit-table-link-requires/edit-table-link-requires.component';
import {PublicServiceCreateGuard, PublicServiceEditGuard} from '../services/guards';
import {ArrayToStringPipe} from '../shared/formatting/array-to-string.pipe';
import {FallbackPipe} from '../shared/fallback/fallback.pipe';
import {ModalDialogChannelComponent} from './description/edit-form/edit-table-channel/modal-dialog/modal-dialog.component';
import {PublicServiceMultiIdentifiersValidator} from '../shared/validators/identifier-validator/public-service-multi-identifiers.validator';
import {MultiIdentifiersValidator} from '../shared/validators/identifier-validator/multi-Identifiers.validator';

@NgModule({
	imports: [
		ConceptSharedModule,
		SharedModule,
		TranslateModule,
		RouterModule.forChild([
			{
				path: NAV_VALUE_CREATE,
				component: DescriptionEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [PublicServiceCreateGuard]
			},
			{
				path: ':id/description/' + NAV_VALUE_EDIT,
				component: DescriptionEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [PublicServiceEditGuard]
			},
			{
				path: ':id',
				component: PublicservicesViewComponent,
				children: [
					{path: '', redirectTo: 'description', pathMatch: 'full'},
					{path: 'description', component: DescriptionComponent}
				]
			}
		])
	],
	declarations: [
		PublicservicesViewComponent,
		ChannelDetailComponent,
		ChannelsComponent,
		ChannelsOverviewComponent,
		DescriptionComponent,
		DescriptionEditComponent,
		DescriptionEditFormComponent,
		EditTableLinkIsDescribedComponent,
		EditTableLinkIsLinkedWithComponent,
		EditTableLinkRequiresComponent,
		EditTableChannelComponent,
		ModalDialogChannelComponent
	],
	exports: [],
	providers: [TranslateService, PublicServiceService, ArrayToStringPipe, FallbackPipe, PublicServiceMultiIdentifiersValidator, {provide: MultiIdentifiersValidator, useClass: PublicServiceMultiIdentifiersValidator}]
})
export class PublicServicesModule {}
