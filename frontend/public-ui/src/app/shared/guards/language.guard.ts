import {inject, Injectable} from '@angular/core';
import {CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot} from '@angular/router';
import {TranslateService} from '@ngx-translate/core';

@Injectable({providedIn: 'root'})
export class LanguageGuard implements CanActivate {
	private readonly translate = inject(TranslateService);
	constructor(private readonly router: Router) {}

	canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
		const langId = route.paramMap.get('langId');
		const storedLang = localStorage.getItem('oblique_lang') || 'de';

		if (langId === 'unknown-route') {
			this.router.navigate(['unknown-route'], {skipLocationChange: true});
		} else if (!langId || !this.translate.langs.includes(langId)) {
			// The current URL path (excluding invalid lang)
			const segments = state.url.split('/').filter(Boolean); // removes empty parts
			segments.shift(); // remove invalid lang (first segment)
			const restOfPath = segments.join('/'); // the rest of the path
			const redirectUrl = `/${storedLang}/${restOfPath}`;
			this.translate.use(storedLang);
			console.warn(`Invalid language "${langId}", redirecting to ${redirectUrl}`);
			this.router.navigateByUrl(redirectUrl);
			return false;
		} else if (langId !== this.translate.getCurrentLang()) {
			this.translate.use(langId);
		}

		return true;
	}
}
