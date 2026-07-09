import {Component, inject, Input, OnChanges, OnDestroy, OnInit} from '@angular/core';
import {IMultiLanguage} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {merge, Subject, takeUntil} from 'rxjs';
import {extractIriIdentifier, extractIriVersion, isLocalIri} from '../iri-helpers';
import {ConceptLinkService} from './concept-link.service';

/**
 * Renders a link to a concept referenced by its IRI. When the IRI points to a local (i14y) concept,
 * the main link resolves to the internal admin page `/catalog/concepts/{guid}` (so it works for
 * unpublished concepts too) and a trailing external-link icon still opens the public IRI page.
 * Non-local IRIs (external registers) and unresolvable concepts fall back to a plain external link.
 */
@Component({
	selector: 'app-concept-link',
	templateUrl: './concept-link.component.html',
	styleUrl: './concept-link.component.scss',
	standalone: false
})
export class ConceptLinkComponent implements OnInit, OnChanges, OnDestroy {
	@Input() uri: string | undefined;
	@Input() name: IMultiLanguage | undefined;
	@Input() version: string | undefined;

	currentLanguage: string;
	conceptId: string | undefined;
	displayName: IMultiLanguage | undefined;
	displayVersion: string | undefined;
	loading = false;

	private readonly unsubscribe$ = new Subject<void>();
	private resolveCancel$ = new Subject<void>();
	private readonly translate = inject(TranslateService);
	private readonly conceptLinkService = inject(ConceptLinkService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnChanges(): void {
		this.reset();
		const uri = this.uri;
		if (!uri) return;

		this.displayName = this.name;
		this.displayVersion = this.version ?? extractIriVersion(uri);

		if (!isLocalIri(uri)) return;

		const identifier = extractIriIdentifier(uri);
		const version = extractIriVersion(uri);
		if (!identifier || !version) return;

		this.loading = true;
		this.conceptLinkService
			.resolveConceptEntry(identifier, version)
			.pipe(takeUntil(merge(this.unsubscribe$, this.resolveCancel$)))
			.subscribe({
				next: match => {
					if (this.uri !== uri) return;
					this.loading = false;
					if (match?.id) {
						this.conceptId = match.id;
						this.displayName = this.name ?? match.title;
						this.displayVersion = this.version ?? match.version ?? version;
					}
				},
				error: () => {
					if (this.uri !== uri) return;
					this.loading = false;
				}
			});
	}

	ngOnDestroy(): void {
		this.unsubscribe$.next();
		this.unsubscribe$.complete();
	}

	private reset(): void {
		this.resolveCancel$.next();
		this.conceptId = undefined;
		this.displayName = undefined;
		this.displayVersion = undefined;
		this.loading = false;
	}
}
