import {Component} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {AppConfig} from '../app.config';
import {IAppConfig} from '../app.config.interface';

@Component({
	selector: 'app-news',
	templateUrl: './news.component.html',
	styleUrls: [],
	standalone: false
})
export class NewsComponent {
	constructor(private readonly translate: TranslateService) {
		const handbookBaseUrl = AppConfig.getConfig<IAppConfig>().LINK_HANDBOOK.replace(/\/+$/, '');
		window.location.href = `${handbookBaseUrl}/${this.translate.getCurrentLang()}/news`;
	}
}
