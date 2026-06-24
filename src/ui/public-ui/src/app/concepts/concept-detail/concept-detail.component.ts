import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';
import {ConceptService} from '../services/concept.service';
import {ViewType} from 'src/app/shared/templates/viewtype';
import {FilterConfigurationsClient, FilterConfigurationModel, ConceptViewClient, ConceptView, ConceptType} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {OffCanvasService} from 'src/app/shared/services/off-canvas/off-canvas.service';
import {HttpErrorResponse, HttpStatusCode} from '@angular/common/http';

@Component({
	selector: 'app-concept-detail',
	templateUrl: './concept-detail.component.html',
	styleUrls: ['./concept-detail.component.scss'],
	standalone: false
})
export class ConceptDetailComponent implements OnInit, OnDestroy {
	tabs: string[] = [];
	conceptView: ConceptView;
	registrationStatus: string;
	conceptType = ConceptType;

	currentLanguage: string;
	selectedLanguage: string;
	readonly viewTypeEnum = ViewType;

	private conceptId: string = '';
	private readonly languages: string[] = ['de', 'fr', 'it', 'en', 'rm'];

	private readonly unsubscribe$ = new Subject();

	private readonly conceptViewClient = inject(ConceptViewClient);
	private readonly conceptService = inject(ConceptService);
	private readonly filterConfigurationsClient = inject(FilterConfigurationsClient);
	private readonly offCanvasService = inject(OffCanvasService);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
		this.route.params.subscribe(params => {
			this.conceptId = params.conceptId;
			this.registrationStatus = '';

			if (this.conceptId) {
				this.conceptViewClient.getById(this.conceptId).subscribe(response => {
					this.conceptService.setConceptView(response.result);
					this.conceptView = response.result;

					this.conceptViewClient.getRegistrationStatusById(this.conceptId).subscribe(res => {
						this.registrationStatus = res.result.status;
					});
					const titleKey = 'i18n.offcanvas.filter.title';
					this.filterConfigurationsClient.getById(this.conceptId).subscribe(
						res => {
							this.offCanvasService.setConfiguration(res.result, this.languages, titleKey, this.route);
						},
						(error: HttpErrorResponse) => {
							if (error.status === HttpStatusCode.NoContent) {
								this.offCanvasService.setConfiguration(new FilterConfigurationModel({filters: []}), this.languages, titleKey, this.route);
							}
						}
					);

					this.tabs = [];
					this.tabs.push('description');

					if (response.result.conceptType === this.conceptType.CodeList) {
						this.tabs.push('content');
					}
				});
			}
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}
}
