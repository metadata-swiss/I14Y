import {Observable} from 'rxjs';

export interface DeactivationGuarded {
	canDeactivate: () => Observable<boolean> | Promise<boolean> | boolean;
}
