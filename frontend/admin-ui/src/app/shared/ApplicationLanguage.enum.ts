// eslint-disable-next-line no-shadow
export enum ApplicationLanguage {
	de = 'de',
	fr = 'fr',
	it = 'it',
	en = 'en',
	rm = 'rm'
}
export class Languages {
	// Content languages in display and fallback order (de-fr-it-en(-rm))
	public static ContentLanguages: readonly string[] = Object.values(ApplicationLanguage).slice(0, -1);
	public static ContentLanguagesRm: readonly string[] = Object.values(ApplicationLanguage);
}
