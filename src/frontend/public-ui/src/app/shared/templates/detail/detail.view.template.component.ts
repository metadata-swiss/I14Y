import {Component, EventEmitter, Input, Output} from '@angular/core';
import {ViewType} from '../viewtype';
import {DataFormat} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Component({
	selector: 'app-detail-view-template',
	templateUrl: './detail.view.template.component.html',
	styleUrls: ['./detail.view.template.component.scss'],
	standalone: false
})
export class DetailViewTemplateComponent {
	@Input() tabs: string[] = ['description'];
	@Input() dataFormat = DataFormat;
	@Input() tabIdPrefix: string = 'tab-';
	@Input() viewType: ViewType = ViewType.Unspecified;
	@Input() registrationStatus: string;
	@Output() exportFile: EventEmitter<DataFormat> = new EventEmitter<DataFormat>();

	export(formatSelected: DataFormat) {
		this.exportFile.emit(formatSelected);
	}

	isShowDownloadButton(): boolean {
		return this.viewType === ViewType.Dataset;
	}
}
