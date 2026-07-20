import {Pipe, PipeTransform} from '@angular/core';
import {PublicationLevel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Pipe({
	name: 'publicationlevelclass',
	standalone: false
})
export class PublicationLevelClassPipe implements PipeTransform {
	readonly publicationLevelEnum = PublicationLevel;
	transform(source: string | undefined): string {
		switch (source) {
			case this.publicationLevelEnum.Public:
				return 'success';
			case this.publicationLevelEnum.Internal:
			default:
				return 'info';
		}
	}
}
