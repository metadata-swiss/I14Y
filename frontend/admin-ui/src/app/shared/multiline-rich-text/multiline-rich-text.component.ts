import {Component, Input} from '@angular/core';

@Component({
	selector: 'app-multiline-rich-text',
	templateUrl: './multiline-rich-text.component.html',
	standalone: false
})
export class MultilineRichTextComponent {
	@Input() text = '';

	get lines(): string[] {
		let ret: string[] = [];
		if (this.text && this.text.length > 0) {
			ret = this.text.replace(/\r/g, '').split('\n');
		}
		return ret;
	}
}
