import {Component, inject, Input, OnChanges, OnDestroy, OnInit} from '@angular/core';
import {ConceptViewClient, MultiLanguage, SwaggerResponse} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Observable, Subject, takeUntil} from 'rxjs';
import {isLocalIri} from '../../../../../shared/helper/iri-helpers';

@Component({
	selector: 'app-structure-conforms-to-link',
	templateUrl: './structure-conforms-to-link.component.html',
	styleUrl: './structure-conforms-to-link.component.scss',
	standalone: false
})
export class StructureConformsToLinkComponent implements OnChanges, OnInit, OnDestroy {
	@Input({required: true}) uri!: string;

	linkText: MultiLanguage | undefined;
	version: string | undefined;
	isClickable = false;
	isPublic = false;
	currentLanguage: string;

	private readonly unsubscribe$ = new Subject<void>();
	private readonly conceptViewClient = inject(ConceptViewClient);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnChanges(): void {
		this.linkText = undefined;
		this.version = undefined;
		this.isClickable = false;
		this.isPublic = false;
		if (this.uri?.length > 0) {
			this.checkClickable(this.uri);
		}
	}

	ngOnDestroy(): void {
		this.unsubscribe$.next();
		this.unsubscribe$.complete();
	}

	private checkClickable(url: string) {
		const isI14Y = url.includes('i14y') && url.includes('admin.ch');
		const registerIRI = url.startsWith('https://register.ld.admin.ch/i14y/');
		if (isI14Y) {
			if (registerIRI || isLocalIri(url)) {
				const conceptIdentifier = url.match(/\/concept\/([^/]+)/)?.[1];
				const version = url.match(/\/version\/([^/]+)/)?.[1];
				this.version = version;
				this.handleConceptRequest(this.getConceptByIdentifierAndVersion(conceptIdentifier, version), url);
			} else {
				const conceptId = url.match(/concepts\/([0-9a-fA-F-]{36})/)?.[1];
				this.handleConceptRequest(this.conceptViewClient.getById(conceptId!), url);
			}
		} else {
			this.isClickable = true;
			this.isPublic = false;
		}
	}

	private setLinkPublicConcept(conceptName: MultiLanguage | undefined) {
		this.linkText = conceptName;
		this.isPublic = true;
		this.isClickable = true;
	}

	private setLinkInternalConcept() {
		this.linkText = undefined;
		this.isPublic = false;
		this.isClickable = false;
	}

	private handleConceptRequest<T extends {name?: MultiLanguage; version?: string}>(request$: Observable<SwaggerResponse<T | T[]>>, iri: string) {
		request$.pipe(takeUntil(this.unsubscribe$)).subscribe({
			next: response => {
				if (this.uri !== iri) return;
				const concept = Array.isArray(response.result) ? response.result[0] : response.result;

				if (!concept) {
					this.setLinkInternalConcept();
					return;
				}

				if (!this.version && concept.version) {
					this.version = concept.version;
				}
				this.setLinkPublicConcept(concept.name);
			},
			error: () => {
				if (this.uri !== iri) return;
				this.setLinkInternalConcept();
			}
		});
	}

	private getConceptByIdentifierAndVersion(identifier?: string, version?: string) {
		return this.conceptViewClient.getByConceptIdentifierAndPublisherIdentifierAndVersionAndPublicationLevelAndRegistrationStatusAndPageAndPageSize(
			identifier,
			undefined,
			version,
			undefined,
			undefined,
			1,
			1
		);
	}
}
