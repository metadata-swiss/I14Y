import {CodeListEntryDetail} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class CodelistEntryDialogData {
	contentLanguages: readonly string[];
	conceptId: string | undefined;
	dto: CodeListEntryDetail;
	titleKey: string;

	constructor(_contentLanguages: readonly string[], _conceptId: string | undefined, _dto: CodeListEntryDetail, _titleKey: string) {
		this.contentLanguages = _contentLanguages;
		this.conceptId = _conceptId;
		this.dto = _dto;
		this.titleKey = _titleKey;
	}
}
