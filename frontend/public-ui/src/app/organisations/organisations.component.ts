import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {
	AgentClient,
	AgentStatisticsResult,
	SearchResourceType,
	SearchResourceTypeSearchCountResultItem,
	IMultiLanguage,
	SwaggerResponse
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService, LangChangeEvent} from '@ngx-translate/core';
import {map, Observable, Subject, takeUntil} from 'rxjs';
import {FallbackPipe} from '../shared/fallback/fallback.pipe';

@Component({
	selector: 'app-organisations',
	templateUrl: './organisations.component.html',
	standalone: false
})
export class OrganisationsComponent implements OnInit, OnDestroy {
	agents$: Observable<AgentStatisticsResult[]>;
	orderedAgents$: Observable<AgentStatisticsResult[]>;
	currentLanguage: string;
	columnsToDisplay = ['name', 'datasets', 'publicservices', 'dataservices', 'concepts', 'mappingtables', 'actions'];
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
			this.orderAgents();
		});

		this.agents$ = this.agentClient.getStatistics().pipe(map(response => this.removeAgentsWithoutPublication(response.result)));
		this.orderAgents();
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	getCatalogType(agent: AgentStatisticsResult, type: SearchResourceType): SearchResourceTypeSearchCountResultItem {
		return agent?.types?.find(t => t.value === type);
	}

	private removeAgentsWithoutPublication(agents: AgentStatisticsResult[]): AgentStatisticsResult[] {
		return agents.filter(x => x.types.find(t => t.count > 0));
	}

	private orderAgents() {
		this.orderedAgents$ = this.agents$.pipe(
			map(x =>
				[...x].sort((a, b) => {
					return this.getTranslated(a.publisher.name).localeCompare(this.getTranslated(b.publisher.name));
				})
			)
		);
	}

	private getTranslated(source: IMultiLanguage | undefined): string | undefined {
		return this.fallback.transform(source, this.currentLanguage) ?? undefined;
	}
}
