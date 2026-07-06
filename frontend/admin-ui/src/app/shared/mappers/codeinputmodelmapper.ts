import {ICodeInputModel, CodeInputModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class CodeInputModelMapper {
	public static mapElements(elements: ICodeInputModel[] | string[]): CodeInputModel[] {
		return elements?.map(
			x =>
				new CodeInputModel({
					code: (x as ICodeInputModel)?.code ?? (x as string)
				})
		);
	}
}
