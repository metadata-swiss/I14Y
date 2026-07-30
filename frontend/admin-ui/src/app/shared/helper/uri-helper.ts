export class UriHelper {
	public static GetUriFragment(uri: string | undefined): string {
		if (!uri) {
			return '';
		}
		const hashIndex = uri.lastIndexOf('#');
		if (hashIndex !== -1) {
			return uri.substring(hashIndex + 1);
		}

		const slashIndex = uri.lastIndexOf('/');
		return slashIndex !== -1 ? uri.substring(slashIndex + 1) : uri;
	}

	public static RemoveHash(value: string | undefined): string | undefined {
		if (!value) return undefined;
		return value.replace(/^\s*#/, '');
	}
	
	public static completePathUriForUnique(uriOrgi: string, suffix: string): string{
		return uriOrgi?.concat(this.GetUriFragment(suffix));
	}

	public static replaceLastSegment(uri: string, newSegment: string): string {
		if (!uri) {
			return newSegment;
		}
		const hashIndex = uri.lastIndexOf('#');
		const separatorIndex = hashIndex !== -1 ? hashIndex : uri.lastIndexOf('/');

		return separatorIndex !== -1
			? uri.substring(0, separatorIndex + 1) + newSegment
			: newSegment;
	}
}
