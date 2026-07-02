import {Component, Input} from '@angular/core';

@Component({
	selector: 'app-infobox-required',
	templateUrl: './infobox-required.component.html',
	standalone: false
})
export class InfoboxRequiredComponent {
	@Input() datasource: Array<any>;

	constructor() {
		this.datasource = [];
	}

	hasNoContent(): boolean {
		return this.datasource.length > 0;
	}
}
