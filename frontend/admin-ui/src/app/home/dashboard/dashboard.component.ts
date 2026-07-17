import {Component, inject, OnDestroy} from '@angular/core';
import {CatalogClient, FilterCountResultItem} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Subject, takeUntil} from 'rxjs';
import {AuthService} from 'src/app/auth/auth.service';

@Component({
	selector: 'app-dashboard',
	templateUrl: './dashboard.component.html',
	styleUrls: ['./dashboard.component.scss'],
	standalone: false
})
export class DashboardComponent implements OnDestroy {
	public publishersIdendifier!: string[];
	public userEmail: string | undefined;
	// Quoted email used as the "My Data" search query
	public userEmailQuery: string | undefined;

	public catalogAgencyData: FilterCountResultItem[] | undefined;
	public catalogAgencyPenndingLevels: FilterCountResultItem[] | undefined;
	public catalogAgencyPenndingStatuses: FilterCountResultItem[] | undefined;
	public catalogUserData: FilterCountResultItem[] | undefined;

	public conceptAgencyData: FilterCountResultItem[] | undefined;
	public conceptAgencyPenndingLevels: FilterCountResultItem[] | undefined;
	public conceptAgencyPenndingStatuses: FilterCountResultItem[] | undefined;

	private readonly unsubscribe$ = new Subject<void>();

	private readonly catalogClient = inject(CatalogClient);
	private readonly authService = inject(AuthService);

	constructor() {
		this.authService.userInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(userInfo => {
			if (userInfo) {
				this.publishersIdendifier = (userInfo.agents ?? []).map(value => value.identifier as string).filter((x): x is string => !!x) ?? [];
				this.userEmail = userInfo.email;
				this.getData();
				this.getUserData();
			} else {
				this.catalogAgencyData = undefined;
				this.catalogAgencyPenndingLevels = undefined;
				this.catalogAgencyPenndingStatuses = undefined;
				this.catalogUserData = undefined;
			}
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next();
		this.unsubscribe$.complete();
	}

	getData() {
		this.catalogClient // eslint-disable-next-line max-len
			.getSearchcountByQueryAndAccessRightsAndConceptValueTypesAndBusinessEventsAndFormatsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypes(
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				this.publishersIdendifier,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined
			)
			.subscribe(response => {
				this.catalogAgencyData = response.result.types ?? undefined;
				this.catalogAgencyPenndingLevels = response.result.publicationLevelProposals?.filter(x => x.count && x.count > 0) ?? undefined;
				this.catalogAgencyPenndingStatuses = response.result.registrationStatusProposals?.filter(x => x.count && x.count > 0) ?? undefined;
			});
	}

	getUserData() {
		if (!this.userEmail) return;
		this.userEmailQuery = `"${this.userEmail}"`;
		this.catalogClient // eslint-disable-next-line max-len
			.getSearchcountByQueryAndAccessRightsAndConceptValueTypesAndBusinessEventsAndFormatsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypes(
				this.userEmailQuery,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined
			)
			.subscribe(response => {
				this.catalogUserData = response.result.types ?? undefined;
			});
	}

	getRegistrationStatusKey(code: string): string {
		return 'i18n.status.registrationstatus.' + code.replace(' ', '').toLowerCase();
	}

	getPublicationLevelKey(code: string): string {
		return 'i18n.status.publicationlevel.' + code.replace(' ', '').toLowerCase();
	}
}
