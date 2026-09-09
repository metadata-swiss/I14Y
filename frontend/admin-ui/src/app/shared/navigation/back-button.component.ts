import {Component, inject} from '@angular/core';
import {Observable} from 'rxjs';
import {NavigationStackService} from './navigation-stack.service';

@Component({
	selector: 'app-back-button',
	templateUrl: './back-button.component.html',
	styleUrls: ['./back-button.component.scss'],
	standalone: false
})
export class BackButtonComponent {
	readonly canGoBack$: Observable<boolean>;

	private readonly navigationStack = inject(NavigationStackService);

	constructor() {
		this.canGoBack$ = this.navigationStack.canGoBack$;
	}

	onBack(): void {
		this.navigationStack.back();
	}
}
