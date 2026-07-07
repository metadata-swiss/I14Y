import {ConceptInput, ConceptView} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class ConceptInputMapper {
	public static mapToInputModel(dto: ConceptView): ConceptInput {
		return new ConceptInput({
			codelistEntryValueMaxLength: dto.codelistEntryValueMaxLength,
			codeListEntryValueType: dto.codeListEntryValueType,
			codeListEntryDefaultSortProperty: dto.codeListEntryDefaultSortProperty,
			conceptType: dto.conceptType,
			conformsTo: dto.conformsTo,
			description: dto.description,
			identifiers: dto.identifiers,
			id: dto.id,
			keywords: dto.keywords,
			maxLength: dto.maxLength,
			maxValue: dto.maxValue,
			measurementUnit: dto.measurementUnit,
			minLength: dto.minLength,
			minValue: dto.minValue,
			name: dto.name,
			nbDecimal: dto.nbDecimal,
			pattern: dto.pattern,
			publisher: dto.publisher,
			responsibleDeputy: dto.responsibleDeputy,
			responsiblePerson: dto.responsiblePerson,
			themeCodes: dto.themes ? dto.themes.map(x => x.code!) : undefined,
			validFrom: dto.validFrom!,
			validTo: dto.validTo,
			version: dto.version
		});
	}
}
