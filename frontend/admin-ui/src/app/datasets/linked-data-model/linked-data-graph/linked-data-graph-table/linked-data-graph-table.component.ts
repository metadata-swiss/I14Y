import {Component, EventEmitter, Input, Output, OnInit, inject, model} from '@angular/core';
import {ISchemaClass, SchemaClass, SchemaProperty} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {EFConnectableSide} from '@foblex/flow';
import {Subject, takeUntil} from 'rxjs';
import {UriHelper} from '../../../../shared/helper/uri-helper';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import { INode } from '../../linked-data-entity';
@Component({
	selector: 'app-linked-data-graph-table',
	templateUrl: './linked-data-graph-table.component.html',
	styleUrl: './linked-data-graph-table.component.scss',
	standalone: false
})
export class LinkedDataGraphTableComponent implements OnInit {
	@Input({required: true}) public viewTable!: SchemaClass;
	@Output() classSelected: EventEmitter<SchemaClass> = new EventEmitter();
	@Output() propertySelected = new EventEmitter<{property: SchemaProperty; classUri: string}>();
	public outputSide: EFConnectableSide = EFConnectableSide.RIGHT;
	public inputSide: EFConnectableSide = EFConnectableSide.LEFT;
	isEditMode = model(false);
	currentLanguage: string;

	private readonly unsubscribe$ = new Subject();
	private readonly translate = inject(TranslateService);
	private readonly fallback = inject(FallbackPipe);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	get viewTableLabel(): string {
		if (!this.viewTable) return '';
		return this.fallback.transform(this.viewTable.label, this.currentLanguage) || this.uriFragment(this.viewTable.uriComplete);
	}

	getPropertyLabel(property: any): string {
		if (!this.viewTable) return '';

		// Use your fallback pipe/service
		const label = this.fallback.transform(property.label, this.currentLanguage);
		return label || this.uriFragment(property.path);
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	selectClass() {
		this.classSelected.emit(this.viewTable);
	}

	uriFragment(uri: string | undefined): string {
		return UriHelper.GetUriFragment(uri);
	}

	removeHash(value: string | undefined): string | undefined {
		return UriHelper.RemoveHash(value);
	}

	getUniquePath(schemaClass: SchemaClass, property: SchemaProperty | undefined): string {
		return property?.uriComplete?? UriHelper.completePathUriForUnique(schemaClass?.uriComplete!, property?.path ?? '');
	}

	selectProperty(property: SchemaProperty, classUri: string) {
		this.propertySelected.emit({
			property,
			classUri
		});
	}
}
