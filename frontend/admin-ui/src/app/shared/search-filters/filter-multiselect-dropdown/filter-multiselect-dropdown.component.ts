import {Component, Input, EventEmitter, Output, OnInit, ViewChild, ElementRef, OnChanges, SimpleChanges, inject, OnDestroy} from '@angular/core';
import {FormGroup, FormControl} from '@angular/forms';
import {MatSelect} from '@angular/material/select';
import {FilterCountResultItem, MultiLanguage, PublicationLevel, RegistrationStatus} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {SearchFilterService} from '../search-filters.service';
import {Subject, takeUntil} from 'rxjs';
import {SearchFilters} from '../search-filters';
import {VOCAB_ID_EU_DATA_THEME} from 'src/app/app-constants';
import {splitThemeSearchKey} from 'src/app/shared/helper/theme-search-key';
import {ThemeTaxonomiesService} from 'src/app/services/theme-taxonomies.service';

const THEME_GROUP_PREFIX = 'theme_group:';

interface MultiselectForm {
	selectedOptions: FormControl<string[]>;
}

interface ThemeGroup {
	identifier: string;
	name?: MultiLanguage;
	options: FilterCountResultItem[];
	expanded: boolean;
}

@Component({
	selector: 'app-filter-multiselect-dropdown',
	templateUrl: './filter-multiselect-dropdown.component.html',
	styleUrls: ['./filter-multiselect-dropdown.component.scss'],
	standalone: false
})
export class FilterMultiSelectDropdownComponent implements OnInit, OnChanges, OnDestroy {
	readonly selectAllValue = 'select_all';
	@Input() public set selectedOptionsFromFilter(value: string[]) {
		setTimeout(() => {
			this.multiselectForm.controls.selectedOptions.setValue(value);
		});
	}
	@Input() translationDropdownFieldName = '';
	@Input() dropdownOptions: FilterCountResultItem[] = [];
	@Input() section: string = '';
	@Input() displaySelectAllOption = false;
	@Input() defaultSelection: string[] = [];
	@Input() displaySearch = true;
	@Input() searchLabel = '';
	@Input() searchPlaceholder = '';
	@Input() minElementsToDisplaySearch = 10;

	@Output() changeEvent = new EventEmitter<string[]>();
	@Output() selectAllEvent = new EventEmitter<void>();
	@Output() selectChangeEvent = new EventEmitter<string>();

	query: string | undefined;
	allChecked = false;
	dropdownOptionsToDisplay: FilterCountResultItem[] = [];
	euThemeOptions: FilterCountResultItem[] = [];
	themeGroups: ThemeGroup[] = [];
	canUpdateDisplaySelectAllOption = false;
	currentLanguage: string;

	multiselectForm = new FormGroup<MultiselectForm>({
		selectedOptions: new FormControl<string[]>([], {
			nonNullable: true
		})
	});

	@ViewChild(MatSelect)
	private readonly matSelect!: MatSelect;
	private filteredValues: string[] = [];
	private groupedFrom: FilterCountResultItem[] | null = null;
	private readonly themeTaxonomiesService = inject(ThemeTaxonomiesService);

	@ViewChild('searchInput')
	private readonly searchInput!: ElementRef<HTMLInputElement>;

	private readonly unsubscribe$ = new Subject();

