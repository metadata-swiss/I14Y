import {DcatPublicServiceService} from './../services/dcat-publicservice.service';
import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {Observable, Subject} from 'rxjs';
import {map} from 'rxjs/operators';
import {ChannelModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Component({
	selector: 'app-channels',
	templateUrl: './channels.component.html',
	styleUrls: [],
	standalone: false
})
export class ChannelsComponent implements OnInit, OnDestroy {
	channels$: Observable<ChannelModel[]>;
	hasActiveChildRoute = false;

	private readonly unsubscribe$ = new Subject();

	private readonly dcatPublicServiceService = inject(DcatPublicServiceService);

	ngOnInit() {
		this.channels$ = this.dcatPublicServiceService.publicService$.pipe(map(x => x.channels));
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}
}
