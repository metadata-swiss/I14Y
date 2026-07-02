// Types for the navigation stack that powers the back button.

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

export interface NavNode {
	key: string;
	type: NodeType;
	url: string;
}
