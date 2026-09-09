import {MappingRelationModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class MappingRelationDialogData {
	dto: MappingRelationModel;
	titleKey: string;

	constructor(_dto: MappingRelationModel, _titleKey: string) {
		this.dto = _dto;
		this.titleKey = _titleKey;
	}
}
