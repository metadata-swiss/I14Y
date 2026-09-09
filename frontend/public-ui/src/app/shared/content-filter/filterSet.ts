import {MultiLanguageModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {FilterValue} from './filterValue';

export class FilterSet {
	description: MultiLanguageModel | undefined;
	descriptionKey?: string;
	name: MultiLanguageModel | undefined;
	nameKey?: string;
	param: string;
	values: FilterValue[];

	constructor(description: MultiLanguageModel | undefined, name: MultiLanguageModel | undefined, param: string, values: FilterValue[]) {
		this.description = description;
		this.name = name;
		this.param = param;
		this.values = values;

		this.values.forEach(v => v.setParentParamName(this.param));
	}

	getNameKey(): string {
		return this.nameKey ?? `i18n.off_canvas_filter.name.${this.param.toLowerCase()}`;
	}

	getDescriptionKey(): string {
		return this.descriptionKey ?? `i18n.off_canvas_filter.description.${this.param.toLowerCase()}`;
	}
}
