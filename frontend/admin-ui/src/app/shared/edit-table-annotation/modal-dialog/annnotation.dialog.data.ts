import {Annotation} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class AnnotationDialogData {
	contentLanguages: readonly string[];
	dto: Annotation;
	titleKey: string;

	constructor(_contentLanguages: readonly string[], _dto: Annotation, _titleKey: string) {
		this.contentLanguages = _contentLanguages;
		this.dto = _dto;
		this.titleKey = _titleKey;
	}
}
