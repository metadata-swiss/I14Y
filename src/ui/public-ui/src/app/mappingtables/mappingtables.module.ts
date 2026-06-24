import {NgModule} from '@angular/core';
import {MappingTableComponent} from './mappingtables.component';
import {DescriptionComponent} from './description/description.component';
import {TranslateModule} from '@ngx-translate/core';
import {SharedModule} from '../shared/shared.module';
import {CommonModule} from '@angular/common';
import {RouterModule} from '@angular/router';
import {MappingTableService} from './services/mappingtable.service';
import {ContentComponent} from './content/content.component';

@NgModule({
	declarations: [MappingTableComponent, DescriptionComponent, ContentComponent],
	imports: [
		TranslateModule,
		SharedModule,
		CommonModule,
		RouterModule,
		RouterModule.forChild([
			{
				path: '',
				component: MappingTableComponent,
				children: [
					{path: '', redirectTo: 'description', pathMatch: 'full'},
					{path: 'description', component: DescriptionComponent},
					{
						path: 'content',
						component: ContentComponent
					}
				]
			}
		])
	],
	providers: [MappingTableService]
})
export class MappingTablesModule {}
