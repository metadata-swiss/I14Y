import {Component, inject, Input, OnChanges, OnDestroy, OnInit, SimpleChanges} from '@angular/core';
import {AbstractControl, UntypedFormControl} from '@angular/forms';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';
import {VOCAB_ID_EU_DATA_THEME} from 'src/app/app-constants';
import {ThemeTaxonomiesService, ThemeTaxonomy} from 'src/app/services/theme-taxonomies.service';
import {ThemeSelection} from './theme-selection';

interface ThemeTaxonomyRow {
	taxonomy: ThemeTaxonomy;
	control: UntypedFormControl;
	removable: boolean;
}

@Component({
	selector: 'app-theme-taxonomies-select',
	templateUrl: './theme-taxonomies-select.component.html',
	styleUrl: './theme-taxonomies-select.component.scss',
	standalone: false
})
export class ThemeTaxonomiesSelectComponent implements OnChanges, OnInit, OnDestroy {
	@Input({required: true}) control!: AbstractControl;
	@Input() idPrefix = 'themes';
	/** Vocabularies already edited by linked catalogues. */
	@Input() handledElsewhere: string[] = [];
	@Input() disabled = false;

	rows: ThemeTaxonomyRow[] = [];
	currentLanguage: string;

	private taxonomies: ThemeTaxonomy[] = [];
	// Keep newly added rows when the control changes before any theme is selected.
	private readonly opened = new Set<string>();
	private writingBack = false;

	private readonly unsubscribe$ = new Subject();
	private readonly themeTaxonomiesService = inject(ThemeTaxonomiesService);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => (this.currentLanguage = language.lang));
	}

	// Avoid multiple editors overwriting the same vocabulary.
	get addableTaxonomies(): ThemeTaxonomy[] {
		const handled = new Set(this.handledElsewhere);

		return this.taxonomies.filter(taxonomy => !handled.has(taxonomy.identifier) && !this.rows.some(row => row.taxonomy.identifier === taxonomy.identifier));
	}

	get isDisabled(): boolean {
		return this.disabled || !!this.control?.disabled;
	}

	private get selection(): ThemeSelection {
		return new ThemeSelection(this.control);
	}

	ngOnInit(): void {
		this.themeTaxonomiesService.taxonomies$.pipe(takeUntil(this.unsubscribe$)).subscribe(taxonomies => {
			this.taxonomies = taxonomies;
			this.buildRows();
		});

		this.control?.statusChanges.pipe(takeUntil(this.unsubscribe$)).subscribe(() => {
			if (!this.writingBack) {
				this.buildRows();
			}
		});

		this.control?.valueChanges.pipe(takeUntil(this.unsubscribe$)).subscribe(() => {
			if (!this.writingBack) {
				this.buildRows();
			}
		});
	}

	ngOnChanges(changes: SimpleChanges): void {
		if (changes.handledElsewhere && !changes.handledElsewhere.firstChange) {
			this.buildRows();
		}
	}

	ngOnDestroy(): void {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	addTaxonomy(taxonomy: ThemeTaxonomy): void {
		if (this.isDisabled) {
			return;
		}

		this.opened.add(taxonomy.identifier);
		// Preserve existing themes when adding an editor for this vocabulary.
		this.rows = [...this.rows, this.createRow(taxonomy, this.selection.codesOf(taxonomy.identifier))];
	}

	removeTaxonomy(row: ThemeTaxonomyRow): void {
		if (!row.removable || this.isDisabled) {
			return;
		}

		this.opened.delete(row.taxonomy.identifier);
		this.rows = this.rows.filter(x => x !== row);
		this.inWriteBack(() => this.selection.remove(row.taxonomy.identifier));
	}

	private buildRows(): void {
		if (this.taxonomies.length === 0) {
			return;
		}

		const selection = this.selection;
		const present = new Set(selection.vocabularies());
		const handled = new Set(this.handledElsewhere);

		// Always show EU themes, even when a catalogue also uses this vocabulary.
		const shown = this.taxonomies.filter(
			taxonomy =>
				taxonomy.identifier === VOCAB_ID_EU_DATA_THEME ||
				(!handled.has(taxonomy.identifier) && (present.has(taxonomy.identifier) || this.opened.has(taxonomy.identifier)))
		);

		this.rows = shown.map(taxonomy => this.createRow(taxonomy, selection.codesOf(taxonomy.identifier)));

		const known = new Set([...this.taxonomies.map(taxonomy => taxonomy.identifier), ...handled]);

		for (const identifier of [...present].filter(x => !known.has(x))) {
			this.themeTaxonomiesService
				.taxonomyFor(identifier)
				.pipe(takeUntil(this.unsubscribe$))
				.subscribe(taxonomy => {
					if (!this.rows.some(row => row.taxonomy.identifier === identifier)) {
						this.rows = [...this.rows, this.createRow(taxonomy, this.selection.codesOf(identifier))];
					}
				});
		}
	}

	private createRow(taxonomy: ThemeTaxonomy, codes: string[]): ThemeTaxonomyRow {
		const control = new UntypedFormControl({value: codes, disabled: this.isDisabled});

		control.valueChanges
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(value => this.inWriteBack(() => this.selection.replace(taxonomy.identifier, (value ?? []) as string[], taxonomy.entries)));

		return {taxonomy, control, removable: taxonomy.identifier !== VOCAB_ID_EU_DATA_THEME};
	}

	private inWriteBack(write: () => void): void {
		this.writingBack = true;
		write();
		this.writingBack = false;
	}
}
