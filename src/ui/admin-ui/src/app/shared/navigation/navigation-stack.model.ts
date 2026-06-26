// Types for the navigation stack that powers the back button.
// The stack is a list of "pages" the user has visited, with the collapsing rules described in the
// FR (single Catalog entry, circular-dependency collapse, same-type replace).

export enum NodeType {
	Home = 'home',
	Catalog = 'catalog',
	Dataset = 'dataset',
	DataService = 'dataservice',
	PublicService = 'publicservice',
	Concept = 'concept',
	MappingTable = 'mappingtable',
	Distribution = 'distribution'
}

/**
 * A page on the navigation stack, derived from a router URL. `classifyUrl()` returns `null` for
 * URLs that should not get their own stack entry (edit/create forms, sub-tabs, …).
 */
export interface NavNode {
	key: string;
	type: NodeType;
	url: string;
}
