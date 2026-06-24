import {MultiLanguageModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {FilterValueContainer} from './filterValueContainer';

export class FilterContainer {
	description: MultiLanguageModel | undefined;
	descriptionKey: string;
	name: MultiLanguageModel | undefined;
	nameKey: string;
	param: string;
	values: FilterValueContainer[];

	constructor(
		description: MultiLanguageModel | undefined,
		descriptionKey: string,
		name: MultiLanguageModel | undefined,
		nameKey: string,
		param: string,
		values: FilterValueContainer[]
	) {
		this.description = description;
		this.descriptionKey = descriptionKey;
		this.name = name;
		this.nameKey = nameKey;
		this.param = param;
		this.values = values;
	}
}
