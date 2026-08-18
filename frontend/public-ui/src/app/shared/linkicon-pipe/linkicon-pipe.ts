import {Pipe, PipeTransform} from '@angular/core';
import {ObEExternalLinkIcon} from '@oblique/oblique';

@Pipe({
	name: 'linkicon',
	standalone: false
})
export class LinkiconPipe implements PipeTransform {
	transform(source: string | undefined): ObEExternalLinkIcon {
		return source?.includes(window.location.origin) ? 'none' : 'right';
	}
}
