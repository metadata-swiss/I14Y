import {BrowserModule} from '@angular/platform-browser';
import {inject, LOCALE_ID, NgModule, provideAppInitializer} from '@angular/core';
import {DatePipe, registerLocaleData} from '@angular/common';
import {AppRoutingModule} from './app-routing.module';
import {AppComponent} from './app.component';
import {ObHttpApiInterceptor, ObMasterLayoutConfig, OB_BANNER, provideObliqueConfiguration} from '@oblique/oblique';
import localeDECH from '@angular/common/locales/de-CH';
import localeFRCH from '@angular/common/locales/fr-CH';
import localeITCH from '@angular/common/locales/it-CH';
import localeENCH from '@angular/common/locales/en-CH';
import {TranslateModule} from '@ngx-translate/core';
import {HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {SharedModule} from './shared/shared.module';
import {LoggerModule, NgxLoggerLevel} from 'ngx-logger';
import {MatPaginatorIntlMultiLang} from './shared/matpaginatorintl.multilang';
import {MatPaginatorIntl} from '@angular/material/paginator';
import {environment} from './../environments/environment';
import {IAppConfig} from './app.config.interface';
import {AppConfig} from './app.config';
import {IOP_ADMIN_API_BASE_URL} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {EnvironmentService} from './services/environment.serivce';
import {MatomoConfiguration, provideMatomo, withRouter} from 'ngx-matomo-client';
import {NavigationStackService} from './shared/navigation/navigation-stack.service';
import {LindasNotFoundInterceptor} from './shared/interceptors/lindas-not-found.interceptor';

let appConfig = AppConfig.getConfig<IAppConfig>();
const matomoConfig: MatomoConfiguration = {
	disabled: appConfig.ANALYTICS_SITE_ID === null || appConfig.ANALYTICS_SITE_ID === '' ? true : false,
	trackerUrl: 'https://analytics.bit.admin.ch',
	siteId: appConfig.ANALYTICS_SITE_ID
};

registerLocaleData(localeDECH, 'de');
registerLocaleData(localeFRCH, 'fr');
registerLocaleData(localeITCH, 'it');
registerLocaleData(localeENCH, 'en');

@NgModule({
	declarations: [AppComponent],
	bootstrap: [AppComponent],
	imports: [
		AppRoutingModule,
		BrowserModule,
		SharedModule,
		TranslateModule,

		LoggerModule.forRoot({
			level: environment.production ? NgxLoggerLevel.ERROR : NgxLoggerLevel.DEBUG
		})
	],
	providers: [
		provideObliqueConfiguration({
			accessibilityStatement: {
				applicationName: 'i18n.application_name',
				applicationOperator: 'i18n.application_operator',
				conformity: 'none',
				contact: [{email: 'i14y@bfs.admin.ch'}],
				createdOn: new Date('2026-03-31')
			}
		}),
		provideMatomo(matomoConfig, withRouter()),
		provideAppInitializer(() => {
			inject(NavigationStackService);
		}),
		{provide: OB_BANNER, useClass: EnvironmentService},
		{provide: LOCALE_ID, useValue: 'de-CH'},
		{provide: HTTP_INTERCEPTORS, useClass: ObHttpApiInterceptor, multi: true},
		{provide: HTTP_INTERCEPTORS, useClass: LindasNotFoundInterceptor, multi: true},
		{provide: MatPaginatorIntl, useClass: MatPaginatorIntlMultiLang},
		{
			provide: IOP_ADMIN_API_BASE_URL,
			useFactory: () => AppConfig.getConfig<IAppConfig>().IOP_ADMIN_API_BASE_URL
		},
		DatePipe,
		provideHttpClient(withInterceptorsFromDi())
	]
})
export class AppModule {
	constructor(config: ObMasterLayoutConfig) {
		config.locale.locales = ['de', 'fr', 'it', 'en'];
		config.homePageRoute = '/home';
		config.layout.hasMaxWidth = true;
		config.layout.hasOffCanvas = true;
	}
}
