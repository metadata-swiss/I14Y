export class SpatialMapper {
	public static mapElements(elements: Array<any>): string[] {
		return elements?.map(x => x.spatial).filter((x: string) => x?.length > 0);
	}
}