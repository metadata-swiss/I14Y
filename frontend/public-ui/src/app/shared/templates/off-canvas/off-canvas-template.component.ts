import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {OffCanvasService} from '../../services/off-canvas/off-canvas.service';
import {ActivatedRoute, NavigationEnd, Router} from '@angular/router';
import {filter, map, Subject, takeUntil} from 'rxjs';
import {FilterContainer} from '../../content-filter/filterContainer';
import {FilterSet} from '../../content-filter/filterSet';
import {FilterValue} from '../../content-filter/filterValue';
import {FilterValueContainer} from '../../content-filter/filterValueContainer';

@Component({
	selector: 'app-off-canvas-template',
	templateUrl: './off-canvas-template.component.html',
	styleUrls: ['./off-canvas-template.component.scss'],
	standalone: false
})
export class OffCanvasTemplateComponent implements OnInit, OnDestroy {
	filterItems: FilterContainer[];
	selectedLanguage: string | null = null;
	languages: string[] = ['de', 'fr', 'it', 'en', 'rm'];
	hasQuery: boolean = false;
	hasSeletectFilter: boolean = false;
	relativeRoute: ActivatedRoute;

	private readonly unsubscribe$ = new Subject();

	private readonly offCanvasService = inject(OffCanvasService);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);

	ngOnInit() {
		this.router.events
			.pipe(
				takeUntil(this.unsubscribe$),
				filter((e): e is NavigationEnd => e instanceof NavigationEnd),
				map(e => e.urlAfterRedirects ?? e.url)
			)
			.subscribe(_ => {
				this.matchFilterSelectionToQueryParams();
			});

		this.offCanvasService.languages$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.languages = x));

		this.offCanvasService.filterConfiguration$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			const queryParams = this.route.snapshot.queryParamMap;

			this.filterItems = x.filters?.map(f =>
				this.mapFilter(
					new FilterSet(
						f.description,
						f.name,
						f.filterIdentifier,
						f.values.map(v => new FilterValue(v.name, v.filterValueIdentifier))
					),
					queryParams.getAll(f.filterIdentifier)
				)
			);
			this.matchFilterSelectionToQueryParams();
		});

		this.offCanvasService.route$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.relativeRoute = x));

		this.offCanvasService.resetFilterSelection$.pipe(takeUntil(this.unsubscribe$)).subscribe(() => this.resetFilterSelection());

		this.matchFilterSelectionToQueryParams();
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	onLanguageChange() {
		this.navigateToFilterState();
	}

	onFilterSelectionChanged() {
		this.navigateToFilterState();
	}

	private mapFilter(filterSet: FilterSet, selectedValues: string[]): FilterContainer {
		return new FilterContainer(
			filterSet.description,
			filterSet.getDescriptionKey(),
			filterSet.name,
			filterSet.getNameKey(),
			filterSet.param,
			filterSet.values.map(v => new FilterValueContainer(v.name, v.getNameKeyForApiTab(), v.value, selectedValues.includes(v.value)))
		);
	}

	private resetFilterSelection(): void {
		this.filterItems.forEach(x => {
			x.values.forEach(v => (v.selected = false));
		});
	}

	private navigateToFilterState() {
		const queryParams = this.getFilterStateAsQueryParams();

		if (this.hasQuery || this.hasSeletectFilter) {
			this.offCanvasService.navigateToSearch(queryParams);
		} else {
			this.offCanvasService.navigateToList(queryParams);
		}
	}

	private getFilterStateAsQueryParams(): {[key: string]: string[]} {
		const queryParams: {[key: string]: string[]} = {};

		this.hasSeletectFilter = false;
		this.filterItems.forEach(f => {
			const values = f.values.filter(v => v.selected).map(v => v.value);
			if (values.length > 0) {
				queryParams[f.param] = values;
				this.hasSeletectFilter = true;
			}
		});

		if (this.selectedLanguage) {
			queryParams.lang = [this.selectedLanguage];
		}

		const qpMap = this.route.snapshot.queryParamMap;
		this.hasQuery = false;
		if (qpMap.has('query')) {
			queryParams.query = [qpMap.get('query')];
			this.hasQuery = true;
		}

		return queryParams;
	}

	private matchFilterSelectionToQueryParams() {
		const queryParams = this.route.snapshot.queryParamMap;

		const language = queryParams.get('lang');
		if (this.languages) {
			const isKnownLanguage = language !== null && this.languages.includes(language);
			this.selectedLanguage = isKnownLanguage ? language : null;
			if (language !== null && !isKnownLanguage) {
				this.navigateToFilterState();
			}
		}
	}
}
