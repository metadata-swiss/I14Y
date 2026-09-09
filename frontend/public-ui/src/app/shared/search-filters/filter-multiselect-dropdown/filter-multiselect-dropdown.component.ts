import {Component, Input, EventEmitter, Output, OnInit, ViewChild, ElementRef, OnChanges, SimpleChanges, inject} from '@angular/core';
import {FormGroup, FormControl} from '@angular/forms';
import {MatSelect} from '@angular/material/select';
import {FilterCountResultItem, PublicationLevel, RegistrationStatus} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {SearchFilterService} from '../search-filters.service';
import {Subject, takeUntil} from 'rxjs';
import {SearchFilters} from '../search-filters';

interface MultiselectForm {
	selectedOptions: FormControl<string[]>;
}

@Component({
	selector: 'app-filter-multiselect-dropdown',
	templateUrl: './filter-multiselect-dropdown.component.html',
	styleUrls: ['./filter-multiselect-dropdown.component.scss'],
	standalone: false
})
export class FilterMultiSelectDropdownComponent implements OnInit, OnChanges {
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
	@Input() componentId: string;

	@Output() changeEvent = new EventEmitter<string[]>();
	@Output() selectAllEvent = new EventEmitter<void>();
	@Output() selectChangeEvent = new EventEmitter<string>();

	query: string | undefined;
	allChecked = false;
	dropdownOptionsToDisplay: FilterCountResultItem[] = [];
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
		const filteredSelectedValues = this.multiselectForm.controls.selectedOptions.value;
		let selectedValues: string[] = [];
		selectedValues = this.filteredValues.concat(this.multiselectForm.controls.selectedOptions.value);
		selectedValues = selectedValues.filter((value, index) => selectedValues.indexOf(value) === index);
		this.dropdownOptionsToDisplay.forEach(element => {
			if (!filteredSelectedValues.includes(element.reference!)) {
				selectedValues = selectedValues.filter(value => value !== element.reference!);
			}
		});

		this.multiselectForm.controls.selectedOptions.setValue(selectedValues);
		this.emitChangeEvent(selectedValues);
	}

	public emitChangeEvent(values: string[]): void {
		this.updateAllSelectionCheckboxMark();
		this.changeEvent.emit(values.filter(value => value !== this.selectAllValue));
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

	public getSortedOptions(): FilterCountResultItem[] {
		switch (this.section) {
			case SearchFilters.KeyPublisher:
				return this.dropdownOptionsToDisplay.sort((a, b) => this.sortOptions(a, b));
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
}
