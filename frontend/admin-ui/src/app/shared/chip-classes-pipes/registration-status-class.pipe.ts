import {Pipe, PipeTransform} from '@angular/core';
import {RegistrationStatus} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Pipe({
	name: 'registrationstatusclass',
	standalone: false
})
export class RegistrationStatusClassPipe implements PipeTransform {
	readonly registrationStatusEnum = RegistrationStatus;

	transform(source: string | undefined): string {
		switch (source) {
			case this.registrationStatusEnum.Recorded:
			case this.registrationStatusEnum.Qualified:
			case this.registrationStatusEnum.Standard:
			case this.registrationStatusEnum.PreferredStandard:
				return 'success';
			case this.registrationStatusEnum.Superseded:
			case this.registrationStatusEnum.Retired:
				return 'warning';
			case this.registrationStatusEnum.Incomplete:
			case this.registrationStatusEnum.Candidate:
			default:
				return 'info';
		}
	}
}
