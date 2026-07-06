import {Agent, AgentModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {VocabularyEntryMapper} from './vocabularyentrymapper';

export class AgentMapper {
	public static mapToAgent(dto: AgentModel): Agent {
		return new Agent({
			classification: dto.classification ? VocabularyEntryMapper.mapToVocabularyEntry(dto.classification) : undefined,
			contactPoint: dto.contactPoint,
			description: dto.description,
			homePage: dto.homePage,
			id: dto.id || undefined,
			identifier: dto.identifier,
			name: dto.name,
			prefLabel: dto.prefLabel,
			spatial: dto.spatial,
			spatialCH: dto.spatialCH?.map(x => VocabularyEntryMapper.mapToVocabularyEntry(x)) ?? undefined,
			subAgents: dto.subAgents,
			uid: dto.uid
		});
	}

	public static mapToAgentModel(dto: Agent): AgentModel {
		return new AgentModel({
			classification: dto.classification ? VocabularyEntryMapper.mapToVocabularyEntryModel(dto.classification) : undefined,
			contactPoint: dto.contactPoint,
			description: dto.description,
			homePage: dto.homePage,
			id: dto.id || undefined,
			identifier: dto.identifier,
			name: dto.name,
			prefLabel: dto.prefLabel,
			spatial: dto.spatial,
			spatialCH: dto.spatialCH?.map(x => VocabularyEntryMapper.mapToVocabularyEntryModel(x)) ?? undefined,
			subAgents: dto.subAgents,
			system: dto.system,
			uid: dto.uid
		});
	}
}
