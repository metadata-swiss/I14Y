import {MatTooltipModule} from '@angular/material/tooltip';
import {MatSelectModule} from '@angular/material/select';
import {NgModule} from '@angular/core';
import {ClipboardModule} from '@angular/cdk/clipboard';
import {CommonModule} from '@angular/common';
import {MatButtonModule} from '@angular/material/button';
import {MatCardModule} from '@angular/material/card';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatChipsModule} from '@angular/material/chips';
import {MatExpansionModule} from '@angular/material/expansion';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatRadioModule} from '@angular/material/radio';
import {MatRippleModule} from '@angular/material/core';
import {MatSortModule} from '@angular/material/sort';
import {MatTableModule} from '@angular/material/table';
import {MatTabsModule} from '@angular/material/tabs';
import {MatTreeModule} from '@angular/material/tree';
import {ObliqueModule} from '@oblique/oblique';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {MatMenuModule} from '@angular/material/menu';
import {AngularSplitModule} from 'angular-split';
import {MatListModule} from '@angular/material/list';
import {MatPaginatorModule} from '@angular/material/paginator';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {FileSizeFormatService} from './services/file-size-format/file-size-format.service';
import {DateFormatService} from './services/date-format/date-format.service';
import {SearchEngineOptimizationService} from './services/search-engine-optimization/search-engine-optimization.service';
import {TranslateModule} from '@ngx-translate/core';
import {DetailTableRowComponent} from './detail-table-row/detail-table-row.component';
import {MultilineTextComponent} from './multiline-text/multiline-text.component';
import {RichTextPanelComponent} from './rich-text-panel/rich-text-panel.component';
import {SearchInputComponent} from './search/search-input/search-input.component';
import {SafeHtmlPipe} from './safe-html/safe-html.pipe';
import {FallbackArrayToStringPipe} from './fallback/fallback-array-to-string.pipe';
import {FallbackPipe} from './fallback/fallback.pipe';
import {FilterMultiSelectDropdownComponent} from './search-filters/filter-multiselect-dropdown/filter-multiselect-dropdown.component';
import {SearchFiltersComponent} from './search-filters/search-filters.component';
import {PublicationLevelClassPipe} from './chip-classes-pipes/publication-level-class.pipe';
import {RegistrationStatusClassPipe} from './chip-classes-pipes/registration-status-class.pipe';
import {ArrayToStringPipe} from './formating/array-to-string.pipe';
import {InfoboxNoContentComponent} from './infobox-no-content/infobox-no-content.component';
import {DetailViewTemplateComponent} from './templates/detail/detail.view.template.component';
import {DescriptionViewTemplateComponent} from './templates/description/description.view.template.component';
import {RouterModule} from '@angular/router';
import {ResourceModelComponent} from './resourcemodel/resourcemodel.component';
import {ExtendedFallbackPipe} from './fallback/extendedfallback.pipe';
import {AccessRightsClassPipe} from './accessright-pipes/accessrights-class-pipe';
import {AccessRightsIconPipe} from './accessright-pipes/accessrights-icon-pipe';
import {AccessRightsTempClassFromTextPipe} from './accessright-pipes/accessrights-temp-class-from-text-pipe';
import {AccessRightsTempIconFromTextPipe} from './accessright-pipes/accessrights-temp-icon-from-text-pipe';
import {MultilineRichTextComponent} from './multiline-rich-text/multiline-rich-text.component';
import {TextToLinkPipe} from './text-to-link/text-to-link.pipe';
import {OffCanvasTemplateComponent} from './templates/off-canvas/off-canvas-template.component';
import {FilterItemComponent} from './content-filter/filter-item/filter-item.component';
import {VocabularyEntryListComponent} from './templates/description/vocabulary-entry-list/vocabulary-entry-list.component';
import {MatAutocompleteModule} from '@angular/material/autocomplete';

const MODULES = [
	AngularSplitModule,
	ClipboardModule,
	CommonModule,
	FormsModule,
	MatAutocompleteModule,
	MatButtonModule,
	MatCardModule,
	MatCheckboxModule,
	MatChipsModule,
	MatExpansionModule,
	MatFormFieldModule,
	MatIconModule,
	MatInputModule,
	MatMenuModule,
	MatPaginatorModule,
	MatProgressSpinnerModule,
	MatRadioModule,
	MatRippleModule,
	MatSelectModule,
	MatSortModule,
	MatTableModule,
	MatTabsModule,
	MatTooltipModule,
	MatTreeModule,
	MatListModule,
	ObliqueModule,
	ReactiveFormsModule,
	TranslateModule
];

const COMPONENTS = [
	DetailTableRowComponent,
	DetailViewTemplateComponent,
	DescriptionViewTemplateComponent,
	FilterItemComponent,
	FilterMultiSelectDropdownComponent,
	InfoboxNoContentComponent,
	MultilineRichTextComponent,
	MultilineTextComponent,
	OffCanvasTemplateComponent,
	ResourceModelComponent,
	RichTextPanelComponent,
	SearchFiltersComponent,
	SearchInputComponent,
	VocabularyEntryListComponent
];

const PIPES = [
	AccessRightsClassPipe,
	AccessRightsIconPipe,
	AccessRightsTempClassFromTextPipe,
	AccessRightsTempIconFromTextPipe,
	ArrayToStringPipe,
	ExtendedFallbackPipe,
	FallbackPipe,
	FallbackArrayToStringPipe,
	PublicationLevelClassPipe,
	RegistrationStatusClassPipe,
	SafeHtmlPipe,
	TextToLinkPipe
];

@NgModule({
	declarations: [...COMPONENTS, ...PIPES],
	imports: [...MODULES, RouterModule],
	exports: [...COMPONENTS, ...MODULES, ...PIPES],
	providers: [...PIPES, DateFormatService, FileSizeFormatService, SearchEngineOptimizationService]
})
export class SharedModule {}
