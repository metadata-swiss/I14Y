import {MappingRelationInputModel, MappingRelationModel, UriInputModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class MappingRelationInputMapper {
	public static mapToInputModel(dto: MappingRelationModel): MappingRelationInputModel {
		return new MappingRelationInputModel({
			source: new UriInputModel({uri: dto.source?.uri}),
			target: new UriInputModel({uri: dto.target?.uri}),
			relationType: dto.relationType
		});
	}
}
