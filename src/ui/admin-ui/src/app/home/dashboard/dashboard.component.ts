import {Component, inject} from '@angular/core';
import {Agent, AgentClient, CatalogClient, FilterCountResultItem} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {AuthService} from 'src/app/auth/auth.service';

@Component({
	selector: 'app-dashboard',
	templateUrl: './dashboard.component.html',
	styleUrls: ['./dashboard.component.scss'],
	standalone: false
})
export class DashboardComponent {
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

	private readonly agentClient = inject(AgentClient);
	private readonly catalogClient = inject(CatalogClient);
	private readonly authService = inject(AuthService);

	constructor() {
		this.agentClient.getUser().subscribe((agent: any) => {
			this.publishersIdendifier = agent.result.map((value: Agent) => value.identifier);
			this.userEmail = this.authService.currentUser?.profile?.email ?? undefined;
			this.getData();
			this.getUserData();
		});
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
