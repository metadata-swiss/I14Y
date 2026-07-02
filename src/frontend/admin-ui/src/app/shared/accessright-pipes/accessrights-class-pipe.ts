import {Pipe, PipeTransform} from '@angular/core';

@Pipe({
	name: 'accessrightsclass',
	standalone: false
})
export class AccessRightsClassPipe implements PipeTransform {
	transform(source: string | undefined): string | null {
		switch (source) {
			case 'NON_PUBLIC':
				return 'nonpublic';
			case 'PUBLIC':
				return 'public';
			case 'RESTRICTED':
				return 'restricted';
			case 'CONFIDENTIAL':
				return 'confidential';
			default:
				return null;
		}
	}
}
