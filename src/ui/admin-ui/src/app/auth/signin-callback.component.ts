import {Component, OnInit, inject} from '@angular/core';
import {Router} from '@angular/router';
import {AuthService} from './auth.service';

@Component({
	selector: 'app-signin-callback',
	template: '',
	styles: '',
	standalone: false
})
export class SigninCallbackComponent implements OnInit {
	private readonly router = inject(Router);
	private readonly authService = inject(AuthService);

	ngOnInit() {
		this.authService.completeAuthentication().finally(() => {
			const redirectUrl = localStorage.getItem('redirectUrl') || '/home';
			this.router.navigateByUrl(this.router.parseUrl(redirectUrl));
		});
	}
}
