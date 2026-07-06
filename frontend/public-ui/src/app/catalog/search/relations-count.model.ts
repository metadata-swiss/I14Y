import {RelationsCountModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export interface WithRelations {
	relations?: RelationsCountModel;
	relationsCount?: number;
	relationsLoading?: boolean;
}
