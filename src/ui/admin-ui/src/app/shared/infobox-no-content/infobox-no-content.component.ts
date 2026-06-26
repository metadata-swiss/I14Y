import {Component, Input} from '@angular/core';

@Component({
	selector: 'app-infobox-no-content',
	templateUrl: './infobox-no-content.component.html',
	standalone: false
})
export class InfoboxNoContentComponent {
	@Input() datasource: Array<any>;

	constructor() {
		this.datasource = [];
	}

	hasNoContent(): boolean {
		return this.datasource.length > 0;
	}
}
