import {Annotation, AnnotationInputModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {MultiLanguageMapper} from './multilanguagemapper';

export class AnnotationInputModelMapper {
	public static mapToInputModel(dto: Annotation): AnnotationInputModel {
		return new AnnotationInputModel({
			identifier: dto.identifier || undefined,
			text: MultiLanguageMapper.cloneOrUndefined(dto.text),
			title: dto.title || undefined,
			type: dto.type,
			uri: dto.uri || undefined
		});
	}
}
