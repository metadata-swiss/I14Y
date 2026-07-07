import {PublicServiceInput, PublicServiceModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ChannelInputModelMapper} from './channelinputmodelmapper';
import {AgentMapper} from './agentmapper';
import {VocabularyEntryMapper} from './vocabularyentrymapper';

export class PublicServiceInputMapper {
	public static mapToInputModel(dto: PublicServiceModel): PublicServiceInput {
		return new PublicServiceInput({
			businessEventsCodes: dto.businessEvents ? dto.businessEvents.map(x => x.code!) : undefined,
			channels: dto.channels?.map(x => ChannelInputModelMapper.mapToInputModel(x)) ?? undefined,
			competentAuthority: dto.publisher ? AgentMapper.mapToAgent(dto.publisher) : undefined,
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
			sectorCodes: dto.sectors ? dto.sectors.map(x => x.code!) : undefined,
			spatial: dto.spatial,
			thematicAreaCodes: dto.thematicAreas ? dto.thematicAreas.map(x => x.code!) : undefined,
			title: dto.name,
			spatialCH: dto.spatialCH?.map(x => VocabularyEntryMapper.mapToVocabularyEntry(x)) ?? undefined
		});
	}
}
