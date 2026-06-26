import {NAV_VALUE_CREATE, NAV_VALUE_EDIT, NAV_VALUE_VERSION} from '../../app-constants';
import {NodeType} from './navigation-stack.model';

// Router path segments and helpers used to classify a URL into a navigation-stack page.
// Keeping them here avoids magic strings in the reducer and keeps the URL contract in one place.

/** Root segment of the home page (`/home`). */
export const HOME_SEGMENT = 'home';

/** Root segment of every catalog page (`/catalog/...`). */
export const CATALOG_SEGMENT = 'catalog';

/** Catalog tab that only shows the result list and has no detail page of its own. */
export const CATALOG_ALL_TAB = 'all';

/** Sub-segment of a dataset URL that identifies a distribution detail page. */
export const DISTRIBUTIONS_SEGMENT = 'distributions';

/** Separator between a resource type and its id in a stack-entry key (`dataset:<id>`). */
export const KEY_SEPARATOR = ':';

/** Catalog tab segments mapped to the resource type their detail page represents. */
export const TAB_TO_TYPE: Readonly<Record<string, NodeType>> = {
	datasets: NodeType.Dataset,
	dataservices: NodeType.DataService,
	publicservices: NodeType.PublicService,
	concepts: NodeType.Concept,
	mappingtables: NodeType.MappingTable
};

/** All valid catalog tab segments: the list-only tab plus every resource tab. */
export const CATALOG_TABS: readonly string[] = [CATALOG_ALL_TAB, ...Object.keys(TAB_TO_TYPE)];

/** Sub-route segments that fold into their parent resource instead of getting their own entry. */
export const FOLDED_SEGMENTS: readonly string[] = [NAV_VALUE_EDIT, NAV_VALUE_CREATE, NAV_VALUE_VERSION];

/** Full router URLs of the synthesized base entries (used as the deep-link fallback trail). */
export const HOME_URL = `/${HOME_SEGMENT}`;
export const CATALOG_URL = `/${CATALOG_SEGMENT}/${CATALOG_ALL_TAB}`;
