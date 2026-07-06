import {QualifiedRelation, Resource, VocabularyEntry} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {MultiLanguageMapper} from './multilanguagemapper';

export class QualifiedRelationMapper {
	public static mapElements(elements: QualifiedRelation[]): QualifiedRelation[] {
		return elements?.map(
			x =>
				new QualifiedRelation({
					hadRole: new VocabularyEntry({
						code: x.hadRole?.code,
						name: MultiLanguageMapper.clone(x?.hadRole?.name)
					}),
					relation: new Resource({
						href: x.relation?.href,
						label: MultiLanguageMapper.clone(x?.relation?.label)
					})
				})
		);
	}
}
