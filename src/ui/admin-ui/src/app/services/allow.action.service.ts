import {inject, Injectable} from '@angular/core';
import {AllowActionResourceType, AllowActionResult, AllowActionsClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, ReplaySubject, Subject} from 'rxjs';

@Injectable({
	providedIn: 'root'
})
export class AllowActionService {
	readonly globalAllowActions$: Observable<AllowActionResult[]>;
	readonly allowActions$: Observable<AllowActionResult[]>;
	readonly childAllowActions$: Observable<AllowActionResult[]>;
	private last: string | undefined;
	private lastChild: string | undefined;

	private readonly globalAllowActions: Subject<AllowActionResult[]> = new ReplaySubject<AllowActionResult[]>();
	private readonly allowAction: Subject<AllowActionResult[]> = new ReplaySubject<AllowActionResult[]>();
	private readonly childAallowAction: Subject<AllowActionResult[]> = new ReplaySubject<AllowActionResult[]>();

	private readonly client = inject(AllowActionsClient);

	constructor() {
		this.globalAllowActions$ = this.globalAllowActions.asObservable();
		this.client.getAllowCreate().subscribe(respose => {
			this.globalAllowActions.next(respose.result);
		});
		this.allowActions$ = this.allowAction.asObservable();
		this.allowAction.next([]);
		this.childAllowActions$ = this.childAallowAction.asObservable();
		this.childAallowAction.next([]);
	}

	load(id: string, type: AllowActionResourceType, force: boolean = false): void {
		if (force || !this.last || this.last !== id) {
			this.client.getV2ByResourceTypeAndId(type, id).subscribe(response => {
				this.allowAction.next(response.result);
				this.last = id;
			});
		}
	}

	loadChild(id: string, type: AllowActionResourceType, force: boolean = false): void {
		if (force || !this.lastChild || this.lastChild !== id) {
			this.client.getV2ByResourceTypeAndId(type, id).subscribe(response => {
				this.childAallowAction.next(response.result);
				this.lastChild = id;
			});
		}
	}
}
