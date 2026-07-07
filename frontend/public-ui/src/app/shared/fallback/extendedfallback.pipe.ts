import {Pipe, PipeTransform} from '@angular/core';
import {IMultiLanguage} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {FallBackModel} from './fallbackmodel';

//@let result = source | extendedfallback: language;
@Pipe({
	name: 'extendedfallback',
	standalone: false
})
export class ExtendedFallbackPipe implements PipeTransform {
	transform(source: IMultiLanguage | undefined, language: string): FallBackModel | undefined {
		let result: FallBackModel | undefined;
		let ml = source as IMultiLanguage;
		let fallback = [language, 'de', 'fr', 'it', 'en', 'rm'];

		if (ml) {
			for (const key of fallback) {
				let text: string | undefined = ml[key as keyof IMultiLanguage];
				if (text) {
					result = new FallBackModel(text, key);
					break;
				}
			}
		}
		return result;
	}
}
