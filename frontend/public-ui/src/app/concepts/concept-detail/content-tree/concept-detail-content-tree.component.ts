import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {PageEvent} from '@angular/material/paginator';
import {ActivatedRoute, NavigationEnd, Router} from '@angular/router';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {filter, map, Observable, Subject, takeUntil} from 'rxjs';
import {ObNavTreeItemModelPlus} from 'src/app/datasets/content/ObNavTreeItemModelPlus';
import {SearchResultPagingInfo} from 'src/app/shared/search/SearchResultPagingInfo';
import {ConceptService} from '../../services/concept.service';

@Component({
	selector: 'app-concept-detail-content-tree',
	templateUrl: './concept-detail-content-tree.component.html',
	styleUrls: [],
	standalone: false
})
export class ConceptDetailContentTreeComponent implements OnInit, OnDestroy {
	items: ObNavTreeItemModelPlus[] = [] as ObNavTreeItemModelPlus[];
	selectedItem: Observable<ObNavTreeItemModelPlus>;

	pagingInfo: SearchResultPagingInfo = new SearchResultPagingInfo(null);

	currentLanguage: string;

	private readonly unsubscribe$ = new Subject();
	private readonly selectedItem$ = new Subject<ObNavTreeItemModelPlus>();
	private readonly defaultPage: number = 1;
	private readonly defaultPageSize: number = 50;

	private lastUrl = '';
	private readonly nodeIdSeparator: string = '-';

	private readonly conceptService = inject(ConceptService);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.selectedItem = this.selectedItem$.asObservable();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
		this.router.events
			.pipe(
				takeUntil(this.unsubscribe$),
				filter((e): e is NavigationEnd => e instanceof NavigationEnd),
				map(e => e.urlAfterRedirects ?? e.url)
			)
			.subscribe(url => {
				this.loadContentByUrl(url);
			});
		this.selectedItem$.pipe(takeUntil(this.unsubscribe$)).subscribe(_ => this.scrollToSelectedNode());
		this.conceptService.navTreeWithPagingInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.pagingInfo = x.pagingInfo;
			this.items = x.codeListEntries;
			this.selectedItem$.next(x.selectedItem);
		});
		this.loadContentByUrl(this.router.url);
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	onChangePage(pageEvent: PageEvent) {
		const queryParams = {
			page: pageEvent.pageIndex + 1 === this.defaultPage ? null : pageEvent.pageIndex + 1,
			pageSize: pageEvent.pageSize === this.defaultPageSize ? null : pageEvent.pageSize
		};

		let routerCommands = [];
		if (this.route.snapshot.children[0].url.length > 0 && pageEvent.pageIndex !== pageEvent.previousPageIndex) {
			routerCommands = ['./'];
		}

		this.router.navigate(routerCommands, {
			relativeTo: this.route,
			queryParams: queryParams,
			queryParamsHandling: 'merge'
		});
	}

	private getNodePath(): string[] {
		return this.route.snapshot.children[0].url.map(x => x.path.replace(/%25F2/g, '/'));
	}

	private buildNodeId(path: string[], level: number) {
		return path.slice(0, level + 1).join(this.nodeIdSeparator);
	}

	private loadContentByUrl(url: string) {
		if (url !== this.lastUrl) {
			this.lastUrl = url;
			const path = this.getNodePath();

			const queryParams = this.route.snapshot.queryParamMap;
			const lang = queryParams.get('lang') ?? this.translate.getCurrentLang();
			const page = queryParams.has('page') ? Number(queryParams.get('page')) : this.defaultPage;
			const pageSize = queryParams.has('pageSize') ? Number(queryParams.get('pageSize')) : this.defaultPageSize;

			const conceptId = this.route.parent.parent.snapshot.params.conceptId;
			this.conceptService.generateNavTree(conceptId, lang, page, pageSize, path);
		}
	}

	/** Delay scrolling with setetimeout if node is not yet available in the dom */
	private scrollToSelectedNode() {
		const path = this.getNodePath();
		const nodeId = this.buildNodeId(path, path.length - 1);
		let ctr = 0;
		const scrollUntilSuccess = () => {
			if (!this.scrollContentIntoView(nodeId) && ctr++ < 20) {
				setTimeout(scrollUntilSuccess, 10);
			}
		};
		scrollUntilSuccess();
	}

	/** Scroll the tree node with the given nodeId into view if necessary */
	private scrollContentIntoView(nodeId: string): boolean {
		const node = document.getElementById(`nav-tree-${nodeId}`);
		const area = document.getElementById('tree-area');
		if (node && area) {
			const pos = node.offsetTop - area.offsetTop - area.offsetHeight / 2;
			if (pos > 0 && (area.scrollTop < pos - area.offsetHeight / 2 || area.scrollTop > pos + area.offsetHeight / 2)) {
				area.scrollTo({top: pos, behavior: 'smooth'});
			}
			return true;
		}
		return false;
	}
}
