import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {
	AllowActionResourceType,
	DataServiceModel,
	DatasetClient,
	DatasetsClient,
	DcatCatalogInputClient,
	DcatCatalogRecordInput,
	DcatDatasetModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {FormatFunctions} from 'src/app/shared/format-functions';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {takeUntil} from 'rxjs/operators';
import {NAV_VALUE_EDIT} from 'src/app/app-constants';
import {Subject} from 'rxjs';
import {DatasetService} from '../services/dataset.service';
import {AllowActionService} from 'src/app/services/allow.action.service';
import {ViewType} from 'src/app/shared/templates/viewtype';

@Component({
	selector: 'app-description',
	templateUrl: './description.component.html',
	styleUrls: ['./description.component.scss'],
	standalone: false
})
export class DescriptionComponent implements OnInit, OnDestroy {
	dataset: DcatDatasetModel = new DcatDatasetModel();
	nextVersions: DcatDatasetModel[] | undefined;
	previousVersion: DcatDatasetModel | undefined;
	isServedBy: DataServiceModel[] = [];
	currentLanguage: string;
	readonly nav_value_edit: string = NAV_VALUE_EDIT;
	readonly viewTypeEnum = ViewType;
	catalogsAndThemes: DcatCatalogRecordInput[] = [];
	geoIvList: DcatCatalogRecordInput[] = [];

	private readonly unsubscribe$ = new Subject();

	private readonly allowActionService = inject(AllowActionService);
	private readonly datasetClient = inject(DatasetClient);
	private readonly datasetsClient = inject(DatasetsClient);
	private readonly datasetService = inject(DatasetService);
	private readonly dcatCatalogInputClient = inject(DcatCatalogInputClient);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.route.parent?.params.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
			this.datasetService.load(params.id);
			this.allowActionService.load(params.id, AllowActionResourceType.Dataset);
		});
		this.datasetService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.dataset = x;
			this.getDcatCatalogRecordInput();

			this.datasetClient.getIsServedByById(x.id!).subscribe(response => {
				this.isServedBy = response.result;
			});

			this.datasetsClient.getNextVersionsByIdAndPageAndPageSize(x.id!, undefined, undefined).subscribe(response => {
				this.nextVersions = response.result;
			});

			if (x.previousVersion?.id) {
				this.datasetsClient.getById(x.previousVersion.id).subscribe(response => {
					this.previousVersion = response.result;
				});
			}
		});

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	getDcatCatalogRecordInput(): void {
		if (this.dataset.id) {
			this.dcatCatalogInputClient.getRecordsByResourceByResourceId(this.dataset.id).subscribe(response => {
				this.catalogsAndThemes = response.result;
			});
		}
	}

	getCatalogsAndThemes(themes: any[] | undefined): string[] | undefined {
		return FormatFunctions.getTranslatedVocabularyEntries(themes, this.currentLanguage);
	}
}
