import {Component, Input} from '@angular/core';

@Component({
	selector: 'app-detail-table-row',
	templateUrl: './detail-table-row.component.html',
	styleUrls: ['./detail-table-row.component.scss'],
	standalone: false
})
export class DetailTableRowComponent {
	@Input() label: string;
	@Input() id: string;
}
