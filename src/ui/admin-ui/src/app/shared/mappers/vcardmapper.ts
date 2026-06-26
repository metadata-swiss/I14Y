import {IVcard, IVCardModel, Vcard} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {MultiLanguageMapper} from './multilanguagemapper';

export class VcardMapper {
	public static mapElements(elements: IVcard[] | IVCardModel[]): Vcard[] {
		return elements?.map(
			x =>
				new Vcard({
					telWorkVoice: ((x as IVcard).telWorkVoice ?? (x as IVCardModel).hasTelephone) || undefined,
					emailInternet: (x as IVcard).emailInternet ?? (x as IVCardModel).hasEmail,
					adrWork: MultiLanguageMapper.clone((x as IVcard)?.adrWork ?? (x as IVCardModel)?.hasAddress),
					fn: MultiLanguageMapper.clone(x?.fn),
					note: MultiLanguageMapper.clone(x?.note)
				})
		);
	}
}
