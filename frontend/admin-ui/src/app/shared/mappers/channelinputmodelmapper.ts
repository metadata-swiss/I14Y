import {ChannelInputModel, ChannelModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class ChannelInputModelMapper {
	public static mapToInputModel(dto: ChannelModel): ChannelInputModel {
		return new ChannelInputModel({
			address: dto.address,
			description: dto.description,
			email: dto.email,
			fax: dto.fax,
			id: dto.id || undefined,
			identifier: dto.identifier,
			mobile: dto.mobile,
			openingHours: dto.openingHours,
			ownedBy: dto.ownedBy,
			phone: dto.phone,
			type: dto.type,
			url: dto.url
		});
	}
}
