import {NgModule} from '@angular/core';
import {SharedModule} from '../shared/shared.module';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {RouterModule} from '@angular/router';
import {DeactivationGuarded} from '../shared/deactivationguarded.interface';
import {ConceptSharedModule} from './concept-shared.module';
import {ConceptEditComponent} from './concept-edit/concept-edit.component';
import {NAV_VALUE_CREATE, NAV_VALUE_EDIT, NAV_VALUE_VERSION} from '../app-constants';
import {ConceptCreateGuard, ConceptCreateVersionGuard, ConceptEditGuard} from '../services/guards';
import {DescriptionComponent} from './description/description.component';
import {ConceptViewComponent} from './concept.view/concept.view.component';

@NgModule({
	imports: [
		RouterModule.forChild([
			{
				path: NAV_VALUE_CREATE,
				component: ConceptEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [ConceptCreateGuard]
			},
			{
				path: ':conceptId/description/' + NAV_VALUE_EDIT,
				component: ConceptEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [ConceptEditGuard]
			},
			{
				path: ':conceptId/' + NAV_VALUE_VERSION,
				component: ConceptEditComponent,
				canDeactivate: [(component: DeactivationGuarded) => (component.canDeactivate ? component.canDeactivate() : true)],
				canActivate: [ConceptCreateVersionGuard]
			},
			{
				path: ':conceptId',
				component: ConceptViewComponent,
				children: [
					{path: '', redirectTo: 'description', pathMatch: 'full'},
					{path: 'description', component: DescriptionComponent}
				]
			}
		]),
		ConceptSharedModule,
		SharedModule,
		TranslateModule
	],
	declarations: [],
	providers: [TranslateService, DescriptionComponent]
})
export class ConceptsModule {}
