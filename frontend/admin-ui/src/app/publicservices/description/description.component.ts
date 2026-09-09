import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {AllowActionResourceType, Dataset, PublicServiceModel, PublicServiceView} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Subject, takeUntil} from 'rxjs';
import {NAV_VALUE_EDIT} from 'src/app/app-constants';
import {PublicServiceService} from '../services/publicservice.service';
import {AllowActionService} from 'src/app/services/allow.action.service';
import {ViewType} from 'src/app/shared/templates/viewtype';

@Component({
	selector: 'app-description',
	templateUrl: './description.component.html',
	standalone: false
})
export class DescriptionComponent implements OnInit, OnDestroy {
	publicService: PublicServiceModel = new PublicServiceModel();
	isDescribedAt: Dataset[] | undefined;
	relations: PublicServiceView[] | undefined;
	requires: PublicServiceView[] | undefined;
	publicServiceId: string | undefined;

	readonly nav_value_edit: string = NAV_VALUE_EDIT;
	readonly viewTypeEnum = ViewType;

	private readonly unsubscribe$ = new Subject();

	private readonly allowActionService = inject(AllowActionService);
	private readonly publicServiceService = inject(PublicServiceService);
	private readonly route = inject(ActivatedRoute);

	ngOnInit() {
		this.route.parent?.params.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
			this.publicServiceId = params.id;
			this.allowActionService.load(params.id, AllowActionResourceType.PublicService);
			this.publicServiceService.load(params.id, true);
			this.publicServiceService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe((x: any) => {
				this.publicService = x;
			});
			this.publicServiceService.isDescribedAt$.pipe(takeUntil(this.unsubscribe$)).subscribe((x: any) => {
				this.isDescribedAt = x;
			});
			this.publicServiceService.relations$.pipe(takeUntil(this.unsubscribe$)).subscribe((x: any) => {
				this.relations = x;
			});
			this.publicServiceService.requires$.pipe(takeUntil(this.unsubscribe$)).subscribe((x: any) => {
				this.requires = x;
			});
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}
}
