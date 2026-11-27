import {MultiLanguageModel, SearchResourceTypeSearchCountResultItem} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export interface IAgentWithSubagentsResult {
	id: string;
	identifier: string;
	name: MultiLanguageModel;
	types: SearchResourceTypeSearchCountResultItem[];
	subagents: IAgentWithSubagentsResult[];
	collapsed?: boolean;
}
