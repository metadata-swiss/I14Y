import {IdentifierInputModel, PublicServiceInput, PublicServiceModel, ThemeInputModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ChannelInputModelMapper} from './channelinputmodelmapper';
import {VocabularyEntryMapper} from './vocabularyentrymapper';

export class PublicServiceInputMapper {
	public static mapToInputModel(dto: PublicServiceModel): PublicServiceInput {
		return new PublicServiceInput({
			businessEventsCodes: dto.businessEvents ? dto.businessEvents.map(x => x.code!) : undefined,
			channels: dto.channels?.map(x => ChannelInputModelMapper.mapToInputModel(x)) ?? undefined,
			competentAuthority: dto.publisher?.identifier ? new IdentifierInputModel({identifier: dto.publisher.identifier}) : undefined,
			description: dto.description,
			id: dto.id || undefined,
			identifiers: dto.identifiers,
			isDescribedAt: dto.isDescribedAt,
			keywords: dto.keywords,
			languageCodes: dto.languages ? dto.languages.map(x => x.code!) : undefined,
			lifeEventsCodes: dto.lifeEvents ? dto.lifeEvents.map(x => x.code!) : undefined,
			relations: dto.relations,
			requires: dto.requires,
			responsibleDeputy: dto.responsibleDeputy,
			responsiblePerson: dto.responsiblePerson,
			sectors: dto.sectors?.map(x => new ThemeInputModel({code: x.code, uri: x.uri, vocabularyIdentifier: x.vocabularyIdentifier})),
			spatial: dto.spatial,
			thematicAreas: dto.thematicAreas?.map(x => new ThemeInputModel({code: x.code, uri: x.uri, vocabularyIdentifier: x.vocabularyIdentifier})),
			title: dto.name,
			spatialCH: dto.spatialCH?.map(x => VocabularyEntryMapper.mapToVocabularyEntry(x)) ?? undefined
		});
	}
}
