import {Component, inject, Input, ViewChild} from '@angular/core';
import {MatPaginator} from '@angular/material/paginator';
import {MatSort, Sort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {MultiLanguage, MultiLanguageModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService} from '@ngx-translate/core';
import {Subject} from 'rxjs';
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
		this.data$.next(data);
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

	public readonly dataSource: MatTableDataSource<T>;
	public abstract displayedColumns: string[];

	protected data$: Subject<T[]> = new Subject<T[]>();
	protected dataObservable$ = this.data$.asObservable();

	protected readonly translate = inject(TranslateService);

	private paginator: MatPaginator;
	private sort: MatSort;
	private readonly SORT_DIRECTION_ASCENDING = 'asc';
	private readonly SORT_DIRECTION_DESCENDING = 'desc';

	private readonly fallbackPipe = inject(FallbackPipe);

	constructor() {
		this.dataSource = new MatTableDataSource([]);
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

		if (typeof leftValue === 'string' && typeof rightValue === 'string') {
			return this.compareStringObject(leftValue, rightValue);
		}

		if (typeof leftValue === 'number' && typeof rightValue === 'number') {
			return this.compareReturn(leftValue, rightValue);
		}

		if (typeof leftValue === 'boolean' && typeof rightValue === 'boolean') {
			return this.compareReturn(leftValue, rightValue);
		}

		if (Array.isArray(leftValue) && typeof leftValue[0] === 'string' && Array.isArray(rightValue) && typeof rightValue[0] === 'string') {
			return this.compareStringObject(leftValue.join(','), rightValue.join(','));
		}

		if (Array.isArray(leftValue) && leftValue[0] instanceof MultiLanguage && Array.isArray(rightValue) && rightValue[0] instanceof MultiLanguage) {
			return this.compareMultiLanguageArrayReturn(leftValue, rightValue);
		}

		// eslint-disable-next-line max-len
		if (Array.isArray(leftValue) && leftValue[0] instanceof MultiLanguageModel && Array.isArray(rightValue) && rightValue[0] instanceof MultiLanguageModel) {
			return this.compareMultiLanguageModelArrayReturn(leftValue, rightValue);
		}

		if (Array.isArray(leftValue) && Array.isArray(rightValue)) {
			return this.compareReturn(leftValue.length, rightValue.length);
		}

		if (leftValue instanceof MultiLanguage && rightValue instanceof MultiLanguage) {
			return this.compareMultiLanguageReturn(leftValue, rightValue);
		}

		if (leftValue instanceof MultiLanguageModel && rightValue instanceof MultiLanguageModel) {
			return this.compareMultiLanguageModelReturn(leftValue, rightValue);
		}

		return this.compareReturn(leftValue, rightValue);
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

	private compareMultiLanguageReturn(left: MultiLanguage, right: MultiLanguage): number {
		// eslint-disable-next-line max-len
		return this.compareReturn(this.fallbackPipe.transform(left, this.translate.getCurrentLang()), this.fallbackPipe.transform(right, this.translate.getCurrentLang()));
	}

	private compareMultiLanguageModelReturn(left: MultiLanguageModel, right: MultiLanguageModel): number {
		// eslint-disable-next-line max-len
		return this.compareReturn(this.fallbackPipe.transform(left, this.translate.getCurrentLang()), this.fallbackPipe.transform(right, this.translate.getCurrentLang()));
	}

	private compareMultiLanguageArrayReturn(left: MultiLanguageModel[], right: MultiLanguageModel[]): number {
		return this.compareReturn(
			this.fallbackPipe.transform(left[0], this.translate.getCurrentLang()),
			this.fallbackPipe.transform(right[0], this.translate.getCurrentLang())
		);
	}

	private compareMultiLanguageModelArrayReturn(left: MultiLanguageModel[], right: MultiLanguageModel[]): number {
		return this.compareReturn(
			this.fallbackPipe.transform(left[0], this.translate.getCurrentLang()),
			this.fallbackPipe.transform(right[0], this.translate.getCurrentLang())
		);
	}
}
