import {IResource, IResourceModel, Resource} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {MultiLanguageMapper} from './multilanguagemapper';

export class ResourceMapper {
	public static mapElements(elements: IResource[] | IResourceModel[]): Resource[] {
		return elements?.map(
			x =>
				new Resource({
					href: (x as IResource).href ?? (x as IResourceModel).uri,
					label: MultiLanguageMapper.cloneOrUndefined(x?.label)
				})
		);
	}
}
