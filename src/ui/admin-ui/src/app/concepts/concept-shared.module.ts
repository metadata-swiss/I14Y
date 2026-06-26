import {CUSTOM_ELEMENTS_SCHEMA, NgModule} from '@angular/core';
import {RouterModule} from '@angular/router';
import {TranslateModule, TranslateService} from '@ngx-translate/core';
import {SharedModule} from '../shared/shared.module';
import {ConceptEditFormComponent} from './concept-edit/concept-edit-form/concept-edit-form.component';
import {ConceptEditComponent} from './concept-edit/concept-edit.component';
import {ConceptViewComponent} from './concept.view/concept.view.component';
import {ConceptService} from './services/concept.service';
import {DescriptionComponent} from './description/description.component';
import {ConceptMultiIdentifiersValidator} from '../shared/validators/identifier-validator/concept-multi-identifiers.validator';
import {MultiIdentifiersValidator} from '../shared/validators/identifier-validator/multi-Identifiers.validator';

@NgModule({
	imports: [SharedModule, TranslateModule, RouterModule],
	declarations: [ConceptEditComponent, ConceptEditFormComponent, ConceptViewComponent, DescriptionComponent],
	exports: [ConceptEditComponent, ConceptEditFormComponent],
	providers: [TranslateService, ConceptService, ConceptMultiIdentifiersValidator, {provide: MultiIdentifiersValidator, useClass: ConceptMultiIdentifiersValidator}],
	schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class ConceptSharedModule {}
