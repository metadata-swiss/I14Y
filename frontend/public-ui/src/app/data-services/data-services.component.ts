import {Component, OnInit, OnDestroy, inject} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {SearchEngineOptimizationService} from '../shared/services/search-engine-optimization/search-engine-optimization.service';
import {DataFormat, DataService, DataServiceClient, DataServicesClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ViewType} from '../shared/templates/viewtype';
import {TranslateService, LangChangeEvent} from '@ngx-translate/core';
import {catchError, of, Subject, takeUntil} from 'rxjs';
import {FallbackPipe} from '../shared/fallback/fallback.pipe';
import {DataServiceService} from './services/dataservice.service';
import {PublisherContextService} from '../shared/services/publisher-context/publisher-context.service';
import { ObHttpApiInterceptorEvents, ObNotificationService } from '@oblique/oblique';
import { HttpErrorResponse } from '@angular/common/http';
import { MessageHelperFunctions } from '../shared/message-helper-functions';

@Component({
	selector: 'app-dataservices',
	templateUrl: './data-services.component.html',
	styleUrls: ['./data-services.component.scss'],
	standalone: false
})
export class DataServicesComponent implements OnInit, OnDestroy {
	dataServiceId: string;
	dataService: DataService;
	registrationStatus: string;
	tabs: string[] = ['description'];
	currentLanguage: string;
	readonly viewTypeEnum = ViewType;

	private readonly unsubscribe$ = new Subject();

	private readonly dataServiceClient = inject(DataServiceClient);
	private readonly dataServicesClient = inject(DataServicesClient);
	private readonly dataServiceService = inject(DataServiceService);
	private readonly fallback = inject(FallbackPipe);
	private readonly notification = inject(ObNotificationService);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);
	private readonly publisherContextService = inject(PublisherContextService);
	private readonly route = inject(ActivatedRoute);
	private readonly searchEngineOptimizationService = inject(SearchEngineOptimizationService);
	private readonly translate = inject(TranslateService);

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
		this.route.params.subscribe(params => {
			this.loadDataService(params.dataServiceId);
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	exportDataService(formatSelected: DataFormat) {
		const skippedErrorNotifications = 1;
		this.obHttpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(skippedErrorNotifications);
		this.dataServicesClient
			.getExportByIdAndFormat(this.dataServiceId, formatSelected)
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
				const fileName = `DataService_${this.dataService?.identifiers![0]}${this.dataService.version ? '-' + this.dataService.version : ''}.${formatSelected.toLocaleLowerCase()}`;

				a.href = objectUrl;
				a.download = response.result.fileName ?? fileName;
				a.click();

				URL.revokeObjectURL(objectUrl);
				a.remove();
			});
	}

	public loadDataService(id: string) {
		this.dataServicesClient
			.getByAccessRightsAndDataServiceIdentifierAndPublisherIdentifierAndPublicationLevelAndRegistrationStatusAndPageAndPageSize(
				undefined,
				id,
				undefined,
				undefined,
				undefined,
				1,
				1
			)
			.subscribe(response => {
				this.dataServiceId = response.result.length > 0 ? response.result[0]?.id : id;

				this.publisherContextService.set(response.result[0]?.publisher?.identifier ?? undefined);

				this.dataServiceClient.getById(this.dataServiceId).subscribe(x => {
					this.dataService = x.result;
					this.dataServiceService.setDataService(x.result);
					this.updateMetaData();
				});

				this.registrationStatus = '';
				this.dataServiceClient.getRegistrationStatusById(this.dataServiceId).subscribe(y => {
					if (y.result.status) {
						this.registrationStatus = y.result.status.toString();
					}
				});
			});
	}

	private updateMetaData() {
		this.searchEngineOptimizationService.UpdateMetaTitle(this.fallback.transform(this.dataService?.title, this.currentLanguage));
		this.searchEngineOptimizationService.UpdateMetaDescrition(this.fallback.transform(this.dataService?.description, this.currentLanguage));
	}
}
