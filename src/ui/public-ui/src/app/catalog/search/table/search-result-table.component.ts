import {Component, inject, Input, OnChanges, OnDestroy, OnInit, SimpleChanges} from '@angular/core';
import {LangChangeEvent} from '@ngx-translate/core';
import {CatalogEntry, SearchResourceType, ConceptType, ICatalogEntry} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {filter, takeUntil} from 'rxjs/operators';
import {Subject} from 'rxjs';
import {MatTableDataSource} from '@angular/material/table';
import {SortableListViewComponent} from 'src/app/shared/sortable-list-view/sortable-list-view.component';
import {SearchResultPagingInfo} from 'src/app/shared/search/SearchResultPagingInfo';
import {DateFormatService} from 'src/app/shared/services/date-format/date-format.service';
import {ActivatedRoute, NavigationEnd, Router} from '@angular/router';
import {WithRelations} from '../relations-count.model';
import {buildRelationsTooltip} from '../relations-count.util';

type CatalogRow = ICatalogEntry & WithRelations;

type SortableKeys = 'title' | 'identifiers' | 'publisherName' | 'registrationStatus' | 'relationsCount';

@Component({
	selector: 'app-search-result-table',
	templateUrl: './search-result-table.component.html',
	styleUrls: ['./search-result-table.component.scss'],
	standalone: false
})
export class SearchResultTableComponent extends SortableListViewComponent<CatalogRow, SortableKeys> implements OnInit, OnDestroy, OnChanges {
	@Input() dto!: CatalogEntry[];

	readonly COLUMN_TITLE = 'title';
	readonly COLUMN_PUBLISHERNAME = 'publisherName';
	readonly COLUMN_TYPE = 'type';
	readonly COLUMN_CONCEPTTYPE = 'conceptType';
	readonly COLUMN_VERSION = 'version';
	readonly COLUMN_STATUS = 'registrationStatus';
	readonly COLUMN_ACTIONS = 'actions';
	readonly COLUMN_VALID_FROM = 'validFrom';
	readonly COLUMN_VALID_TO = 'validTo';
	readonly COLUMN_ACCESSRIGHTS = 'accessRights';
	readonly COLUMN_RELATIONS = 'relationsCount';

	displayedColumns: string[] = [this.COLUMN_TITLE, this.COLUMN_PUBLISHERNAME, this.COLUMN_TYPE, this.COLUMN_VERSION, this.COLUMN_STATUS, this.COLUMN_ACTIONS];

	currentLanguage: string;
	pagingInfo: SearchResultPagingInfo = new SearchResultPagingInfo(undefined);
	dataSource = new MatTableDataSource<CatalogRow>([]);

	private readonly unsubscribe$ = new Subject();

	private readonly dateFormatService = inject(DateFormatService);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);

	constructor() {
		super();

		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});

		this.router.events
			.pipe(
				takeUntil(this.unsubscribe$),
				filter((e): e is NavigationEnd => e instanceof NavigationEnd)
			)
			.subscribe(_ => {
				this.updateVisibleColumns();
			});
		this.updateVisibleColumns();
	}

	ngOnChanges(changes: SimpleChanges) {
		const change = changes.dto;
		if (change.currentValue !== undefined) {
			this.data = this.dto;
		}
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	isDataSet(catalogEntry: CatalogEntry): boolean {
		return catalogEntry.type === 'Dataset';
	}

	isDataService(catalogEntry: CatalogEntry): boolean {
		return catalogEntry.type === 'DataService';
	}

	isPublicService(catalogEntry: CatalogEntry): boolean {
		return catalogEntry.type === 'PublicService';
	}

	getDetailUrl(catalogEntry: CatalogEntry): string {
		let routerLink = '';

		switch (catalogEntry.type) {
			case SearchResourceType.Dataset:
				routerLink = `../datasets/${catalogEntry.identifiers[0]}`;
				break;
			case SearchResourceType.DataService:
				routerLink = `../dataservices/${catalogEntry.identifiers[0]}`;
				break;
			case SearchResourceType.PublicService:
				routerLink = `../publicservices/${catalogEntry.identifiers[0]}`;
				break;
			case SearchResourceType.Concept:
				routerLink = `../concepts/${catalogEntry.id}`;
				break;
			case SearchResourceType.MappingTable:
				routerLink = `../mappingtables/${catalogEntry.id}`;
				break;
			default:
				break;
		}

		return routerLink;
	}

	getFormattedDate(date: Date): string | undefined {
		return date ? this.dateFormatService.formatShortDate(date) : undefined;
	}

	getConceptType(conceptType: ConceptType): string {
		const conceptTypeEnum = ConceptType;
		let type: string = '';

		if (conceptType) {
			type = conceptTypeEnum[conceptType].toLocaleLowerCase();
		}

		return type;
	}

	getRelationsTooltip(element: CatalogRow): string {
		return buildRelationsTooltip(element?.relations, this.translate);
	}

	private updateVisibleColumns() {
		switch (this.route.parent.snapshot.url[1].path) {
			case 'publicservices':
				this.displayedColumns = [this.COLUMN_TITLE, this.COLUMN_PUBLISHERNAME, this.COLUMN_TYPE, this.COLUMN_STATUS, this.COLUMN_RELATIONS, this.COLUMN_ACTIONS];
				break;
			case 'concepts':
				this.displayedColumns = [
					this.COLUMN_TITLE,
					this.COLUMN_PUBLISHERNAME,
					this.COLUMN_TYPE,
					this.COLUMN_CONCEPTTYPE,
					this.COLUMN_STATUS,
					this.COLUMN_RELATIONS,
					this.COLUMN_VERSION,
					this.COLUMN_VALID_FROM,
					this.COLUMN_VALID_TO,
					this.COLUMN_ACTIONS
				];
				break;
			case 'datasets':
			case 'dataservices':
				this.displayedColumns = [
					this.COLUMN_TITLE,
					this.COLUMN_PUBLISHERNAME,
					this.COLUMN_TYPE,
					this.COLUMN_VERSION,
					this.COLUMN_STATUS,
					this.COLUMN_ACCESSRIGHTS,
					this.COLUMN_RELATIONS,
					this.COLUMN_ACTIONS
				];
				break;
			case 'mappingtables':
				this.displayedColumns = [
					this.COLUMN_TITLE,
					this.COLUMN_PUBLISHERNAME,
					this.COLUMN_TYPE,
					this.COLUMN_STATUS,
					this.COLUMN_VERSION,
					this.COLUMN_VALID_FROM,
					this.COLUMN_VALID_TO,
					this.COLUMN_RELATIONS,
					this.COLUMN_ACTIONS
				];
				break;

			case 'all':
			default:
				// eslint-disable-next-line max-len
				this.displayedColumns = [this.COLUMN_TITLE, this.COLUMN_PUBLISHERNAME, this.COLUMN_TYPE, this.COLUMN_VERSION, this.COLUMN_STATUS, this.COLUMN_RELATIONS, this.COLUMN_ACTIONS];
				break;
		}
	}
}
