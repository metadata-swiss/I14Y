import {NavNode, NodeType} from './navigation-stack.model';
import {CATALOG_SEGMENT, CATALOG_TABS, DISTRIBUTIONS_SEGMENT, FOLDED_SEGMENTS, HOME_SEGMENT, KEY_SEPARATOR, TAB_TO_TYPE} from './navigation-stack.constants';

// Positions of the meaningful segments in a `/catalog/<tab>/<id>/<sub>/<distributionId>` URL.
const ROOT_INDEX = 0;
const TAB_INDEX = 1;
const RESOURCE_ID_INDEX = 2;
const SUB_SEGMENT_INDEX = 3;
const DISTRIBUTION_ID_INDEX = 4;

function buildResourceKey(type: NodeType, id: string): string {
	return `${type}${KEY_SEPARATOR}${id}`;
}

/**
 * Maps a router URL to the page that should appear on the navigation stack, or `null` when the
 * URL should not get its own entry (root redirect, edit/create forms, unknown routes).
 *
 * Recognised pages: `home`, the catalog list (any tab), the five top-level resources and the
 * dataset distribution detail. Their own sub-tabs (description/structure/…) and edit/create
 * forms fold into the owning resource entry.
 */
export function classifyUrl(url: string): NavNode | null {
	const path = url.split('?')[0].split('#')[0];
	const segments = path.split('/').filter(Boolean);

	if (!segments.length) {
		return null;
	}
	if (segments[ROOT_INDEX] === HOME_SEGMENT) {
		return {key: NodeType.Home, type: NodeType.Home, url};
	}
	if (segments[ROOT_INDEX] !== CATALOG_SEGMENT) {
		return null;
	}

	const tab = segments[TAB_INDEX];

	if (segments.length <= RESOURCE_ID_INDEX) {
		return tab === undefined || CATALOG_TABS.includes(tab) ? {key: NodeType.Catalog, type: NodeType.Catalog, url} : null;
	}

	if (segments.some(segment => FOLDED_SEGMENTS.includes(segment))) {
		return null;
	}

	const type = TAB_TO_TYPE[tab];
	if (!type) {
		return null;
	}

	if (type === NodeType.Dataset && segments[SUB_SEGMENT_INDEX] === DISTRIBUTIONS_SEGMENT && segments[DISTRIBUTION_ID_INDEX]) {
		return {key: buildResourceKey(NodeType.Distribution, segments[DISTRIBUTION_ID_INDEX]), type: NodeType.Distribution, url};
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
