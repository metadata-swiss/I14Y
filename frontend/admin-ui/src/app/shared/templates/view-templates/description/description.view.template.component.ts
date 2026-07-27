import {ArrayToStringPipe} from '../../../formatting/array-to-string.pipe';
import {Component, inject, Input, OnDestroy, OnInit} from '@angular/core';
import {ViewType} from '../../viewtype';
import {
	CodeListEntryValueTypeEnum,
	ConceptType,
	ConceptView,
	DataServiceModel,
	Dataset,
	DcatDatasetModel,
	DcatQualifiedAttributionModel,
	DcatQualifiedRelationModel,
	ConceptReferenceModel,
	KeywordModel,
	MappingTableModel,
	MultiLanguage,
	PublicServiceModel,
	PublicServiceView,
	Resource,
	ResourceModel,
	VCardModel,
	VocabularyEntry
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {FallbackPipe} from '../../../fallback/fallback.pipe';
import {ObEExternalLinkIcon, ObNavTreeItemModel} from '@oblique/oblique';
import {FormatFunctions} from '../../../format-functions';
import {Subject, takeUntil} from 'rxjs';
import {ActivatedRoute, Router} from '@angular/router';
import {VOCAB_ID_ACCESS_RIGHT, VOCAB_ID_BUSINESS_EVENTS, VOCAB_ID_DATASET_THEME, VOCAB_ID_LIFE_EVENTS} from 'src/app/app-constants';
import {
	buildConceptIri,
	buildDataServiceIri,
	buildDatasetIri,
	buildMappingTableIri,
	buildPublicServiceIri,
	extractIriVersion
} from 'src/app/shared/helper/iri-helpers';
import {VocabularyConfigService} from 'src/app/services/vocabulary-config.service';
import {AppConfig} from 'src/app/app.config';
import {IAppConfig} from 'src/app/app.config.interface';

// eslint-disable-next-line no-shadow
enum Section {
	GeneralInformation = 'section-general-information',
	Responsible = 'section-responsible',
	Properties = 'section-properties',
	CataloguesAndThemes = 'section-catalogues-and-themes',
	Lineage = 'section-lineage',
	Versions = 'section-versions',
	Relations = 'section-relations',
	MappingRelations = 'mapping-relations',
	Channels = 'section-channels',
	CodelistEntries = 'section-codelist-entries'
}

@Component({
	selector: 'app-description-view-template',
	templateUrl: './description.view.template.component.html',
	styleUrls: ['./description.view.template.component.scss'],
	standalone: false
})
export class DescriptionViewTemplateComponent implements OnInit, OnDestroy {
	@Input() dto: DcatDatasetModel | DataServiceModel | PublicServiceModel | ConceptView | MappingTableModel | undefined;
	@Input() viewType: ViewType = ViewType.Unspecified;
	@Input() nextVersions: DcatDatasetModel[] | DataServiceModel[] | undefined;
	@Input() previousVersion: DcatDatasetModel | DataServiceModel | undefined;
	@Input() servesDatasets: Dataset[] | undefined;
	@Input() isDescribedAt: Dataset[] | undefined;
	@Input() relations: PublicServiceView[] | undefined;
	@Input() requires: PublicServiceView[] | undefined;
	@Input() conceptReferencesCount?: number;
	@Input() mappingTablesCount?: number;
	currentLanguage: string;
	icon: ObEExternalLinkIcon = 'none';
	target = '_blank';
	rel = 'noopener noreferrer';
	readonly viewTypeEnum = ViewType;
	readonly conceptTypeEnum = ConceptType;
	readonly codeListEntryValueTypeEnum = CodeListEntryValueTypeEnum;
	readonly sectionEnum = Section;

	accessRightsConceptPageIri: string | undefined = undefined;
	themesConceptPageIri: string | undefined = undefined;
	businessEventsConceptPageIri: string | undefined = undefined;
	lifeEventsConceptPageIri: string | undefined = undefined;

	private readonly sections: ObNavTreeItemModel[] = [];
	private readonly generalnformationKey: string = 'i18n.general_information.title';
	private readonly responsibleKey: string = 'i18n.responsible.title';
	private readonly propertiesKey: string = 'i18n.properties.title';
	private readonly cataloguesAndThemesKey: string = 'i18n.catalogues_and_themes.title';
	private readonly lineageKey: string = 'i18n.lineage.title';
	private readonly versionsKey: string = 'i18n.versions.title';
	private readonly relationsKey: string = 'i18n.relations.title';
	private readonly mappingRelationsKey: string = 'i18n.mapping_relations.title';
	private readonly channelKey: string = 'i18n.channels.title';
	private readonly codelistEntriesKey: string = 'i18n.codelist.title';
	private readonly unsubscribe$ = new Subject();

	private readonly arrayToString = inject(ArrayToStringPipe);
	private readonly fallback = inject(FallbackPipe);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyConfigService = inject(VocabularyConfigService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});

		this.vocabularyConfigService
			.resolveConceptPageIris([VOCAB_ID_DATASET_THEME, VOCAB_ID_ACCESS_RIGHT, VOCAB_ID_BUSINESS_EVENTS, VOCAB_ID_LIFE_EVENTS])
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(iris => {
				this.themesConceptPageIri = iris[VOCAB_ID_DATASET_THEME];
				this.accessRightsConceptPageIri = iris[VOCAB_ID_ACCESS_RIGHT];
				this.businessEventsConceptPageIri = iris[VOCAB_ID_BUSINESS_EVENTS];
				this.lifeEventsConceptPageIri = iris[VOCAB_ID_LIFE_EVENTS];
			});
		this.translate
			.stream([
				this.generalnformationKey,
				this.responsibleKey,
				this.propertiesKey,
				this.cataloguesAndThemesKey,
				this.lineageKey,
				this.versionsKey,
				this.relationsKey,
				this.mappingRelationsKey,
				this.channelKey,
				this.codelistEntriesKey
			])
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(result => {
				this.addOrUpdateNavTreeItem(Section.GeneralInformation, result[this.generalnformationKey]);
				this.addOrUpdateNavTreeItem(Section.Responsible, result[this.responsibleKey]);
				this.addOrUpdateNavTreeItem(Section.Properties, result[this.propertiesKey]);
				this.addOrUpdateNavTreeItem(Section.CataloguesAndThemes, result[this.cataloguesAndThemesKey]);
				this.addOrUpdateNavTreeItem(Section.CodelistEntries, result[this.codelistEntriesKey]);
				this.addOrUpdateNavTreeItem(Section.MappingRelations, result[this.mappingRelationsKey]);
				this.addOrUpdateNavTreeItem(Section.Lineage, result[this.lineageKey]);
				this.addOrUpdateNavTreeItem(Section.Versions, result[this.versionsKey]);
				this.addOrUpdateNavTreeItem(Section.Relations, result[this.relationsKey]);
				this.addOrUpdateNavTreeItem(Section.Channels, result[this.channelKey]);
			});
	}

	ngOnDestroy(): void {
		this.unsubscribe$.next(undefined);
		this.unsubscribe$.complete();
	}

	getSidePanelItems(): ObNavTreeItemModel[] {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
				return this.sections.filter(
					x =>
						x.id === Section.GeneralInformation ||
						x.id === Section.Responsible ||
						x.id === Section.Properties ||
						x.id === Section.CataloguesAndThemes ||
						x.id === Section.Versions ||
						x.id === Section.Relations
				);
			case PublicServiceModel:
				return this.sections.filter(
					x =>
						x.id === Section.GeneralInformation ||
						x.id === Section.Responsible ||
						x.id === Section.Properties ||
						x.id === Section.Relations ||
						x.id === Section.Channels
				);
			case ConceptView:
				return this.sections.filter(
					x =>
						x.id === Section.GeneralInformation ||
						x.id === Section.Responsible ||
						x.id === Section.Properties ||
						x.id === Section.CodelistEntries ||
						(((this.getConceptReplaces().length ?? 0) > 0 || (this.getConceptIsReplacedBy().length ?? 0) > 0) && x.id === Section.Lineage) ||
						x.id === Section.Versions ||
						(((this.conceptReferencesCount ?? 0) > 0 || (this.mappingTablesCount ?? 0) > 0) && x.id === Section.Relations)
				);
			case MappingTableModel:
				return this.sections.filter(
					x => x.id === Section.GeneralInformation || x.id === Section.Responsible || x.id === Section.Properties || x.id === Section.MappingRelations
				);
			default:
				return this.sections;
		}
	}

	isRendered(viewTypes: ViewType[]): boolean {
		return viewTypes?.includes(this.viewType) || false;
	}

	getTitle(): string | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
				return this.fallback.transform((this.dto as DcatDatasetModel | DataServiceModel).title, this.currentLanguage) ?? '-';
			case PublicServiceModel:
			case ConceptView:
			case MappingTableModel:
				return this.fallback.transform((this.dto as ConceptView | PublicServiceModel | MappingTableModel).name, this.currentLanguage) ?? '-';

			default:
				return undefined;
		}
	}

	getIdentifier(): string[] | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
			case MappingTableModel:
				return (this.dto as DcatDatasetModel | DataServiceModel | MappingTableModel).identifiers;
			case PublicServiceModel:
			case ConceptView:
				return (this.dto as PublicServiceModel | ConceptView).identifiers?.length ? (this.dto as PublicServiceModel | ConceptView).identifiers : ['-'];
			default:
				return undefined;
		}
	}

	getFormattedIriPattern(): string | undefined {
		const [identifier] = this.getIdentifier() ?? [];
		if (!identifier) {
			return undefined;
		}
		switch (this.dto?.constructor) {
			case ConceptView: {
				const version = this.getVersion() ?? '';
				if (!version) {
					return undefined;
				}
				return buildConceptIri(identifier, version);
			}
			case DcatDatasetModel:
				return buildDatasetIri(identifier);
			case DataServiceModel:
				return buildDataServiceIri(identifier);
			case PublicServiceModel:
				return buildPublicServiceIri(identifier);
			case MappingTableModel: {
				const ver = this.getVersion() ?? '';
				if (!ver) {
					return undefined;
				}
				return buildMappingTableIri(identifier, ver);
			}
			default:
				return undefined;
		}
	}

	getPublisher(): string | undefined {
		return this.fallback.transform(this.dto?.publisher?.name, this.currentLanguage) ?? '-';
	}

	getVersion(): string | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
			case ConceptView:
			case MappingTableModel:
				return (this.dto as DcatDatasetModel | DataServiceModel | ConceptView | MappingTableModel).version ?? '-';
			default:
				return undefined;
		}
	}

	getValidFrom(): string | undefined {
		switch (this.dto?.constructor) {
			case ConceptView:
			case MappingTableModel:
				return FormatFunctions.getFormattedDate((this.dto as ConceptView | MappingTableModel).validFrom) ?? '-';
			default:
				return undefined;
		}
	}

	getValidTo(): string | undefined {
		switch (this.dto?.constructor) {
			case ConceptView:
			case MappingTableModel:
				return FormatFunctions.getFormattedDate((this.dto as ConceptView | MappingTableModel).validTo) ?? '-';
			default:
				return undefined;
		}
	}

	getVersionNotes(): string | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
				return this.fallback.transform((this.dto as DcatDatasetModel | DataServiceModel).versionNotes, this.currentLanguage) ?? '-';
			default:
				return undefined;
		}
	}

	getDataOwner(): string | undefined {
		if (this.dto instanceof DcatDatasetModel) {
			return this.dto.dataOwner ?? '-';
		}
		return undefined;
	}

	getResponsiblePerson(): string | undefined {
		switch (this.dto?.constructor) {
			case ConceptView:
				return (this.dto as ConceptView).responsiblePerson?.name ?? '-';
			case DcatDatasetModel:
			case DataServiceModel:
			case MappingTableModel:
			case PublicServiceModel:
				// eslint-disable-next-line max-len
				return FormatFunctions.getFullName((this.dto as DcatDatasetModel | DataServiceModel | MappingTableModel | PublicServiceModel).responsiblePerson) ?? '-';
			default:
				return undefined;
		}
	}

	getResponsibleDeputy(): string | undefined {
		switch (this.dto?.constructor) {
			case ConceptView:
				return (this.dto as ConceptView).responsibleDeputy?.name ?? '-';
			case DcatDatasetModel:
			case DataServiceModel:
			case MappingTableModel:
			case PublicServiceModel:
				// eslint-disable-next-line max-len
				return FormatFunctions.getFullName((this.dto as DcatDatasetModel | DataServiceModel | MappingTableModel | PublicServiceModel).responsibleDeputy) ?? '-';
			default:
				return undefined;
		}
	}

	getContactPoints(): VCardModel[] | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
				return (this.dto as DcatDatasetModel | DataServiceModel).contactPoints;
			default:
				return undefined;
		}
	}

	getLandingPages(): ResourceModel[] | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
				return (this.dto as DcatDatasetModel | DataServiceModel).landingPages;
			default:
				return undefined;
		}
	}

	getEndpointUrls(): ResourceModel[] | undefined {
		if (this.dto instanceof DataServiceModel) {
			return this.dto.endpointUrls;
		}
		return undefined;
	}

	getEndpointDescriptions(): ResourceModel[] | undefined {
		if (this.dto instanceof DataServiceModel) {
			return this.dto.endpointDescriptions;
		}
		return undefined;
	}

	getLicense(): string | undefined {
		if (this.dto instanceof DataServiceModel) {
			return this.fallback.transform(this.dto.license?.name, this.currentLanguage) ?? '-';
		}
		return undefined;
	}

	getQualifiedAttributions(): DcatQualifiedAttributionModel[] | undefined {
		if (this.dto instanceof DcatDatasetModel) {
			return this.dto.qualifiedAttributions;
		}
		return undefined;
	}

	getQualifiedAttributionComplement(): string | undefined {
		if (this.dto instanceof DcatDatasetModel) {
			return this.fallback.transform(this.dto.qualifiedAttributionComplement, this.currentLanguage) ?? '-';
		}
		return undefined;
	}

	getPublicationDate(): string | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
				return FormatFunctions.getFormattedDate((this.dto as DcatDatasetModel | DataServiceModel).issued) ?? '-';
			default:
				return undefined;
		}
	}

	getModifiedDate(): string | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
				return FormatFunctions.getFormattedDate((this.dto as DcatDatasetModel | DataServiceModel).modified) ?? '-';
			default:
				return undefined;
		}
	}

	getSystemCreatedAt(): string {
		return FormatFunctions.getFormattedDateTime(this.dto?.system?.createdAt) ?? '-';
	}

	getSystemModifiedAt(): string {
		return FormatFunctions.getFormattedDateTime(this.dto?.system?.modifiedAt) ?? '-';
	}

	getSystemCreationType(): string {
		const creationType = this.dto?.system?.creationType;
		if (!creationType) return '-';
		return this.translate.instant(`i18n.system.creation_type.${creationType.toLowerCase()}`);
	}

	getAccessRights(): string | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
				return this.fallback.transform((this.dto as DcatDatasetModel | DataServiceModel).accessRights?.name, this.currentLanguage) ?? '-';
			default:
				return undefined;
		}
	}

	getAccessRightsCode(): string | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
				return (this.dto as DcatDatasetModel | DataServiceModel).accessRights?.code ?? undefined;
			default:
				return undefined;
		}
	}

	getConfidentiality(): string | undefined {
		if (this.dto instanceof DcatDatasetModel) {
			return this.fallback.transform(this.dto.confidentialityPerson?.name, this.currentLanguage) ?? '-';
		}
		return undefined;
	}

	getLanguages(): string | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case PublicServiceModel:
				return this.arrayToString.transform(
					FormatFunctions.getTranslatedVocabularyEntries((this.dto as DcatDatasetModel | PublicServiceModel).languages, this.currentLanguage),
					', ',
					'-'
				);
			default:
				return undefined;
		}
	}

	getThemeEntries(): VocabularyEntry[] | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
			case ConceptView:
			case MappingTableModel:
				return (this.dto as DcatDatasetModel | DataServiceModel | ConceptView | MappingTableModel).themes;
			case PublicServiceModel:
				return (this.dto as PublicServiceModel).thematicAreas;
			default:
				return undefined;
		}
	}

	openAccessRightsUrl(code: string): void {
		void this.router.navigate(['/catalog/all'], {queryParams: {accessRights: code}});
	}

	getBusinessEventEntries(): VocabularyEntry[] | undefined {
		if (this.dto instanceof PublicServiceModel) {
			return this.dto.businessEvents;
		}
		return undefined;
	}

	getLifeEventEntries(): VocabularyEntry[] | undefined {
		if (this.dto instanceof PublicServiceModel) {
			return this.dto.lifeEvents;
		}
		return undefined;
	}

	getPublisherOrganisationsUrl(): string {
		const inputRoute = AppConfig.getConfig<IAppConfig>().I14Y_PUBLIC_ROUTE;
		return `${inputRoute}/${this.currentLanguage}/organisations`;
	}

	getKeyWords(): KeywordModel[] | undefined {
		return this.dto?.keywords;
	}

	getSectors(): string | undefined {
		if (this.dto instanceof PublicServiceModel) {
			return this.arrayToString.transform(FormatFunctions.getTranslatedVocabularyEntries(this.dto.sectors, this.currentLanguage), ', ', '-');
		}
		return undefined;
	}

	getSpatialCH(): string | undefined {
		if (this.dto instanceof PublicServiceModel) {
			return this.arrayToString.transform(FormatFunctions.getTranslatedVocabularyEntriesWithCode(this.dto.spatialCH, this.currentLanguage), ', ', '-');
		}
		return undefined;
	}

	getSpatialCoverage(): string[] | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case PublicServiceModel:
				return (this.dto as DcatDatasetModel | PublicServiceModel).spatial;
			default:
				return undefined;
		}
	}

	getTemporalCoverage(): string[] | undefined {
		if (this.dto instanceof DcatDatasetModel) {
			return this.dto.temporalCoverage?.map(e => `${FormatFunctions.getFormattedDate(e.start)} - ${FormatFunctions.getFormattedDate(e.end)}`);
		}
		return undefined;
	}

	getFrequency(): string | undefined {
		if (this.dto instanceof DcatDatasetModel) {
			return this.fallback.transform(this.dto.frequency?.name, this.currentLanguage) ?? '-';
		}
		return undefined;
	}

	getRetentionPeriod(): string | undefined {
		if (this.dto instanceof DcatDatasetModel) {
			return FormatFunctions.getFormattedDate(this.dto.retentionPeriod) ?? '-';
		}
		return undefined;
	}

	getRetentionPeriodComplement(): string | undefined {
		if (this.dto instanceof DcatDatasetModel) {
			return this.fallback.transform(this.dto.retentionPeriodComplement, this.currentLanguage) ?? '-';
		}
		return undefined;
	}

	getIsReferencedBy(): ResourceModel[] | undefined {
		if (this.dto instanceof DcatDatasetModel) {
			return this.dto.isReferencedBy;
		}
		return undefined;
	}

	getQualifiedRelations(): DcatQualifiedRelationModel[] | undefined {
		if (this.dto instanceof DcatDatasetModel) {
			return this.dto.qualifiedRelations;
		}
		return undefined;
	}

	generateExternalLink(href: string | undefined): string {
		if (href && (href.includes('http') || href.includes('https'))) {
			return `${href}`;
		}
		return `//${href}`;
	}

	getRelatedResources(): ResourceModel[] | undefined {
		if (this.dto instanceof DcatDatasetModel) {
			return this.dto.relations;
		}
		return undefined;
	}

	getConformsTo(): ResourceModel[] | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
			case MappingTableModel:
				return (this.dto as DcatDatasetModel | DataServiceModel | MappingTableModel).conformsTo;
			case ConceptView:
				return (this.dto as ConceptView).conformsTo?.map(x => this.convertToResourceModel(x));
			default:
				return undefined;
		}
	}

	getDocumentations(): ResourceModel[] | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
				return (this.dto as DcatDatasetModel | DataServiceModel).documentation;
			default:
				return undefined;
		}
	}

	getGeoIvId(): string | undefined {
		if (this.dto instanceof DcatDatasetModel) {
			return this.arrayToString.transform(FormatFunctions.geGeoIvtTranslatedVocabularyEntries(this.dto.geoIvIds, this.currentLanguage), ', ', '-');
		}
		return undefined;
	}

	getProcessId(): string | undefined {
		if (this.dto instanceof DcatDatasetModel) {
			return this.dto.processId ?? '-';
		}
		return undefined;
	}

	getImages(): ResourceModel[] | undefined {
		if (this.dto instanceof DcatDatasetModel) {
			return this.dto.images;
		}
		return undefined;
	}

	getConceptType(): ConceptType | undefined {
		if (this.dto instanceof ConceptView) {
			return this.dto.conceptType;
		}
		return undefined;
	}

	isProperty(conceptTypes: ConceptType[]): boolean {
		if (this.dto instanceof ConceptView) {
			return this.dto.conceptType ? conceptTypes.includes(this.dto.conceptType) : false;
		}
		return false;
	}

	getCodeListEntyValueType(): CodeListEntryValueTypeEnum | undefined {
		if (this.dto instanceof ConceptView) {
			return this.dto.codeListEntryValueType;
		}
		return undefined;
	}

	getCodeListEntyValueMaxLength(): number | undefined {
		if (this.dto instanceof ConceptView) {
			return this.dto.codelistEntryValueMaxLength;
		}
		return undefined;
	}

	getPattern(): string | undefined {
		if (this.dto instanceof ConceptView) {
			return this.dto.pattern;
		}
		return undefined;
	}

	getNbDezimal(): number | undefined {
		if (this.dto instanceof ConceptView) {
			return this.dto.nbDecimal;
		}
		return undefined;
	}

	getMeasurementUnit(): string | undefined {
		if (this.dto instanceof ConceptView) {
			return this.dto.measurementUnit;
		}
		return undefined;
	}

	getMinValue(): number | undefined {
		if (this.dto instanceof ConceptView) {
			return this.dto.minValue;
		}
		return undefined;
	}

	getMaxValue(): number | undefined {
		if (this.dto instanceof ConceptView) {
			return this.dto.maxValue;
		}
		return undefined;
	}

	getMinLenght(): number | undefined {
		if (this.dto instanceof ConceptView) {
			return this.dto.minLength;
		}
		return undefined;
	}

	getMaxLenght(): number | undefined {
		if (this.dto instanceof ConceptView) {
			return this.dto.maxLength;
		}
		return undefined;
	}

	getSourceName(): MultiLanguage | undefined {
		if (this.dto instanceof MappingTableModel) {
			return this.dto.source?.name;
		}
		return undefined;
	}

	getSourceVersion(): string | undefined {
		if (this.dto instanceof MappingTableModel && this.dto.source?.uri) {
			return this.getVersionFromRegisterUri(this.dto.source?.uri);
		}
		return undefined;
	}

	getSourceUri(): string | undefined {
		if (this.dto instanceof MappingTableModel) {
			return this.dto.source?.uri;
		}
		return undefined;
	}

	getTargetName(): MultiLanguage | undefined {
		if (this.dto instanceof MappingTableModel) {
			return this.dto.target?.name ?? undefined;
		}
		return undefined;
	}

	getTargetVersion(): string | undefined {
		if (this.dto instanceof MappingTableModel && this.dto.target?.uri) {
			return this.getVersionFromRegisterUri(this.dto.target?.uri);
		}
		return undefined;
	}

	getTargetUri(): string | undefined {
		if (this.dto instanceof MappingTableModel) {
			return this.dto.target?.uri;
		}
		return undefined;
	}

	getPreviousVersion(): DcatDatasetModel | DataServiceModel | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
				return this.previousVersion;
			default:
				return undefined;
		}
	}

	getNextVersions(): DcatDatasetModel[] | DataServiceModel[] | undefined {
		switch (this.dto?.constructor) {
			case DcatDatasetModel:
			case DataServiceModel:
				return this.nextVersions;
			default:
				return undefined;
		}
	}

	getServesDatasets(): Dataset[] | undefined {
		if (this.dto instanceof DataServiceModel) {
			return this.servesDatasets;
		}
		return undefined;
	}

	getIsDescribedAt(): Dataset[] | undefined {
		if (this.dto instanceof PublicServiceModel) {
			return this.isDescribedAt;
		}
		return undefined;
	}

	getRelations(): PublicServiceView[] | undefined {
		if (this.dto instanceof PublicServiceModel) {
			return this.relations;
		}
		return undefined;
	}

	getRequires(): PublicServiceView[] | undefined {
		if (this.dto instanceof PublicServiceModel) {
			return this.requires;
		}
		return undefined;
	}

	private addOrUpdateNavTreeItem(section: Section, text: string) {
		let navTreeItem = this.sections.find(x => x.id === section);
		if (navTreeItem) {
			navTreeItem.label = text;
		} else {
			this.sections.push(
				new ObNavTreeItemModel({
					id: section,
					path: './',
					label: text,
					fragment: section,
					queryParams: this.route.snapshot.queryParams
				})
			);
		}
	}

	private convertToResourceModel(item: Resource): ResourceModel {
		return new ResourceModel({
			label: item.label,
			uri: item.href
		});
	}

	private getVersionFromRegisterUri(uri: string | undefined): string | undefined {
		return uri ? extractIriVersion(uri) : undefined;
	}

	getConceptReplaces(): ConceptReferenceModel[] {
		return (this.dto as ConceptView)?.replaces ?? [];
	}

	getConceptIsReplacedBy(): ConceptReferenceModel[] {
		return (this.dto as ConceptView)?.isReplacedBy ?? [];
	}

	getConceptReferenceName(item: ConceptReferenceModel): string {
		return this.fallback.transform(item.name, this.currentLanguage) ?? item.uri ?? '';
	}

	getConceptReferenceVersionSuffix(item: ConceptReferenceModel): string {
		const version = extractIriVersion(item.uri ?? '');
		return version ? ` (${version})` : '';
	}
}
