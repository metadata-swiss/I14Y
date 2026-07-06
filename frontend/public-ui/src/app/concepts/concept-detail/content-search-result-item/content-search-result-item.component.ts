import {Component, inject, Input, OnInit} from '@angular/core';
import {CodeListEntrySearchResultEntryModel, CodeListEntrySearchResultPathModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';

@Component({
	selector: 'app-content-search-result-item',
	templateUrl: './content-search-result-item.component.html',
	styleUrls: ['./content-search-result-item.component.scss'],
	standalone: false
})
export class ContentSearchResultItemComponent implements OnInit {
	@Input() result: CodeListEntrySearchResultEntryModel;
	@Input() filterLanguage: string | null;
	currentLanguage: string;

	queryParamsForSearchOnly: {[key: string]: string[]} = {
		query: null,
		page: null,
		pageSize: null
	};

	private readonly unsubscribe$ = new Subject();

	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	getResultPath(): string {
		return `../${this.result.path.map(r => r.code).join('/')}`;
	}

	getBreadCrumbPath(breadCrumb: CodeListEntrySearchResultPathModel): string {
		return breadCrumb.parentCode
			? `../${this.getPathParent(this.result.path.find(p => p.code === breadCrumb.parentCode))}${breadCrumb.code}`
			: `../${breadCrumb.code}`;
	}

	private getPathParent(breadCrumb: CodeListEntrySearchResultPathModel): string {
		return breadCrumb.parentCode
			? `${this.getPathParent(this.result.path.find(p => p.code === breadCrumb.parentCode))}/${breadCrumb.code}/`
			: `${breadCrumb.code}/`;
	}
}
