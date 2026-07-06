import {Component, AfterViewInit, OnDestroy, OnInit, inject} from '@angular/core';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';
import {AppConfig} from '../app.config';
import {IAppConfig} from '../app.config.interface';
import {SearchEngineOptimizationService} from '../shared/services/search-engine-optimization/search-engine-optimization.service';

@Component({
	selector: 'app-home',
	templateUrl: './home.component.html',
	styleUrls: ['./home.component.scss'],
	standalone: false
})
export class HomeComponent implements AfterViewInit, OnDestroy, OnInit {
	currentLanguage: string;
	query: string | undefined;
	readonly dashboardUrl = AppConfig.getConfig<IAppConfig>().DASHBOARD_URL;
	readonly showInfoVideo = AppConfig.getConfig<IAppConfig>().SHOW_INFO_VIDEO;
	readonly linkHandbook = AppConfig.getConfig<IAppConfig>().LINK_HANDBOOK;
	readonly emailAddress: string = 'i14y@bfs.admin.ch';

	private readonly unsubscribe$ = new Subject();

	private readonly searchEngineOptimizationService = inject(SearchEngineOptimizationService);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngAfterViewInit(): void {
		this.updateMetaData();
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	get newsletterLink(): string {
		const de = 'https://www.bfs.admin.ch/bfs/de/home/dienstleistungen/kontakt/newsmail-abonnement.html';
		const fr = 'https://www.bfs.admin.ch/bfs/fr/home/services/contact/abonnement-newsmail.html';
		const it = 'https://www.bfs.admin.ch/bfs/it/home/servizi/contatto/abbonamento-newsmail.html';

		switch (this.currentLanguage) {
			case 'fr':
				return fr;
			case 'it':
				return it;
			default:
				return de;
		}
	}

	onQueryChange(query: string | undefined) {
		this.query = query;
	}

	private updateMetaData() {
		const titleKey = 'i18n.title.home';
		const descriptionKey = 'i18n.meta.description.home';

		this.translate
			.stream([titleKey, descriptionKey])
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(result => {
				this.searchEngineOptimizationService.UpdateMetaTitle(result[titleKey]);
				this.searchEngineOptimizationService.UpdateMetaDescrition(result[descriptionKey]);
			});
	}
}
