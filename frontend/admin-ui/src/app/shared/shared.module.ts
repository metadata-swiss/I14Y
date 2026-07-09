import {ArrayToStringPipe} from './formatting/array-to-string.pipe';
import {CodevalueTableComponent} from './codevalue-table/codevalue-table.component';
import {CommonModule} from '@angular/common';
import {DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE, MatNativeDateModule} from '@angular/material/core';
import {DialogComponent} from './dialog/dialog.component';
import {EditTableCodelistComponent} from './edit-table-codelist/edit-table-codelist.component';
import {EditTableDataserviceLinkComponent} from './edit-table-dataservice-link/edit-table-dataservice-link.component';
import {EditTableKeywordsComponent} from './edit-table-keywords/edit-table-keywords.component';
import {EditTableResourceComponent} from './edit-table-resource/edit-table-resource.component';
import {EditTableResourceModelComponent} from './edit-table-resource-model/edit-table-resource-model.component';
import {EditConceptReferencesComponent} from './edit-concept-references/edit-concept-references.component';
import {FallbackPipe} from './fallback/fallback.pipe';
import {FallbackArrayToStringPipe} from './fallback/fallback-array-to-string.pipe';
import {FileSizePipe} from './formatting/file-size.pipe';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {IsInvalidAndUntouchedPipe} from './validators/is-invalid-and-untouched.pipe';
import {MAT_FORM_FIELD_DEFAULT_OPTIONS, MatFormFieldModule} from '@angular/material/form-field';
import {MatAutocompleteModule} from '@angular/material/autocomplete';
import {MatButtonModule} from '@angular/material/button';
import {MatCardModule} from '@angular/material/card';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatChipsModule} from '@angular/material/chips';
import {MatDialogModule} from '@angular/material/dialog';
import {MatExpansionModule} from '@angular/material/expansion';
import {MatInputModule} from '@angular/material/input';
import {MatIconModule} from '@angular/material/icon';
import {MatDatepickerModule} from '@angular/material/datepicker';
import {MatMenuModule} from '@angular/material/menu';
import {MatMomentDateModule, MomentDateAdapter} from '@angular/material-moment-adapter';
import {MatPaginatorModule} from '@angular/material/paginator';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatRadioModule} from '@angular/material/radio';
import {MatSelectModule} from '@angular/material/select';
import {MatSortModule} from '@angular/material/sort';
import {MatTableModule} from '@angular/material/table';
import {MatTabsModule} from '@angular/material/tabs';
import {MatTooltipModule} from '@angular/material/tooltip';
import {ModalDialogComponent} from './modal-dialog/modal-dialog.component';
import {MultilineTextComponent} from './multiline-text/multiline-text.component';
import {NgModule} from '@angular/core';
import {ObliqueModule} from '@oblique/oblique';
import {PersonSearchPipe} from './person-search/person-search.pipe';
import {RouterModule} from '@angular/router';
import {TranslateModule} from '@ngx-translate/core';
import {InfoboxNoContentComponent} from './infobox-no-content/infobox-no-content.component';
import {EditTableDatasetLinkComponent} from './edit-table-dataset-link/edit-table-dataset-link.component';
import {TableAnnotationComponent} from './table-annotation/table-annotation.component';
import {EditTableAnnotationComponent} from './edit-table-annotation/edit-table-annotation.component';
import {ModalDialogAnnotationComponent} from './edit-table-annotation/modal-dialog/modal-dialog.component';
import {StatusComponent} from './status/status.component';
import {EditTableChannelLinkComponent} from './edit-table-channel-link/edit-table-channel-link.component';
import {EditTableQualifiedAttributionComponent} from './edit-table-qualified-attribution/edit-table-qualified-attribution.component';
import {EditTableQualifiedRelationComponent} from './edit-table-qualified-relation/edit-table-qualified-relation.component';
import {ConceptRelationTableComponent} from './concept-relation-table/concept-relation-table.component';
import {SearchFiltersComponent} from './search-filters/search-filters.component';
import {FilterMultiSelectDropdownComponent} from './search-filters/filter-multiselect-dropdown/filter-multiselect-dropdown.component';
import {ModalDialogCodeListComponent} from './edit-table-codelist/modal-dialog/modal-dialog.component';
import {MatSlideToggleModule} from '@angular/material/slide-toggle';
import {PublicationLevelClassPipe} from './chip-classes-pipes/publication-level-class.pipe';
import {RegistrationStatusClassPipe} from './chip-classes-pipes/registration-status-class.pipe';
import {DetailViewTemplateComponent} from './templates/view-templates/detail/detail.view.template.component';
import {DescriptionViewTemplateComponent} from './templates/view-templates/description/description.view.template.component';
import {ExtendedFallbackPipe} from './fallback/extendedfallback.pipe';
import {AccessRightsClassPipe} from './accessright-pipes/accessrights-class-pipe';
import {AccessRightsIconPipe} from './accessright-pipes/accessrights-icon-pipe';
import {ImportDialogComponent} from './importdialog/importdialog.component';
import {InfoboxRequiredComponent} from './infobox-required/infobox-required.component';
import {EditIdentifierComponent} from './edit-identifier/edit-identifier.component';
import {MultilineRichTextComponent} from './multiline-rich-text/multiline-rich-text.component';
import {TextToLinkPipe} from './text-to-link/text-to-link.pipe';
import {SafeHtmlPipe} from './safe-html/safe-html.pipe';
import {ThemesSelectComponent} from './themes-select/themes-select.component';
import {ClipboardModule} from '@angular/cdk/clipboard';
import {VocabularyEntryListComponent} from './templates/view-templates/vocabulary-entry-list/vocabulary-entry-list.component';
import {VocabularyEntryMultiSelectDropdownComponent} from './vocabularyentry-multiselect-dropdown/vocabularyentry-multiselect-dropdown.component';
import {BackButtonComponent} from './navigation/back-button.component';
import {ConceptLinkComponent} from './concept-link/concept-link.component';

