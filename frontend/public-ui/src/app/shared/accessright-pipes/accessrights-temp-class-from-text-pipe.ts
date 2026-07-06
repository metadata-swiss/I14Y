import {Pipe, PipeTransform} from '@angular/core';

@Pipe({
	name: 'accessrightstempclassfromtext',
	standalone: false
})
export class AccessRightsTempClassFromTextPipe implements PipeTransform {
	transform(source: string | undefined): string | null {
		switch (source) {
			case 'Nicht-öffentlich':
			case 'Non public':
			case 'Non pubblico':
			case 'Non-public':
				return 'nonpublic';
			case 'Öffentlich':
			case 'Public':
			case 'Pubblico':
				return 'public';
			case 'Eingeschränkt':
			case 'Restreint':
			case 'Ristretto':
			case 'Restricted':
				return 'restricted';
			case 'Vertraulich':
			case 'Confidentiel':
			case 'Confidenziale':
			case 'Confidential':
				return 'confidential';
			default:
				return null;
		}
	}
}
