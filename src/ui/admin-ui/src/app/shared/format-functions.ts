import {IopPersonModel, MultiLanguage, VocabularyEntry} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService} from '@ngx-translate/core';
import moment from 'moment';

export class FormatFunctions {
	public static getFormattedDate(date: Date | undefined, translate: TranslateService): string | null {
		if (!date) {
			return '-';
		}
		return moment(date).locale(translate.currentLang).format('DD.MM.yyyy');
	}

	public static getFormattedDateTime(date: Date | undefined): string {
		if (!date) {
			return '-';
		}
		return moment(date).format('DD.MM.YYYY, HH:mm');
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

	public static getTranslatedVocabularyEntries(input: VocabularyEntry[] | undefined, language: string): string[] | undefined {
		return (input ?? [])
			.map(i => (i.name ? (i.name[language as keyof MultiLanguage] as string) ?? i.name['de' as keyof MultiLanguage] ?? '' : ''))
			.filter(e => {
				return e;
			});
	}

	public static getTranslatedVocabularyEntriesWithCode(input: VocabularyEntry[] | undefined, language: string): string[] | undefined {
		return (input ?? [])
			.map(
				// eslint-disable-next-line max-len
				i => (i.code ? i.code + ' ' : '') + (i.name ? (i.name[language as keyof MultiLanguage] as string) ?? i.name['de' as keyof MultiLanguage] ?? '' : '')
			)
			.filter(e => {
				return e;
			});
	}

	public static geGeoIvtTranslatedVocabularyEntries(input: VocabularyEntry[] | undefined, language: string): string[] | undefined {
		return (input ?? [])
			.map(i => (i.name && i.code ? `${i.code} - ${(i.name[language as keyof MultiLanguage] as string) || i.name.de || ''}` : ''))
			.filter(Boolean);
	}

	public static getTranslatedMultiLanguages(input: MultiLanguage[] | undefined, language: string): string[] | undefined {
		return (input ?? [])
			.map(i => (i[language as keyof MultiLanguage] as string) ?? i['de' as keyof MultiLanguage] ?? '')
			.filter(e => {
				return e;
			});
	}

	public static getFullName(input: IopPersonModel | undefined): string | undefined {
		return input?.givenName || input?.familyName ? `${input?.givenName ?? ''} ${input?.familyName ?? ''}`.trim() : undefined;
	}
}
