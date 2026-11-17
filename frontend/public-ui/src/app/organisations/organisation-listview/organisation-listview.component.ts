import {Component, inject, Input, OnDestroy, OnInit} from '@angular/core';
import {IAgentWithSubagentsResult} from '../agent-with-subagent';
import {IMultiLanguage, SearchResourceType, SearchResourceTypeSearchCountResultItem} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import { Subject, takeUntil } from 'rxjs';
import { FallbackPipe } from 'src/app/shared/fallback/fallback.pipe';

@Component({
	selector: 'app-organisation-listview',
	templateUrl: './organisation-listview.component.html',
	styleUrls: ['./organisation-listview.component.scss'],
	standalone: false
})
export class OrganisationListViewComponent implements OnInit, OnDestroy {
	@Input() agents: IAgentWithSubagentsResult[] = [];
	readonly prefix = 'agent-list';
	catalogType = SearchResourceType;
	currentLanguage: string;

	private readonly unsubscribe$ = new Subject();

	private readonly fallback = inject(FallbackPipe);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
			this.agents = this.orderAgents(this.agents);
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	itemKey(item: IAgentWithSubagentsResult): string {
		return `${this.prefix}-${item.id}`;
	}

	getCatalogType(agent: IAgentWithSubagentsResult, type: SearchResourceType): SearchResourceTypeSearchCountResultItem | undefined {
		return agent?.types?.find(t => t.value === type);
	}

	changeCollapsed(items: IAgentWithSubagentsResult[], collapsed: boolean, all = false): void {
		items
			.filter(item => item.subagents)
			.forEach((item: IAgentWithSubagentsResult) => {
				item.collapsed = collapsed;
				if (all) {
					this.changeCollapsed(item.subagents, collapsed, all);
				}
			});
	}

	private orderAgents(agents: IAgentWithSubagentsResult[]): IAgentWithSubagentsResult[] {
		let orderedAgents = [...agents].sort((a, b) => {
				return this.getTranslated(a.name)?.localeCompare(this.getTranslated(b.name) ?? '') ?? 0;
		});

		orderedAgents.forEach(agent => {
			if (agent.subagents && agent.subagents.length > 0) {
				agent.subagents = this.orderAgents(agent.subagents);
			}
		})

		return orderedAgents;
	}

	private getTranslated(source: IMultiLanguage | undefined): string | undefined {
			return this.fallback.transform(source, this.currentLanguage) ?? undefined;
	}
}
