import {Pipe, PipeTransform} from '@angular/core';
import {ObEExternalLinkIcon} from '@oblique/oblique';

@Pipe({
	name: 'linkicon',
	standalone: false
})
export class LinkiconPipe implements PipeTransform {
	transform(source: string | undefined): ObEExternalLinkIcon {
		if (!source || typeof window === 'undefined') {
 			return 'none';
 		}
 		try {
 			return new URL(source).origin === window.location.origin ? 'none' : 'right';
 		} catch {
 			return 'none';
 		}
	}
}