const MODULES = [
	CommonModule,
	MatAutocompleteModule,
	MatButtonModule,
	MatCardModule,
	MatCheckboxModule,
	MatChipsModule,
	MatDatepickerModule,
	MatDialogModule,
	MatExpansionModule,
	MatFormFieldModule,
	MatIconModule,
	MatInputModule,
	MatMenuModule,
	MatMomentDateModule,
	MatNativeDateModule,
	MatPaginatorModule,
	MatProgressSpinnerModule,
	MatRadioModule,
	MatSelectModule,
	MatSlideToggleModule,
	MatSortModule,
	MatTableModule,
	MatTabsModule,
	MatTooltipModule,
	FormsModule,
	ObliqueModule,
	ReactiveFormsModule,
	TranslateModule,
	ClipboardModule
];

const COMPONENTS = [
	BackButtonComponent,
	CodevalueTableComponent,
	ConceptLinkComponent,
	ConceptRelationTableComponent,
	DetailViewTemplateComponent,
	DescriptionViewTemplateComponent,
	DialogComponent,
	EditTableCodelistComponent,
	EditTableDatasetLinkComponent,
	EditTableKeywordsComponent,
	EditTableResourceComponent,
	EditTableResourceModelComponent,
	EditConceptReferencesComponent,
	FilterMultiSelectDropdownComponent,
	ModalDialogComponent,
	MultilineRichTextComponent,
	MultilineTextComponent,
	ImportDialogComponent,
	InfoboxNoContentComponent,
	InfoboxRequiredComponent,
	EditTableDataserviceLinkComponent,
	EditTableChannelLinkComponent,
	EditTableQualifiedAttributionComponent,
	EditTableQualifiedRelationComponent,
	TableAnnotationComponent,
	ThemesSelectComponent,
	EditTableAnnotationComponent,
	ModalDialogAnnotationComponent,
	ModalDialogCodeListComponent,
	SearchFiltersComponent,
	StatusComponent,
	EditIdentifierComponent,
	VocabularyEntryListComponent,
	VocabularyEntryMultiSelectDropdownComponent
];
const PIPES = [
	AccessRightsClassPipe,
	AccessRightsIconPipe,
	ArrayToStringPipe,
	ExtendedFallbackPipe,
	FallbackPipe,
	FallbackArrayToStringPipe,
	FileSizePipe,
	IsInvalidAndUntouchedPipe,
	PersonSearchPipe,
	PublicationLevelClassPipe,
	RegistrationStatusClassPipe,
	SafeHtmlPipe,
	TextToLinkPipe
];

@NgModule({
	declarations: [...COMPONENTS, ...PIPES],
	imports: [...MODULES, RouterModule],
	exports: [...COMPONENTS, ...PIPES, ...MODULES],
	providers: [
		...PIPES,
		{
			provide: MAT_FORM_FIELD_DEFAULT_OPTIONS,
			useValue: {appearance: 'outline'}
		},
		{
			provide: DateAdapter,
			useClass: MomentDateAdapter,
			deps: [MAT_DATE_LOCALE]
		},
		{
			provide: MAT_DATE_FORMATS,
			useValue: {
				parse: {
					dateInput: 'DD.MM.YYYY'
				},
				display: {
					dateInput: 'DD.MM.YYYY',
					monthYearLabel: 'MMM YYYY',
					dateA11yLabel: 'LL',
					monthYearA11yLabel: 'MMMM YYYY'
				}
			}
		}
	]
})
export class SharedModule {}
