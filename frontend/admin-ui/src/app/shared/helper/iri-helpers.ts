import {AppConfig} from '../../app.config';
import {IAppConfig} from '../../app.config.interface';
import {I14Y_IRI_URL} from '../../app-constants';

const IRI_PATH = {
	concept:       'concept',
	dataset:       'dataset',
	dataservice:   'dataservice',
	publicservice: 'publicservice',
	mappingtable:  'mappingtable',
} as const;

function getIriBaseUrl(): string {
	return AppConfig.getConfig<IAppConfig>()?.I14Y_IRI_URL ?? I14Y_IRI_URL;
}

export function buildConceptIri(identifier: string, version: string): string {
	return `${getIriBaseUrl()}/${IRI_PATH.concept}/${identifier}/version/${version}`;
}

export function buildConceptPageIri(identifier: string): string {
	return `${getIriBaseUrl()}/${IRI_PATH.concept}/${identifier}`;
}

export function buildConceptCodeIri(identifier: string, code: string, version: string): string {
	return `${getIriBaseUrl()}/${IRI_PATH.concept}/${identifier}/${code}/version/${version}`;
}

export function buildDatasetIri(identifier: string): string {
	return `${getIriBaseUrl()}/${IRI_PATH.dataset}/${identifier}`;
}

export function buildDataServiceIri(identifier: string): string {
	return `${getIriBaseUrl()}/${IRI_PATH.dataservice}/${identifier}`;
}

export function buildPublicServiceIri(identifier: string): string {
	return `${getIriBaseUrl()}/${IRI_PATH.publicservice}/${identifier}`;
}

export function buildMappingTableIri(identifier: string, version: string): string {
	return `${getIriBaseUrl()}/${IRI_PATH.mappingtable}/${identifier}/version/${version}`;
}

export function isLocalIri(uri: string): boolean {
	return uri.startsWith(getIriBaseUrl());
}

export function extractIriVersion(uri: string): string | undefined {
	return uri.includes('/version/') ? uri.match(/\/version\/([^/]+)/)?.[1] : undefined;
}

export function extractIriIdentifier(uri: string): string | undefined {
	const types = Object.values(IRI_PATH).join('|');
	const match = uri.match(new RegExp(`/(${types})/([^/]+)`));
	return match ? match[2] : undefined;
}
