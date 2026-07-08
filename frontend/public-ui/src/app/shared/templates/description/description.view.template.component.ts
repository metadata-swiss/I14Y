import {Component, inject, Input, OnDestroy, OnInit} from '@angular/core';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ObEExternalLinkIcon, ObNavTreeItemModel} from '@oblique/oblique';
import {ViewType} from '../viewtype';
import {FormatFunctions} from '../../format-functions';
import {ArrayToStringPipe} from '../../formating/array-to-string.pipe';
import {FallbackPipe} from '../../fallback/fallback.pipe';
import {
	DataService,
	DataServiceVersionSummary,
	Dataset,
	DatasetVersionSummary,
	IdLabel,
	MultiLanguage,
	PublicServiceView,
	QualifiedAttribution,
	QualifiedRelation,
	Resource,
	Vcard,
	ConceptReferenceModel,
	ConceptView,
	ConceptType,
	CodeListEntryValueTypeEnum,
	VocabularyEntry,
	KeywordModel,
	MappingTableModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Subject, takeUntil} from 'rxjs';
import {take} from 'rxjs/operators';
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
import {VocabularyConfigService} from '../../services/vocabulary-config/vocabulary-config.service';

// eslint-disable-next-line no-shadow
enum Section {
	GeneralInformation = 'section-general-information',
	Distributions = 'section-distributions',
	Properties = 'section-properties',
	Lineage = 'section-lineage',
	Versions = 'section-versions',
	Relations = 'section-relations',
	Channels = 'section-channels'
}

@Component({
	selector: 'app-description-view-template',
	templateUrl: './description.view.template.component.html',
	styleUrls: ['./description.view.template.component.scss'],
	standalone: false
})
export class DescriptionViewTemplateComponent implements OnInit, OnDestroy {
	@Input() dto: Dataset | DataService | PublicServiceView | ConceptView | MappingTableModel | undefined;
	@Input() viewType: ViewType = ViewType.Unspecified;
	@Input() hasIsServedBy: boolean = false;
	@Input() hasChannels: boolean = false;
	@Input() hasMappingTables: boolean = false;
	@Input() publisherIdentifier?: string;
	@Input() conceptReferencesCount?: number;
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
	private readonly propertiesKey: string = 'i18n.properties.title';
	private readonly cataloguesAndThemesKey: string = 'i18n.catalogues_and_themes.title';
	private readonly lineageKey: string = 'i18n.lineage.title';
	private readonly versionsKey: string = 'i18n.versions.title';
	private readonly relationsKey: string = 'i18n.relations.title';
	private readonly channelsKey: string = 'i18n.channels.title';
	private readonly distribituinsKey: string = 'i18n.distributions.title';
	private readonly unsubscribe$ = new Subject();

	private readonly arrayToString = inject(ArrayToStringPipe);
	private readonly fallback = inject(FallbackPipe);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyConfigService = inject(VocabularyConfigService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang() ?? this.translate.getFallbackLang() ?? 'de';
	}

	ngOnInit(): void {
		this.vocabularyConfigService
			.resolveConceptPageIris([VOCAB_ID_DATASET_THEME, VOCAB_ID_ACCESS_RIGHT, VOCAB_ID_BUSINESS_EVENTS, VOCAB_ID_LIFE_EVENTS])
			.pipe(take(1))
			.subscribe(iris => {
				this.themesConceptPageIri = iris[VOCAB_ID_DATASET_THEME];
				this.accessRightsConceptPageIri = iris[VOCAB_ID_ACCESS_RIGHT];
				this.businessEventsConceptPageIri = iris[VOCAB_ID_BUSINESS_EVENTS];
				this.lifeEventsConceptPageIri = iris[VOCAB_ID_LIFE_EVENTS];
			});

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
		this.translate
			.stream([
				this.generalnformationKey,
				this.propertiesKey,
				this.cataloguesAndThemesKey,
				this.lineageKey,
				this.versionsKey,
				this.relationsKey,
				this.channelsKey,
				this.distribituinsKey
			])
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(result => {
				this.addOrUpdateNavTreeItem(Section.GeneralInformation, result[this.generalnformationKey]);
				this.addOrUpdateNavTreeItem(Section.Distributions, result[this.distribituinsKey]);
				this.addOrUpdateNavTreeItem(Section.Properties, result[this.propertiesKey]);
				this.addOrUpdateNavTreeItem(Section.Lineage, result[this.lineageKey]);
				this.addOrUpdateNavTreeItem(Section.Versions, result[this.versionsKey]);
				this.addOrUpdateNavTreeItem(Section.Relations, result[this.relationsKey]);
				this.addOrUpdateNavTreeItem(Section.Channels, result[this.channelsKey]);
			});
	}

