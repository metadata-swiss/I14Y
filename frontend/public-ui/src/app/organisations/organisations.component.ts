import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {
	AgentClient,
	AgentStatisticsResult,
	SearchResourceType,
	IMultiLanguage,
	IdNameModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService, LangChangeEvent} from '@ngx-translate/core';
import {map, Observable, Subject, takeUntil} from 'rxjs';
import {FallbackPipe} from '../shared/fallback/fallback.pipe';
import { IAgentWithSubagentsResult } from './agent-with-subagent';

@Component({
	selector: 'app-organisations',
	templateUrl: './organisations.component.html',
	styleUrls: ['./organisations.component.scss'],
	standalone: false
})
export class OrganisationsComponent implements OnInit, OnDestroy {
	displayedAgents$: Observable<IAgentWithSubagentsResult[]>;
	currentLanguage: string;
	catalogType = SearchResourceType;

	private readonly unsubscribe$ = new Subject();

	private readonly agentClient = inject(AgentClient);
	private readonly fallback = inject(FallbackPipe);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});

		this.displayedAgents$ = this.agentClient.getStatistics().pipe(map(response => this.mapAgents(this.removeAgentsWithoutPublication(response.result))));
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}


	private removeAgentsWithoutPublication(agents: AgentStatisticsResult[]): AgentStatisticsResult[] {
		return agents.filter(x => x.types.find(t => t.count > 0));
	}

	private getTranslated(source: IMultiLanguage | undefined): string | undefined {
		return this.fallback.transform(source, this.currentLanguage) ?? undefined;
	}

	private mapAgents(agents: AgentStatisticsResult[]): IAgentWithSubagentsResult[] {
		const firstStepAgents = [...agents]
			.filter(x => x.publisher?.subOrganizationOf === undefined)
			.sort((a, b) => {
				return this.getTranslated(a.publisher?.name)?.localeCompare(this.getTranslated(b.publisher?.name) ?? '') ?? 0;
			});

		return firstStepAgents.map(
			agent =>
				({
					id: agent.publisher?.id,
					identifier: agent.publisher?.identifier,
					name: agent.publisher?.name,
					types: agent.types,
					subagents: this.mapSubAgents(agents, agent.publisher?.hasSubOrganizations ?? []),
					collapsed: true
				}) as IAgentWithSubagentsResult
		);
	}
	
	private mapSubAgents(agents: AgentStatisticsResult[], subagents: IdNameModel[]): IAgentWithSubagentsResult[] {
		const orderedSubagents = [...subagents].sort((a, b) => this.getTranslated(a.name)?.localeCompare(this.getTranslated(b.name) ?? '') ?? 0);

		return orderedSubagents.map(subagent => {
			const fullSubagent = agents.find(x => x.publisher?.id === subagent.id);
			return {
				id: subagent.id,
				identifier: fullSubagent?.publisher?.identifier,
				name: subagent.name,
				types: fullSubagent?.types ?? [],
				subagents: this.mapSubAgents(agents, fullSubagent?.publisher?.hasSubOrganizations ?? []),
				collapsed: true
			} as IAgentWithSubagentsResult;
		});
	}
}
