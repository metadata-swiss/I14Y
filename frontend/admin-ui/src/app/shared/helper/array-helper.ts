export class ArrayHelper {
	public static hasElements<T>(array: T[] | undefined): boolean {
		return Array.isArray(array) && array.length > 0;
	}

	public static convertStringToArray(value: string | null | undefined): string[] {
		if (!value) return [];
		return value
			.split(',')
			.map(v => v.trim())
			.filter(v => v.length > 0);
	}

	public static convertArrayToString(values: (string | null | undefined)[] | null | undefined, separator: string = ', '): string {
		if (!values) return '';

		return values
			.map(v => (v ?? '').toString().trim())
			.filter(Boolean)
			.join(separator);
	}
}
