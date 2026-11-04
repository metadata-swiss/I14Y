// Must match backend ThemeSearchKey: codes are unique only within a vocabulary.
export const THEME_SEARCH_KEY_SEPARATOR = '!';

export function themeSearchKey(entry: {code?: string; vocabularyIdentifier?: string}): string {
	return entry.vocabularyIdentifier ? `${entry.vocabularyIdentifier}${THEME_SEARCH_KEY_SEPARATOR}${entry.code}` : (entry.code ?? '');
}

export function splitThemeSearchKey(key: string | undefined): {vocabularyIdentifier: string; code: string} | undefined {
	const at = key?.indexOf(THEME_SEARCH_KEY_SEPARATOR) ?? -1;

	return at > 0 ? {vocabularyIdentifier: key!.slice(0, at), code: key!.slice(at + 1)} : undefined;
}