	private readonly fallbackPipe = inject(FallbackPipe);
	private readonly searchFilterService = inject(SearchFilterService);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	public ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => (this.currentLanguage = language.lang));
		this.dropdownOptionsToDisplay = this.dropdownOptions;
		this.multiselectForm.controls.selectedOptions.setValue(this.defaultSelection);
		this.canUpdateDisplaySelectAllOption = this.displaySelectAllOption;
		this.allChecked = this.defaultSelection?.length === this.dropdownOptions?.length;
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	public ngOnChanges(changes: SimpleChanges): void {
		let change = changes?.dropdownOptions;
		if (change?.currentValue) {
			this.dropdownOptionsToDisplay = this.dropdownOptions;
		}
	}

	public getI18n(key: string, reference: string): string {
		return this.searchFilterService.getI18n(key, reference);
	}

	public updateActiveClass(): void {
		// setTimeout needed when we select select-all option
		setTimeout(() => {
			const activeOptions = this.matSelect.panel.nativeElement.querySelectorAll(`.mat-active`);
			if (activeOptions === null) {
				return;
			}
			const selectAllOption = this.getMatOptionSelectAll();
			if (selectAllOption === null) {
				return;
			}

			const tooManyMatActiveCount = 2;
			if (activeOptions.length === tooManyMatActiveCount) {
				selectAllOption.classList.remove(`mat-active`);
				return;
			}
			if (activeOptions.length !== 0) {
				return;
			}

			selectAllOption.classList.add(`mat-active`);
		}, 1);
	}

	public selectOption(): void {
		const selectedOptions = this.multiselectForm.controls.selectedOptions;

		if (this.toggleHeadingIfPicked(selectedOptions.value)) {
			return;
		}

		const selectAllExists = selectedOptions.value.includes(this.selectAllValue);

		if (selectAllExists) {
			this.toggleAllSelection();
			this.updateActiveClass();
		}
		if (this.allChecked) {
			this.allChecked = !this.allChecked;
		}

		if (selectedOptions.value.length === this.dropdownOptions.length) {
			this.allChecked = true;
		}
		if (this.displaySearch) {
			this.emitChangeEventWithSearch();
		} else {
			this.emitChangeEvent(selectedOptions.value);
		}
	}

	public resetAllSelections() {
		this.multiselectForm.controls.selectedOptions.patchValue([]);
	}

	public toggleAllSelection(): void {
		this.allChecked = !this.allChecked;
		if (this.allChecked) {
			this.multiselectForm.controls.selectedOptions.patchValue([...this.dropdownOptions.map(item => item.reference!)]);
		} else {
			this.multiselectForm.controls.selectedOptions.patchValue([]);
		}
		this.emitChangeEvent(this.multiselectForm.controls.selectedOptions.value);
	}

	public search(): void {
		if (this.query) {
			this.dropdownOptionsToDisplay = this.dropdownOptions.filter(value => this.filterOptions(value));

			this.filteredValues = this.multiselectForm.controls.selectedOptions.value ? this.multiselectForm.controls.selectedOptions.value : [];

			if (this.canUpdateDisplaySelectAllOption) {
				this.displaySelectAllOption = false;
			}
		} else {
			this.dropdownOptionsToDisplay = this.dropdownOptions;

			if (this.canUpdateDisplaySelectAllOption) {
				this.displaySelectAllOption = true;
			}
		}
	}

	public emitChangeEventWithSearch(): void {
		const selectedValues = this.reconcileWithSearch(this.multiselectForm.controls.selectedOptions.value);

		this.multiselectForm.controls.selectedOptions.setValue(selectedValues);
		this.emitChangeEvent(selectedValues);
	}

	public emitChangeEvent(values: string[]): void {
		this.updateAllSelectionCheckboxMark();
		this.changeEvent.emit(values.filter(value => value !== this.selectAllValue && !value.startsWith(THEME_GROUP_PREFIX)));
	}

	public setFocus(): void {
		this.updateAllSelectionCheckboxMark();
		if (this.displaySearch && this.dropdownOptions.length >= this.minElementsToDisplaySearch) {
			this.searchInput.nativeElement.focus();
		}
	}

	public resetOptions(): void {
		if (this.displaySearch && this.searchInput?.nativeElement) {
			this.searchInput.nativeElement.value = '';
			this.dropdownOptionsToDisplay = this.dropdownOptions;
		}
	}

	public getOptionsCount(): number {
		if (this.dropdownOptions) {
			return this.dropdownOptions.length;
		}

		return 0;
	}

	get isThemeSection(): boolean {
		return this.section === SearchFilters.KeyThemes;
	}

	isOptionSelected(option: FilterCountResultItem): boolean {
		return this.multiselectForm.controls.selectedOptions.value?.includes(option.reference!) ?? false;
	}

	// Use mat-option for keyboard navigation; the sentinel toggles the group without selecting it.
	themeGroupValue(identifier: string): string {
		return THEME_GROUP_PREFIX + identifier;
	}

	public getThemeSections(): ThemeGroup[] {
		const options = this.dropdownOptionsToDisplay;

		if (this.groupedFrom !== options) {
			this.groupThemes(options);
			this.groupedFrom = options;
		}

		return this.themeGroups;
	}

	public getSortedOptions(): FilterCountResultItem[] {
		switch (this.section) {
			case SearchFilters.KeyPublisher:
			case SearchFilters.KeyAttributedAgents:
				return this.dropdownOptionsToDisplay.sort((a, b) => this.sortOptions(a, b));
			case SearchFilters.KeyLevelProposals:
			case SearchFilters.KeyLevels:
				return this.sortPublicationLevel();
			case SearchFilters.KeyStatusProposals:
			case SearchFilters.KeyStatuses:
				return this.sortRegistrationStatus();
			default:
				return this.dropdownOptionsToDisplay;
		}
	}

	private sortRegistrationStatus(): FilterCountResultItem[] {
		let items: FilterCountResultItem[] = [];

		Object.keys(RegistrationStatus).forEach(value => {
			let item = this.dropdownOptionsToDisplay.find(x => x.reference === value);

			if (item) {
				items = [...items, item];
			}
		});

		return items;
	}

	private sortPublicationLevel(): FilterCountResultItem[] {
		let items: FilterCountResultItem[] = [];

		Object.keys(PublicationLevel).forEach(value => {
			let item = this.dropdownOptionsToDisplay.find(x => x.reference === value);

			if (item) {
				items = [...items, item];
			}
		});

		return items;
	}

	private sortOptions(a: FilterCountResultItem, b: FilterCountResultItem): number {
		return this.compareStringReturn(this.getText(a), this.getText(b));
	}

	private getText(item: FilterCountResultItem): string {
		let res = '';
		if (item.label) {
			res = this.fallbackPipe.transform(item.label, this.translate.getCurrentLang()) ?? '';
		} else {
			res = this.translate.instant(this.searchFilterService.getI18n(this.section, item.reference!)) ?? '';
		}
		return res;
	}

	private compareStringReturn(left: string, right: string): number {
		const collator = new Intl.Collator();
		return collator.compare(left, right);
	}

	private getMatOptionSelectAll(): HTMLElement | null {
		return this.matSelect.panel?.nativeElement.querySelector(`#all-option`) ?? null;
	}

	private updateAllSelectionCheckboxMark(): void {
		const matSelectedClass = `mat-selected`; // needed to have the blue text when selected
		const indeterminateClass = `mat-pseudo-checkbox-indeterminate`;
		const checkedClass = `mat-pseudo-checkbox-checked`;

		const selectAllOption = this.getMatOptionSelectAll();
		if (selectAllOption === null) {
			return;
		}
		const selectAllCheckbox = selectAllOption.querySelector(`mat-pseudo-checkbox`);
		if (selectAllCheckbox === null) {
			return;
		}

		const allOptionsCount = this.dropdownOptions.length;
		const selectedOptionsCount = this.multiselectForm.controls.selectedOptions.value.length;

		const allAreSelected = allOptionsCount === selectedOptionsCount;
		if (allAreSelected) {
			selectAllCheckbox.classList.add(checkedClass);
			selectAllCheckbox.classList.remove(indeterminateClass);
			selectAllOption.classList.add(matSelectedClass);
			selectAllOption.setAttribute(`aria-selected`, `true`);
			return;
		}

		const noneAreSelected = selectedOptionsCount === 0;
		if (noneAreSelected) {
			selectAllOption.setAttribute(`aria-selected`, `false`);
			selectAllCheckbox.classList.remove(checkedClass);
			selectAllCheckbox.classList.remove(indeterminateClass);
			selectAllOption.classList.remove(matSelectedClass);
			return;
		}

		// Partially selected
		selectAllOption.setAttribute(`aria-selected`, `false`);
		selectAllCheckbox.classList.add(indeterminateClass);
		selectAllCheckbox.classList.remove(checkedClass);
		selectAllOption.classList.add(matSelectedClass);
	}

	private filterOptions(value: FilterCountResultItem): boolean {
		let isIncluded = false;

		if (value?.label?.de) {
			isIncluded = isIncluded || value.label.de.toLowerCase().includes(this.query!.toLowerCase());
		}

		if (value?.label?.fr) {
			isIncluded = isIncluded || value.label.fr.toLowerCase().includes(this.query!.toLowerCase());
		}

		if (value?.label?.it) {
			isIncluded = isIncluded || value.label.it.toLowerCase().includes(this.query!.toLowerCase());
		}

		if (value?.label?.en) {
			isIncluded = isIncluded || value.label.en.toLowerCase().includes(this.query!.toLowerCase());
		}

		if (value?.label?.rm) {
			isIncluded = isIncluded || value.label.rm.toLowerCase().includes(this.query!.toLowerCase());
		}

		return isIncluded;
	}

	private toggleHeadingIfPicked(values: string[]): boolean {
		const heading = values.find(value => value.startsWith(THEME_GROUP_PREFIX));

		if (!heading) {
			return false;
		}

		const group = this.themeGroups.find(x => this.themeGroupValue(x.identifier) === heading);

		if (group) {
			group.expanded = !group.expanded;
		}

		const kept = values.filter(value => value !== heading);

		this.multiselectForm.controls.selectedOptions.setValue(this.displaySearch ? this.reconcileWithSearch(kept) : kept);

		return true;
	}

	// Preserve selections hidden by the search, while allowing visible options to be deselected.
	private reconcileWithSearch(values: string[]): string[] {
		const merged = [...new Set([...this.filteredValues, ...values])];

		return merged.filter(value => values.includes(value) || !this.dropdownOptionsToDisplay.some(option => option.reference === value));
	}

	private groupThemes(options: FilterCountResultItem[]): void {
		const byVocabulary = new Map<string, FilterCountResultItem[]>();

		this.euThemeOptions = [];

		for (const option of options) {
			const parts = splitThemeSearchKey(option.reference);

			if (!parts || parts.vocabularyIdentifier === VOCAB_ID_EU_DATA_THEME) {
				this.euThemeOptions.push(option);
			} else {
				byVocabulary.set(parts.vocabularyIdentifier, [...(byVocabulary.get(parts.vocabularyIdentifier) ?? []), option]);
			}
		}

		this.themeGroups = [...byVocabulary.entries()].map(([identifier, groupOptions]) => {
			const previous = this.themeGroups.find(existingGroup => existingGroup.identifier === identifier);
			const group: ThemeGroup = {identifier, name: previous?.name, options: groupOptions, expanded: previous?.expanded ?? false};

			if (!group.name) {
				this.themeTaxonomiesService.nameOf(identifier).subscribe(name => (group.name = name));
			}

			return group;
		});
	}
}
