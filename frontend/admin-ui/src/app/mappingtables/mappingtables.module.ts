import {NgModule} from '@angular/core';
import {SharedModule} from '../shared/shared.module';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {RouterModule} from '@angular/router';
import {MappingTableService} from './services/mappingtable.service';
import {MappingTableViewComponent} from './mappingtable.view.component';
import {DescriptionComponent} from './description/description.component';
import {DescriptionEditComponent} from './description/description.edit.component';
import {DescriptionEditFormComponent} from './description/edit-form/description-edit-form.component';
import {EditTableRelationsComponent} from './description/edit-form/edit-table-relations/edit-table-relations.component';
import {ModalDialogMappingRelationComponent} from './description/edit-form/edit-table-relations/modal-dialog/modal-dialog.component';
import {NAV_VALUE_CREATE, NAV_VALUE_EDIT, NAV_VALUE_VERSION} from '../app-constants';
import {DeactivationGuarded} from '../shared/deactivationguarded.interface';
import {MappingTableCreateGuard, MappingTableVersionGuard, MappingTableEditGuard} from '../services/guards';
import {MappingTableMultiIdentifiersValidator} from '../shared/validators/identifier-validator/mappingtable-multi-identifiers.validator';
import {MultiIdentifiersValidator} from '../shared/validators/identifier-validator/multi-Identifiers.validator';

@NgModule({
	imports: [
		RouterModule.forChild([
			{
				path: NAV_VALUE_CREATE,
				component: DescriptionEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [MappingTableCreateGuard]
			},
			{
				path: ':id/description/' + NAV_VALUE_EDIT,
				component: DescriptionEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [MappingTableEditGuard]
			},
			{
				path: ':id/' + NAV_VALUE_VERSION,
				component: DescriptionEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [MappingTableVersionGuard]
			},
			{
				path: ':id',
				component: MappingTableViewComponent,
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
		MappingTableViewComponent,
		DescriptionComponent,
		DescriptionEditComponent,
		DescriptionEditFormComponent,
		EditTableRelationsComponent,
		ModalDialogMappingRelationComponent
	],
	providers: [
		MappingTableService,
		TranslateService,
		MappingTableMultiIdentifiersValidator,
		{provide: MultiIdentifiersValidator, useClass: MappingTableMultiIdentifiersValidator}
	]
})
export class MappingTablesModule {}