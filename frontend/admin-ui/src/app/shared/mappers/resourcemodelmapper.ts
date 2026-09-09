import {IResourceModel, ResourceModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {MultiLanguageMapper} from './multilanguagemapper';

export class ResourceModelMapper {
	public static mapElements(elements: IResourceModel[]): ResourceModel[] {
		return elements?.map(
			x =>
				new ResourceModel({
					uri: x.uri,
					label: MultiLanguageMapper.clone(x?.label ?? undefined)
				})
		);
	}
}
