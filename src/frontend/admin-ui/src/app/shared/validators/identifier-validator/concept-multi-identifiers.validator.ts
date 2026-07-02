import {AbstractControl, ValidationErrors} from '@angular/forms';
import {ConceptInputClient, IdentifierVersionExistsResultMessage} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {Observable, of} from 'rxjs';
import {catchError, map} from 'rxjs/operators';
import {MultiIdentifiersValidator} from './multi-Identifiers.validator';
import {inject, Injectable} from '@angular/core';

@Injectable()
export class ConceptMultiIdentifiersValidator extends MultiIdentifiersValidator {
	private readonly client = inject(ConceptInputClient);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);

	validate(control: AbstractControl): Observable<ValidationErrors | null> {
		if (!control.value?.length) {
			return of(null);
		}

		const otherControls = this._formGroupToValidate?.controls.filter(x => x !== control.parent);

		if (this._initalValue?.includes(control.value)) {
			return otherControls ? this.validateWithOtherControls(control, otherControls) : of(null);
		}

		const version = this._versionFormControl?.value;
		if (!version) {
			return of(null);
		}

		const skippedErrorNotifications = 1;
		this.obHttpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(skippedErrorNotifications);
		return this.client.getIdentifierExistsByIdentifierAndVersion(control.value, version).pipe(
			map(response => {
				if (!response.result?.result) {
					this.updateVersionControl(null);
					return null;
				}
				if (response.result.message === IdentifierVersionExistsResultMessage.IdentifierAndVersionAlreadyExists) {
					this.updateVersionControl({identifierAndVersionExists: true});
					return {identifierAndVersionExists: true};
				}
				if (response.result.message === IdentifierVersionExistsResultMessage.IdentifierFromAnotherPublisherAlreadyExists) {
					this.updateVersionControl(null);
					return {anotherPublisher: true};
				}
				this.updateVersionControl(null);
				return null;
			}),
			catchError(() => of(null))
		);
	}

	private updateVersionControl(errors: ValidationErrors | null) {
		this._versionFormControl?.setErrors(errors);
		this._versionFormControl?.markAsTouched();
		this._versionFormControl?.updateValueAndValidity({emitEvent: false});
	}

	private validateWithOtherControls(control: AbstractControl, otherControls: Array<AbstractControl>): Observable<ValidationErrors | null> {
		if (otherControls.map(x => x.value.identifier).includes(control.value)) {
			return of({identifierAndVersionExists: true});
		}
		return of(null);
	}
}
