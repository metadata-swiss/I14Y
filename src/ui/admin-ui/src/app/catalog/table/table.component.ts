import {Component, inject, Input, OnChanges, OnDestroy, OnInit, SimpleChanges} from '@angular/core';
import {LangChangeEvent} from '@ngx-translate/core';
import {CatalogEntry, SearchResourceType, ConceptType, ICatalogEntry, RelationsCountModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {SortableListViewComponent} from '../../shared/sortable-list-view/sortable-list-view.component';
import {filter, takeUntil} from 'rxjs/operators';
import {Subject} from 'rxjs';
import {SearchResultPagingInfo} from '../../shared/searchResultPagingInfo';
import {MatTableDataSource} from '@angular/material/table';
import {ActivatedRoute, NavigationEnd, Router} from '@angular/router';
import {FormatFunctions} from 'src/app/shared/format-functions';

type CatalogRow = ICatalogEntry & {
	relationsCount?: number;
	relations?: RelationsCountModel;
	relationsLoading?: boolean;
};

type SortableKeys = 'title' | 'identifiers' | 'publisherName' | 'registrationStatus' | 'publicationLevel' | 'relationsCount';

@Component({
	selector: 'app-table',
	templateUrl: './table.component.html',
	styleUrls: ['./table.component.scss'],
	standalone: false
})
export class TableComponent extends SortableListViewComponent<CatalogRow, SortableKeys> implements OnInit, OnDestroy, OnChanges {
	@Input() dto: CatalogRow[] = [];
	isTableView: boolean = true;

	readonly COLUMN_TITLE = 'title';
	readonly COLUMN_PUBLISHERNAME = 'publisherName';
	readonly COLUMN_STATUS = 'registrationStatus';
	readonly COLUMN_PUBLICATION = 'publicationLevel';
	readonly COLUMN_TYPE = 'type';
	readonly COLUMN_CONCEPTTYPE = 'conceptType';
	readonly COLUMN_VERSION = 'version';
	readonly COLUMN_ACTIONS = 'actions';
	readonly COLUMN_VALID_FROM = 'validFrom';
	readonly COLUMN_VALID_TO = 'validTo';
	readonly COLUMN_ACCESSRIGHTS = 'accessRights';
	readonly COLUMN_RELATIONS = 'relationsCount';

	displayedColumns: string[] = [
		this.COLUMN_TITLE,
		this.COLUMN_PUBLISHERNAME,
		this.COLUMN_TYPE,
		this.COLUMN_STATUS,
		this.COLUMN_PUBLICATION,
		this.COLUMN_VERSION,
		this.COLUMN_ACTIONS
	];

	currentLanguage: string;
	pagingInfo: SearchResultPagingInfo = new SearchResultPagingInfo(undefined);
	dataSource = new MatTableDataSource<ICatalogEntry | any>([]);

	private readonly unsubscribe$ = new Subject();

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

	setTableView() {
		this.isTableView = true;
	}

	setListView() {
		this.isTableView = false;
	}

	isConcept(catalogEntry: CatalogEntry): boolean {
		return catalogEntry.type === 'Concept';
	}

	/**
	 * Builds the Relations breakdown tooltip listing only the sub-counts that apply to the row's
	 * resource type (e.g. concepts show structure-attributes + mapping-tables, datasets show
	 * data-services + public-services).
	 */
	getRelationsTooltip(element: ICatalogEntry & {relations?: RelationsCountModel}): string {
		const relations = element?.relations;
		if (!relations) {
			return '';
		}

		const parts: string[] = [];
		const append = (count: number | null | undefined, key: string) => {
			if (count !== null && count !== undefined) {
				parts.push(`${this.translate.instant(key)}: ${count}`);
			}
		};

		append(relations.structureAttribute, 'i18n.catalog.table.column.relations.tooltip.structure');
		append(relations.mappingTable, 'i18n.catalog.table.column.relations.tooltip.mappingtables');
		append(relations.dataService, 'i18n.catalog.table.column.relations.tooltip.dataservice');
		append(relations.publicService, 'i18n.catalog.table.column.relations.tooltip.publicservice');
		append(relations.dataset, 'i18n.catalog.table.column.relations.tooltip.dataset');

		return parts.join(', ');
	}

	getRouterLink(catalogEntry: CatalogEntry): string {
		let routerLink = '';

		switch (catalogEntry.type) {
			case SearchResourceType.Dataset:
				routerLink = `../datasets/${catalogEntry.id}`;
				break;
			case SearchResourceType.DataService:
				routerLink = `../dataservices/${catalogEntry.id}`;
				break;
			case SearchResourceType.PublicService:
				routerLink = `../publicservices/${catalogEntry.id}`;
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

	getType(catalogEntry: CatalogEntry): string | undefined {
		let type: string | undefined;

		if (catalogEntry?.type) {
			type = catalogEntry.type.toLocaleLowerCase();
		}

		return type;
	}

	getFormattedDate(date: Date | undefined): string | null {
		return FormatFunctions.getFormattedDate(date, this.translate);
	}

	getConceptType(conceptType: ConceptType): string {
		const conceptTypeEnum = ConceptType;
		let type: string = '';

		if (conceptType) {
			type = conceptTypeEnum[conceptType].toLocaleLowerCase();
		}

		return type;
	}

	private updateVisibleColumns() {
		switch (this.route.snapshot.url[0].path) {
			case 'publicservices':
				this.displayedColumns = [
					this.COLUMN_TITLE,
					this.COLUMN_PUBLISHERNAME,
					this.COLUMN_TYPE,
					this.COLUMN_STATUS,
					this.COLUMN_PUBLICATION,
					this.COLUMN_RELATIONS,
					this.COLUMN_ACTIONS
				];
				break;
			case 'concepts':
				this.displayedColumns = [
					this.COLUMN_TITLE,
					this.COLUMN_PUBLISHERNAME,
					this.COLUMN_TYPE,
					this.COLUMN_CONCEPTTYPE,
					this.COLUMN_STATUS,
					this.COLUMN_PUBLICATION,
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
					this.COLUMN_STATUS,
					this.COLUMN_PUBLICATION,
					this.COLUMN_ACCESSRIGHTS,
					this.COLUMN_RELATIONS,
					this.COLUMN_VERSION,
					this.COLUMN_ACTIONS
				];
				break;
			case 'mappingtables':
				this.displayedColumns = [
					this.COLUMN_TITLE,
					this.COLUMN_PUBLISHERNAME,
					this.COLUMN_TYPE,
					this.COLUMN_STATUS,
					this.COLUMN_PUBLICATION,
					this.COLUMN_RELATIONS,
					this.COLUMN_VALID_FROM,
					this.COLUMN_VALID_TO,
					this.COLUMN_ACTIONS
				];
				break;
			case 'all':
			default:
				this.displayedColumns = [
					this.COLUMN_TITLE,
					this.COLUMN_PUBLISHERNAME,
					this.COLUMN_TYPE,
					this.COLUMN_STATUS,
					this.COLUMN_PUBLICATION,
					this.COLUMN_RELATIONS,
					this.COLUMN_VERSION,
					this.COLUMN_ACTIONS
				];
				break;
		}
	}
}
