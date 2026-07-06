import {ActivatedRouteSnapshot, CanActivateFn, Router, RouterStateSnapshot} from '@angular/router';
import {inject, Injectable} from '@angular/core';
import {map} from 'rxjs/operators';
import {Observable} from 'rxjs';
import {AllowActionService} from './allow.action.service';
import {AllowActionResourceType, AllowActionsClient, AllowActionType} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Injectable({
	providedIn: 'root'
})
class GuardService {
	private readonly client = inject(AllowActionsClient);
	private readonly router = inject(Router);
	private readonly service = inject(AllowActionService);

	canActivateCreate(type: AllowActionResourceType): Observable<boolean> {
		return this.service.globalAllowActions$.pipe(
			map(result => {
				let allowCreate = result.find(x => x.resourceType === type && x.actionType === AllowActionType.Create)?.value as boolean;
				if (!allowCreate) {
					this.router.navigate(['']);
				}
				return allowCreate;
			})
		);
	}

	canActivateEdit(type: AllowActionResourceType, id: string): Observable<boolean> {
		return this.checkPermissionToEdit(type, id);
	}

	canActivateCreateVersion(type: AllowActionResourceType, id: string): Observable<boolean> {
		return this.checkPermissionToCreateVersion(type, id);
	}

	private checkPermissionToEdit(type: AllowActionResourceType, id: string): Observable<boolean> {
		return this.client.getV2ByResourceTypeAndId(type, id).pipe(
			map(response => {
				let allowEdit = response.result.find(x => x.actionType === AllowActionType.Edit)?.value as boolean;
				if (!allowEdit) {
					this.router.navigate(['']);
				}
				return allowEdit;
			})
		);
	}

	private checkPermissionToCreateVersion(type: AllowActionResourceType, id: string): Observable<boolean> {
		return this.client.getV2ByResourceTypeAndId(type, id).pipe(
			map(response => {
				let allowEdit = response.result.find(x => x.actionType === AllowActionType.Version)?.value as boolean;
				if (!allowEdit) {
					this.router.navigate(['']);
				}
				return allowEdit;
			})
		);
	}
}

/* Concept */
export const ConceptCreateGuard: CanActivateFn = (_next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateCreate(AllowActionResourceType.Concept);
};

export const ConceptEditGuard: CanActivateFn = (next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateEdit(AllowActionResourceType.Concept, next.params.conceptId);
};

export const ConceptCreateVersionGuard: CanActivateFn = (next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateCreateVersion(AllowActionResourceType.Concept, next.params.conceptId);
};

/* DataService */
export const DataServiceCreateGuard: CanActivateFn = (_next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateCreate(AllowActionResourceType.DataService);
};

export const DataServiceEditGuard: CanActivateFn = (next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateEdit(AllowActionResourceType.DataService, next.params.id);
};

export const DataServiceCreateVersionGuard: CanActivateFn = (next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateCreateVersion(AllowActionResourceType.DataService, next.params.id);
};

/* Dataset */
export const DatasetCreateGuard: CanActivateFn = (_next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateCreate(AllowActionResourceType.Dataset);
};

export const DatasetEditGuard: CanActivateFn = (next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateEdit(AllowActionResourceType.Dataset, next.params.id);
};

export const DatasetCreateVersionGuard: CanActivateFn = (next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateCreateVersion(AllowActionResourceType.Dataset, next.params.id);
};

/* PublicService */
export const PublicServiceCreateGuard: CanActivateFn = (_next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateCreate(AllowActionResourceType.PublicService);
};

export const PublicServiceEditGuard: CanActivateFn = (next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateEdit(AllowActionResourceType.PublicService, next.params.id);
};

export const PublicServiceCreateVersionGuard: CanActivateFn = (next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateCreateVersion(AllowActionResourceType.PublicService, next.params.id);
};

/* MappingTable */
export const MappingTableCreateGuard: CanActivateFn = (_next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateCreate(AllowActionResourceType.MappingTable);
};

export const MappingTableEditGuard: CanActivateFn = (next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateEdit(AllowActionResourceType.MappingTable, next.params.id);
};

export const MappingTableVersionGuard: CanActivateFn = (next: ActivatedRouteSnapshot, _state: RouterStateSnapshot): Observable<boolean> => {
	return inject(GuardService).canActivateCreateVersion(AllowActionResourceType.MappingTable, next.params.id);
};
