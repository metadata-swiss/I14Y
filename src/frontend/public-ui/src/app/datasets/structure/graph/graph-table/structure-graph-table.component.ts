import {Component, EventEmitter, Input, Output, OnInit} from '@angular/core';
import {SchemaClass, SchemaProperty} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {EFConnectableSide} from '@foblex/flow';
import {Subject, takeUntil} from 'rxjs';
import {UriHelper} from '../../../../shared/helper/uri-helper';

@Component({
	selector: 'app-structure-graph-table',
	templateUrl: './structure-graph-table.component.html',
	styleUrl: './structure-graph-table.component.scss',
	standalone: false
})
export class StructureGraphTableComponent implements OnInit {
	@Input({required: true}) public viewTable!: SchemaClass;
	@Input() selectedProperty: SchemaProperty | undefined;
	@Output() classSelected: EventEmitter<SchemaClass> = new EventEmitter();
	@Output() propertySelected = new EventEmitter<SchemaProperty>();
	public outputSide: EFConnectableSide = EFConnectableSide.RIGHT;
	public inputSide: EFConnectableSide = EFConnectableSide.LEFT;
	currentLanguage: string;

	private readonly unsubscribe$ = new Subject();

	constructor(private readonly translate: TranslateService) {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	selectClass() {
		this.classSelected.emit(this.viewTable);
	}

	selectProperty(property: SchemaProperty) {
		this.propertySelected.emit(property);
	}

	uriFragment(uri: string | undefined): string {
		return UriHelper.GetUriFragment(uri);
	}

	removeHash(value: string | undefined): string | undefined {
		return UriHelper.RemoveHash(value);
	}

	isPropertySelected(property: SchemaProperty): boolean {
		return this.selectedProperty === property;
	}

	completePathUriForUnique(schemaClass: SchemaClass, property: SchemaProperty | undefined): string {
		return UriHelper.completePathUriForUnique(schemaClass?.uriComplete!, property?.path ?? '');
	}
}
