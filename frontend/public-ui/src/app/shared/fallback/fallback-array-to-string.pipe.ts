import {Pipe, PipeTransform} from '@angular/core';
import {IMultiLanguage} from '@I14Y-ch/bfs-iop-admin-web-api-client';

/*
 * Display the fallback language if the current language is not available
 * Usage:
 *   value | fallbackarraytostring:language:separator:defaultValue
 * Example:
 *   {{ ml | fallbackarraytostring:"fr":", ": "-" }}
 */
@Pipe({
	name: 'fallbackarraytostring',
	standalone: false
})
export class FallbackArrayToStringPipe implements PipeTransform {
	transform(source: IMultiLanguage[] | undefined, language: string, separator: string = ', ', defaultValue: string = ''): string | null {
		return this.convertArrayToString(
			source
				?.map(ml => {
					if (ml) {
						return [ml[language as keyof IMultiLanguage], ml.de, ml.fr, ml.it, ml.en, ml.rm].find(x => x || x === '') ?? '';
					}
					return '';
				})
				.filter(x => x !== ''),
			separator,
			defaultValue
		);
	}

	private convertArrayToString(input: string[] | undefined, separator = ', ', emptyValue = ''): string {
		return (input ?? []).join(separator) || emptyValue;
	}
}
