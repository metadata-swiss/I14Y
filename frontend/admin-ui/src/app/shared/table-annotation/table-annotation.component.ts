import {AfterViewInit, Component, inject, Input, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {MatSort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {Annotation} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {FallbackPipe} from '../fallback/fallback.pipe';
import {Subject, takeUntil} from 'rxjs';

@Component({
	selector: 'app-table-annotation',
	templateUrl: './table-annotation.component.html',
	styleUrls: ['./table-annotation.component.scss'],
	standalone: false
})
export class TableAnnotationComponent implements OnInit, AfterViewInit, OnDestroy {
	@ViewChild(MatSort, {static: false}) sort!: MatSort;
	@Input() annotations: Annotation[] = [];
	dataSource = new MatTableDataSource<Annotation>(this.annotations);

	COLUMN_TYPE = 'type';
	COLUMN_TITLE = 'title';
	COLUMN_TEXT = 'text';
	COLUMN_URI = 'uri';
	COLUMN_IDENTIFIER = 'identifier';

	currentLanguage: string;

	displayedColumns: string[] = [this.COLUMN_TYPE, this.COLUMN_TITLE, this.COLUMN_TEXT, this.COLUMN_URI, this.COLUMN_IDENTIFIER];

	private readonly unsubscribe$ = new Subject();

	private readonly fallback = inject(FallbackPipe);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => (this.currentLanguage = language.lang));
	}

	ngOnInit(): void {
		this.dataSource.sort = this.sort;
		this.dataSource = new MatTableDataSource<Annotation>(this.annotations);
	}

	ngAfterViewInit(): void {
		this.sort.sortChange.subscribe(() => {
			this.sortpage();
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	private sortpage() {
		switch (this.sort?.active) {
			case this.COLUMN_TEXT:
				this.annotations.sort((a, b) => {
					const aText = this.fallback.transform(a.text, this.currentLanguage);
					const bText = this.fallback.transform(b.text, this.currentLanguage);
					return this.sortSub(aText!, bText!);
				});
				break;
			case this.COLUMN_TYPE:
				this.annotations.sort((a, b) => {
					return this.sortSub(a.type!.toString(), b.type!.toString());
				});

				break;
			case this.COLUMN_TITLE:
				this.annotations.sort((a, b) => {
					return this.sortSub(a.title!.toString(), b.title!.toString());
				});
				break;
			case this.COLUMN_URI:
				this.annotations.sort((a, b) => {
					return this.sortSub(a.uri!.toString(), b.uri!.toString());
				});
				break;
			case this.COLUMN_IDENTIFIER:
				this.annotations.sort((a, b) => {
					return this.sortSub(a.identifier!.toString(), b.identifier!.toString());
				});
		}

		this.dataSource = new MatTableDataSource<Annotation>(this.annotations);
	}

	private sortSub(a: string, b: string): number {
		if (this.sort.direction === 'asc') {
			return a.toLowerCase().localeCompare(b.toLowerCase());
		} else {
			return b.toLowerCase().localeCompare(a.toLowerCase());
		}
	}
}
