import {IdModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class IdModelMapper {
	public static mapElements(elements: Array<any>): IdModel[] {
		return elements?.map(x => new IdModel({id: x.id})).filter((x: IdModel) => x?.id?.length > 0);
	}
}
