import {Component, EventEmitter, inject, Input, Output, ViewChild, ElementRef} from '@angular/core';
import {CatalogEntry, SchemaClass, SchemaProperty} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService} from '@ngx-translate/core';
import {Languages} from '../../../../../../shared/ApplicationLanguage.enum';
import {FormGroup} from '@angular/forms';
import {FallbackPipe} from '../../../../../../shared/fallback/fallback.pipe';
import {buildConceptIri} from 'src/app/shared/helper/iri-helpers';
import {MatAutocompleteSelectedEvent} from '@angular/material/autocomplete';

@Component({
	selector: 'app-structure-detail-edit-form',
	standalone: false,
	templateUrl: './structure-detail-edit-form.component.html',
	styleUrl: './structure-detail-edit-form.component.scss'
})
export class StructureDetailEditFormComponent {
	@Input({required: true}) dto!: SchemaClass | SchemaProperty;
	@Input({required: true}) form!: FormGroup;
	@Input() autoCompletItems: CatalogEntry[] = [];
	@Input() hasMore = true;
	@Output() cancel: EventEmitter<void> = new EventEmitter();
	@Output() saveAndClose: EventEmitter<void> = new EventEmitter();
	@Output() save: EventEmitter<void> = new EventEmitter();
	@Output() searchAutoComplete: EventEmitter<string | null> = new EventEmitter();
	@Output() loadMore = new EventEmitter<void>();

	@ViewChild('inputRef') inputRef!: ElementRef;

	TRANSLATION_PREFIX = 'i18n.datasets.linkeddatamodel.sidebar';
	currentLanguage: string;
	showAllLanguages = false;
	contentLanguages: readonly string[] = Languages.ContentLanguages;
	autoCompletSelectedValue: string | undefined;
	uri: string | undefined;

	private readonly fallback = inject(FallbackPipe);
	private readonly translate = inject(TranslateService);
	private boundScroll = this.onScroll.bind(this);
	private scrollAttached = false;
	private panelElement: HTMLElement | null = null;

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.translate.onLangChange.subscribe(language => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnInit() {
		const conformsTo = this.form.get('conformsTo')?.value;
		if (conformsTo) {
			this.autoCompletSelectedValue = conformsTo;
		}
		const uri = this.form.get('uri')?.value;
		if (uri) {
			this.uri = uri;
		}

		const identifierControl = this.form.get('identifier');
		if (!this.isSchemaClassIdentifierLocked && identifierControl && typeof this.uri === 'string') {
			const uriPrefix = this.uri.endsWith('/') ? this.uri : this.uri.substring(0, this.uri.lastIndexOf('/') + 1);
			identifierControl.valueChanges.subscribe(identifier => {
				const completUri = `${uriPrefix}${identifier ?? ''}`;
				this.uri = completUri;
				this.form.get('uri')?.setValue(completUri, {emitEvent: false});
			});
		}
	}

	get isSchemaClassIdentifierLocked(): boolean {
		return this.dto instanceof SchemaClass && (this.dto.properties?.length ?? 0) > 0;
	}

	ngOnChanges(changes: any) {
		if (changes.autoCompletItems) {
			this.autoCompletItems = changes.autoCompletItems.currentValue;
		}
	}

	onOpenedAutoComplete(): void {
		if (this.scrollAttached) return;

		setTimeout(() => {
			const panel = document.querySelector('.mat-mdc-autocomplete-panel');

			if (panel) {
				// Get the actual scrollable element (usually a div inside)
				const scrollableContent = panel.querySelector('.mat-mdc-autocomplete-panel-body') || panel.querySelector('[role="listbox"]') || panel;

				if (scrollableContent) {
					this.panelElement = scrollableContent as HTMLElement;
					this.panelElement.addEventListener('scroll', this.boundScroll);
					this.scrollAttached = true;
				}
			}
		}, 150);

		this.searchAutoComplete.emit(null);
	}

	onSearch(query: string) {
		this.searchAutoComplete.emit(query);
	}

	onDeleteConformsTo() {
		this.autoCompletSelectedValue = undefined;
		this.form.get('conformsTo')?.setValue(null);
		if (this.inputRef) {
			this.inputRef.nativeElement.value = '';
		}
	}

	onScroll(event: any) {
		const panel = event.target;
		const atBottom = panel.scrollHeight - panel.scrollTop <= panel.clientHeight + 10;

		if (atBottom) {
			this.loadMore.emit();
		}
	}

	onSaveAndClose(): void {
		this.saveAndClose.emit();
	}

	onSave(): void {
		this.save.emit();
	}

	onCancel(): void {
		this.cancel.emit();
	}

	displayFn = (item: CatalogEntry): string => {
		if (!item) return '';
		return this.getConformsToViewTitle(item);
	};

	onSelect(event: MatAutocompleteSelectedEvent) {
		const iri = buildConceptIri(event.option.value.identifiers?.[0] ?? '', event.option.value.version ?? '');
		this.form.get('conformsTo')?.setValue(iri);
		this.autoCompletSelectedValue = iri;
	}

	getConformsToViewTitle(item: CatalogEntry): string {
		let identifier = item.identifiers?.[0] ?? '';
		let version = item.version ? item.version : '';
		let name = this.fallback.transform(item.title, this.currentLanguage) ?? '';
		return identifier + ' ' + version + ' | ' + name;
	}

	private getPropertyUriPrefix(uri: string, identifier: string): string | undefined {
		if (uri.endsWith('/') || uri.endsWith('#')) {
			return uri;
		}

		if (identifier.length > 0 && uri.endsWith(identifier)) {
			return uri.substring(0, uri.length - identifier.length);
		}

		return undefined;
	}
}
