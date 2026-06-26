import {IVCardModel, VCardKind, VCardModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {MultiLanguageMapper} from './multilanguagemapper';

export class VCardModelMapper {
	public static mapElements(elements: Array<IVCardModel>): VCardModel[] {
		return elements?.map(
			x =>
				new VCardModel({
					hasTelephone: x.hasTelephone || undefined,
					hasEmail: x.hasEmail,
					hasAddress: MultiLanguageMapper.clone(x?.hasAddress),
					fn: MultiLanguageMapper.clone(x?.fn),
					note: MultiLanguageMapper.clone(x?.note),
					kind: VCardKind.Organization
				})
		);
	}
}
