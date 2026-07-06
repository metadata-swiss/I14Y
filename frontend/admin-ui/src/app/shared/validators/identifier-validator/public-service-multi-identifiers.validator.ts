import {AbstractControl, ValidationErrors} from '@angular/forms';
import {PublicServiceInputClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {Observable, of} from 'rxjs';
import {catchError, map} from 'rxjs/operators';
import {MultiIdentifiersValidator} from './multi-Identifiers.validator';
import {inject, Injectable} from '@angular/core';

@Injectable()
export class PublicServiceMultiIdentifiersValidator extends MultiIdentifiersValidator {
	private readonly client = inject(PublicServiceInputClient);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);

	validate(control: AbstractControl): Observable<ValidationErrors | null> {
		if (!control.value?.length) {
			return of(null);
		}

		const otherControls = this._formGroupToValidate?.controls.filter(x => x !== control.parent);

		if (this._initalValue?.includes(control.value)) {
			return otherControls ? this.validateWithOtherControls(control, otherControls) : of(null);
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
		}
		return of(null);
	}
}
