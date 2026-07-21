import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {SearchEngineOptimizationService} from '../shared/services/search-engine-optimization/search-engine-optimization.service';
import {catchError, of, Subject, takeUntil} from 'rxjs';
import {DcatPublicServiceService} from './services/dcat-publicservice.service';
import {DataFormat, PublicServiceView, PublicServiceViewClient, PublicServicesClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ViewType} from '../shared/templates/viewtype';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {FallbackPipe} from '../shared/fallback/fallback.pipe';
import { ObHttpApiInterceptorEvents, ObNotificationService } from '@oblique/oblique';
import { HttpErrorResponse } from '@angular/common/http';
import { MessageHelperFunctions } from '../shared/message-helper-functions';

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
	private readonly fallbackPipe = inject(FallbackPipe);
	private readonly notification = inject(ObNotificationService);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);
	private readonly publicServicesClient = inject(PublicServicesClient);
	private readonly publicServiceViewClient = inject(PublicServiceViewClient);
	private readonly route = inject(ActivatedRoute);
	private readonly searchEngineOptimizationService = inject(SearchEngineOptimizationService);
	private readonly translate = inject(TranslateService);

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

	export(formatSelected: DataFormat) {
		const skippedErrorNotifications = 1;
		this.obHttpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(skippedErrorNotifications);
		this.publicServicesClient
			.getExportByIdAndFormat(this.publicService.id!, formatSelected)
			.pipe(
				catchError((error: HttpErrorResponse) => {
					this.notification.error(MessageHelperFunctions.getExportErrorMessage(error));
					return of();
				})
			)
			.subscribe(response => {
				const a = document.createElement('a');
				const objectUrl = URL.createObjectURL(response.result.data);
				// eslint-disable-next-line max-len
				const fileName = `PublicService_${this.publicService?.identifiers![0]}.${formatSelected.toLocaleLowerCase()}`;

				a.href = objectUrl;
				a.download = response.result.fileName ?? fileName;
				a.click();

				URL.revokeObjectURL(objectUrl);
				a.remove();
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
