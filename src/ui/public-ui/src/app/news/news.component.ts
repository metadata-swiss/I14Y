import {Component} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';

@Component({
	selector: 'app-news',
	templateUrl: './news.component.html',
	styleUrls: [],
	standalone: false
})
export class NewsComponent {
	constructor(private readonly translate: TranslateService) {
		window.location.href = `https://i14y-ch.github.io/handbook/${this.translate.getCurrentLang()}/news`;
	}
}
