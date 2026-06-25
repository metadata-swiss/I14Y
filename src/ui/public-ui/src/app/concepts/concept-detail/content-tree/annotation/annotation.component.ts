import {Component, Input} from '@angular/core';
import {Annotation} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Component({
	selector: 'app-annotation',
	templateUrl: './annotation.component.html',
	styleUrls: ['./annotation.component.scss'],
	standalone: false
})
export class AnnotationComponent {
	@Input() annotation: Annotation | undefined;
}
