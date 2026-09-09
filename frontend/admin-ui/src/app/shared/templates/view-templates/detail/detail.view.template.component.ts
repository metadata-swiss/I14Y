import {Component, Input} from '@angular/core';
import {ViewType} from '../../viewtype';

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
}