	getSidePanelItems(): ObNavTreeItemModel[] {
		switch (this.dto?.constructor) {
			case Dataset:
				return this.sections.filter(
					// eslint-disable-next-line max-len
					x => x.id === Section.GeneralInformation || x.id === Section.Distributions || this.showProperties(x) || this.showVersions(x) || this.showRelations(x)
				);
			case DataService:
			case MappingTableModel:
				// eslint-disable-next-line max-len
				return this.sections.filter(x => x.id === Section.GeneralInformation || this.showProperties(x) || this.showVersions(x) || this.showRelations(x));
			case ConceptView:
				// eslint-disable-next-line max-len
				return this.sections.filter(
					x => x.id === Section.GeneralInformation || this.showProperties(x) || this.showLineage(x) || this.showVersions(x) || this.showRelations(x)
				);
			case PublicServiceView:
				// eslint-disable-next-line max-len
				return this.sections.filter(x => x.id === Section.GeneralInformation || this.showProperties(x) || this.showRelations(x) || x.id === Section.Channels);
			default:
				return this.sections;
		}
	}

	isRendered(viewTypes: ViewType[]): boolean {
		return viewTypes?.includes(this.viewType) || false;
	}

	hasVisibleProperties(section: Section): boolean {
		switch (section) {
			case Section.Properties:
				return this.isPropertiesSectionVisible();
			case Section.Lineage:
				return this.isLineageSectionVisible();
			case Section.Versions:
				return this.isVersionSectionVisible();
			case Section.Relations:
				return this.isRelationSectionVisible();
			default:
				return false;
		}
	}

	getItems<T>(array: T[] | undefined): T[] | undefined {
		return array?.length > 0 ? array : undefined;
	}

