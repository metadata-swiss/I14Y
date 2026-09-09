import {Component, OnDestroy, OnInit} from '@angular/core';
import {SortableListViewComponent} from 'src/app/shared/sortable-list-view/sortable-list-view.component';
import {DcatDistributionModel, ResourceModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';

@Component({
	selector: 'app-distributions-overview',
	templateUrl: './distributions-overview.component.html',
	styleUrls: ['./distributions-overview.component.scss'],
	standalone: false
})
export class DistributionsOverviewComponent extends SortableListViewComponent<DcatDistributionModel, SortableKeys> implements OnInit, OnDestroy {
	currentLang: string;
	readonly emptyPlaceHolder: string = '-';

	columnTitle = 'title';
	columnFormat = 'format';
	columnActions = 'actions';

	displayedColumns: string[] = [this.columnTitle, this.columnFormat, this.columnActions];

	private readonly unsubscribe$ = new Subject();

	constructor() {
		super();

		this.currentLang = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLang = language.lang;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(null);
		this.unsubscribe$.complete();
	}

	hasUrl(resources: ResourceModel[]): boolean {
		if (resources?.length > 0) {
			const url = resources[0];
			return url?.uri && url.uri.length > 0;
		}
		return false;
	}

	getUrl(resources: ResourceModel[]): string {
		if (resources?.length > 0) {
			const url = resources[0];
			if (url?.uri && url.uri.length > 0) {
				return url.uri;
			}
		}
		return null;
	}

	download(element: DcatDistributionModel) {
		window.open(this.getUrl([element.downloadUrl]));
	}
}

type SortableKeys = 'format' | 'title';
