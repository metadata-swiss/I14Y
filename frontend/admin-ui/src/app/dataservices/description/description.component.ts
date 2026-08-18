import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {
	AllowActionResourceType,
	DataServiceInputClient,
	DataServiceModel,
	DataServicesClient,
	Dataset,
	DcatCatalogInputClient,
	DcatCatalogRecordInput,
	DcatVocabularyEntry
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {FormatFunctions} from 'src/app/shared/format-functions';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {takeUntil} from 'rxjs/operators';
import {NAV_VALUE_EDIT} from 'src/app/app-constants';
import {Subject} from 'rxjs';
import {DataserviceService} from '../services/dataservice.service';
import {AllowActionService} from 'src/app/services/allow.action.service';
import {ViewType} from 'src/app/shared/templates/viewtype';

@Component({
	selector: 'app-description',
	templateUrl: './description.component.html',
	styleUrls: ['./description.component.scss'],
	standalone: false
})
export class DescriptionComponent implements OnInit, OnDestroy {
	dataservice: DataServiceModel = new DataServiceModel();
	nextVersions: DataServiceModel[] | undefined;
	previousVersion: DataServiceModel | undefined;
	servesDatasets: Dataset[] | undefined;
	currentLanguage: string;
	dataServiceId: string | undefined;
	readonly nav_value_edit: string = NAV_VALUE_EDIT;
	readonly viewTypeEnum = ViewType;
	catalogsAndThemes: DcatCatalogRecordInput[] = [];

	private readonly unsubscribe$ = new Subject();

	private readonly allowActionService = inject(AllowActionService);
	private readonly dataServiceInputClient = inject(DataServiceInputClient);
	private readonly dataServicesClient = inject(DataServicesClient);
	private readonly dataserviceService = inject(DataserviceService);
	private readonly dcatCatalogInputClient = inject(DcatCatalogInputClient);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.route.parent?.params.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
			this.dataServiceId = params.id;
			this.allowActionService.load(params.id, AllowActionResourceType.DataService);
		});
		this.dataserviceService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.dataservice = x;
			this.getDcatCatalogRecordInput();

			if (this.dataservice.id && this.dataservice.id === this.dataServiceId) {
				this.dataServiceInputClient.getServesDatasetsById(this.dataservice.id!).subscribe(response => {
					this.servesDatasets = response.result ?? undefined;
				});
				this.dataServicesClient.getNextVersionsByIdAndPageAndPageSize(this.dataservice.id!, undefined, undefined).subscribe(response => {
					this.nextVersions = response.result;
				});

				if (this.dataservice.previousVersion?.id) {
					this.dataServicesClient.getById(this.dataservice.previousVersion.id).subscribe(response => {
						this.previousVersion = response.result;
					});
				}
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
		if (this.dataservice.id) {
			this.dcatCatalogInputClient.getRecordsByResourceByResourceId(this.dataservice.id).subscribe(response => {
				this.catalogsAndThemes = response.result;
			});
		}
	}

	getDisplayableCatalogThemes(themes: DcatVocabularyEntry[] | undefined): DcatVocabularyEntry[] {
		return (themes ?? []).filter(theme => (FormatFunctions.getTranslatedVocabularyEntries([theme], this.currentLanguage) ?? []).length > 0);
	}
}
