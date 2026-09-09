import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute, NavigationEnd, Router} from '@angular/router';
import {AppConfig} from 'src/app/app.config';
import {IAppConfig} from 'src/app/app.config.interface';
import {filter, Subject, takeUntil} from 'rxjs';
import {ConceptService, SearchExportInfo} from '../../services/concept.service';
import {ObOffCanvasService} from '@oblique/oblique';
import {OffCanvasService} from 'src/app/shared/services/off-canvas/off-canvas.service';

@Component({
	selector: 'app-content',
	templateUrl: './content.component.html',
	styleUrls: ['./content.component.scss'],
	standalone: false
})
export class ContentComponent implements OnInit, OnDestroy {
	jsonUrl: string;
	jsonWithoutAnnotationsUrl: string;
	csvUrl: string;
	csvWithoutAnnotationsUrl: string;
	filterVidible: boolean = false;

	downloadDisabled: boolean = true;
	searchExport: SearchExportInfo | null = null;
	isSearchView: boolean = false;

	private readonly unsubscribe$ = new Subject();

	private readonly conceptService = inject(ConceptService);
	private readonly offCanvas = inject(ObOffCanvasService);
	private readonly offCanvasService = inject(OffCanvasService);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);

	ngOnInit() {
		this.route.parent.params.subscribe(params => {
			if (params.conceptId) {
				if (AppConfig.getConfig<IAppConfig>().PUBLIC_API_BASE_URL) {
					this.jsonUrl = `${AppConfig.getConfig<IAppConfig>().PUBLIC_API_BASE_URL}/concepts/${params.conceptId}/codelist-entries/exports/json`;
					this.jsonWithoutAnnotationsUrl = `${AppConfig.getConfig<IAppConfig>().PUBLIC_API_BASE_URL}/concepts/${params.conceptId}/codelist-entries/exports/Json?withAnnotations=false`;
					this.csvUrl = `${AppConfig.getConfig<IAppConfig>().PUBLIC_API_BASE_URL}/concepts/${params.conceptId}/codelist-entries/exports/csv`;
					this.csvWithoutAnnotationsUrl = `${AppConfig.getConfig<IAppConfig>().PUBLIC_API_BASE_URL}/concepts/${params.conceptId}/codelist-entries/exports/Csv?withAnnotations=false`;
				}
			}
		});

		this.conceptService.navTreeWithPagingInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.downloadDisabled = x.pagingInfo.totalRows === 0;
		});

		this.conceptService.searchExport$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.searchExport = x;
		});

		this.offCanvasService.toList$.pipe(takeUntil(this.unsubscribe$)).subscribe(queryParams => {
			this.router.navigate(['./'], {
				relativeTo: this.route,
				queryParams
			});
		});

		this.offCanvasService.toSearch$.pipe(takeUntil(this.unsubscribe$)).subscribe(queryParams => {
			this.router.navigate(['search'], {
				relativeTo: this.route,
				queryParams
			});
		});

		this.router.events
			.pipe(
				takeUntil(this.unsubscribe$),
				filter((e): e is NavigationEnd => e instanceof NavigationEnd)
			)
			.subscribe(() => this.updateSearchView());
		this.updateSearchView();
	}

	private updateSearchView() {
		this.isSearchView = this.route.firstChild?.routeConfig?.path === 'search';
	}

	ngOnDestroy() {
		if (this.filterVidible) {
			this.offCanvas.open = false;
		}

		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}
}
