import {TranslateService} from '@ngx-translate/core';
import {RelationsCountModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

/**
 * Builds the Relations breakdown tooltip, listing only the sub-counts that apply to the row's
 * resource type (concepts: structure-attributes + mapping-tables; datasets: data-services +
 * public-services; data services: datasets; etc.). Shared by the table and list-item views.
 */
export function buildRelationsTooltip(relations: RelationsCountModel | undefined, translate: TranslateService): string {
	if (!relations) {
		return '';
	}

	const parts: string[] = [];
	const append = (count: number | null | undefined, key: string) => {
		if (count !== null && count !== undefined) {
			parts.push(`${translate.instant(key)}: ${count}`);
		}
	};

	append(relations.structureAttribute, 'i18n.catalog.table.column.relations.tooltip.structure');
	append(relations.mappingTable, 'i18n.catalog.table.column.relations.tooltip.mappingtables');
	append(relations.dataService, 'i18n.catalog.table.column.relations.tooltip.dataservice');
	append(relations.publicService, 'i18n.catalog.table.column.relations.tooltip.publicservice');
	append(relations.dataset, 'i18n.catalog.table.column.relations.tooltip.dataset');

	return parts.join(', ');
}
