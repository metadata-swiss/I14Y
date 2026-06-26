import {
	DcatQualifiedAttributionInputModel,
	IdentifierInputModel,
	CodeInputModel,
	IDcatQualifiedAttributionInputModel,
	DcatQualifiedAttributionModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class DcatQualifiedAttributionInputModelMapper {
	public static mapElements(elements: IDcatQualifiedAttributionInputModel[]): DcatQualifiedAttributionInputModel[] {
		return elements?.map(
			x =>
				new DcatQualifiedAttributionInputModel({
					agent: new IdentifierInputModel({
						identifier: x.agent?.identifier
					}),
					hadRole: new CodeInputModel({
						code: x.hadRole?.code
					})
				})
		);
	}

	public static mapToInputModel(dto: DcatQualifiedAttributionModel): DcatQualifiedAttributionInputModel {
		return new DcatQualifiedAttributionInputModel({
			agent: dto.agent?.identifier
				? new IdentifierInputModel({
						identifier: dto.agent?.identifier
				  })
				: undefined,
			hadRole: dto.hadRole?.code
				? new CodeInputModel({
						code: dto.hadRole?.code
				  })
				: undefined
		});
	}
}
