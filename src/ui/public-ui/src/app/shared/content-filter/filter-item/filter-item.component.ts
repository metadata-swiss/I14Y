import {Component, EventEmitter, inject, Input, OnDestroy, Output} from '@angular/core';
import {Placement} from '@popperjs/core';
import {FilterContainer} from '../filterContainer';
import {FilterValueContainer} from '../filterValueContainer';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';

export class FilterValueSelectionChangedEventArg {
	filter: FilterContainer;
	value: FilterValueContainer;
	selected: boolean;
	constructor(filter: FilterContainer, value: FilterValueContainer, selected: boolean) {
		this.filter = filter;
		this.value = value;
		this.selected = selected;
	}
}

@Component({
	selector: 'app-filter-item',
	templateUrl: './filter-item.component.html',
	styleUrls: ['./filter-item.component.scss'],
	standalone: false
})
export class FilterItemComponent implements OnDestroy {
	currentLanguage: string;
	placement: Placement = 'top';
	@Input() filter: FilterContainer;
	@Output()
	selectionChanged: EventEmitter<FilterValueSelectionChangedEventArg> = new EventEmitter<FilterValueSelectionChangedEventArg>();

	private readonly unsubscribe$ = new Subject();

	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	valueChanged(filter: FilterContainer, value: FilterValueContainer, selected: boolean) {
		this.selectionChanged.emit(new FilterValueSelectionChangedEventArg(filter, value, selected));
	}
}
