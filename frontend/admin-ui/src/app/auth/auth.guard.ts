import {inject, Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, CanActivateChildFn, CanActivateFn, Router, RouterStateSnapshot, UrlTree} from '@angular/router';
import {Observable, of} from 'rxjs';
import {AuthService} from './auth.service';

@Injectable({
	providedIn: 'root'
})
class AuthGuardSerivce {
	private readonly router = inject(Router);
	private readonly authService = inject(AuthService);

	canActivate(_route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
		return this.isAuthenticatedElseLogin(state);
	}
	canActivateChild(
		_childRoute: ActivatedRouteSnapshot,
		state: RouterStateSnapshot
	): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
		return this.isAuthenticatedElseLogin(state);
	}

	isAuthenticatedElseLogin(state: RouterStateSnapshot): Observable<boolean | UrlTree> | boolean {
		if (this.authService.isAuthenticated()) {
			return this.authService.getRoles().includes('ALLOW') ? true : of(this.router.parseUrl('/unauthorized'));
		}
		const redirectUrl = state.url;
		localStorage.setItem('redirectUrl', redirectUrl);
		this.authService.startAuthentication();
		return false;
	}
}

export const AuthGuard: CanActivateFn = (
	next: ActivatedRouteSnapshot,
	state: RouterStateSnapshot
): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree => {
	return inject(AuthGuardSerivce).canActivate(next, state);
};

export const AuthChildGuard: CanActivateChildFn = (
	childRoute: ActivatedRouteSnapshot,
	state: RouterStateSnapshot
): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree => {
	return inject(AuthGuardSerivce).canActivateChild(childRoute, state);
};
