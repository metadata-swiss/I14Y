export class SpatialMapper {
	public static mapElements(elements: Array<{spatial?: string} | null | undefined> | null | undefined): string[] {
		return (elements ?? []).map(e => e?.spatial).filter((x): x is string => typeof x === 'string' && x.trim().length > 0);
	}
}
