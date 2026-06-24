import {MultiLanguageModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class FilterValueContainer {
	name: MultiLanguageModel | undefined;
	nameKey: string;
	value: string;
	selected: boolean;

	constructor(name: MultiLanguageModel | undefined, nameKey: string, value: string, selected: boolean = false) {
		this.name = name;
		this.nameKey = nameKey;
		this.selected = selected;
		this.value = value;
	}
}
