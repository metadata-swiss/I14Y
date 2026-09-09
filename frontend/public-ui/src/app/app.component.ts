import {DOCUMENT} from '@angular/common';
import {Component, inject, Inject, OnInit, TemplateRef, ViewChild} from '@angular/core';
import {ActivatedRoute, Params, Router, Scroll} from '@angular/router';
import {MatDialog} from '@angular/material/dialog';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ObEExternalLinkIcon, ObINavigationLink, ObMasterLayoutComponent, ObMasterLayoutConfig} from '@oblique/oblique';
import {BehaviorSubject, filter, Subject, takeUntil} from 'rxjs';
import {AppConfig} from './app.config';
import {IAppConfig} from './app.config.interface';

@Component({
	selector: 'app-root',
	templateUrl: './app.component.html',
	styleUrls: ['./app.component.scss'],
	standalone: false
})
export class AppComponent implements OnInit {
	appConfig = AppConfig.getConfig<IAppConfig>();
	currentYear = new Date().getFullYear();
	navigation: ObINavigationLink[] = [];
	readonly icon: ObEExternalLinkIcon = 'none';
	readonly target = '_blank';
	readonly rel = 'noopener noreferrer';
	readonly linkHandbook = this.appConfig.LINK_HANDBOOK.replace(/\/+$/, '');
	readonly releaseVersion = this.appConfig.RELEASE_VERSION;
	readonly thirdPartyLicensesUrl = this.createThirdPartyLicensesUrl();

	@ViewChild(ObMasterLayoutComponent) private readonly masterLayout: ObMasterLayoutComponent | undefined;
	@ViewChild('aboutDialog') private readonly aboutDialog: TemplateRef<unknown> | undefined;

	private readonly unsubscribe$ = new Subject();

	private readonly activeRoute = inject(ActivatedRoute);
	private readonly obConfig = inject(ObMasterLayoutConfig);
	private readonly dialog = inject(MatDialog);
	private readonly router = inject(Router);
	private readonly translate = inject(TranslateService);

	constructor(@Inject(DOCUMENT) private readonly document: Document) {}

	ngOnInit(): void {
		this.updateApplicationLanguage();

		this.router.events
			.pipe(
				takeUntil(this.unsubscribe$),
				filter((e): e is Scroll => e instanceof Scroll)
			)
			.subscribe((e: Scroll) => {
				this.handleScrolling(e);
			});

		setTimeout(() =>
			this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
				const language = event.lang;
				const separator = '/';
				const splittedUrl = window.location.pathname.split(separator);

				this.updateApplicationLanguage();

				if (splittedUrl[1] !== language) {
					splittedUrl[1] = language;
					this.router.navigate([splittedUrl.join(separator)], {
						queryParams: (<BehaviorSubject<Params>>this.activeRoute.queryParams).value
					});
				}
			})
		);
	}

	getAdminUrl(): string {
		return this.document.baseURI;
	}

	openAbout(): void {
		if (this.aboutDialog) {
			this.dialog.open(this.aboutDialog, {ariaLabel: this.translate.instant('i18n.about.title')});
		}
	}

	private updateApplicationLanguage() {
		const language = this.translate.getCurrentLang();
		const handbookBaseUrl = this.linkHandbook;

		this.obConfig.homePageRoute = `/${language}/home`;

		this.navigation = [
			{
				url: `/${language}/home`,
				label: 'i18n.navigation.home',
				id: 'main-home'
			},
			{
				url: `/${language}/catalog`,
				label: 'i18n.navigation.catalog',
				id: 'main-catalog'
			},
			{
				url: `/${language}/organisations`,
				label: 'i18n.navigation.organisations',
				id: 'main-organisations'
			},
			{
				url: `${handbookBaseUrl}/${language}/news`,
				label: 'i18n.navigation.news',
				isExternal: true,
				id: 'main-news'
			}
		];

		if (handbookBaseUrl) {
			this.navigation.push({
				url: `${handbookBaseUrl}/${language}`,
				label: 'i18n.navigation.handbook',
				isExternal: true,
				id: 'main-handbook'
			});
		}
	}

	private handleScrolling(e: Scroll) {
		if (e.anchor) {
			const element = document.getElementById(e.anchor) ?? undefined;
			this.masterLayout!.scrollTop(element);
			//this.masterLayout!.scrollTarget?.scrollTo({top: element?.offsetTop ?? 0, left: element?.offsetLeft ?? 0, behavior: 'smooth'});
		} else if (e.position) {
			//this.masterLayout!.scrollTarget?.scrollTo({top: e.position[0], left: e.position[1], behavior: 'smooth'});
		} else {
			this.masterLayout!.scrollTop();
			//this.masterLayout!.scrollTarget?.scrollTo({top: 0, left: 0, behavior: 'smooth'});
		}
	}

	private createThirdPartyLicensesUrl(): string {
		const reference = /^\d+\.\d+\.\d+$/.test(this.releaseVersion) ? this.releaseVersion : 'main';
		return `https://github.com/I14Y-ch/I14Y/blob/${reference}/THIRD-PARTY-LICENSES.md`;
	}
}
