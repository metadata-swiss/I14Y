import {Pipe, PipeTransform} from '@angular/core';

@Pipe({
	name: 'accessrightstempiconromtext',
	standalone: false
})
export class AccessRightsTempIconFromTextPipe implements PipeTransform {
	transform(source: string | undefined): string {
		switch (source) {
			case 'Öffentlich':
			case 'Public':
			case 'Pubblico':
				return 'lock_open';
			case 'Eingeschränkt':
			case 'Restreint':
			case 'Ristretto':
			case 'Restricted':
				return 'lock';
			case 'Nicht-öffentlich':
			case 'Non public':
			case 'Non pubblico':
			case 'Non-public':
			case 'Vertraulich':
			case 'Confidentiel':
			case 'Confidenziale':
			case 'Confidential':
				return 'xmark_circle';
			default:
				return '';
		}
	}
}
