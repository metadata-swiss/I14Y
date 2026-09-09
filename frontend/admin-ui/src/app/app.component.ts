import {AfterViewInit, Component, inject, OnDestroy, OnInit, TemplateRef, ViewChild} from '@angular/core';
import {ObEExternalLinkIcon, ObINavigationLink, ObMasterLayoutComponent} from '@oblique/oblique';
import {MatDialog} from '@angular/material/dialog';
import {Observable, Subject, takeUntil} from 'rxjs';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Title} from '@angular/platform-browser';
import {UserModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Router, Scroll} from '@angular/router';
import {filter} from 'rxjs/operators';
import {AuthService} from './auth/auth.service';
import {AppConfig} from './app.config';
import {IAppConfig} from './app.config.interface';

@Component({
	selector: 'app-root',
	templateUrl: './app.component.html',
	styleUrls: ['./app.component.scss'],
	standalone: false
})
export class AppComponent implements OnInit, OnDestroy, AfterViewInit {
	currentYear = new Date().getFullYear();
	isLoggedIn: Observable<boolean>;
	user: UserModel | null = null;
	access_token!: string;
	currentLanguage: string;
	navigation: ObINavigationLink[] = [];
	icon: ObEExternalLinkIcon = 'none';
	target = '_blank';
	rel = 'noopener noreferrer';
	readonly appConfig = AppConfig.getConfig<IAppConfig>();
	readonly linkHandbook = this.appConfig.LINK_HANDBOOK.replace(/\/+$/, '');
	readonly releaseVersion = this.appConfig.RELEASE_VERSION;
	readonly thirdPartyLicensesUrl = this.createThirdPartyLicensesUrl();

	@ViewChild(ObMasterLayoutComponent) private readonly masterLayout: ObMasterLayoutComponent | undefined;
	@ViewChild('aboutDialog') private readonly aboutDialog: TemplateRef<unknown> | undefined;

	private readonly unsubscribe$ = new Subject();
	private readonly router = inject(Router);
	private readonly dialog = inject(MatDialog);
	private readonly titleService = inject(Title);
	private readonly translate = inject(TranslateService);
	private readonly authService = inject(AuthService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => (this.currentLanguage = language.lang));
		this.isLoggedIn = this.authService.isAuthenticated$;

		localStorage.setItem('api_base_url', this.appConfig.API_BASE_URL);
	}

	ngOnInit() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.authService.isAuthenticated$.subscribe(() => {
			if (this.authService.isAuthenticated()) {
				this.generateAccessToken();
			}
		});
		this.authService.userInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(userInfo => (this.user = userInfo));

		this.router.events
			.pipe(
				takeUntil(this.unsubscribe$),
				filter((e): e is Scroll => e instanceof Scroll)
			)
			.subscribe((e: Scroll) => {
				this.handleScrolling(e);
			});

		this.navigation = [
			{
				url: 'home',
				label: 'i18n.navigation.home'
			},
			{
				url: 'catalog',
				label: 'i18n.navigation.catalog'
			},
			{
				url: `${this.linkHandbook}/${this.currentLanguage}`,
				label: 'i18n.navigation.handbook',
				isExternal: true
			}
		];
	}

	ngAfterViewInit(): void {
		this.translate.stream('i18n.application_name').subscribe(result => {
			this.titleService.setTitle(result);
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	generateAccessToken(): void {
		const accessToken = this.authService.currentUser?.access_token ?? '';
		this.access_token = `Bearer ${accessToken}`;
	}

	login() {
		this.authService.startAuthentication();
	}

	logout() {
		this.authService.startLogout();
	}

	openAbout(): void {
		if (this.aboutDialog) {
			this.dialog.open(this.aboutDialog, {ariaLabel: this.translate.instant('i18n.about.title')});
		}
	}

	private handleScrolling(e: Scroll) {
		if (e.anchor) {
			const element = document.getElementById(e.anchor) ?? undefined;
			this.masterLayout!.scrollTop(element);
		} else {
			this.masterLayout!.scrollTop();
		}
	}

	private createThirdPartyLicensesUrl(): string {
		const reference = /^\d+\.\d+\.\d+$/.test(this.releaseVersion) ? this.releaseVersion : 'main';
		return `https://github.com/I14Y-ch/I14Y/blob/${reference}/THIRD-PARTY-LICENSES.md`;
	}
}
