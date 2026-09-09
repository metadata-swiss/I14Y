import {ActivatedRoute} from '@angular/router';
import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {
	AllowActionResourceType,
	AllowActionType,
	DataServiceModel,
	DatasetsClient,
	DcatDistributionModel,
	PeriodOfTime,
	ResourceModel,
	VocabularyEntryModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {FormatFunctions} from 'src/app/shared/format-functions';
import {VOCAB_ID_FILE_TYPE, VOCAB_ID_MEDIA_TYPE} from 'src/app/app-constants';
import {VocabularyConfigService} from 'src/app/services/vocabulary-config.service';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {map, startWith, takeUntil} from 'rxjs/operators';
import {NAV_VALUE_DETAIL, NAV_VALUE_EDIT} from 'src/app/app-constants';
import {Observable, of, Subject} from 'rxjs';
import {AllowActionService} from 'src/app/services/allow.action.service';
import {DatasetService} from '../../services/dataset.service';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';

@Component({
	selector: 'app-distributions-detail',
	templateUrl: './distributions-detail.component.html',
	styleUrls: ['./distributions-detail.component.scss'],
	standalone: false
})
export class DistributionsDetailComponent implements OnInit, OnDestroy {
	distributionId = '';
	data: DcatDistributionModel | undefined;
	dataServices: DataServiceModel[] | undefined;
	currentLanguage: string;
	target = '_blank';
	rel = 'noopener noreferrer';
	readonly from: string = NAV_VALUE_DETAIL;
	readonly nav_value_edit: string = NAV_VALUE_EDIT;
	cannotEdit$: Observable<boolean> = of(true);
	formatConceptPageIri: string | undefined = undefined;
	mediaTypeVocabularyIri: string | undefined = undefined;

	private datasetId = '';
	private readonly unsubscribe$ = new Subject();

	private readonly allowActionService = inject(AllowActionService);
	private readonly datasetsClient = inject(DatasetsClient);
	private readonly datasetService = inject(DatasetService);
	private readonly fallback = inject(FallbackPipe);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyConfigService = inject(VocabularyConfigService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => (this.currentLanguage = language.lang));

		this.vocabularyConfigService
			.resolveIris([VOCAB_ID_MEDIA_TYPE])
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(iris => {
				this.mediaTypeVocabularyIri = iris[VOCAB_ID_MEDIA_TYPE];
			});

		this.vocabularyConfigService
			.resolveConceptPageIris([VOCAB_ID_FILE_TYPE])
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(iris => {
				this.formatConceptPageIri = iris[VOCAB_ID_FILE_TYPE];
			});
		this.route.parent?.params.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
			this.datasetId = params.id;
			this.datasetService.load(params.id);
		});

		this.distributionId = this.route.snapshot.params.distributionId;

		this.datasetService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.data = x.distributions?.find(d => d.id === this.distributionId);

			this.datasetsClient.getDistributionsAccessServicesByDatasetIdAndDistributionId(x.id!, this.distributionId).subscribe(response => {
				this.dataServices = response.result;
			});
		});

		this.allowActionService.load(this.datasetId, AllowActionResourceType.Dataset);
		this.cannotEdit$ = this.allowActionService.allowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.actionType === AllowActionType.Edit)?.value),
			startWith(true)
		);
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	getAccessUrl(): ResourceModel[] {
		return this.data?.accessUrl ? [this.data?.accessUrl] : [];
	}

	getDownloadUrl() {
		return this.data?.downloadUrl ? [this.data?.downloadUrl] : [];
	}

	getFormattedDate(date: Date | undefined): string | null {
		return FormatFunctions.getFormattedDate(date);
	}

	getLanguages(languages: VocabularyEntryModel[] | undefined): string {
		return FormatFunctions.convertArrayToString(languages?.map(l => this.fallback.transform(l.name, this.currentLanguage) ?? ''));
	}

	getCoverage(coverage: PeriodOfTime[] | undefined): string[] | undefined {
		return coverage ? coverage.map(e => `${FormatFunctions.getFormattedDate(e.start)} - ${FormatFunctions.getFormattedDate(e.end)}`) : undefined;
	}
}
