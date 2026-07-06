import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {Agent, AgentClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ActivatedRoute} from '@angular/router';
import {ExtendedFallbackPipe} from 'src/app/shared/fallback/extendedfallback.pipe';
import {FallBackModel} from 'src/app/shared/fallback/fallbackmodel';

@Component({
	selector: 'app-description',
	templateUrl: './description.component.html',
	styleUrls: ['./description.component.scss'],
	standalone: false
})
export class DescriptionComponent implements OnInit, OnDestroy {
	agent!: Agent;
	currentLanguage: string;
	logoUrl?: string;

	private readonly unsubscribe$ = new Subject();

	private readonly agentClient = inject(AgentClient);
	private readonly extendedFallback = inject(ExtendedFallbackPipe);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
		this.route.params.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
			this.agentClient.getById(params.agentId).subscribe(response => {
				this.agent = response.result;
				this.logoUrl = this.agent?.images?.[0]?.uri;
			});
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(null);
		this.unsubscribe$.complete();
	}

	getSpatialCH(): FallBackModel[] {
		return this.agent?.spatialCH?.map(s => this.extendedFallback.transform(s.name, this.currentLanguage));
	}
}
