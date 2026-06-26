import {MappingTableInputModel, MappingTableModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class MappingTableInputMapper {
	public static mapToInputModel(dto: MappingTableModel): MappingTableInputModel {
		return new MappingTableInputModel({
			conformsTo: dto.conformsTo,
			description: dto.description,
			identifiers: dto.identifiers,
			keywords: dto.keywords,
			name: dto.name,
			publisher: dto.publisher,
			responsibleDeputy: dto.responsibleDeputy,
			responsiblePerson: dto.responsiblePerson,
			source: dto.source,
			target: dto.target,
			themes: dto.themes,
			validFrom: dto.validFrom!,
			validTo: dto.validTo,
			version: dto.version
		});
	}
}
