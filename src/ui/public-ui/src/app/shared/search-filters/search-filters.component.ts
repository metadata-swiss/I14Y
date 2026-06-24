import {Component, EventEmitter, inject, Input, OnDestroy, OnInit, Output, ViewChildren} from '@angular/core';
import {ActivatedRoute, Router, NavigationEnd} from '@angular/router';
import {
	FilterCountResult,
	SwaggerResponse,
	FilterCountResultItem,
	CatalogClient,
	ConceptType,
	SearchResourceType,
	SearchStructureOption
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {skip, takeUntil, filter, Subject, Observable, Subscription} from 'rxjs';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {FilterMultiSelectDropdownComponent} from './filter-multiselect-dropdown/filter-multiselect-dropdown.component';
import {SearchFilterService} from './search-filters.service';
import {SearchFilters, SearchType} from './search-filters';
import {quoteQueryIfEmail} from 'src/app/shared/search/email-query.util';

@Component({
	selector: 'app-search-filters',
	templateUrl: './search-filters.component.html',
	styleUrls: ['./search-filters.component.scss'],
	standalone: false
})
export class SearchFiltersComponent implements OnInit, OnDestroy {
	filters!: {[key: string]: FilterCountResultItem[]};
	selectedValues!: {[key: string]: string[]};
	orderedKeys: string[] = [];

	@Input() hideFilters: boolean = true;
	@Input() searchType: SearchType | undefined;
	@Input() updateFilter: Observable<void> | undefined;
	@Output() countResultChange: EventEmitter<FilterCountResult> = new EventEmitter();

	@ViewChildren(FilterMultiSelectDropdownComponent) private readonly multiSelects!: FilterMultiSelectDropdownComponent[];

	private updateSubscription: Subscription | undefined;
	private readonly unsubscribe$ = new Subject();

	private readonly catalogClient = inject(CatalogClient);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);
	private readonly fallbackPipe = inject(FallbackPipe);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly searchFilterService = inject(SearchFilterService);
	private readonly translate = inject(TranslateService);

	ngOnInit(): void {
		this.translate.onLangChange.subscribe(_ => this.loadFilters());
		this.route.queryParams.pipe(skip(1)).subscribe(_ => this.setFilters(this.filters));

		this.router.events
			.pipe(
				takeUntil(this.unsubscribe$),
				filter((e): e is NavigationEnd => e instanceof NavigationEnd)
			)
			.subscribe(_ => {
				this.loadFilters();
			});

		if (this.updateFilter) {
			this.updateSubscription = this.updateFilter.subscribe(() => this.loadFilters());
		}
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();

		if (this.updateSubscription) {
			this.updateSubscription.unsubscribe();
		}
	}

	toggleFilterVisibility() {
		this.hideFilters = !this.hideFilters;
	}

	selectionChange(section: string, selectedValues: string[]) {
		let deselected = this.getAllDeselected(this.selectedValues[section], selectedValues);

		if (deselected.length > 0) {
			deselected.forEach(item => {
				this.toggleFilter(section, item);
			});
			this.multiSelects.find(m => m.section === section)?.multiselectForm.controls.selectedOptions.setValue(this.selectedValues[section]);
		}

		selectedValues.forEach(value => {
			if (!this.selectedValues[section].includes(value)) {
				this.toggleFilter(section, value);
			}
		});
	}

	toggleFilter(section: string, value: string) {
		const i = this.selectedValues[section].indexOf(value);
		if (i > -1) {
			this.selectedValues[section].splice(i, 1);
		} else {
			this.selectedValues[section][this.selectedValues[section].length] = value;
		}
		this.navigateToFilterState();
	}

	toggleSection(section: string) {
		if (this.isEntireSectionSelected(section)) {
			this.selectedValues[section] = [];
		} else {
			this.selectedValues[section] = this.filters[section].map(x => x.reference!);
		}

		this.navigateToFilterState();
	}

	isIndeterminate(section: string) {
		return this.selectedValues[section].length > 0 && this.selectedValues[section].length !== this.filters[section].length;
	}

	isEntireSectionSelected(section: string): boolean {
		return this.selectedValues[section].length === this.filters[section].length;
	}

	resetAllFilter() {
		this.orderedKeys.forEach(key => {
			this.selectedValues[key] = [];
			this.multiSelects.find(multiselect => multiselect.section === key)?.resetAllSelections();
		});

		this.navigateToFilterState();
	}

	resetFilter(section: string) {
		this.selectedValues[section] = [];
		this.navigateToFilterState();
	}

	onChipRemove(section: string, toRemove: string) {
		this.toggleFilter(section, toRemove);
		this.multiSelects.find(m => m.section === section)?.multiselectForm.controls.selectedOptions.setValue(this.selectedValues[section]);
	}

	toggleAllSections() {
		if (this.areAllSectionSelected()) {
			this.orderedKeys.forEach(x => (this.selectedValues[x] = []));
		} else {
			this.orderedKeys.forEach(x => (this.selectedValues[x] = this.filters[x].map(c => c.reference!)));
		}
		this.navigateToFilterState();
	}

	areAllSectionsIndeterminate() {
		return this.orderedKeys.some(x => this.isIndeterminate(x));
	}

	areAllSectionSelected(): boolean {
		return this.orderedKeys.every(x => this.selectedValues[x].length === this.filters[x].length);
	}

	onDeselectAll() {
		if (this.searchType) {
			const keys = SearchFilterService.getOrderedKeys(this.searchType);
			Object.assign(this.selectedValues, ...keys.map(k => ({[k]: []})));
			this.navigateToFilterState();
		}
	}

	getFilterSection(section: string) {
		let sectionText;

		this.translate.get(`i18n.filters.${section.toLowerCase()}`).subscribe(result => {
			sectionText = result;
		});

		return `${sectionText}: `;
	}

	getFilterLabel(section: string, selected: string) {
		const selectedFilter = this.filters[section].find(x => x.reference === selected);
		const selectedFilterLabel = this.fallbackPipe.transform(selectedFilter?.label, this.translate.getCurrentLang());
		let label;

		if (selectedFilterLabel) {
			label = `${selectedFilterLabel} (${selectedFilter?.count})`;
		} else {
			const reference = selectedFilter?.reference?.toLowerCase();
			if (reference) {
				this.translate.get(this.searchFilterService.getI18n(section, reference)).subscribe(result => {
					label = `${result} (${selectedFilter?.count})`;
				});
			}
		}

		return label;
	}

	hasSelectedFilters(): boolean {
		let hasSelectedFilters = false;
		this.orderedKeys.forEach(key => {
			hasSelectedFilters = hasSelectedFilters || this.selectedValues[key].length > 0;
		});

		return hasSelectedFilters;
	}

	getSelectedFilters(section: string) {
		return this.filters[section].filter(item => {
			return this.selectedValues[section].includes(item.reference!);
		});
	}

	getDisplaySelectAllOption(section: string): boolean {
		switch (section) {
			case SearchFilters.KeyAccessRights:
			case SearchFilters.KeyBusinessEvents:
			case SearchFilters.KeyFormats:
			case SearchFilters.KeyLevels:
			case SearchFilters.KeyLevelProposals:
			case SearchFilters.KeyLifeEvents:
			case SearchFilters.KeyPublisher:
			case SearchFilters.KeyStatuses:
			case SearchFilters.KeyStatusProposals:
			case SearchFilters.KeyThemes:
			case SearchFilters.KeyTypes:
			case SearchFilters.KeyConceptTypes:
				return true;
			case SearchFilters.KeyStructure:
				return false;
		}
		return false;
	}

	private loadFilters() {
		if (this.searchType) {
			const filters = this.searchFilterService.getSelectedFilters(this.searchType);

			this.loadFilterCounter(filters);
			this.loadTabCounter(filters);
		}
	}

	private loadTabCounter(filters: SearchFilters) {
		// Background count call — keep the global Oblique master loader down (the tab counts have no
		// inline indicator that should block the page). Armed for the next request fired below.
		this.obHttpApiInterceptorEvents.deactivateSpinnerOnNextAPICalls(1);
		this.catalogClient // eslint-disable-next-line max-len
			.getSearchcountByQueryAndAccessRightsAndConceptValueTypesAndBusinessEventsAndFormatsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypes(
				quoteQueryIfEmail(this.route.snapshot.queryParamMap.get('query')),
				filters.accessRights ?? undefined,
				filters.conceptTypes.map(t => t as ConceptType) ?? undefined,
				filters.businessEvents ?? undefined,
				filters.formats ?? undefined,
				filters.levels ?? undefined,
				filters.levelProposals ?? undefined,
				filters.lifeEvents ?? undefined,
				filters.publishers ?? undefined,
				filters.statuses ?? undefined,
				filters.statusProposals ?? undefined,
				(filters.structure as SearchStructureOption) ?? undefined,
				filters.themes ?? undefined,
				undefined
			)
			.subscribe((x: SwaggerResponse<FilterCountResult>) => {
				this.countResultChange.emit(x.result);
			});
	}

	private loadFilterCounter(filters: SearchFilters) {
		// Background count call — keep the global Oblique master loader down (the filter counts have no
		// inline indicator that should block the page). Armed for the next request fired below.
		this.obHttpApiInterceptorEvents.deactivateSpinnerOnNextAPICalls(1);
		this.catalogClient // eslint-disable-next-line max-len
			.getSearchcountByQueryAndAccessRightsAndConceptValueTypesAndBusinessEventsAndFormatsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypes(
				quoteQueryIfEmail(this.route.snapshot.queryParamMap.get('query')),
				filters.accessRights ?? undefined,
				filters.conceptTypes.map(t => t as ConceptType) ?? undefined,
				filters.businessEvents ?? undefined,
				filters.formats ?? undefined,
				filters.levels ?? undefined,
				filters.levelProposals ?? undefined,
				filters.lifeEvents ?? undefined,
				filters.publishers ?? undefined,
				filters.statuses ?? undefined,
				filters.statusProposals ?? undefined,
				(filters.structure as SearchStructureOption) ?? undefined,
				filters.themes ?? undefined,
				this.getCatalogTypes(filters)
			)
			.subscribe((x: SwaggerResponse<FilterCountResult>) => {
				this.setFiltersFromCountResult(x.result);
			});
	}

	private setFiltersFromCountResult(counters: FilterCountResult): void {
		if (counters.publicationLevelProposals && counters.publicationLevelProposals.length > 0) {
			counters.publicationLevelProposals = counters.publicationLevelProposals.filter(x => x.reference !== '');
		}

		if (counters.registrationStatusProposals && counters.registrationStatusProposals.length > 0) {
			counters.registrationStatusProposals = counters.registrationStatusProposals.filter(x => x.reference !== '');
		}

		if (this.searchType) {
			const filters: {[key: string]: FilterCountResult[]} = {};
			const keys = SearchFilterService.getOrderedKeys(this.searchType);

			keys.forEach(element => {
				filters[element] = SearchFilterService.getKeyFilters(counters, element);
			});
			this.setFilters(filters);
		}
	}

	private setFilters(filters: {[key: string]: FilterCountResult[]}): void {
		if (this.searchType) {
			const keys = SearchFilterService.getOrderedKeys(this.searchType);
			const queryValues = this.getselectedValuesFromQueryParams(keys);
			// selectedValues must be set before filters and keys
			this.selectedValues = queryValues;
			this.filters = filters;
			this.orderedKeys = keys;
		}
	}

	private getselectedValuesFromQueryParams(keys: string[]): {[key: string]: string[]} {
		const queryParams = this.route.snapshot.queryParamMap;
		return Object.assign({}, ...keys.map(k => ({[k]: queryParams.getAll(k)})));
	}

	private navigateToFilterState() {
		const queryParams = Object.assign(this.selectedValues);
		this.router.navigate([], {
			relativeTo: this.route,
			queryParams: {...queryParams, page: null},
			queryParamsHandling: 'merge'
		});
	}

	private getAllDeselected(selected: string[], selectedValues: string[]): string[] {
		let deselected: string[] = [];

		selected.forEach(s => {
			if (!selectedValues.find(f => f === s)) {
				deselected.push(s);
			}
		});

		return deselected;
	}

	private getCatalogTypes(filters: SearchFilters): SearchResourceType[] | undefined {
		switch (this.searchType) {
			case SearchType.Dataset:
				return [SearchResourceType.Dataset];
			case SearchType.Dataservice:
				return [SearchResourceType.DataService];
			case SearchType.Publicservice:
				return [SearchResourceType.PublicService];
			case SearchType.Concept:
				return [SearchResourceType.Concept];
			case SearchType.MappingTable:
				return [SearchResourceType.MappingTable];
			case SearchType.All:
			default:
				return filters.types?.map(t => t as SearchResourceType) ?? undefined;
		}
	}
}
