import {inject, Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree, CanActivateFn} from '@angular/router';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {AuthService} from './auth.service';

@Injectable({
	providedIn: 'root'
})
class LoginGuardService {
	private readonly router = inject(Router);
	private readonly authService = inject(AuthService);

	canActivate(_route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
		return this.authService.isAuthenticated$.pipe(map(x => this.isAuthenticatedElseFallback(state, x)));
	}

	private isAuthenticatedElseFallback(state: RouterStateSnapshot, isAuthenticated: boolean) {
		if (isAuthenticated) {
			const redirectUrl = localStorage.getItem('redirectUrl') || '/home';
			localStorage.removeItem('redirectUrl');
			return this.router.parseUrl(redirectUrl);
		}
		return this.router.parseUrl(state.url);
	}
}

export const LoginGuard: CanActivateFn = (
	next: ActivatedRouteSnapshot,
	state: RouterStateSnapshot
): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree => {
	return inject(LoginGuardService).canActivate(next, state);
};
