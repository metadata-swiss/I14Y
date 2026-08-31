import {DataServiceInput, DataServiceModel, EmailInputModel, IdentifierInputModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ResourceMapper} from './resourcemapper';
import {VcardMapper} from './vcardmapper';

export class DataServiceInputMapper {
	public static mapToInputModel(dto: DataServiceModel): DataServiceInput {
		return new DataServiceInput({
			accessRightCode: dto.accessRights?.code || undefined,
			conformTos: dto.conformsTo ? ResourceMapper.mapElements(dto.conformsTo) : undefined,
			contactPoints: dto.contactPoints ? VcardMapper.mapElements(dto.contactPoints) : undefined,
			description: dto.description,
			documents: dto.documentation ? ResourceMapper.mapElements(dto.documentation) : undefined,
			endpointDescriptions: dto.endpointDescriptions ? ResourceMapper.mapElements(dto.endpointDescriptions) : undefined,
			endpointUrls: dto.endpointUrls ? ResourceMapper.mapElements(dto.endpointUrls) : undefined,
			id: dto.id || undefined,
			issued: dto.issued,
			keywords: dto.keywords,
			landingPages: dto.landingPages ? ResourceMapper.mapElements(dto.landingPages) : undefined,
			license: dto.license,
			modified: dto.modified,
			previousVersion: dto.previousVersion,
			publisher: dto.publisher?.identifier ? new IdentifierInputModel({identifier: dto.publisher.identifier}) : undefined,
			responsibleDeputy: dto.responsibleDeputy ? new EmailInputModel({email: dto.responsibleDeputy.email}) : undefined,
			responsiblePerson: dto.responsiblePerson ? new EmailInputModel({email: dto.responsiblePerson.email}) : undefined,
			servesDatasets: dto.servesDatasets,
			themeCodes: dto.themes ? dto.themes.map(x => x.code!) : undefined,
			title: dto.title,
			version: dto.version,
			versionNotes: dto.versionNotes,
			identifiers: dto.identifiers
		});
	}
}
