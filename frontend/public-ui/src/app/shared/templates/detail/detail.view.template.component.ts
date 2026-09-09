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
	@Input() tabIdPrefix: string = 'tab-';
	@Input() viewType: ViewType = ViewType.Unspecified;
	@Input() registrationStatus: string;
	@Output() exportFile: EventEmitter<DataFormat> = new EventEmitter<DataFormat>();
	
	dataFormat = DataFormat;
	
	export(formatSelected: DataFormat) {
		this.exportFile.emit(formatSelected);
	}
}
