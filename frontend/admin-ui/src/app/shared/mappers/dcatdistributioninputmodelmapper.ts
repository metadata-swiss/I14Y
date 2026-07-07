import {ChecksumInputModel, CodeInputModel, DcatDistributionInputModel, DcatDistributionModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class DcatDistributionInputModelMapper {
	public static mapToInputModel(dto: DcatDistributionModel): DcatDistributionInputModel {
		return new DcatDistributionInputModel({
			accessServices: dto.accessServices,
			accessUrl: dto.accessUrl,
			availability: dto.availability?.code ? new CodeInputModel({code: dto.availability.code}) : undefined,
			byteSize: dto.byteSize,
			checksum: dto.checksum
				? new ChecksumInputModel({
						algorithm: dto.checksum.algorithm,
						checksumValue: dto.checksum.checksumValue
				  })
				: undefined,
			conformsTo: dto.conformsTo,
			coverage: dto.coverage,
			description: dto.description,
			documentation: dto.documentation,
			downloadUrl: dto.downloadUrl,
			format: dto.format?.code ? new CodeInputModel({code: dto.format.code}) : undefined,
			id: dto.id,
			identifier: dto.identifier,
			images: dto.images,
			issued: dto.issued,
			languages: dto.languages ? dto.languages.map(l => new CodeInputModel({code: l.code})) : undefined,
			license: dto.license?.code ? new CodeInputModel({code: dto.license.code}) : undefined,
			mediaType: dto.mediaType?.code ? new CodeInputModel({code: dto.mediaType.code}) : undefined,
			modified: dto.modified,
			packagingFormat: dto.packagingFormat?.code ? new CodeInputModel({code: dto.packagingFormat.code}) : undefined,
			rights: dto.rights,
			spatialResolution: dto.spatialResolution,
			temporalResolution: dto.temporalResolution,
			title: dto.title
		});
	}
}
