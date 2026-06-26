import {inject, Injectable} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {NavigationEnd, Router} from '@angular/router';
import {BehaviorSubject, distinctUntilChanged, filter, map, Observable} from 'rxjs';
import {NavNode, NodeType} from './navigation-stack.model';
import {catalogEntry, homeEntry, languageOf} from './navigation-stack.constants';
import {classifyUrl, reduceStack} from './navigation-stack.reducer';

const STORAGE_KEY = 'i14y.public.navigation-stack';

/** Guards against corrupted/legacy sessionStorage by keeping only well-formed entries. */
function isNavNode(value: unknown): value is NavNode {
	const node = value as Partial<NavNode> | null;
	return !!node && typeof node.key === 'string' && typeof node.type === 'string' && typeof node.url === 'string';
}

/*Provided in root and instantiated at app boot so it starts tracking from the first navigation.*/
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
		this.router.navigateByUrl(previous ? previous.url : catalogEntry(languageOf(this.router.url)).url);
	}

	/** The entry directly beneath the current page — where Back goes (undefined if there is none). */
	private previousEntry(stack: NavNode[]): NavNode | undefined {
		return stack[stack.length - 2];
	}

	private track(url: string): void {
		const node = classifyUrl(url);
		if (node) {
			this.commit(reduceStack(this.ensureBase(this.stack$.value, node), node));
		}
	}

	/** Synthesizes a language-aware base trail when navigating in without prior in-app history. */
	private ensureBase(stack: NavNode[], node: NavNode): NavNode[] {
		if (stack.length) {
			return stack;
		}
		if (node.type === NodeType.Home) {
			return [];
		}
		const lang = languageOf(node.url);
		return node.type === NodeType.Catalog ? [homeEntry(lang)] : [homeEntry(lang), catalogEntry(lang)];
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
			const parsed: unknown = raw ? JSON.parse(raw) : null;
			return Array.isArray(parsed) ? parsed.filter(isNavNode) : [];
		} catch {
			return [];
		}
	}
}
