import {IKeywordModel, KeywordModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {MultiLanguageMapper} from './multilanguagemapper';

export class KeywordMapper {
	public static mapElements(elements: IKeywordModel[]): KeywordModel[] {
		return elements?.map(
			x =>
				new KeywordModel({
					label: x?.label ? MultiLanguageMapper.cloneOrUndefined(x?.label) : undefined,
					uri: x?.uri ? x?.uri : undefined
				})
		);
	}
}
