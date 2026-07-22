export const NAV_PARAM_FROM = 'from';
export const NAV_PARAM_CONTENT_TYPE = 'contentType';
export const NAV_VALUE_EDIT = 'edit';
export const NAV_VALUE_CREATE = 'create';
export const NAV_VALUE_DETAIL = 'detail';
export const NAV_VALUE_DATAELEMENT = 'dataElement';
export const NAV_VALUE_OVERVIEW = 'overview';
export const NAV_VALUE_VERSION = 'newversion';
export const NAV_VALUE_CONTENT_CL = 'cl';
export const NAV_VALUE_CONTENT_HCL = 'hcl';

// Shared Dialog Button Keys
export const DIALOG_OK_BUTTON_KEY = 'i18n.dialoge.buttons.ok';
export const DIALOG_CANCEL_BUTTON_KEY = 'i18n.dialoge.buttons.cancel';
export const DIALOG_CONFIRM_BUTTON_KEY = 'i18n.dialoge.buttons.confirm';
export const DIALOG_DISCARD_CHANGES_BUTTON_KEY = 'i18n.dialoge.buttons.discard_changes';
export const DIALOG_LOCK_BUTTON_KEY = 'i18n.dialoge.buttons.lock';
export const DIALOG_UNLOCK_BUTTON_KEY = 'i18n.dialoge.buttons.unlock';
export const DIALOG_SAVE_CHANGES_BUTTON_KEY = 'i18n.dialoge.buttons.save_changes';
export const DIALOG_CREATE_BUTTON_KEY = 'i18n.dialoge.buttons.create';

export enum ComponentMode {
	Create = 'create',
	Edit = 'edit',
	Version = 'version'
}

export const DEFAULT_VERSION = '1.0.0';
export const VERSION_PATTERN = /^(?:(\d+))(?:\.(\d+))?(?:\.(\*|\d+))?$/;

export const URI_PATTERN = // eslint-disable-next-line max-len
	"(?:(https?|ftp):\\/\\/(?:[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*\\.[a-zA-Z]{2,}|(?:\\[[^\\]]+\\]))(?:\\:[0-9]+)?(?:\\/[^\\s?#]*)?(?:\\?[^#\\s]*)?(?:\\#[^\\s]*)?|ldap:\\/\\/(?:\\[[^\\]]+\\]|[^\\/\\s:?#]+)(?:\\:[0-9]+)?(?:\\/[^\\s?#]*)?((?:\\?[^#\\s]*)?(?:\\#[^\\s]*)?)|mailto:[^\\s?#]+|news:[^\\s?#]+|tel:[^\\s?#]+|telnet:\\/\\/(?:\\[[^\\]]+\\]|[^\\/\\s:?#]+)(?:\\:[0-9]+)?(?:\\/[^\\s?#]*)?(?:\\?[^#\\s]*)?(?:\\#[^\\s]*)?|urn:[a-zA-Z0-9][a-zA-Z0-9-]{1,31}:(?:[a-zA-Z0-9()+,\\-.:=@;$_!*'%/?#]+))";

