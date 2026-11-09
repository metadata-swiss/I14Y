import {Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges} from '@angular/core';
import {MultiLanguageModel, SchemaClass, SchemaProperty} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';
import {UriHelper} from '../../../../shared/helper/uri-helper';

@Component({
	selector: 'app-structure-graph-sidebar',
	templateUrl: './structure-graph-sidebar.component.html',
	styleUrl: './structure-graph-sidebar.component.scss',
	standalone: false
})
export class StructureGraphSidebarComponent implements OnChanges, OnInit, OnDestroy {
	@Input() selectedClass: SchemaClass | undefined;
	@Input() selectedProperty: SchemaProperty | undefined;
	@Input() isPropertySelected: boolean | undefined;
	type: string | undefined;
	uri: string | undefined;
	name: MultiLanguageModel | undefined;
	description: MultiLanguageModel | undefined;
	identifier: string | undefined;
	conformsToUris: string[] = [];
	dataType: string | undefined;
	pattern: string | undefined;
	unit: string | undefined;
	minCount: number | undefined;
	maxCount: number | undefined;
	minLength: number | undefined;
	maxLength: number | undefined;
	uriName: string | undefined;
	allowedValues: string[] | undefined;

	showAllAllowedValues = false;
	currentLanguage: string;

	private readonly unsubscribe$ = new Subject<void>();

	get displayedAllowedValues(): string[] | undefined {
		if (this.allowedValues && this.allowedValues.length > 0) {
			return this.showAllAllowedValues ? this.allowedValues : this.allowedValues.slice(0, 10);
		}
		return undefined;
	}

	constructor(private readonly translate: TranslateService) {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnDestroy(): void {
		this.unsubscribe$.next();
		this.unsubscribe$.complete();
	}

	ngOnChanges(changes: SimpleChanges): void {
		//change the property
		if (changes.selectedProperty && changes.selectedProperty.previousValue !== changes.selectedProperty.currentValue && this.isPropertySelected) {
			this.convertPropertyToValueSideBar(changes.selectedProperty.currentValue);
			// change the between class
		} else if (changes.selectedClass && changes.selectedClass.previousValue !== changes.selectedClass.currentValue && !this.isPropertySelected) {
			this.convertClassToValueSideBar(changes.selectedClass.currentValue);
			// change the selection from property to class of same class
		} else if (changes.isPropertySelected && changes.isPropertySelected.currentValue === false && this.selectedClass) {
			this.convertClassToValueSideBar(this.selectedClass);
			// change the selection from class to property of the same class
		} else if (changes.isPropertySelected && changes.isPropertySelected.currentValue === true && this.selectedProperty) {
			this.convertPropertyToValueSideBar(this.selectedProperty);
		}
	}

	convertClassToValueSideBar(schemaClass: SchemaClass) {
		this.clean();
		this.type = 'class';
		this.identifier = schemaClass?.identifier;
		this.uri = schemaClass?.uriComplete;
		this.name = schemaClass?.label;
		this.description = this.selectedClass?.description;
		this.uriName = UriHelper.GetUriFragment(schemaClass?.uriComplete);
	}

	convertPropertyToValueSideBar(schemaProperty: SchemaProperty) {
		this.clean();
		this.type = schemaProperty.toClassUri && schemaProperty.toClassUri.length > 0 ? 'association' : 'attribute';
		this.uri = schemaProperty?.uriComplete;
		this.identifier = schemaProperty?.identifier;
		this.name = schemaProperty?.label;
		this.description = schemaProperty?.description;
		this.conformsToUris = schemaProperty?.conformsTo ?? [];
		this.dataType = schemaProperty?.dataType;
		this.pattern = schemaProperty?.pattern;
		this.unit = schemaProperty?.unit;
		this.minCount = schemaProperty?.minCardinality;
		this.maxCount = schemaProperty?.maxCardinality;
		this.minLength = schemaProperty?.minLength;
		this.maxLength = schemaProperty?.maxLength;
		this.uriName = UriHelper.GetUriFragment(schemaProperty?.path);
		this.allowedValues = schemaProperty.allowedValues;
	}

	clean() {
		this.type = undefined;
		this.uri = undefined;
		this.name = undefined;
		this.description = undefined;
		this.identifier = undefined;
		this.conformsToUris = [];
		this.dataType = undefined;
		this.pattern = undefined;
		this.minCount = undefined;
		this.maxCount = undefined;
		this.minLength = undefined;
		this.maxLength = undefined;
	}
}
