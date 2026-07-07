import {Component, inject, Input, OnDestroy} from '@angular/core';
import {CatalogClient, SearchResourceType, MultiLanguageModel, SchemaClass, SchemaProperty} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';
import {UriHelper} from '../../../../../shared/helper/uri-helper';
import {extractIriIdentifier, extractIriVersion, isLocalIri} from '../../../../../shared/iri-helpers';
@Component({
	selector: 'app-structure-detail-view',
	templateUrl: './structure-detail-view.component.html',
	styleUrl: './structure-detail-view.component.scss',
	standalone: false
})
export class StructureDetailViewComponent implements OnDestroy {
	@Input() selectedDto: SchemaClass | SchemaProperty | undefined;

	TRANSLATION_PREFIX = 'i18n.datasets.linkeddatamodel.sidebar';
	currentLanguage: string;
	showAllAllowedValues = false;
	name: MultiLanguageModel | undefined;
	uri: string | undefined;
	description: MultiLanguageModel | undefined;
	identifier: string | undefined;
	conformsToUri: string | undefined;
	conformsToTitleRaw: MultiLanguageModel | undefined;
	conformsToVersion: string | undefined;
	dataType: string | undefined;
	pattern: string | undefined;
	unit: string | undefined;
	minCount: number | undefined;
	maxCount: number | undefined;
	minLength: number | undefined;
	maxLength: number | undefined;
	shortName: string | undefined;
	allowedValues: string[] | undefined;

	private readonly unsubscribe$ = new Subject<void>();
	private readonly translate = inject(TranslateService);
	private readonly catalogClient = inject(CatalogClient);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	get displayedAllowedValues(): string[] | undefined {
		if (this.allowedValues && this.allowedValues.length > 0) {
			return this.showAllAllowedValues ? this.allowedValues : this.allowedValues.slice(0, 10);
		}
		return undefined;
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnDestroy(): void {
		this.unsubscribe$.next();
		this.unsubscribe$.complete();
	}

	ngOnChanges(): void {
		if (this.selectedDto) {
			if ('path' in this.selectedDto) {
				this.convertPropertyToValueSideBar(this.selectedDto as SchemaProperty);
			} else {
				this.convertClassToValueSideBar(this.selectedDto as SchemaClass);
			}
		}
	}

	convertClassToValueSideBar(schemaClass: SchemaClass) {
		this.clean();
		this.identifier = schemaClass?.identifier;
		this.uri = schemaClass?.uriComplete;
		this.name = schemaClass?.label;
		this.description = schemaClass?.description;
		this.shortName = UriHelper.GetUriFragment(schemaClass?.uriComplete);
	}

	convertPropertyToValueSideBar(schemaProperty: SchemaProperty) {
		this.clean();
		this.identifier = schemaProperty?.identifier;
		this.uri = schemaProperty?.uriComplete;
		this.name = schemaProperty?.label;
		this.description = schemaProperty?.description;
		this.dataType = schemaProperty?.dataType;
		this.pattern = schemaProperty?.pattern;
		this.unit = UriHelper.RemoveHash(schemaProperty?.unit);
		this.minCount = schemaProperty?.minCardinality;
		this.maxCount = schemaProperty?.maxCardinality;
		this.minLength = schemaProperty?.minLength;
		this.maxLength = schemaProperty?.maxLength;
		this.shortName = UriHelper.GetUriFragment(schemaProperty?.path);
		this.allowedValues = schemaProperty.allowedValues;
		if (schemaProperty?.conformsTo) {
			this.resolveConformsTo(schemaProperty.conformsTo);
		}
	}

	private resolveConformsTo(iri: string): void {
		this.conformsToUri = iri;
		if (!isLocalIri(iri)) return;

		const identifier = extractIriIdentifier(iri);
		const version = extractIriVersion(iri);
		if (!identifier || !version) return;

		this.catalogClient
			.getSearchByQueryAndAccessRightsAndConceptValueTypesAndFormatsAndBusinessEventsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypesAndPageAndPageSize(
				identifier,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				[SearchResourceType.Concept],
				1,
				10
			)
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(response => {
				if (this.conformsToUri !== iri) return;
				const match = response.result.find(e => e.identifiers?.[0] === identifier && e.version === version);
				this.conformsToTitleRaw = match?.title;
				this.conformsToVersion = match ? (match.version ?? version) : undefined;
			});
	}

	clean() {
		this.uri = undefined;
		this.name = undefined;
		this.description = undefined;
		this.identifier = undefined;
		this.conformsToUri = undefined;
		this.conformsToTitleRaw = undefined;
		this.conformsToVersion = undefined;
		this.dataType = undefined;
		this.pattern = undefined;
		this.unit = undefined;
		this.minCount = undefined;
		this.maxCount = undefined;
		this.minLength = undefined;
		this.maxLength = undefined;
		this.allowedValues = undefined;
	}
}