	getTitle(): MultiLanguage | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
			case PublicServiceView:
			case DataService:
				return (this.dto as Dataset | DataService | PublicServiceView).title ?? undefined;
			case ConceptView:
			case MappingTableModel:
				return (this.dto as ConceptView | MappingTableModel).name ?? undefined;
			default:
				return undefined;
		}
	}

	getIdentifier(): string[] | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
			case DataService:
			case MappingTableModel:
				return (this.dto as Dataset | DataService | MappingTableModel).identifiers;
			case PublicServiceView:
			case ConceptView:
				return (this.dto as PublicServiceView | ConceptView).identifiers?.length ? (this.dto as PublicServiceView | ConceptView).identifiers : undefined;
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
			case ConceptView:
				const version = this.getVersion() ?? '';
				if (!version) {
					return undefined;
				}
				return buildConceptIri(identifier, version);
			case Dataset:
				return buildDatasetIri(identifier);
			case DataService:
				return buildDataServiceIri(identifier);
			case PublicServiceView:
				return buildPublicServiceIri(identifier);
			case MappingTableModel:
				const ver = this.getVersion() ?? '';
				if (!ver) {
					return undefined;
				}
				return buildMappingTableIri(identifier, ver);
			default:
				return undefined;
		}
	}

	getDescription(): MultiLanguage | undefined {
		return this.dto?.description ?? undefined;
	}

	getPublisher(): MultiLanguage | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
			case DataService:
				return (this.dto as Dataset | DataService).publisherName ?? undefined;
			case PublicServiceView:
				return (this.dto as PublicServiceView).competentAuthority?.name ?? undefined;
			case ConceptView:
			case MappingTableModel:
				return (this.dto as ConceptView | MappingTableModel).publisher?.name ?? undefined;
			default:
				return undefined;
		}
	}

	getPublisherIdentifier(): string | undefined {
		switch (this.dto?.constructor) {
			case PublicServiceView:
				return (this.dto as PublicServiceView).competentAuthority?.identifier ?? undefined;
			case ConceptView:
			case MappingTableModel:
				return (this.dto as ConceptView | MappingTableModel).publisher?.identifier ?? undefined;
			default:
				// Dataset and DataService: identifier comes via @Input from the parent component
				return this.publisherIdentifier;
		}
	}

	getPublisherSearchUrl(): string | undefined {
		const identifier = this.getPublisherIdentifier();
		return identifier ? this.getCatalogSearchUrl('publisher', identifier) : undefined;
	}

	getPublisherSearchQueryParams(): {publisher: string} | undefined {
		const identifier = this.getPublisherIdentifier();
		return identifier ? {publisher: identifier} : undefined;
	}

	getOrganisationsListUrl(): string {
		return `${window.location.origin}/${this.currentLanguage}/organisations`;
	}

	getVersion(): string | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
			case DataService:
			case ConceptView:
			case MappingTableModel:
				return (this.dto as Dataset | DataService | ConceptView).version ?? undefined;
			default:
				return undefined;
		}
	}

	getValidFrom(): string | undefined {
		switch (this.dto?.constructor) {
			case ConceptView:
			case MappingTableModel:
				return (this.dto as ConceptView | MappingTableModel).validFrom
					? FormatFunctions.getFormattedDate((this.dto as ConceptView | MappingTableModel).validFrom)
					: undefined;
			default:
				return undefined;
		}
	}

	getValidTo(): string | undefined {
		switch (this.dto?.constructor) {
			case ConceptView:
			case MappingTableModel:
				return (this.dto as ConceptView | MappingTableModel).validTo
					? FormatFunctions.getFormattedDate((this.dto as ConceptView | MappingTableModel).validTo)
					: undefined;
			default:
				return undefined;
		}
	}

	getVersionNotes(): MultiLanguage | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
			case DataService:
				return (this.dto as Dataset | DataService).versionNotes ?? undefined;
			default:
				return undefined;
		}
	}

	getContactPoints(): Vcard[] | undefined {
		let contactpoints;
		switch (this.dto?.constructor) {
			case Dataset:
			case DataService:
				contactpoints = (this.dto as Dataset | DataService).contactPoints;
				return contactpoints?.length > 0 ? contactpoints : undefined;
			default:
				return undefined;
		}
	}

	getLandingPages(): Resource[] | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
			case DataService:
				return (this.dto as Dataset | DataService).landingPages ?? undefined;
			default:
				return undefined;
		}
	}

	getEndpointUrls(): Resource[] | undefined {
		if (this.dto instanceof DataService) {
			return this.dto.endpointUrls;
		}
		return undefined;
	}

	getEndpointDescriptions(): Resource[] | undefined {
		if (this.dto instanceof DataService) {
			return this.dto.endpointDescriptions;
		}
		return undefined;
	}

	getLicense(): VocabularyEntry | undefined {
		if (this.dto instanceof DataService) {
			return this.dto.license ?? undefined;
		}
		return undefined;
	}

	getQualifiedAttributions(): QualifiedAttribution[] | undefined {
		if (this.dto instanceof Dataset) {
			return this.dto.qualifiedAttribution?.length > 0 ? this.dto.qualifiedAttribution : undefined;
		}
		return undefined;
	}

	getQualifiedAttributionComplement(): MultiLanguage | undefined {
		if (this.dto instanceof Dataset) {
			return this.dto.qualifiedAttributionComplement ?? undefined;
		}
		return undefined;
	}

	getPublicationDate(): string | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
				return (this.dto as Dataset).published ? FormatFunctions.getFormattedDate((this.dto as Dataset).published) : undefined;
			case DataService:
				return (this.dto as DataService).issued ? FormatFunctions.getFormattedDate((this.dto as DataService).issued) : undefined;
			default:
				return undefined;
		}
	}

	getModifiedDate(): string | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
				return (this.dto as Dataset).lastUpdated ? FormatFunctions.getFormattedDate((this.dto as Dataset).lastUpdated) : undefined;
			case DataService:
				return (this.dto as DataService).modified ? FormatFunctions.getFormattedDate((this.dto as DataService).modified) : undefined;
			default:
				return undefined;
		}
	}

	getAccessRights(): MultiLanguage | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
			case DataService:
				return (this.dto as Dataset | DataService).accessRights?.name ?? undefined;

			default:
				return undefined;
		}
	}

	getAccessRightsCode(): string | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
			case DataService:
				return (this.dto as Dataset | DataService).accessRights?.code ?? undefined;
			default:
				return undefined;
		}
	}

	getConfidentiality(): MultiLanguage | undefined {
		if (this.dto instanceof Dataset) {
			return this.dto.confidentialityPerson?.name ?? undefined;
		}
		return undefined;
	}

	getLanguages(): string | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
				return FormatFunctions.getLanguagesTranslated((this.dto as Dataset).languages, this.translate) ?? undefined;
			case PublicServiceView:
				return (this.dto as PublicServiceView).languages
					? this.arrayToString.transform(
							FormatFunctions.getTranslatedVocabularyEntries(
								(this.dto as PublicServiceView).languages,
								this.currentLanguage
							),
							', '
						)
					: undefined;
			default:
				return undefined;
		}
	}

	getThemes(): string | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
			case DataService:
			case ConceptView:
			case MappingTableModel:
				return (
					// eslint-disable-next-line max-len
					this.arrayToString.transform(
						FormatFunctions.getTranslatedVocabularyEntries((this.dto as Dataset | DataService | ConceptView | MappingTableModel).themes, this.currentLanguage),
						', '
					) ?? undefined
				);
			case PublicServiceView:
				return (
					this.arrayToString.transform(
						FormatFunctions.getTranslatedVocabularyEntries((this.dto as PublicServiceView).thematicAreas, this.currentLanguage),
						', '
					) ?? undefined
				);
			default:
				return undefined;
		}
	}

	getKeyWords(): KeywordModel[] | undefined {
		return this.dto?.keywords ?? undefined;
	}

	getSectors(): MultiLanguage[] | undefined {
		if (this.dto instanceof PublicServiceView) {
			return this.dto.sectors?.map(s => s.name) ?? undefined;
		}
		return undefined;
	}

	getBusinessEvents(): MultiLanguage[] | undefined {
		if (this.dto instanceof PublicServiceView) {
			return this.dto.businessEvents?.map(s => s.name) ?? undefined;
		}
		return undefined;
	}

	getLifeEvents(): MultiLanguage[] | undefined {
		if (this.dto instanceof PublicServiceView) {
			return this.dto.lifeEvents?.map(s => s.name) ?? undefined;
		}
		return undefined;
	}

	getThemeEntries(): VocabularyEntry[] | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
			case DataService:
			case ConceptView:
			case MappingTableModel:
				return (this.dto as Dataset | DataService | ConceptView | MappingTableModel).themes;
			case PublicServiceView:
				return (this.dto as PublicServiceView).thematicAreas;
			default:
				return undefined;
		}
	}

	getBusinessEventEntries(): VocabularyEntry[] | undefined {
		if (this.dto instanceof PublicServiceView) {
			return this.dto.businessEvents;
		}
		return undefined;
	}

	getLifeEventEntries(): VocabularyEntry[] | undefined {
		if (this.dto instanceof PublicServiceView) {
			return this.dto.lifeEvents;
		}
		return undefined;
	}

	getThemeSearchUrl(code: string): string {
		return this.getCatalogSearchUrl('themes', code);
	}

	getBusinessEventSearchUrl(code: string): string {
		return this.getCatalogSearchUrl('businessEvents', code);
	}

	getLifeEventSearchUrl(code: string): string {
		return this.getCatalogSearchUrl('lifeEvents', code);
	}

	getAccessRightsSearchUrl(code: string): string {
		return this.getCatalogSearchUrl('accessRights', code);
	}

	openAccessRightsUrl(code: string): void {
		void this.router.navigate([`/${this.currentLanguage}/catalog/all`], {queryParams: {accessRights: code}});
	}

	private getCatalogSearchUrl(param: string, code: string): string {
		return `/${this.currentLanguage}/catalog/all?${param}=${code}`;
	}

	getSpatialCH(): string | undefined {
		if (this.dto instanceof PublicServiceView) {
			return this.dto.spatialCH
				? this.arrayToString.transform(FormatFunctions.getTranslatedVocabularyEntriesWithCode(this.dto.spatialCH, this.currentLanguage), ', ')
				: undefined;
		}
		return undefined;
	}

	getSpatialCoverage(): string[] | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
				return (this.dto as Dataset).spatialCoverages;
			case PublicServiceView:
				return (this.dto as PublicServiceView).spatial;
			default:
				return undefined;
		}
	}

	getTemporalCoverage(): string[] | undefined {
		if (this.dto instanceof Dataset) {
			return this.dto.temporalCoverage?.map(e => `${FormatFunctions.getFormattedDate(e.start)} - ${FormatFunctions.getFormattedDate(e.end)}`);
		}
		return undefined;
	}

	getFrequency(): MultiLanguage | undefined {
		if (this.dto instanceof Dataset) {
			return this.dto.frequency?.name ?? undefined;
		}
		return undefined;
	}

	getRetentionPeriod(): string | undefined {
		if (this.dto instanceof Dataset) {
			return this.dto.retentionPeriod ? FormatFunctions.getFormattedDate(this.dto.retentionPeriod) : undefined;
		}
		return undefined;
	}

	getRetentionPeriodComplement(): MultiLanguage | undefined {
		if (this.dto instanceof Dataset) {
			return this.dto.retentionPeriodDescription ?? undefined;
		}
		return undefined;
	}

	getIsReferencedBy(): Resource[] | undefined {
		if (this.dto instanceof Dataset) {
			return this.dto.isReferencedBy;
		}
		return undefined;
	}

	getQualifiedRelations(): QualifiedRelation[] | undefined {
		if (this.dto instanceof Dataset) {
			return this.dto.qualifiedRelation ?? undefined;
		}
		return undefined;
	}

	getRelatedResources(): Resource[] | undefined {
		if (this.dto instanceof Dataset) {
			return this.dto.relations;
		}
		return undefined;
	}

	getConformsTo(): Resource[] | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
			case DataService:
				return (this.dto as Dataset | DataService).conformTos ?? undefined;
			case ConceptView:
				return (this.dto as ConceptView).conformsTo ?? undefined;
			case MappingTableModel:
				return (this.dto as MappingTableModel).conformsTo.map(
					x =>
						new Resource({
							href: x.uri,
							label: x.label
						})
				);
			default:
				return undefined;
		}
	}

	getDocumentations(): Resource[] | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
			case DataService:
				return (this.dto as Dataset | DataService).documents ?? undefined;
			default:
				return undefined;
		}
	}

	getGeoIvId(): string | undefined {
		if (this.dto instanceof Dataset) {
			return this.dto.geoIvId
				? this.arrayToString.transform(FormatFunctions.getTranslatedVocabularyEntries(this.dto.geoIvId, this.currentLanguage), ', ')
				: undefined;
		}
		return undefined;
	}

	getProcessId(): string | undefined {
		if (this.dto instanceof Dataset) {
			return this.dto.processId ?? undefined;
		}
		return undefined;
	}

	getImages(): Resource[] | undefined {
		if (this.dto instanceof Dataset) {
			return this.dto.image;
		}
		return undefined;
	}

	getConceptType(): ConceptType | undefined {
		if (this.dto instanceof ConceptView) {
			return this.dto.conceptType;
		}
		return undefined;
	}

	getCodeListEntyValueType(): CodeListEntryValueTypeEnum | undefined {
		if (this.dto instanceof ConceptView) {
			return this.dto.codeListEntryValueType;
		}
		return undefined;
	}

	getCodeListEntyValueMaxLength(): number | undefined {
		if (this.dto instanceof ConceptView) {
			return typeof this.dto.codelistEntryValueMaxLength === 'number' ? this.dto.codelistEntryValueMaxLength : undefined;
		}
		return undefined;
	}

	getPattern(): string | undefined {
		if (this.dto instanceof ConceptView) {
			return this.dto.pattern ?? undefined;
		}
		return undefined;
	}

	getNbDezimal(): number | undefined {
		if (this.dto instanceof ConceptView) {
			return typeof this.dto.nbDecimal === 'number' ? this.dto.nbDecimal : undefined;
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
			return typeof this.dto.minValue === 'number' ? this.dto.minValue : undefined;
		}
		return undefined;
	}

	getMaxValue(): number | undefined {
		if (this.dto instanceof ConceptView) {
			return typeof this.dto.maxValue === 'number' ? this.dto.maxValue : undefined;
		}
		return undefined;
	}

	getMinLenght(): number | undefined {
		if (this.dto instanceof ConceptView) {
			return typeof this.dto.minLength === 'number' ? this.dto.minLength : undefined;
		}
		return undefined;
	}

	getMaxLenght(): number | undefined {
		if (this.dto instanceof ConceptView) {
			return typeof this.dto.maxLength === 'number' ? this.dto.maxLength : undefined;
		}
		return undefined;
	}

	getSourceName(): MultiLanguage | undefined {
		if (this.dto instanceof MappingTableModel) {
			return this.dto.source.name;
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
			return this.dto.source.uri;
		}
		return undefined;
	}

	getTargetName(): MultiLanguage | undefined {
		if (this.dto instanceof MappingTableModel) {
			return this.dto.target.name ?? undefined;
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
			return this.dto.target.uri;
		}
		return undefined;
	}

	getPreviousVersion(): DatasetVersionSummary | DataServiceVersionSummary | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
			case DataService:
				return (this.dto as Dataset | DataService).previousVersion;
			default:
				return undefined;
		}
	}

	getNextVersions(): DatasetVersionSummary[] | DataServiceVersionSummary[] | undefined {
		switch (this.dto?.constructor) {
			case Dataset:
			case DataService:
				return (this.dto as Dataset | DataService).nextVersions;
			default:
				return undefined;
		}
	}

	getServesDatasets(): IdLabel[] | undefined {
		if (this.dto instanceof DataService) {
			return this.dto.servesDatasets;
		}
		return undefined;
	}

	getIsDescribedAt(): IdLabel[] | undefined {
		if (this.dto instanceof PublicServiceView) {
			return this.dto.isDescribedAt;
		}
		return undefined;
	}

	getRelations(): IdLabel[] | undefined {
		if (this.dto instanceof PublicServiceView) {
			return this.dto.relation;
		}
		return undefined;
	}

	getRequires(): IdLabel[] | undefined {
		if (this.dto instanceof PublicServiceView) {
			return this.dto.requires;
		}
		return undefined;
	}

	getLinkedIdentiferOrId(item: DatasetVersionSummary | DataServiceVersionSummary | undefined): string {
		switch (item?.constructor) {
			case DatasetVersionSummary:
				return (item as DatasetVersionSummary).identifiers[0];
			case DataServiceVersionSummary:
				return (item as DataServiceVersionSummary).id;
			default:
				return undefined;
		}
	}

	private showRelations(x: ObNavTreeItemModel): boolean {
		return this.isRelationSectionVisible() ? x.id === Section.Relations : false;
	}

	private showVersions(x: ObNavTreeItemModel): boolean {
		return this.isVersionSectionVisible() ? x.id === Section.Versions : false;
	}

	private showProperties(x: ObNavTreeItemModel): boolean {
		return this.isPropertiesSectionVisible() ? x.id === Section.Properties : false;
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

	private isPropertiesSectionVisible(): boolean {
		if (
			this.getContactPoints() ||
			this.getItems(this.getLandingPages()) ||
			this.getItems(this.getEndpointUrls()) ||
			this.getItems(this.getEndpointUrls()) ||
			this.getItems(this.getEndpointDescriptions()) ||
			this.getLicense() ||
			this.getItems(this.getQualifiedAttributions()) ||
			this.getQualifiedAttributionComplement() ||
			this.getPublicationDate() ||
			this.getModifiedDate() ||
			this.getAccessRights() ||
			this.getConfidentiality() ||
			this.getLanguages() ||
			this.getThemes() ||
			this.getItems(this.getKeyWords()) ||
			this.getItems(this.getSectors()) ||
			this.getItems(this.getBusinessEvents()) ||
			this.getItems(this.getLifeEvents()) ||
			this.getSpatialCH() ||
			this.getSpatialCoverage() ||
			this.getTemporalCoverage() ||
			this.getFrequency() ||
			this.getRetentionPeriod() ||
			this.getRetentionPeriodComplement() ||
			this.getItems(this.getIsReferencedBy()) ||
			this.getItems(this.getQualifiedRelations()) ||
			this.getItems(this.getRelatedResources()) ||
			this.getItems(this.getConformsTo()) ||
			this.getItems(this.getDocumentations()) ||
			this.getGeoIvId() ||
			this.getProcessId() ||
			this.getItems(this.getImages()) ||
			this.getConceptType() ||
			this.getCodeListEntyValueType() ||
			this.getCodeListEntyValueMaxLength() ||
			this.getPattern() ||
			this.getNbDezimal() ||
			this.getMeasurementUnit() ||
			this.getMinValue() ||
			this.getMaxValue() ||
			this.getMinLenght() ||
			this.getMaxLenght() ||
			this.getSourceUri() ||
			this.getTargetUri()
		) {
			return true;
		}
		return false;
	}

	private isVersionSectionVisible(): boolean {
		switch (this.dto?.constructor) {
			case Dataset:
				if (this.getPreviousVersion() || this.getItems(this.getNextVersions() as DatasetVersionSummary[] | undefined)) {
					return true;
				}
				return false;
			case DataService:
				if (this.getPreviousVersion() || this.getItems(this.getNextVersions() as DataServiceVersionSummary[] | undefined)) {
					return true;
				}
				return false;
			case ConceptView:
				return true;
			default:
				return false;
		}
	}

	private isRelationSectionVisible(): boolean {
		switch (this.dto?.constructor) {
			case Dataset:
				return this.hasIsServedBy;
			case DataService:
				if (this.getItems(this.getServesDatasets())) {
					return true;
				}
				return false;
			case PublicServiceView:
				if (this.getItems(this.getIsDescribedAt()) || this.getItems(this.getRelations()) || this.getItems(this.getRequires())) {
					return true;
				}
				return false;
			case ConceptView:
				return (this.conceptReferencesCount ?? 0) > 0 || this.hasMappingTables;
			default:
				return false;
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

	private getVersionFromRegisterUri(uri: string | undefined): string | undefined {
		return uri ? extractIriVersion(uri) : undefined;
	}

	private showLineage(x: ObNavTreeItemModel): boolean {
		return this.isLineageSectionVisible() ? x.id === Section.Lineage : false;
	}

	private isLineageSectionVisible(): boolean {
		return this.getConceptReplaces().length > 0 || this.getConceptIsReplacedBy().length > 0;
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

	ngOnDestroy(): void {
		this.unsubscribe$.next(undefined);
		this.unsubscribe$.complete();
	}
}
