import {Component, EventEmitter, inject, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges} from '@angular/core';
import {AbstractControl, UntypedFormControl} from '@angular/forms';
import {AllowActionType, DcatCatalogInputClient, DcatCatalogRecordInput, DcatVocabularyEntry, MultiLanguage} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {map, Observable, of, startWith, Subject, takeUntil} from 'rxjs';
import {AllowActionService} from 'src/app/services/allow.action.service';
import {ThemeTaxonomiesService} from 'src/app/services/theme-taxonomies.service';
import {ThemeSelection} from '../theme-taxonomies-select/theme-selection';

interface CatalogueTaxonomyRow {
	identifier: string;
	name?: MultiLanguage;
	entries: DcatVocabularyEntry[];
	control: UntypedFormControl;
}

/** Edits catalogue vocabularies through the resource's shared themes control. */
@Component({
	selector: 'app-themes-select',
	templateUrl: './themes-select.component.html',
	styleUrl: './themes-select.component.scss',
	standalone: false
})
export class ThemesSelectComponent implements OnInit, OnChanges, OnDestroy {
	@Input() catalog: DcatCatalogRecordInput = new DcatCatalogRecordInput();
	@Input({required: true}) control!: AbstractControl;
	/** Vocabularies excluded from the separate taxonomy picker. */
	@Output() vocabulariesChange = new EventEmitter<string[]>();

	cannotEdit$: Observable<boolean> = of<boolean>(true);
	rows: CatalogueTaxonomyRow[] = [];
	currentLanguage: string;

	private writingBack = false;
	private cannotEdit = true;

	private readonly unsubscribe$ = new Subject();
	private readonly allowActionService = inject(AllowActionService);
	private readonly dcatCatalogInputClient = inject(DcatCatalogInputClient);
	private readonly themeTaxonomiesService = inject(ThemeTaxonomiesService);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	private get selection(): ThemeSelection {
		return new ThemeSelection(this.control);
	}

	// Disable through the row controls to avoid competing [disabled] and [formControl] bindings.
	private get isDisabled(): boolean {
		return this.cannotEdit || !!this.control?.disabled;
	}

	ngOnInit() {
		this.cannotEdit$ = this.allowActionService.childAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.actionType === AllowActionType.Edit)?.value),
			startWith(true)
		);

		this.cannotEdit$.pipe(takeUntil(this.unsubscribe$)).subscribe(cannotEdit => {
			this.cannotEdit = cannotEdit;
			this.syncDisabled();
		});

		this.control?.valueChanges.pipe(takeUntil(this.unsubscribe$)).subscribe(() => {
			if (!this.writingBack) {
				this.fillRows();
			}
		});

		this.control?.statusChanges.pipe(takeUntil(this.unsubscribe$)).subscribe(() => this.syncDisabled());
	}

	ngOnChanges(changes: SimpleChanges): void {
		const catalogId = changes.catalog?.currentValue?.catalogId;

		if (catalogId) {
			this.dcatCatalogInputClient.getThemesById(catalogId).subscribe(response => {
				this.buildRows(response.result ?? []);
			});
		}
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	private buildRows(entries: DcatVocabularyEntry[]): void {
		const identifiers = [...new Set(entries.map(entry => entry.themeTaxonomy).filter((x): x is string => !!x))];

		this.rows = identifiers.map(identifier => {
			const control = new UntypedFormControl({value: [], disabled: this.isDisabled});

			const row: CatalogueTaxonomyRow = {identifier, entries: entries.filter(entry => entry.themeTaxonomy === identifier), control};

			control.valueChanges.pipe(takeUntil(this.unsubscribe$)).subscribe(() => this.writeBack(row));

			this.themeTaxonomiesService
				.nameOf(identifier)
				.pipe(takeUntil(this.unsubscribe$))
				.subscribe(name => (row.name = name));

			return row;
		});

		this.fillRows();
		this.vocabulariesChange.emit(identifiers);
	}

	private syncDisabled(): void {
		for (const row of this.rows) {
			if (this.isDisabled !== row.control.disabled) {
				if (this.isDisabled) {
					row.control.disable({emitEvent: false});
				} else {
					row.control.enable({emitEvent: false});
				}
			}
		}
	}

	private fillRows(): void {
		for (const row of this.rows) {
			row.control.setValue(this.selection.codesOf(row.identifier), {emitEvent: false});
		}
	}

	private writeBack(row: CatalogueTaxonomyRow): void {
		if (this.isDisabled) {
			return;
		}

		this.writingBack = true;
		this.selection.replace(row.identifier, (row.control.value ?? []) as string[], row.entries);
		this.writingBack = false;
	}
}
