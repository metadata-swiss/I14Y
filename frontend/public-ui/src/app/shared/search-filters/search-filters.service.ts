import {Injectable} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {FilterCountResult, FilterCountResultItem, PublicationLevel, RegistrationStatus} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {SearchFilters, SearchType} from './search-filters';

@Injectable({
	providedIn: 'root'
})
export class SearchFilterService {
	constructor(private readonly route: ActivatedRoute) {}

	public static getKeyFilters(counters: FilterCountResult, key: string): FilterCountResultItem[] {
		switch (key) {
			case SearchFilters.KeyAccessRights:
				return counters.accessRights ?? [];
			case SearchFilters.KeyBusinessEvents:
				return counters.businessEvents ?? [];
			case SearchFilters.KeyFormats:
				return counters.formats ?? [];
			case SearchFilters.KeyLevels:
				return counters.publicationLevels ?? [];
			case SearchFilters.KeyLevelProposals:
				return counters.publicationLevelProposals ?? [];
			case SearchFilters.KeyLifeEvents:
				return counters.lifeEvents ?? [];
			case SearchFilters.KeyPublisher:
				return counters.publishers ?? [];
			case SearchFilters.KeyStatuses:
				return counters.registrationStatuses ?? [];
			case SearchFilters.KeyStatusProposals:
				return counters.registrationStatusProposals ?? [];
			case SearchFilters.KeyStructure:
				return counters.structures ?? [];
			case SearchFilters.KeyThemes:
				return counters.themes ?? [];
			case SearchFilters.KeyTypes:
				return counters.types ?? [];
			case SearchFilters.KeyConceptTypes:
				return counters.conceptValueTypes ?? [];
		}
		return [];
	}

	public static getOrderedKeys(searchType: SearchType): string[] {
		return this.getAllKeys(searchType);
	}

	public static getAllKeys(searchType: SearchType): string[] {
		switch (searchType) {
			case SearchType.All:
				return [
					SearchFilters.KeyTypes,
					SearchFilters.KeyPublisher,
					SearchFilters.KeyThemes,
					SearchFilters.KeyStatuses,
					SearchFilters.KeyConceptTypes,
					SearchFilters.KeyAccessRights,
					SearchFilters.KeyFormats,
					SearchFilters.KeyBusinessEvents,
					SearchFilters.KeyLifeEvents
				];
			case SearchType.Dataset:
				return [
					SearchFilters.KeyPublisher,
					SearchFilters.KeyThemes,
					SearchFilters.KeyStatuses,
					SearchFilters.KeyAccessRights,
					SearchFilters.KeyFormats,
					SearchFilters.KeyStructure
				];
			case SearchType.Dataservice:
				return [SearchFilters.KeyPublisher, SearchFilters.KeyThemes, SearchFilters.KeyStatuses, SearchFilters.KeyAccessRights];
			case SearchType.Publicservice: // eslint-disable-next-line max-len
				return [SearchFilters.KeyPublisher, SearchFilters.KeyThemes, SearchFilters.KeyStatuses, SearchFilters.KeyBusinessEvents, SearchFilters.KeyLifeEvents];
			case SearchType.Concept:
				return [SearchFilters.KeyPublisher, SearchFilters.KeyThemes, SearchFilters.KeyStatuses, SearchFilters.KeyConceptTypes];
			case SearchType.MappingTable:
				return [SearchFilters.KeyPublisher, SearchFilters.KeyThemes, SearchFilters.KeyStatuses];
			default:
				return [];
		}
	}

	public getI18n(key: string, reference: string): string {
		switch (key) {
			case SearchFilters.KeyAccessRights:
				return SearchFilters.i18nAccessRights + reference.replace(' ', '').toLowerCase();
			case SearchFilters.KeyBusinessEvents:
				return SearchFilters.i18nBusinessEvents + reference.replace(' ', '').toLowerCase();
			case SearchFilters.KeyLevels:
			case SearchFilters.KeyLevelProposals:
				return SearchFilters.i18nLevels + reference.replace(' ', '').toLowerCase();
			case SearchFilters.KeyLifeEvents:
				return SearchFilters.i18nLifeEvents + reference.replace(' ', '').toLowerCase();
			case SearchFilters.KeyStatuses:
			case SearchFilters.KeyStatusProposals:
				return SearchFilters.i18nStatus + reference.replace(' ', '').toLowerCase();
			case SearchFilters.KeyStructure:
				return SearchFilters.i18nStructure + reference.replace(' ', '').toLowerCase();
			case SearchFilters.KeyThemes:
				return SearchFilters.i18nThemes + reference.replace(' ', '').toLowerCase();
			case SearchFilters.KeyFormats:
				return SearchFilters.i18nFormats + reference.replace(' ', '').toLowerCase();
			case SearchFilters.KeyTypes:
				return SearchFilters.i18nTypes + reference.replace(' ', '').toLowerCase();
			case SearchFilters.KeyConceptTypes:
				return SearchFilters.i18nConceptTypes + reference.replace(' ', '').toLowerCase();
		}
		return 'i18n.filters.nolabel';
	}

	public getSelectedFilters(searchType: SearchType): SearchFilters {
		const keys = SearchFilterService.getOrderedKeys(searchType);
		return this.getSelectedFiltersByKeys(keys);
	}

	public getSelectedFiltersByKeys(keys: string[]): SearchFilters {
		const filters = new SearchFilters();
		const queryParams = this.route.snapshot.queryParamMap;
		const f = (key: string) => (keys.includes(key) ? queryParams.getAll(key) : []);
		filters.accessRights = f(SearchFilters.KeyAccessRights);
		filters.businessEvents = f(SearchFilters.KeyBusinessEvents);
		filters.formats = f(SearchFilters.KeyFormats);
		filters.levels = f(SearchFilters.KeyLevels).map(item => item as PublicationLevel);
		filters.levelProposals = f(SearchFilters.KeyLevelProposals).map(item => item as PublicationLevel);
		filters.lifeEvents = f(SearchFilters.KeyLifeEvents);
		filters.publishers = f(SearchFilters.KeyPublisher);
		filters.statuses = f(SearchFilters.KeyStatuses).map(item => item as RegistrationStatus);
		filters.statusProposals = f(SearchFilters.KeyStatusProposals).map(item => item as RegistrationStatus);
		filters.structure = f(SearchFilters.KeyStructure)[0];
		filters.themes = f(SearchFilters.KeyThemes);
		filters.types = f(SearchFilters.KeyTypes);
		filters.conceptTypes = f(SearchFilters.KeyConceptTypes);

		return filters;
	}
}
