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
}
