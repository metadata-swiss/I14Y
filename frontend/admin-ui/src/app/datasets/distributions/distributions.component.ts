import {ActivatedRoute} from '@angular/router';
import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {map, startWith, takeUntil} from 'rxjs/operators';
import {NAV_VALUE_CREATE} from 'src/app/app-constants';
import {Observable, of, Subject} from 'rxjs';
import {AllowActionService} from 'src/app/services/allow.action.service';
import {AllowActionResourceType, AllowActionType} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Component({
	selector: 'app-distributions',
	templateUrl: './distributions.component.html',
	standalone: false
})
export class DistributionsComponent implements OnInit, OnDestroy {
	datasetId: string;
	readonly nav_value_create: string = NAV_VALUE_CREATE;
	cannotEdit$: Observable<boolean> = of(true);

	private readonly unsubscribe$ = new Subject();

	private readonly allowActionService = inject(AllowActionService);
	private readonly route = inject(ActivatedRoute);

	constructor() {
		this.datasetId = this.route.parent?.snapshot.params.id;
	}

	ngOnInit() {
		this.allowActionService.load(this.datasetId, AllowActionResourceType.Dataset);
		this.cannotEdit$ = this.allowActionService.allowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.actionType === AllowActionType.Edit)?.value),
			startWith(true)
		);
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}
}
