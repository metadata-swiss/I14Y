import {Input, Output, EventEmitter, Component, inject, OnDestroy, OnInit} from '@angular/core';
import {SchemaClass, SchemaGraph, SchemaProperty} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';
import {UriHelper} from 'src/app/shared/helper/uri-helper';

@Component({
	selector: 'app-linked-data-class-table',
	templateUrl: './linked-data-class-table.component.html',
	standalone: false
})
export class LinkedDataClassTableComponent implements OnInit, OnDestroy {
	@Input() classUri: string | undefined;
	@Input() dataSource: SchemaProperty[] = [];
	@Input() schemaGraph: SchemaGraph | undefined;
	@Output() propertySelected = new EventEmitter<{property: SchemaProperty; classUri: string}>();
	@Output() classSelected = new EventEmitter<SchemaClass>();

	TRANSLATION_PREFIX = 'i18n.datasets.linkeddatamodel.sidebar';
	currentLanguage: string;

	readonly COLUMN_IDENTIFIER = 'identifier';
	readonly COLUMN_LABEL = 'label';
	readonly COLUMN_DATA_TYPE = 'data_type';
	readonly COLUMN_DESCRIPTION = 'description';
	readonly COLUMN_CONFORMS_TO = 'conforms_to';
	readonly COLUMN_ASSOCIATION_TARGET = 'association_target';
	readonly COLUMN_ACTIONS = 'actions';

	displayedColumns: string[] = [
		this.COLUMN_IDENTIFIER,
		this.COLUMN_LABEL,
		this.COLUMN_DATA_TYPE,
		this.COLUMN_DESCRIPTION,
		this.COLUMN_CONFORMS_TO,
		this.COLUMN_ASSOCIATION_TARGET,
		this.COLUMN_ACTIONS
	];

	private readonly unsubscribe$ = new Subject<void>();

	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next();
		this.unsubscribe$.complete();
	}

	selectClass(selectedClass: SchemaClass): void {
		this.classSelected.emit(selectedClass);
	}

	selectProperty(selectedProperty: SchemaProperty): void {
		this.propertySelected.emit({
			property: selectedProperty,
			classUri: selectedProperty.uriComplete ?? UriHelper.completePathUriForUnique(this.classUri ?? '', selectedProperty?.path ?? '')
		});
	}

	getIdentifier(element: SchemaProperty | SchemaClass): string {
		if (element instanceof SchemaClass) {
			return UriHelper.GetUriFragment(element.uriComplete ?? '');
		}
		return UriHelper.GetUriFragment(element.uriComplete ?? '') ?? element?.path ?? '';
	}

	getTargetClass(toClassUri: string): SchemaClass | undefined {
		const identifier = UriHelper.GetUriFragment(toClassUri);

		return this.schemaGraph?.classes?.find(schemaClass => UriHelper.RemoveHash(schemaClass.targetClass) === identifier);
	}
}
