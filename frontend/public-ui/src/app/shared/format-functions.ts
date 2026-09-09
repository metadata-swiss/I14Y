import {MultiLanguage, VocabularyEntry, LocalizedText} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService} from '@ngx-translate/core';
import {format} from 'date-fns';

export class FormatFunctions {
	public static getFormattedDate(date: Date | undefined): string {
		if (!date) {
			return '-';
		}
		return format(date, 'dd.MM.yyyy');
	}

	public static getFormattedDateTime(date: Date | undefined): string {
		if (!date) {
			return '-';
		}
		return format(date, 'dd.MM.yyyy, HH:mm');
	}

	public static getLanguagesTranslated(languages: string[] | undefined, translate: TranslateService): string {
		const input = languages ?? [];
		let output = '-';

		if (input && input.length > 0) {
			const keys = input.map(x => `i18n.languages.${x}`);

			translate.get(keys).subscribe(x => {
				output = keys.map(k => x[k]).join(', ');
			});
		}

		return output;
	}

	public static getTranslatedVocabularyEntries(input: VocabularyEntry[] | undefined, language: string): string[] | undefined {
		return (input ?? [])
			.map(i => (i.name ? ((i.name[language as keyof MultiLanguage] as string) ?? i.name['de' as keyof MultiLanguage] ?? '') : ''))
			.filter(e => {
				return e;
			}) as string[] | undefined;
	}

	public static getTranslatedVocabularyEntriesWithCode(input: VocabularyEntry[] | undefined, language: string): string[] | undefined {
		return (input ?? [])
			.map(
				// eslint-disable-next-line max-len
				i => (i.code ? i.code + ' ' : '') + (i.name ? ((i.name[language as keyof MultiLanguage] as string) ?? i.name['de' as keyof MultiLanguage] ?? '') : '')
			)
			.filter(e => {
				return e;
			});
	}

	public static getTranslatedMultiLanguages(
		input: MultiLanguage[] | undefined,
		language: string
	): (string | ((_data?: any) => void) | ((data?: any) => any))[] {
		return (input ?? [])
			.map(i => (i[language as keyof MultiLanguage] as string) ?? i['de' as keyof MultiLanguage] ?? '')
			.filter(e => {
				return e;
			});
	}

	public static convertArrayToString(input: string[] | undefined, separator = ', ', emptyValue = ''): string {
		return (input ?? []).join(separator) || emptyValue;
	}

	public static convertStringToArray(input: string | undefined, seperator = ','): string[] {
		let output: string[] = [];
		if (input) {
			output = input.split(seperator);
			output = output.map(i => i.trim());
		}
		return output;
	}

	public static convertLocalizedTextToMultiLanguages(input: LocalizedText): MultiLanguage | undefined {
		if (input) {
			return {
				de: input.cultureCode === 'de' ? input.text : undefined,
				en: input.cultureCode === 'en' ? input.text : undefined,
				fr: input.cultureCode === 'fr' ? input.text : undefined,
				it: input.cultureCode === 'it' ? input.text : undefined,
				init: () => {},
				toJSON: () => ({})
			};
		}
	}

	public static escapeHtml(value: string | undefined): string {
		return value?.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;') ?? '';
	}
}
