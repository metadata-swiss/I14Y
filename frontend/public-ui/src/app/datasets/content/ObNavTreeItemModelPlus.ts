import {Annotation} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ObNavTreeItemModel} from '@oblique/oblique';

export class ObNavTreeItemModelPlus extends ObNavTreeItemModel {
	annotations?: Annotation[];
	code?: string;
	description?: string;
	title?: string;
	validFrom?: Date;
	validTo?: Date;
	conceptIdentifier?: string;
	conceptVersion?: string;

	constructor(
		json: any,
		parent?: ObNavTreeItemModel,
		annotations?: Annotation[],
		code?: string,
		title?: string,
		description?: string,
		validFrom?: Date,
		validTo?: Date,
		conceptIdentifier?: string,
		conceptVersion?: string
	) {
		super(json, parent);
		this.annotations = annotations;
		this.code = code;
		this.title = title;
		this.description = description;
		this.validFrom = validFrom;
		this.validTo = validTo;
		this.conceptIdentifier = conceptIdentifier;
		this.conceptVersion = conceptVersion;
	}
}
