import {Component, OnInit, inject} from '@angular/core';
import {Router} from '@angular/router';
import {AuthService} from './auth.service';

@Component({
	selector: 'app-signin-callback',
	template: '',
	styles: '',
	standalone: false
})
export class SignoutCallbackComponent implements OnInit {
	private readonly router = inject(Router);
	private readonly authService = inject(AuthService);

	ngOnInit() {
		this.authService.completeLogout().finally(() => {
			this.router.navigateByUrl(this.router.parseUrl('/home'));
		});
	}
}
