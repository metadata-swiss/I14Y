import {NavNode, NodeType} from './navigation-stack.model';

// Router path segments and helpers used to classify a URL into a navigation-stack page.

/** Supported language prefixes (the first URL segment). */
export const LANGUAGES: readonly string[] = ['de', 'fr', 'it', 'en'];

/** Fallback language when a URL has no recognizable language prefix. */
export const DEFAULT_LANGUAGE = 'de';

/** Root segment of the home page (`/<lang>/home`). */
export const HOME_SEGMENT = 'home';

/** Root segment of every catalog page (`/<lang>/catalog/...`). */
export const CATALOG_SEGMENT = 'catalog';

/** Catalog tab that only shows the result list and has no detail page of its own. */
export const CATALOG_ALL_TAB = 'all';

/** Catalog tab holding the external metasearch integrations (`metasearch/geocat|opendata`). */
export const METASEARCH_TAB = 'metasearch';

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

/** Extracts the language prefix from a router URL, falling back to the default. */
export function languageOf(url: string): string {
	const [segment] = url.split('?')[0].split('#')[0].split('/').filter(Boolean);
	return segment && LANGUAGES.includes(segment) ? segment : DEFAULT_LANGUAGE;
}

/** The Home base entry for a given language. */
export function homeEntry(lang: string): NavNode {
	return {key: NodeType.Home, type: NodeType.Home, url: `/${lang}/${HOME_SEGMENT}`};
}

/** The Catalog base entry for a given language (defaults to the "all" tab). */
export function catalogEntry(lang: string): NavNode {
	return {key: NodeType.Catalog, type: NodeType.Catalog, url: `/${lang}/${CATALOG_SEGMENT}/${CATALOG_ALL_TAB}`};
}
