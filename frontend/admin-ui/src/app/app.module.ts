import {inject, LOCALE_ID, NgModule, provideAppInitializer} from '@angular/core';
import {AppComponent} from './app.component';
import {AppConfig} from './app.config';
import {AppRoutingModule} from './app-routing.module';
import {BrowserModule} from '@angular/platform-browser';
import {CatalogComponent} from './catalog/catalog.component';
import {environment} from './../environments/environment';
import {HomeComponent} from './home/home.component';
import {UnauthorizedComponent} from './unauthorized/unauthorized.component';
import {HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {IAppConfig} from './app.config.interface';
import {IOP_ADMIN_API_BASE_URL} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LoggerModule, NgxLoggerLevel} from 'ngx-logger';
import {MatPaginatorIntl} from '@angular/material/paginator';
import {MatPaginatorIntlMultiLang} from './shared/matpaginatorintl.multilang';
import {
	ObHttpApiInterceptor,
	ObMasterLayoutConfig,
	ObNotificationConfig,
	OB_BANNER,
	provideObliqueConfiguration,
	ObHttpApiInterceptorConfig
} from '@oblique/oblique';
import {registerLocaleData} from '@angular/common';
import {SharedModule} from './shared/shared.module';
import {TableComponent} from './catalog/table/table.component';
import {TranslateModule} from '@ngx-translate/core';
import localeDECH from '@angular/common/locales/de-CH';
import localeFRCH from '@angular/common/locales/fr-CH';
import localeITCH from '@angular/common/locales/it-CH';
import localeENGB from '@angular/common/locales/en-GB';
import {DashboardComponent} from './home/dashboard/dashboard.component';
import {ClipboardModule} from '@angular/cdk/clipboard';
import {EnvironmentService} from './services/environment.serivce';
import {SigninCallbackComponent} from './auth/signin-callback.component';
import {ApiAuthInterceptor} from './auth/api_auth.interceptor';
import {SignoutCallbackComponent} from './auth/signout-callback.component';
import {NavigationStackService} from './shared/navigation/navigation-stack.service';

registerLocaleData(localeDECH);
registerLocaleData(localeFRCH);
registerLocaleData(localeITCH);
registerLocaleData(localeENGB);

export function loadAppConfig() {
	return AppConfig.loadConfig('assets/config/appconfig.json').catch(e =>
		/* eslint-disable no-console */
		console.debug(e)
	);
}

@NgModule({
	declarations: [
		AppComponent,
		CatalogComponent,
		DashboardComponent,
		HomeComponent,
		SigninCallbackComponent,
		SignoutCallbackComponent,
		TableComponent,
		UnauthorizedComponent
	],
	bootstrap: [AppComponent],
	imports: [
		ClipboardModule,
		AppRoutingModule,
		BrowserModule,
		LoggerModule.forRoot({
			level: environment.production ? NgxLoggerLevel.ERROR : NgxLoggerLevel.DEBUG
		}),
		SharedModule,
		TranslateModule
	],
	providers: [
		provideObliqueConfiguration({
			accessibilityStatement: {
				createdOn: new Date('2026-03-31'),
				conformity: 'none',
				applicationName: 'i18n.application_name',
				applicationOperator: 'i18n.application_operator',
				contact: [{email: 'i14y@bfs.admin.ch'}]
			}
		}),
		provideAppInitializer(async () => {
			await loadAppConfig();
		}),
		// Eagerly start the navigation stack service so it tracks router events from the first navigation.
		provideAppInitializer(() => {
			inject(NavigationStackService);
		}),
		{provide: OB_BANNER, useClass: EnvironmentService},
		{provide: LOCALE_ID, useValue: 'de-CH'},
		{provide: MatPaginatorIntl, useClass: MatPaginatorIntlMultiLang},
		{
			provide: IOP_ADMIN_API_BASE_URL,
			useFactory: () => AppConfig.getConfig<IAppConfig>().API_BASE_URL
		},
		provideHttpClient(withInterceptorsFromDi()),
		{provide: HTTP_INTERCEPTORS, useClass: ObHttpApiInterceptor, multi: true},
		{provide: HTTP_INTERCEPTORS, useClass: ApiAuthInterceptor, multi: true}
	]
})
export class AppModule {
	constructor(
		config: ObMasterLayoutConfig,
		private readonly notificationConfig: ObNotificationConfig,
		readonly interceptorConfig: ObHttpApiInterceptorConfig
	) {
		this.notificationConfig.sticky = false;
		this.notificationConfig.error = {title: 'i18n.oblique.notification.type.error', sticky: false};

		config.locale.locales = ['de-CH', 'fr-CH', 'it-CH', 'en-GB'];
		config.layout.hasMaxWidth = true;
	}
}
