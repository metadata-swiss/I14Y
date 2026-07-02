import {TranslateService} from '@ngx-translate/core';
import {inject, Injectable, OnDestroy} from '@angular/core';
import {MatPaginatorIntl} from '@angular/material/paginator';
import {Subject, takeUntil} from 'rxjs';
@Injectable({providedIn: 'root'})
export class MatPaginatorIntlMultiLang extends MatPaginatorIntl implements OnDestroy {
	ofLabel = 'of';

	private readonly unsubscribe$ = new Subject();

	private readonly translate = inject(TranslateService);

	constructor() {
		super();

		this.SetLanguageTextes();
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	getRangeLabel = (page: number, pageSize: number, length: number) => {
		if (length === 0 || pageSize === 0) {
			return `0 ${this.ofLabel} ${length}`;
		}
		length = Math.max(length, 0);
		const startIndex = page * pageSize;
		const endIndex = startIndex < length ? Math.min(startIndex + pageSize, length) : startIndex + pageSize;
		return `${startIndex + 1} - ${endIndex} ${this.ofLabel} ${length}`;
	};

	private SetLanguageTextes() {
		this.translate
			.stream([
				'i18n.paginator.itemsPerPageLabel',
				'i18n.paginator.nextPageLabel',
				'i18n.paginator.previousPageLabel',
				'i18n.paginator.firstPageLabel',
				'i18n.paginator.lastPageLabel',
				'i18n.paginator.ofLabel'
			])
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(translation => {
				this.itemsPerPageLabel = translation['i18n.paginator.itemsPerPageLabel'];
				this.nextPageLabel = translation['i18n.paginator.nextPageLabel'];
				this.previousPageLabel = translation['i18n.paginator.previousPageLabel'];
				this.firstPageLabel = translation['i18n.paginator.firstPageLabel'];
				this.lastPageLabel = translation['i18n.paginator.lastPageLabel'];
				this.ofLabel = translation['i18n.paginator.ofLabel'];
				this.changes.next();
			});
	}
}
