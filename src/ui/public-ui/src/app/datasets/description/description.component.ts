import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {DcatDatasetService} from '../services/dcat-dataset.service';
import {PublisherContextService} from 'src/app/shared/services/publisher-context/publisher-context.service';
import {ViewType} from 'src/app/shared/templates/viewtype';
import {DataServiceModel, Dataset, DatasetClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';

@Component({
	selector: 'app-description',
	templateUrl: './description.component.html',
	styleUrls: ['./description.component.scss'],
	standalone: false
})
export class DescriptionComponent implements OnInit, OnDestroy {
	dataset: Dataset;
	isServedBy: DataServiceModel[] = [];
	publisherIdentifier: string | undefined;
	currentLanguage: string;
	readonly emptyPlaceHolder: string = '-';
	readonly viewTypeEnum = ViewType;

	private readonly unsubscribe$ = new Subject();

	private readonly datasetClient = inject(DatasetClient);
	private readonly dcatDatasetService = inject(DcatDatasetService);
	private readonly publisherContextService = inject(PublisherContextService);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
		this.publisherContextService.identifier$.pipe(takeUntil(this.unsubscribe$)).subscribe(id => {
			this.publisherIdentifier = id;
		});
		this.dcatDatasetService.dataset$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.dataset = x;
			this.datasetClient.getIsServedByById(x.id).subscribe(response => {
				this.isServedBy = response.result;
			});
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(null);
		this.unsubscribe$.complete();
	}
}
