import {Component, Input} from '@angular/core';

@Component({
	selector: 'app-rich-text-panel',
	templateUrl: './rich-text-panel.component.html',
	standalone: false
})
export class RichTextPanelComponent {
	@Input() text = '';
}
