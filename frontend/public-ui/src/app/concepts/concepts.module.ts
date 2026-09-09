import {NgModule} from '@angular/core';
import {RouterModule} from '@angular/router';
import {TranslateModule} from '@ngx-translate/core';
import {SharedModule} from '../shared/shared.module';
import {ConceptDetailComponent} from './concept-detail/concept-detail.component';
import {ConceptDetailDescriptionComponent} from './concept-detail/description/concept-detail-description.component';
import {ConceptService} from './services/concept.service';
import {ConceptRelationTableComponent} from '../shared/concept-relation-table/concept-relation-table.component';
import {ConceptDetailContentTreeComponent} from './concept-detail/content-tree/concept-detail-content-tree.component';
import {AnnotationComponent} from './concept-detail/content-tree/annotation/annotation.component';
import {CodeListEntryDetailComponent} from './concept-detail/content-tree/codelistentry-detail/codelistentry-detail.component';
import {ContentSearchResultComponent} from './concept-detail/content-search-result/content-search-result.component';
import {ContentComponent} from './concept-detail/content/content.component';
import {ContentSearchResultItemComponent} from './concept-detail/content-search-result-item/content-search-result-item.component';

@NgModule({
	declarations: [
		AnnotationComponent,
		CodeListEntryDetailComponent,
		ConceptDetailComponent,
		ConceptDetailDescriptionComponent,
		ConceptDetailContentTreeComponent,
		ConceptRelationTableComponent,
		ContentComponent,
		ContentSearchResultComponent,
		ContentSearchResultItemComponent
	],
	imports: [
		SharedModule,
		TranslateModule,
		RouterModule,
		RouterModule.forChild([
			{
				path: '',
				component: ConceptDetailComponent,
				children: [
					{path: '', redirectTo: 'description', pathMatch: 'full'},
					{path: 'description', component: ConceptDetailDescriptionComponent},
					{
						path: 'content',
						component: ContentComponent,
						children: [
							{path: 'search', component: ContentSearchResultComponent},
							{
								path: '',
								component: ConceptDetailContentTreeComponent,
								children: [
									{
										path: '**',
										component: ConceptDetailContentTreeComponent
									}
								]
							}
						]
					}
				]
			}
		])
	],
	providers: [ConceptService]
})
export class ConceptsModule {}
