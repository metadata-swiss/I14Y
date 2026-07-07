import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {ChannelModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {PublicServiceService} from '../services/publicservice.service';

@Component({
	selector: 'app-channels',
	templateUrl: './channels.component.html',
	styleUrls: [],
	standalone: false
})
export class ChannelsComponent implements OnInit, OnDestroy {
	channels: ChannelModel[] = [];
	hasActiveChildRoute = false;

	private readonly unsubscribe$ = new Subject();

	private readonly publicServiceService = inject(PublicServiceService);

	ngOnInit() {
		this.publicServiceService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.channels = x.channels ?? []));
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}
}