export const URL_PATTERN = /https?:\/\/[-a-zA-Z0-9@:%._+~#=]{1,256}\.[a-zA-Z0-9]{1,6}\b[-a-zA-Z0-9@:%._+~#?&/=]*/;

export const EMAIL_PATTERN =
	/[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*/;

export const IDENTIFIER_PATTERN = /^[A-Za-z0-9._~@:-]+$/;

export const I14Y_IRI_URL = 'https://register.ld.admin.ch/i14y';

// Vocabulary identifiers
export const VOCAB_ID_DATASET_THEME = 'Concept_DATASET_THEME'; // conceptIdentifier: DV_DCAT_DATASET_THEME
export const VOCAB_ID_ACCESS_RIGHT = 'RightsStatement_ACCESS_RIGHTS'; // conceptIdentifier: CL_DCAT_ACCESS_RIGHT
export const VOCAB_ID_EU_FREQUENCY = 'VOCAB_EU_FREQUENCY'; // conceptIdentifier: VOCAB_EU_FREQUENCY
export const VOCAB_ID_ATTRIBUTION_ROLE = 'VOCAB_I14Y_ATTRIBUTION_ROLE'; // conceptIdentifier: VOCAB_I14Y_ATTRIBUTION_ROLE
export const VOCAB_ID_RELATIONSHIP_ROLE = 'VOCAB_I14Y_RELATIONSHIP_ROLE'; // conceptIdentifier: VOCAB_I14Y_RELATIONSHIP_ROLE
export const VOCAB_ID_LICENSE = 'VOCAB_I14Y_LICENSE'; // conceptIdentifier: VOCAB_I14Y_LICENSE
export const VOCAB_ID_CHECKSUM_ALGORITHM = 'VOCAB_I14Y_CHECKSUM_ALGORITHM'; // conceptIdentifier: VOCAB_I14Y_CHECKSUM_ALGORITHM
export const VOCAB_ID_MEDIA_TYPE = 'VOCAB_I14Y_MEDIA_TYPE'; // conceptIdentifier: VOCAB_I14Y_MEDIA_TYPE
export const VOCAB_ID_PACKAGING_FORMAT = 'VOCAB_I14Y_PACKAGING_FORMAT'; // conceptIdentifier: VOCAB_I14Y_PACKAGING_FORMAT
export const VOCAB_ID_CONFIDENTIALITY_PERSON = 'VOCAB_I14Y_CONFIDENTIALITY_PERSON'; // conceptIdentifier: VOCAB_I14Y_CONFIDENTIALITY_PERSON
export const VOCAB_ID_EU_DATA_THEME = 'VOCAB_EU_DATA_THEME'; // conceptIdentifier: VOCAB_EU_DATA_THEME
export const VOCAB_ID_GEOBASISDATEN = 'VOCAB_GEOBASISDATEN'; // conceptIdentifier: VOCAB_GEOBASISDATEN
export const VOCAB_ID_FILE_TYPE = 'VOCAB_I14Y_FILE_TYPE'; // conceptIdentifier: VOCAB_I14Y_FILE_TYPE
export const VOCAB_ID_EU_PLANNED_AVAILABILITY = 'VOCAB_EU_PLANNED_AVAILABILITY'; // conceptIdentifier: VOCAB_EU_PLANNED_AVAILABILITY
export const VOCAB_ID_BUSINESS_EVENTS = 'VOCAB_BK_BUSINESSEVENTS'; // conceptIdentifier: VOCAB_BK_BUSINESSEVENTS
export const VOCAB_ID_LIFE_EVENTS = 'VOCAB_BK_LIFEEVENTS'; // conceptIdentifier: VOCAB_BK_LIFEEVENTS
export const VOCAB_ID_KT_BEZ_GDE_SNAP = 'DV_KT_BEZ_GDE_SNAP'; // conceptIdentifier: DV_KT_BEZ_GDE_SNAP
export const VOCAB_ID_LEGAL_FORM = 'legalForm'; // conceptIdentifier: legalForm
export const VOCAB_ID_CHANNEL_TYPES = 'EU_Channel_Types'; // conceptIdentifier: VOCAB_I14Y_COMMUNICATION_CHANNEL_TYPE
export const VOCAB_ID_LANGUAGES_ISO_639 = 'Languages_Iso_639'; // conceptIdentifier: VOCAB_I14Y_LANGUAGES_ISO_639_1
export const VOCAB_ID_MAPPING_PREDICATE = 'VOCAB_I14Y_MAPPING_PREDICATE'; // conceptIdentifier: VOCAB_I14Y_MAPPING_PREDICATE

export const FORM_FIELD_TITLE = 'title';
export const FORM_FIELD_DESCRIPTION = 'description';
export const FORM_FIELD_IDENTIFIERS = 'identifiers';
export const FORM_FIELD_IDENTIFIER = 'identifier';
export const FORM_FIELD_VERSION = 'version';
export const FORM_FIELD_VERSION_NOTES = 'versionNotes';
