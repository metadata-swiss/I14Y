import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {SearchEngineOptimizationService} from '../shared/services/search-engine-optimization/search-engine-optimization.service';
import {Subject, takeUntil} from 'rxjs';
import {DcatPublicServiceService} from './services/dcat-publicservice.service';
import {PublicServiceView, PublicServiceViewClient, PublicServicesClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ViewType} from '../shared/templates/viewtype';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {FallbackPipe} from '../shared/fallback/fallback.pipe';

@Component({
	selector: 'app-public-services',
	templateUrl: './public-services.component.html',
	styleUrls: ['./public-services.component.scss'],
	standalone: false
})
export class PublicServicesComponent implements OnInit, OnDestroy {
	publicServiceId: string;
	publicService: PublicServiceView;
	registrationStatus: string;
	tabs: string[] = ['description'];
	currentLanguage: string;
	readonly viewTypeEnum = ViewType;

	private readonly unsubscribe$ = new Subject();

	private readonly dcatPublicServiceService = inject(DcatPublicServiceService);
	private readonly publicServicesClient = inject(PublicServicesClient);
	private readonly publicServiceViewClient = inject(PublicServiceViewClient);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);
	private readonly fallbackPipe = inject(FallbackPipe);
	private readonly searchEngineOptimizationService = inject(SearchEngineOptimizationService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
		this.route.params.subscribe(params => {
			this.publicServiceId = params.publicServiceId;
			this.loadPublicService(this.publicServiceId);
		});
		this.dcatPublicServiceService.publicService$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.publicService = x;
			this.updateMetaData(x);
		});
	}

	public loadPublicService(identifier: string) {
		this.registrationStatus = '';
		this.publicServicesClient
			.getByPublicServiceIdentifierAndPublisherIdentifierAndPublicationLevelAndRegistrationStatusAndPageAndPageSize(
				identifier,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined
			)
			.subscribe(x => {
				const id = x.result.length > 0 ? x.result[0]?.id : this.publicServiceId;
				this.publicServiceViewClient.getById(id).subscribe(response => {
					this.dcatPublicServiceService.setPublicService(response.result);
				});
				this.publicServiceViewClient.getRegistrationStatusById(id).subscribe(y => {
					if (y.result.status) {
						this.registrationStatus = y.result.status.toString();
					}
				});
			});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	private updateMetaData(publicService: PublicServiceView) {
		let titleText = this.fallbackPipe.transform(publicService.title, this.translate.getCurrentLang()) ?? '';
		this.searchEngineOptimizationService.UpdateMetaTitle(titleText);
		let descriptionText = this.fallbackPipe.transform(publicService.description, this.translate.getCurrentLang()) ?? '';
		this.searchEngineOptimizationService.UpdateMetaDescrition(descriptionText);
	}
}
