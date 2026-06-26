export class IdentifierMapper {
	public static mapElements(elements: Array<any>): string[] {
		return elements?.map(x => x.identifier).filter((x: string) => x?.length > 0);
	}
}
