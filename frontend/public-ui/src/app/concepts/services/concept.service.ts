import {inject, Injectable} from '@angular/core';
import {Observable} from 'rxjs/internal/Observable';
import {map, ReplaySubject, Subject} from 'rxjs';
import {ObNavTreeItemModelPlus} from 'src/app/datasets/content/ObNavTreeItemModelPlus';
import {SearchResultPagingInfo} from 'src/app/shared/search/SearchResultPagingInfo';
import {CodeListEntryDetail, ConceptViewClient, ConceptView} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ExtendedFallbackPipe} from 'src/app/shared/fallback/extendedfallback.pipe';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {ActivatedRoute} from '@angular/router';
import {FallBackModel} from 'src/app/shared/fallback/fallbackmodel';
import {FormatFunctions} from 'src/app/shared/format-functions';

@Injectable()
export class ConceptService {
	readonly conceptView$: Observable<ConceptView>;
	readonly navTreeWithPagingInfo$: Observable<NavTreeWithPagingInfo>;
	readonly searchExport$: Observable<SearchExportInfo | null>;

	private currentConceptView?: ConceptView;

	private cachedNavTreeWithPagingInfo: NavTreeWithPagingInfo;

	private readonly conceptView: Subject<ConceptView> = new ReplaySubject<ConceptView>(1);
	private readonly navTreeWithPagingInfo: Subject<NavTreeWithPagingInfo> = new ReplaySubject<NavTreeWithPagingInfo>(1);
	private readonly searchExport: Subject<SearchExportInfo | null> = new ReplaySubject<SearchExportInfo | null>(1);
	private readonly nodeIdSeparator: string = '-';

	private readonly conceptViewClient = inject(ConceptViewClient);
	private readonly extendedFallback = inject(ExtendedFallbackPipe);
	private readonly fallback = inject(FallbackPipe);
	private readonly route = inject(ActivatedRoute);

	constructor() {
		this.conceptView$ = this.conceptView.asObservable();
		this.navTreeWithPagingInfo$ = this.navTreeWithPagingInfo.asObservable();
		this.searchExport$ = this.searchExport.asObservable();
	}

	setConceptView(conceptView: ConceptView) {
		this.currentConceptView = conceptView;
		this.conceptView.next(conceptView);
	}

	setSearchExport(info: SearchExportInfo | null) {
		this.searchExport.next(info);
	}

	public generateNavTree(id: string, lang: string, page: number, pageSize: number | null | undefined, path: string[] | undefined): void {
		const root = path[0];

		if (root) {
			this.getPage(id, root, pageSize).then(p => {
				this.loadRoot(id, lang, p, pageSize, path, root);
			});
		} else {
			this.loadRoot(id, lang, page, pageSize, path ?? [], root);
		}
	}

	private getPage(id: string, codeValue: string, pageSize: number): Promise<number> {
		return new Promise<number>(resolve => {
			this.conceptViewClient
				.getCodelistEntriesPageNumberFromSameParentByIdAndCodeAndSortPropertyAndSortOrderAndPageSize(id, codeValue, undefined, undefined, pageSize)
				.subscribe(response => {
					resolve(response.result);
				});
		});
	}

	private loadRoot(id: string, lang: string, page: number, pageSize: number, path: string[], root: string) {
		this.getRoot(id, lang, page, pageSize, root).then(result => {
			this.cachedNavTreeWithPagingInfo = result;

			if (path.length > 0 && result?.codeListEntries?.find(r => r.code === root)?.items) {
				this.loadChildrend(
					id,
					lang,
					this.cachedNavTreeWithPagingInfo?.codeListEntries?.find(n => n.code === root),
					path
				);
			} else {
				this.navTreeWithPagingInfo.next(this.cachedNavTreeWithPagingInfo);
			}
		});
	}

