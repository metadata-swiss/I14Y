import {Pipe, PipeTransform} from '@angular/core';

@Pipe({
	name: 'accessrightsicon',
	standalone: false
})
export class AccessRightsIconPipe implements PipeTransform {
	transform(source: string | undefined): string {
		switch (source) {
			case 'PUBLIC':
				return 'lock_open';
			case 'RESTRICTED':
				return 'lock';
			case 'NON_PUBLIC':
			case 'CONFIDENTIAL':
				return 'xmark_circle';
			default:
				return '';
		}
	}
}
