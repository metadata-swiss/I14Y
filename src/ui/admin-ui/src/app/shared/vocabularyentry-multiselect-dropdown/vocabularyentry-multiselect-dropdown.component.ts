import {Component, Input, OnInit, ViewChild, ElementRef, OnChanges, SimpleChanges, inject, OnDestroy} from '@angular/core';
import {UntypedFormGroup} from '@angular/forms';
import {VocabularyEntry} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {Subject, takeUntil} from 'rxjs';

@Component({
	selector: 'app-vocabularyentry-multiselect-dropdown',
	templateUrl: './vocabularyentry-multiselect-dropdown.component.html',
	styleUrls: ['./vocabularyentry-multiselect-dropdown.component.scss'],
	standalone: false
})
export class VocabularyEntryMultiSelectDropdownComponent implements OnInit, OnChanges, OnDestroy {
	@Input() translationDropdownFieldName = '';
	@Input() controlName = '';
	@Input() form!: UntypedFormGroup;
	@Input() dropdownOptions: VocabularyEntry[] = [];
	@Input() displaySearch = true;
	@Input() minElementsToDisplaySearch = 10;

	query: string | undefined;
	dropdownOptionsToDisplay: VocabularyEntry[] = [];
	currentLanguage: string;

	@ViewChild('searchInput')
	private readonly searchInput!: ElementRef<HTMLInputElement>;

	private readonly unsubscribe$ = new Subject();

	private readonly fallbackPipe = inject(FallbackPipe);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	public ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => (this.currentLanguage = language.lang));
		this.dropdownOptionsToDisplay = this.dropdownOptions;
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

	public search(): void {
		if (this.query) {
			this.dropdownOptionsToDisplay = this.dropdownOptions.filter(value => this.filterOptions(value));
		} else {
			this.dropdownOptionsToDisplay = this.dropdownOptions;
		}
	}

	public setFocus(): void {
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

	private filterOptions(value: VocabularyEntry): boolean {
		let name = this.fallbackPipe.transform(value.name, this.currentLanguage);

		return name ? name.toLowerCase().includes(this.query!.toLowerCase()) : false;
	}
}