	private getRoot(id: string, lang: string, page: number, pageSize: number, root: string): Promise<NavTreeWithPagingInfo> {
		return new Promise<NavTreeWithPagingInfo>(resolve =>
			this.conceptViewClient
				.getCodelistEntriesByRootByIdAndSortPropertyAndSortOrderAndPageAndPageSize(id, undefined, undefined, page, pageSize)
				.pipe(
					map(
						response =>
							new NavTreeWithPagingInfo(
								response.result.map(r => this.convert(r, lang)),
								new SearchResultPagingInfo(response.headers, [10, 25, 50, 100, 200]),
								root
									? this.convert(
											response.result.find(r => r.value === root),
											lang
										)
									: undefined
							)
					)
				)
				.subscribe(result => {
					resolve(result);
				})
		);
	}

	private loadChildrend(id: string, lang: string, parent: ObNavTreeItemModelPlus, path: string[]) {
		let current = path.shift();

		this.getChildren(id, current).then(result => {
			parent.items = result.map(r => this.convert(r, lang, parent));
			if (path[0]) {
				this.cachedNavTreeWithPagingInfo.selectedItem = parent.items.find(n => n.path === path[0]);
			}
			parent.collapsed = false;

			if (path.length > 0 && result.find(r => r.value === path[0]).hasChildren) {
				this.loadChildrend(
					id,
					lang,
					parent.items.find(n => n.path === path[0]),
					path
				);
			} else {
				this.navTreeWithPagingInfo.next(this.cachedNavTreeWithPagingInfo);
			}
		});
	}

	private getChildren(id: string, codeValue: string): Promise<CodeListEntryDetail[]> {
		return new Promise<CodeListEntryDetail[]>(resolve =>
			this.conceptViewClient
				.getCodelistEntriesChildrenOfByIdAndCodeAndSortPropertyAndSortOrderAndPageAndPageSize(id, codeValue, undefined, undefined, undefined, undefined)
				.subscribe(resonse => {
					resolve(resonse.result);
				})
		);
	}

	private convert(node: CodeListEntryDetail, lang: string, parent?: ObNavTreeItemModelPlus): ObNavTreeItemModelPlus {
		let name = this.extendedFallback.transform(node.name, lang);

		const escapedValue = FormatFunctions.escapeHtml(node.value);

		return new ObNavTreeItemModelPlus(
			{
				id: parent == null ? node.value : parent.id + this.nodeIdSeparator + node.value,
				label: `<strong>${escapedValue}</strong>${this.getLabelTextPart(name, lang)}`,
				collapsed: true,
				path: node.value?.replace(/\//g, '%F2'),
				queryParams: this.route.snapshot.queryParams,
				items: node.hasChildren ? ([] as ObNavTreeItemModelPlus[]) : null
			},
			parent,
			node.annotations,
			node.value,
			this.fallback.transform(node.name, lang),
			this.fallback.transform(node.description, lang),
			node.validFrom,
			node.validTo,
			this.currentConceptView?.identifiers?.[0],
			this.currentConceptView?.version
		);
	}

	private getLabelTextPart(name: FallBackModel | undefined, lang: string): string {
		if (name) {
			const escapedText = FormatFunctions.escapeHtml(name.text);

			return ` | <span ${name.cultureCode !== lang ? 'lang="' + name.cultureCode + '"' : ''}>${escapedText}</span>`;
		}
		return '';
	}
}

export interface SearchExportInfo {
	jsonUrl?: string;
	csvUrl?: string;
	totalRows: number;
}

export class NavTreeWithPagingInfo {
	codeListEntries: ObNavTreeItemModelPlus[];
	pagingInfo: SearchResultPagingInfo;
	selectedItem: ObNavTreeItemModelPlus;

	constructor(codeListEntries: ObNavTreeItemModelPlus[], pagingInfo: SearchResultPagingInfo, selectedItem: ObNavTreeItemModelPlus) {
		this.codeListEntries = codeListEntries;
		this.pagingInfo = pagingInfo;
		this.selectedItem = selectedItem;
	}
}
