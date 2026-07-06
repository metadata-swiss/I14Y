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

	getNameKeyForContentTab(): string {
		return this.nameKey ?? `i18n.datasets.content.filter.name.${this.param.toLowerCase()}`;
	}

	getNameKeyForApiTab(): string {
		return this.nameKey ?? `i18n.datasets.api.filter.name.${this.param.toLowerCase()}`;
	}

	getDescriptionKeyForContentTab(): string {
		return this.descriptionKey ?? `i18n.datasets.content.filter.description.${this.param.toLowerCase()}`;
	}

	getDescriptionKeyForApiTab(): string {
		return this.descriptionKey ?? `i18n.datasets.api.filter.description.${this.param.toLowerCase()}`;
	}
}
