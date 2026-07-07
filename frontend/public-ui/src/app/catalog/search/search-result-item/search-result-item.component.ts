import {Component, inject, Input, OnDestroy, OnInit} from '@angular/core';
import {CatalogEntry, SearchResourceType} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';
import {DateFormatService} from 'src/app/shared/services/date-format/date-format.service';
import {WithRelations} from '../relations-count.model';
import {buildRelationsTooltip} from '../relations-count.util';

@Component({
	selector: 'app-catalog-search-result-item',
	templateUrl: './search-result-item.component.html',
	styleUrls: ['./search-result-item.component.scss'],
	standalone: false
})
export class CatalogSearchResultItemComponent implements OnInit, OnDestroy {
	@Input() result: CatalogEntry & WithRelations;
	@Input() index: number;

	currentLanguage: string;

	private readonly unsubscribe$ = new Subject();

	private readonly dateFormatService = inject(DateFormatService);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	getRouterLink(): string {
		let routerLink = '';

		switch (this.result.type) {
			case SearchResourceType.Dataset:
				routerLink = `../datasets/${this.result.identifiers[0]}`;
				break;
			case SearchResourceType.DataService:
				routerLink = `../dataservices/${this.result.identifiers[0]}`;
				break;
			case SearchResourceType.PublicService:
				routerLink = `../publicservices/${this.result.identifiers[0]}`;
				break;
			case SearchResourceType.Concept:
				routerLink = `../concepts/${this.result.id}`;
				break;
			case SearchResourceType.MappingTable:
				routerLink = `../mappingtables/${this.result.id}`;
				break;
			default:
				break;
		}

		return routerLink;
	}

	isConcept(): boolean {
		return this.result?.type === SearchResourceType.Concept;
	}

	getRelationsTooltip(): string {
		return buildRelationsTooltip(this.result?.relations, this.translate);
	}

	getType(): string | undefined {
		let type: string;

		if (this.result?.type) {
			type = this.result.type.toLocaleLowerCase();
		}

		return type;
	}

	getFormattedDate(date: Date): string | undefined {
		return date ? this.dateFormatService.formatShortDate(date) : undefined;
	}
}
