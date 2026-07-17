import {inject, Injectable} from '@angular/core';
import {MatDialog} from '@angular/material/dialog';
import {TranslateService} from '@ngx-translate/core';
import {Observable, ReplaySubject} from 'rxjs';
import {User, UserManager, UserManagerSettings, WebStorageStateStore} from 'oidc-client-ts';
import {AppConfig} from '../app.config';
import {IAppConfig} from '../app.config.interface';
import {DIALOG_OK_BUTTON_KEY} from '../app-constants';
import {DialogComponent, DialogType} from '../shared/dialog/dialog.component';
import {jwtDecode} from 'jwt-decode';
import {UserModel, UsersClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Injectable({
	providedIn: 'root'
})
export class AuthService {
	config: UserManagerSettings = {
		accessTokenExpiringNotificationTimeInSeconds: 900,
		authority: AppConfig.getConfig<IAppConfig>().KEYCLOAK_AUTHORITY_URL,
		client_id: 'BFS.SIS',

		redirect_uri: `${window.location.origin}/signin-callback`,
		post_logout_redirect_uri: `${window.location.origin}/signout-callback`,

		response_type: 'code',

		scope: 'openid offline_access roles',
		loadUserInfo: true,
		automaticSilentRenew: false,
		userStore: new WebStorageStateStore({store: window.localStorage})
	};

	currentUser: User | null = null;
	isAuthenticated$: Observable<boolean>;
	userInfo$: Observable<UserModel | null>;

	private readonly isAuthenticatedSubject: ReplaySubject<boolean> = new ReplaySubject<boolean>(1);
	private readonly userInfoSubject: ReplaySubject<UserModel | null> = new ReplaySubject<UserModel | null>(1);
	private readonly userManager: UserManager;
	private readonly EIAM_DCAT_PREFIX = 'BFS-i14y.';

	private readonly dialog = inject(MatDialog);
	private readonly translate = inject(TranslateService);
	private readonly usersClient = inject(UsersClient);

	constructor() {
		const clientId = AppConfig.getConfig<IAppConfig>().KEYCLOAK_CLIENT_ID;
		this.isAuthenticated$ = this.isAuthenticatedSubject.asObservable();
		this.userInfo$ = this.userInfoSubject.asObservable();
		if (clientId) {
			this.config.client_id = clientId;
		}

		this.userManager = new UserManager(this.config);

		this.userManager.getUser().then(user => {
			this.updateCurrendUser(user);
		});

		this.userManager.events.addAccessTokenExpiring(() => {
			const headertextKey: string = 'i18n.token_expired_dialog.headertext';
			const bodytextKey: string = 'i18n.token_expired_dialog.bodytext';

			this.translate.get([headertextKey, bodytextKey, DIALOG_OK_BUTTON_KEY]).subscribe(result => {
				this.dialog.open(DialogComponent, {
					data: {
						showHeader: true,
						enableSave: false,
						headerText: result[headertextKey],
						bodyText: result[bodytextKey],
						dialogType: DialogType.info,
						okButtonText: result[DIALOG_OK_BUTTON_KEY],
						cancelButtonText: '',
						confirmButtonText: '',
						discardChangesButtonText: '',
						saveChangesButtonText: ''
					},
					disableClose: true
				});
			});
		});
	}

	startAuthentication(): Promise<void> {
		return this.userManager.signinRedirect();
	}

	completeAuthentication(): Promise<void> {
		return this.userManager.signinCallback().then(user => {
			this.updateCurrendUser(user);
		});
	}

	startLogout(): Promise<void> {
		return this.userManager.signoutRedirect();
	}

	completeLogout(): Promise<void> {
		return this.userManager.signoutCallback().then(() => {
			this.updateCurrendUser(null);
		});
	}

	isAuthenticated(): boolean {
		return !!this.currentUser && !!this.currentUser.access_token && !this.currentUser.expired;
	}

	getRoles(): string[] {
		const eiamRoles = (jwtDecode<any>(this.currentUser?.access_token as string).role as string[]) ?? [];
		const eRoles = eiamRoles.filter(r => r.includes(this.EIAM_DCAT_PREFIX)).map(x => x.split(this.EIAM_DCAT_PREFIX).pop() as string);
		return [...eRoles];
	}

	private updateCurrendUser(user: User | null | undefined) {
		if (user) {
			localStorage.setItem('access_token', user.access_token);
			this.currentUser = user;
			this.isAuthenticatedSubject.next(true);
			this.usersClient.getUserInfo().subscribe({
				next: response => {
					this.userInfoSubject.next(response.result);
				},
				error: () => {
					this.userInfoSubject.next(null);
				}
			});
		} else {
			localStorage.removeItem('access_token');
			this.currentUser = null;
			this.isAuthenticatedSubject.next(false);
			this.userInfoSubject.next(null);
		}
	}
}
