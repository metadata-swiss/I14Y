import {NavNode, NodeType} from './navigation-stack.model';
import {CATALOG_SEGMENT, CATALOG_TABS, DISTRIBUTIONS_SEGMENT, HOME_SEGMENT, KEY_SEPARATOR, METASEARCH_TAB, TAB_TO_TYPE} from './navigation-stack.constants';

const ROOT_INDEX = 1;
const TAB_INDEX = 2;
const RESOURCE_ID_INDEX = 3;

function buildResourceKey(type: NodeType, id: string): string {
	return `${type}${KEY_SEPARATOR}${id}`;
}

/**
 * Maps a (language-prefixed) router URL to the page that should appear on the navigation stack, or
 * `null` when the URL should not get its own entry (root redirect, non-catalog pages, unknown routes).
 */
export function classifyUrl(url: string): NavNode | null {
	const path = url.split('?')[0].split('#')[0];
	const segments = path.split('/').filter(Boolean);

	const root = segments[ROOT_INDEX];
	if (root === HOME_SEGMENT) {
		return {key: NodeType.Home, type: NodeType.Home, url};
	}
	if (root !== CATALOG_SEGMENT) {
		return null;
	}

	const tab = segments[TAB_INDEX];

	if (tab === METASEARCH_TAB) {
		return {key: NodeType.Catalog, type: NodeType.Catalog, url};
	}

	// Catalog list (`/<lang>/catalog` or `/<lang>/catalog/<tab>`): no resource id segment present.
	if (segments.length <= RESOURCE_ID_INDEX) {
		return tab === undefined || CATALOG_TABS.includes(tab) ? {key: NodeType.Catalog, type: NodeType.Catalog, url} : null;
	}

	const type = TAB_TO_TYPE[tab];
	if (!type) {
		return null;
	}

	// Distribution detail: `/<lang>/catalog/datasets/<id>/description/distributions/<distributionId>`.
	const distributionIndex = segments.indexOf(DISTRIBUTIONS_SEGMENT);
	if (type === NodeType.Dataset && distributionIndex >= 0 && segments[distributionIndex + 1]) {
		return {key: buildResourceKey(NodeType.Distribution, segments[distributionIndex + 1]), type: NodeType.Distribution, url};
	}

	return {key: buildResourceKey(type, segments[RESOURCE_ID_INDEX]), type, url};
}

/**
 * Applies the FR stack rules for a freshly visited page. Pure — no Angular / storage deps.
 *
 * - Home resets the stack to its base.
 * - Catalog appears at most once: revisiting it truncates back to the existing entry (refreshing
 *   its URL so the latest filters/tab win).
 * - A resource already on the stack collapses the stack down to it (circular dependency, FR ex. 1).
 * - A resource of the same type as the current top replaces that top (FR ex. 2, e.g. PS1 → PS2).
 * - Otherwise the resource is pushed.
 */
export function reduceStack(stack: NavNode[], node: NavNode): NavNode[] {
	const entry: NavNode = {...node};

	if (node.type === NodeType.Home) {
		return [entry];
	}

	if (node.type === NodeType.Catalog) {
		const catalogIndex = stack.findIndex(item => item.type === NodeType.Catalog);
		if (catalogIndex >= 0) {
			const truncated = stack.slice(0, catalogIndex + 1);
			truncated[catalogIndex] = entry;
			return truncated;
		}
		return [...stack, entry];
	}

	// Circular dependency: collapse the stack down to the existing entry.
	const existingIndex = stack.findIndex(item => item.key === node.key);
	if (existingIndex >= 0) {
		const truncated = stack.slice(0, existingIndex + 1);
		truncated[existingIndex] = entry;
		return truncated;
	}

	// Consecutive navigation between two resources of the same type collapses to one entry.
	const top = stack[stack.length - 1];
	if (top && top.type === node.type) {
		return [...stack.slice(0, -1), entry];
	}

	return [...stack, entry];
}
