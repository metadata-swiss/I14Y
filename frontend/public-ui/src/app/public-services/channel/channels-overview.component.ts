import {Component, Input} from '@angular/core';
import {ChannelModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Component({
	selector: 'app-channels-overview',
	templateUrl: './channels-overview.component.html',
	styleUrls: ['./channels-overview.component.scss'],
	standalone: false
})
export class ChannelsOverviewComponent {
	languages: string[];
	channels: ChannelModel[];
	readonly emptyPlaceHolder: string = '-';

	@Input()
	set data(data: ChannelModel[]) {
		if (data) {
			this.channels = data;
		}
	}
}
