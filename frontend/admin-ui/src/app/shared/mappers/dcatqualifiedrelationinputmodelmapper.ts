import {
	CodeInputModel,
	DcatQualifiedRelationInputModel,
	DcatQualifiedRelationModel,
	IDcatQualifiedRelationInputModel,
	ResourceModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {MultiLanguageMapper} from './multilanguagemapper';

export class DcatQualifiedRelationInputModelMapper {
	public static mapElements(elements: IDcatQualifiedRelationInputModel[]): DcatQualifiedRelationInputModel[] {
		return elements?.map(
			x =>
				new DcatQualifiedRelationInputModel({
					hadRole: new CodeInputModel({
						code: x.hadRole?.code
					}),
					relation: new ResourceModel({
						uri: x.relation?.uri,
						label: MultiLanguageMapper.clone(x?.relation?.label ?? undefined)
					})
				})
		);
	}

	public static mapToInputModel(dto: DcatQualifiedRelationModel): DcatQualifiedRelationInputModel {
		return new DcatQualifiedRelationInputModel({
			hadRole: dto.hadRole?.code
				? new CodeInputModel({
						code: dto.hadRole?.code
				  })
				: undefined,
			relation: dto.relation
				? new ResourceModel({
						uri: dto.relation?.uri,
						label: MultiLanguageMapper.clone(dto.relation?.label ?? undefined)
				  })
				: undefined
		});
	}
}
