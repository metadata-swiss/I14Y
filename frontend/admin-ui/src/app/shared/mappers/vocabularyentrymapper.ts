import {VocabularyEntry, VocabularyEntryModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class VocabularyEntryMapper {
	public static mapToVocabularyEntry(dto: VocabularyEntryModel): VocabularyEntry {
		return new VocabularyEntry({
			code: dto.code,
			name: dto.name,
			uri: dto.uri
		});
	}

	public static mapToVocabularyEntryModel(dto: VocabularyEntry): VocabularyEntryModel {
		return new VocabularyEntryModel({
			code: dto.code,
			name: dto.name,
			uri: dto.uri
		});
	}
}
