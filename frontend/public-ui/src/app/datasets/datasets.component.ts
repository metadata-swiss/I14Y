import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {DataFormat, Dataset, DatasetClient, DatasetInputClient, DatasetQualityInformationClient, DatasetsClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ActivatedRoute} from '@angular/router';
import {of, Subject} from 'rxjs';
import {catchError, take, takeUntil} from 'rxjs/operators';
import {ObHttpApiInterceptorEvents, ObIHttpApiRequest, ObNotificationService} from '@oblique/oblique';
import {ViewType} from '../shared/templates/viewtype';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {MessageHelperFunctions} from '../shared/message-helper-functions';
import {DcatDatasetService} from './services/dcat-dataset.service';
import {SearchEngineOptimizationService} from '../shared/services/search-engine-optimization/search-engine-optimization.service';
import {PublisherContextService} from '../shared/services/publisher-context/publisher-context.service';
import {DatasetQualityInformationDataService} from './services/dataset-quality-information-data.service';
import {HttpErrorResponse} from '@angular/common/http';

@Component({
	selector: 'app-datasets',
	templateUrl: './datasets.component.html',
	standalone: false
})
export class DatasetsComponent implements OnInit, OnDestroy {
	dataset: Dataset;
	registrationStatus: string;
	tabs: string[] = [];
	currentLanguage: string;
	datasetId: string;
	hasQualityInfo: boolean = false;
	structureExist: boolean = false;
	readonly viewTypeEnum = ViewType;

	private configIdentifier: string = '';
	private readonly unsubscribe$ = new Subject();

	private readonly datasetClient = inject(DatasetClient);
	private readonly datasetInputClient = inject(DatasetInputClient);
	private readonly datasetsClient = inject(DatasetsClient);
	private readonly dcatDatasetService = inject(DcatDatasetService);
	private readonly datasetQualityInformationClient = inject(DatasetQualityInformationClient);
	private readonly datasetQualityInformationDataService = inject(DatasetQualityInformationDataService);
	private readonly fallbackPipe = inject(FallbackPipe);
	private readonly notification = inject(ObNotificationService);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);
	private readonly route = inject(ActivatedRoute);
	private readonly publisherContextService = inject(PublisherContextService);
	private readonly searchEngineOptimizationService = inject(SearchEngineOptimizationService);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
		this.route.params.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
			this.configIdentifier = params.configIdentifier;
			this.loadDcatDataset(this.configIdentifier);
		});
		this.dcatDatasetService.dataset$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.dataset = x;
			this.updateMetaData(x);
		});
	}

	exportDataset(formatSelected: DataFormat) {
		const skippedErrorNotifications = 1;
		this.obHttpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(skippedErrorNotifications);
		this.datasetClient
			.getExportByIdAndFormat(this.datasetId, formatSelected)
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
				const fileName = `Dataset_${this.dataset?.identifiers![0]}${this.dataset.version ? '-' + this.dataset.version : ''}.${formatSelected.toLocaleLowerCase()}`;

				a.href = objectUrl;
				a.download = response.result.fileName ?? fileName;
				a.click();

				URL.revokeObjectURL(objectUrl);
				a.remove();
			});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	private updateMetaData(dataset: Dataset) {
		let titleText = this.fallbackPipe.transform(dataset.title, this.translate.getCurrentLang()) ?? '';
		this.searchEngineOptimizationService.UpdateMetaTitle(titleText);
		let descriptionText = this.fallbackPipe.transform(dataset.description, this.translate.getCurrentLang()) ?? '';
		this.searchEngineOptimizationService.UpdateMetaDescrition(descriptionText);
	}

	private loadDcatDataset(datasetIdentifier: string) {
		this.datasetsClient
			.getByAccessRightsAndDatasetIdentifierAndPublisherIdentifierAndPublicationLevelAndRegistrationStatusAndPageAndPageSize(
				undefined,
				datasetIdentifier,
				undefined,
				undefined,
				undefined,
				1,
				1
			)
			.subscribe(response => {
				this.datasetId = response.result.length > 0 ? response.result[0]?.id : datasetIdentifier;

				this.publisherContextService.set(response.result[0]?.publisher?.identifier ?? undefined);

				this.datasetClient.getById(this.datasetId).subscribe(r => {
					this.dcatDatasetService.setDataset(r.result);
					this.dataset = r.result;
				});

				this.loadStructureExists(this.datasetId).then(() => {
					this.loadQualityInformation(this.datasetId).then(() => {
						this.tabs = this.getVisibleTabs();
					});
				});
				this.datasetClient.getRegistrationStatusById(this.datasetId).subscribe(y => {
					if (y.result.status) {
						this.registrationStatus = y.result.status.toString();
					}
				});
			});
	}

	private loadStructureExists(id: string): Promise<void> {
		return new Promise<void>(resolve => {
			this.datasetInputClient.getModelExistsById(id).subscribe(response => {
				this.structureExist = response.result;
				resolve();
			});
		});
	}

	private loadQualityInformation(datasetId: string): Promise<void> {
		return new Promise<void>(resolve => {
			this.obHttpApiInterceptorEvents.requestIntercepted.pipe(take(1)).subscribe((evt: ObIHttpApiRequest) => {
				evt.notification.active = false;
				evt.spinner = false;
			});
			this.datasetQualityInformationClient.getByDatasetId(datasetId).subscribe({
				next: x => {
					this.datasetQualityInformationDataService.setData(x.result);
					if (x.result?.documentation?.length || x.result?.qualityInformations?.length > 0) {
						this.hasQualityInfo = true;
						resolve();
					} else {
						this.hasQualityInfo = false;
						resolve();
					}
				},
				error: _ => {
					this.hasQualityInfo = false;
					resolve();
				}
			});
		});
	}

	private getVisibleTabs(): string[] {
		let tabs = ['description'];
		if (this.structureExist) {
			tabs.push('structure');
		}
		if (this.hasQualityInfo) {
			tabs.push('qualityinfo');
		}
		return tabs;
	}
}
