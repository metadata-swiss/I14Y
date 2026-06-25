import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {ViewType} from 'src/app/shared/templates/viewtype';
import {DataServiceService} from '../services/dataservice.service';
import {PublisherContextService} from 'src/app/shared/services/publisher-context/publisher-context.service';
import {Subject, takeUntil} from 'rxjs';
import {DataService} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Component({
	selector: 'app-data-service-description',
	templateUrl: './data-service-description.component.html',
	standalone: false
})
export class DataServiceDescriptionComponent implements OnInit, OnDestroy {
	dataService: DataService;
	publisherIdentifier: string | undefined;
	readonly emptyPlaceHolder: string = '-';
	readonly viewTypeEnum = ViewType;

	private readonly unsubscribe$ = new Subject();

	private readonly dcatDataServiceService = inject(DataServiceService);
	private readonly publisherContextService = inject(PublisherContextService);

	ngOnInit() {
		this.publisherContextService.identifier$.pipe(takeUntil(this.unsubscribe$)).subscribe(id => {
			this.publisherIdentifier = id;
		});
		this.dcatDataServiceService.dataService$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.dataService = x;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(null);
		this.unsubscribe$.complete();
	}
}
