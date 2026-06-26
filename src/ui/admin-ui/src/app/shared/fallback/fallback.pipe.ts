import {Pipe, PipeTransform} from '@angular/core';
import {IMultiLanguage} from '@I14Y-ch/bfs-iop-admin-web-api-client';

/*
 * Display the fallback language if the current language is not available
 * Usage:
 *   value | fallback:language
 * Example:
 *   {{ ml | fallback:"fr" }}
 */
@Pipe({
	name: 'fallback',
	standalone: false
})
export class FallbackPipe implements PipeTransform {
	transform(source: IMultiLanguage | undefined, language: string): string | null {
		let ml = source as IMultiLanguage;
		if (ml) {
			return [ml[language as keyof IMultiLanguage], ml.de, ml.fr, ml.it, ml.en, ml.rm].find(x => x || x === '') ?? null;
		}
		return null;
	}
}
