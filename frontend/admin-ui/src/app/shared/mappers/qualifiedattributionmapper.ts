import {QualifiedAttribution, VocabularyEntry, Agent} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {MultiLanguageMapper} from './multilanguagemapper';

export class QualifiedAttributionMapper {
	public static mapElements(elements: QualifiedAttribution[]): QualifiedAttribution[] {
		return elements?.map(
			x =>
				new QualifiedAttribution({
					agent: new Agent({
						id: x.agent?.id,
						name: MultiLanguageMapper.clone(x?.agent?.name)
					}),
					hadRole: new VocabularyEntry({
						code: x.hadRole?.code,
						name: MultiLanguageMapper.clone(x?.hadRole?.name)
					})
				})
		);
	}
}
