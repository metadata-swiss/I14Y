import {Component, Input} from '@angular/core';

@Component({
	selector: 'app-multiline-text',
	templateUrl: './multiline-text.component.html',
	standalone: false
})
export class MultilineTextComponent {
	@Input() text: string | undefined;

	getLines(): string[] {
		let ret: string[] = [];
		if (this.text && this.text.length > 0) {
			ret = this.text.replace(/\r/g, '').split('\n');
		}
		return ret;
	}
}
