import {AbstractControl, ValidationErrors} from '@angular/forms';
import {DataServicesClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {Observable, of} from 'rxjs';
import {catchError, map} from 'rxjs/operators';
import {MultiIdentifiersValidator} from './multi-Identifiers.validator';
import {inject, Injectable} from '@angular/core';

@Injectable()
export class DataServiceMultiIdentifiersValidator extends MultiIdentifiersValidator {
	private readonly client = inject(DataServicesClient);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);

	validate(control: AbstractControl): Observable<ValidationErrors | null> {
		if (control.value?.length === 0) {
			return of(null);
		}

		let otherControls: Array<AbstractControl> | undefined = this._formGroupToValidate?.controls.filter(x => x !== control.parent);

		if (this._initalValue?.find(x => x === control.value)) {
			if (otherControls) {
				return this.validateWithOtherControls(control, otherControls);
			} else {
				return of(null);
			}
		}

		const skippedErrorNotifications = 1;
		this.obHttpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(skippedErrorNotifications);
		return this.client.getIdentifierExistsByIdentifier(control.value).pipe(
			map(exists => (exists.result ? {identifierExists: true} : null)),
			catchError(() => of(null))
		);
	}

	private validateWithOtherControls(control: AbstractControl, otherControls: Array<AbstractControl>): Observable<ValidationErrors | null> {
		if (otherControls.map(x => x.value.identifier).includes(control.value)) {
			return of({identifierExists: true});
		} else {
			return of(null);
		}
	}
}
