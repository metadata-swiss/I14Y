import {inject, Injectable} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {NavigationEnd, Router} from '@angular/router';
import {BehaviorSubject, distinctUntilChanged, filter, map, Observable} from 'rxjs';
import {NavNode, NodeType} from './navigation-stack.model';
import {CATALOG_URL, HOME_URL} from './navigation-stack.constants';
import {classifyUrl, reduceStack} from './navigation-stack.reducer';

const STORAGE_KEY = 'i14y.navigation-stack';
const HOME_ENTRY: NavNode = {key: NodeType.Home, type: NodeType.Home, url: HOME_URL};
const CATALOG_ENTRY: NavNode = {key: NodeType.Catalog, type: NodeType.Catalog, url: CATALOG_URL};

@Injectable({providedIn: 'root'})
export class NavigationStackService {
	readonly canGoBack$: Observable<boolean>;

	private readonly router = inject(Router);
	private readonly stack$ = new BehaviorSubject<NavNode[]>(this.restore());

	constructor() {
		this.canGoBack$ = this.stack$.pipe(
			map(stack => this.previousEntry(stack) !== undefined),
			distinctUntilChanged()
		);
		this.router.events
			.pipe(
				takeUntilDestroyed(),
				filter((event): event is NavigationEnd => event instanceof NavigationEnd)
			)
			.subscribe(event => this.track(event.urlAfterRedirects));
	}

	/** Navigates to the previous stack entry; the stack collapses to it via the router event. */
	back(): void {
		const previous = this.previousEntry(this.stack$.value);
		this.router.navigateByUrl(previous ? previous.url : CATALOG_ENTRY.url);
	}

	/** The entry directly beneath the current page — where Back goes */
	private previousEntry(stack: NavNode[]): NavNode | undefined {
		return stack[stack.length - 2];
	}

	private track(url: string): void {
		const node = classifyUrl(url);
		if (node) {
			this.commit(reduceStack(this.ensureBase(this.stack$.value, node), node));
		}
	}

	/** Synthesizes a base trail when navigating in without prior in-app history (deep link). */
	private ensureBase(stack: NavNode[], node: NavNode): NavNode[] {
		if (stack.length) {
			return stack;
		}
		if (node.type === NodeType.Home) {
			return [];
		}
		return node.type === NodeType.Catalog ? [HOME_ENTRY] : [HOME_ENTRY, CATALOG_ENTRY];
	}

	private commit(stack: NavNode[]): void {
		this.stack$.next(stack);
		this.persist(stack);
	}

	private persist(stack: NavNode[]): void {
		try {
			sessionStorage.setItem(STORAGE_KEY, JSON.stringify(stack));
		} catch {
			// Ignore storage failures (private mode, quota, …) — the in-memory stack still works.
		}
	}

	private restore(): NavNode[] {
		try {
			const raw = sessionStorage.getItem(STORAGE_KEY);
			const parsed = raw ? JSON.parse(raw) : null;
			return Array.isArray(parsed) ? parsed : [];
		} catch {
			return [];
		}
	}
}
