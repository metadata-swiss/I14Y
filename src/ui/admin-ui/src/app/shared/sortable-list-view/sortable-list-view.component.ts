import {Component, inject, Input, ViewChild} from '@angular/core';
import {MatPaginator} from '@angular/material/paginator';
import {MatSort, Sort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {MultiLanguage, MultiLanguageModel, VocabularyEntryModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService} from '@ngx-translate/core';
import {FallbackPipe} from '../fallback/fallback.pipe';

@Component({
	selector: 'app-sortable-list-view',
	template: '',
	standalone: false
})
export abstract class SortableListViewComponent<T extends {}, K extends keyof T> {
	@Input()
	set data(data: T[]) {
		if (data) {
			this.dataSource.data = data;
		}
	}
	get data(): T[] {
		return this.dataSource.data;
	}

	@Input('paginator')
	set outerPaginator(paginator: MatPaginator) {
		this.setPaginator(paginator);
	}

	@ViewChild(MatPaginator)
	set matPaginator(paginator: MatPaginator) {
		this.setPaginator(paginator);
	}

	@ViewChild(MatSort)
	set matSort(sort: MatSort) {
		this.sort = sort;
		this.dataSource.sort = this.sort;
	}

	readonly dataSource: MatTableDataSource<T>;

	abstract displayedColumns: string[];

	protected readonly translate = inject(TranslateService);

	private paginator!: MatPaginator;
	private sort!: MatSort;
	private readonly SORT_DIRECTION_ASCENDING = 'asc';
	private readonly SORT_DIRECTION_DESCENDING = 'desc';

	private readonly fallbackPipe = inject(FallbackPipe);

	constructor() {
		this.dataSource = new MatTableDataSource<T>([]);
	}

	public sortData(sort: Sort) {
		const sortColumn = sort.active as K;
		const sortDirection = sort.direction;
		const sortedData = [...this.data].sort((left: T, right: T) => {
			switch (sortDirection) {
				case this.SORT_DIRECTION_DESCENDING:
					return this.sortDescending(left, right, sortColumn);
				case this.SORT_DIRECTION_ASCENDING:
				default:
					return this.sortAscending(left, right, sortColumn);
			}
		});
		this.dataSource.data = sortedData;
	}

	private setPaginator(paginator: MatPaginator) {
		if (paginator) {
			this.paginator = paginator;
			this.dataSource.paginator = this.paginator;
		}
	}

	private sortAscending(left: T, right: T, sortColumn: K): number {
		return this.compareObjects(left, right, sortColumn);
	}

	private sortDescending(left: T, right: T, sortColumn: K): number {
		const newLeft = right;
		const newRight = left;

		return this.compareObjects(newLeft, newRight, sortColumn);
	}

	private compareObjects(left: T, right: T, key: K): number {
		const leftValue = left[key];
		const rightValue = right[key];

		if (Array.isArray(leftValue) && Array.isArray(rightValue)) {
			return this.compareArray(leftValue, rightValue);
		}

		if (typeof leftValue === 'string' && typeof rightValue === 'string') {
			return this.compareStringObject(leftValue, rightValue);
		}

		if (typeof leftValue === 'number' && typeof rightValue === 'number') {
			return this.compareReturn(leftValue, rightValue);
		}

		if (typeof leftValue === 'boolean' && typeof rightValue === 'boolean') {
			return this.compareReturn(leftValue, rightValue);
		}

		if (leftValue instanceof MultiLanguage && rightValue instanceof MultiLanguage) {
			return this.compareStringObject(
				this.fallbackPipe.transform(leftValue, this.translate.getCurrentLang()) || '',
				this.fallbackPipe.transform(rightValue, this.translate.getCurrentLang()) || ''
			);
		}

		if (leftValue instanceof MultiLanguageModel && rightValue instanceof MultiLanguageModel) {
			return this.compareStringObject(
				this.fallbackPipe.transform(leftValue, this.translate.getCurrentLang()) || '',
				this.fallbackPipe.transform(rightValue, this.translate.getCurrentLang()) || ''
			);
		}

		if (leftValue instanceof VocabularyEntryModel && rightValue instanceof VocabularyEntryModel) {
			return this.compareStringObject(
				this.fallbackPipe.transform(leftValue.name, this.translate.getCurrentLang()) || '',
				this.fallbackPipe.transform(rightValue.name, this.translate.getCurrentLang()) || ''
			);
		}

		return this.compareReturn(leftValue, rightValue);
	}

	private compareArray(leftValue: T[K] & any[], rightValue: T[K] & any[]): number {
		if (typeof leftValue[0] === 'string' && typeof rightValue[0] === 'string') {
			return this.compareStringObject(leftValue.join(','), rightValue.join(','));
		}

		if (leftValue[0] instanceof MultiLanguage && rightValue[0] instanceof MultiLanguage) {
			return this.compareStringObject(
				leftValue.map(x => this.fallbackPipe.transform(x.name, this.translate.getCurrentLang())).join(','),
				rightValue.map(x => this.fallbackPipe.transform(x.name, this.translate.getCurrentLang())).join(',')
			);
		}

		if (leftValue[0] instanceof MultiLanguageModel && rightValue[0] instanceof MultiLanguageModel) {
			return this.compareStringObject(
				leftValue.map(x => this.fallbackPipe.transform(x, this.translate.getCurrentLang())).join(','),
				rightValue.map(x => this.fallbackPipe.transform(x, this.translate.getCurrentLang())).join(',')
			);
		}

		if (leftValue[0] instanceof VocabularyEntryModel && rightValue[0] instanceof VocabularyEntryModel) {
			return this.compareStringObject(
				leftValue.map(x => this.fallbackPipe.transform(x.name, this.translate.getCurrentLang())).join(','),
				rightValue.map(x => this.fallbackPipe.transform(x.name, this.translate.getCurrentLang())).join(',')
			);
		}

		return this.compareReturn(leftValue.length, rightValue.length);
	}

	private compareStringObject(leftValue: string, rightValue: string): number {
		if (!isNaN(Number(leftValue)) && !isNaN(Number(rightValue))) {
			return this.compareReturn(Number(leftValue), Number(rightValue));
		}

		if (!isNaN(Date.parse(leftValue)) && !isNaN(Date.parse(rightValue))) {
			return this.compareReturn(Date.parse(leftValue), Date.parse(rightValue));
		}

		return this.compareStringReturn(leftValue, rightValue);
	}

	private compareReturn<TCompare = number | string | T[K]>(left: TCompare, right: TCompare): number {
		if (left < right) {
			return -1;
		}

		if (left === right) {
			return 0;
		}

		return 1;
	}

	private compareStringReturn(left: string, right: string): number {
		const collator = new Intl.Collator();
		return collator.compare(left, right);
	}
}
