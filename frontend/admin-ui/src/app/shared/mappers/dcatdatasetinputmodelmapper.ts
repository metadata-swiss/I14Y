import {CodeInputModel, DcatDatasetInputModel, DcatDatasetModel, EmailInputModel, IdentifierInputModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {DcatDistributionInputModelMapper} from './dcatdistributioninputmodelmapper';
import {DcatQualifiedAttributionInputModelMapper} from './dcatqualifiedattributioninputmodelmapper';
import {DcatQualifiedRelationInputModelMapper} from './dcatqualifiedrelationinputmodelmapper';

export class DcatDatasetInputModelMapper {
	public static mapToInputModel(dto: DcatDatasetModel): DcatDatasetInputModel {
		return new DcatDatasetInputModel({
			accessRights: dto.accessRights?.code ? new CodeInputModel({code: dto.accessRights.code}) : undefined,
			confidentialityPerson: dto.confidentialityPerson?.code ? new CodeInputModel({code: dto.confidentialityPerson.code}) : undefined,
			conformsTo: dto.conformsTo,
			contactPoints: dto.contactPoints,
			dataOwner: dto.dataOwner,
			description: dto.description,
			distributions: dto.distributions?.map(x => DcatDistributionInputModelMapper.mapToInputModel(x)) ?? undefined,
			documentation: dto.documentation,
			frequency: dto.frequency?.code ? new CodeInputModel({code: dto.frequency.code}) : undefined,
			geoIvIds: dto.geoIvIds ? dto.geoIvIds.map(x => new CodeInputModel({code: x.code})) : undefined,
			identifiers: dto.identifiers,
			images: dto.images,
			isReferencedBy: dto.isReferencedBy,
			issued: dto.issued,
			keywords: dto.keywords,
			landingPages: dto.landingPages,
			languages: dto.languages ? dto.languages.map(x => new CodeInputModel({code: x.code})) : undefined,
			modified: dto.modified,
			previousVersion: dto.previousVersion,
			processId: dto.processId,
			publisher: dto.publisher?.identifier ? new IdentifierInputModel({identifier: dto.publisher.identifier}) : undefined,
			qualifiedAttributions: dto.qualifiedAttributions?.map(x => DcatQualifiedAttributionInputModelMapper.mapToInputModel(x)) ?? undefined,
			qualifiedAttributionComplement: dto.qualifiedAttributionComplement,
			qualifiedRelations: dto.qualifiedRelations?.map(x => DcatQualifiedRelationInputModelMapper.mapToInputModel(x)) ?? undefined,
			relations: dto.relations,
			responsibleDeputy: dto.responsibleDeputy?.email ? new EmailInputModel({email: dto.responsibleDeputy.email}) : undefined,
			responsiblePerson: dto.responsiblePerson?.email ? new EmailInputModel({email: dto.responsiblePerson?.email}) : undefined,
			retentionPeriod: dto.retentionPeriod,
			retentionPeriodComplement: dto.retentionPeriodComplement,
			spatial: dto.spatial,
			temporalCoverage: dto.temporalCoverage,
			themes: dto.themes ? dto.themes.map(x => new CodeInputModel({code: x.code})) : undefined,
			title: dto.title,
			version: dto.version,
			versionNotes: dto.versionNotes
		});
	}
}
