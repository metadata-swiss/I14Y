import {PublicationLevel, RegistrationStatus} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class SearchFilters {
	public static readonly KeyAccessRights: string = 'accessRights';
	public static readonly KeyBusinessEvents: string = 'businessEvents';
	public static readonly KeyFormats: string = 'formats';
	public static readonly KeyLevels: string = 'publicationLevels';
	public static readonly KeyLevelProposals: string = 'levelProposals';
	public static readonly KeyLifeEvents: string = 'lifeEvents';
	public static readonly KeyPublisher: string = 'publisher';
	public static readonly KeyStatuses: string = 'registrationStatuses';
	public static readonly KeyStatusProposals: string = 'statusProposals';
	public static readonly KeyStructure: string = 'structure';
	public static readonly KeyThemes: string = 'themes';
	public static readonly KeyTypes: string = 'types';
	public static readonly KeyConceptTypes: string = 'conceptValueTypes';

	public static readonly i18nAccessRights: string = 'i18n.filters.accessrights.';
	public static readonly i18nBusinessEvents: string = 'i18n.filters.businessevents.';
	public static readonly i18nFormats: string = 'i18n.filters.formats.';
	public static readonly i18nLevels: string = 'i18n.status.publicationlevel.';
	public static readonly i18nLifeEvents: string = 'i18n.filters.lifeevents.';
	public static readonly i18nStatus: string = 'i18n.status.registrationstatus.';
	public static readonly i18nStructure: string = 'i18n.filters.structure.';
	public static readonly i18nThemes: string = 'i18n.filters.themes.';
	public static readonly i18nTypes: string = 'i18n.filters.types.';
	public static readonly i18nConceptTypes: string = 'i18n.filters.concepttypes.';

	public accessRights: string[] = [];
	public businessEvents: string[] = [];
	public formats: string[] = [];
	public levels: PublicationLevel[] = [];
	public levelProposals: PublicationLevel[] = [];
	public lifeEvents: string[] = [];
	public publishers: string[] = [];
	public statuses: RegistrationStatus[] = [];
	public statusProposals: RegistrationStatus[] = [];
	public structure: string;
	public themes: string[] = [];
	public types: string[] = [];
	public conceptTypes: string[] = [];
}

export class SearchType {
	public static readonly All: string = 'all';
	public static readonly Dataset: string = 'dataset';
	public static readonly Publicservice: string = 'publicservice';
	public static readonly Dataservice: string = 'dataservice';
	public static readonly Concept: string = 'concept';
	public static readonly MappingTable: string = 'mappingtable';
}
