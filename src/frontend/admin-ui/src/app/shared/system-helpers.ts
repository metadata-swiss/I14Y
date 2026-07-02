import {CreationType} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export function isAutomatedCreation(model: {system?: {creationType?: CreationType}} | undefined | null): boolean {
	return model?.system?.creationType === CreationType.Automated;
}
