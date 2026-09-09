import {MultiLanguageModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class FilterValue {
	name: MultiLanguageModel | undefined;
	nameKey?: string;
	value: string;
	private parentParamName: string;

	constructor(name: MultiLanguageModel | undefined, value: string) {
		this.name = name;
		this.value = value;
	}

	setParentParamName(parentParamName: string) {
		this.parentParamName = parentParamName.toLowerCase();
	}

	getNameKeyForContentTab(): string {
		return this.nameKey ?? `i18n.off_canvas_filter.${this.parentParamName}.name.value.${this.value}`;
	}

	getNameKeyForApiTab(): string {
		return this.nameKey ?? `i18n.off_canvas_filter.${this.parentParamName}.name.value.${this.value}`;
	}
}
