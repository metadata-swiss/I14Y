import {Component, inject, OnInit} from '@angular/core';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';
import {AppConfig} from '../app.config';
import {IAppConfig} from '../app.config.interface';

@Component({
	selector: 'app-home',
	templateUrl: './home.component.html',
	styleUrls: ['./home.component.scss'],
	standalone: false
})
export class HomeComponent implements OnInit {
	currentLanguage: string;
	readonly linkHandbook = AppConfig.getConfig<IAppConfig>().LINK_HANDBOOK.replace(/\/+$/, '');
	private readonly unsubscribe$ = new Subject();

	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	get handbookUrl(): string {
		return `${this.linkHandbook}/${this.currentLanguage}`;
	}
}
