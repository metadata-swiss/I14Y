import {inject, Injectable} from '@angular/core';
import {MultiLanguage, VocabularyClient, VocabularyEntry} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {catchError, forkJoin, map, Observable, of, shareReplay, switchMap} from 'rxjs';
import {VOCAB_ID_EU_DATA_THEME, VOCAB_ID_THEME_TAXONOMY} from '../app-constants';
import {VocabularyConfigService} from './vocabulary-config.service';

export interface ThemeTaxonomy {
	identifier: string;
	name?: MultiLanguage;
	entries: VocabularyEntry[];
}

@Injectable({providedIn: 'root'})
export class ThemeTaxonomiesService {
	readonly taxonomies$: Observable<ThemeTaxonomy[]>;

	private readonly vocabularyClient = inject(VocabularyClient);
	private readonly vocabularyConfigService = inject(VocabularyConfigService);

	private readonly taxonomiesByIdentifier = new Map<string, Observable<ThemeTaxonomy>>();

	constructor() {
		this.taxonomies$ = this.vocabularyClient.getByIdentifier(VOCAB_ID_THEME_TAXONOMY).pipe(
			map(response => (response.result ?? []).filter(entry => !!entry.code)),
			switchMap(registryEntries => (registryEntries.length === 0 ? of([]) : forkJoin(registryEntries.map(entry => this.loadTaxonomy(entry))))),
			map(taxonomies => this.euFirst(taxonomies.filter(taxonomy => taxonomy.entries.length > 0))),
			catchError(() => of([] as ThemeTaxonomy[])),
			shareReplay(1)
		);
	}

	/** Loads vocabularies absent from the theme registry so existing selections remain editable. */
	taxonomyFor(identifier: string): Observable<ThemeTaxonomy> {
		let taxonomy$ = this.taxonomiesByIdentifier.get(identifier);

		if (!taxonomy$) {
			taxonomy$ = forkJoin({
				entries: this.vocabularyClient.getByIdentifier(identifier).pipe(
					map(response => response.result ?? []),
					catchError(() => of([] as VocabularyEntry[]))
				),
				name: this.nameOf(identifier)
			}).pipe(
				map(({entries, name}) => ({identifier, name, entries})),
				shareReplay(1)
			);

			this.taxonomiesByIdentifier.set(identifier, taxonomy$);
		}

		return taxonomy$;
	}

	nameOf(vocabularyIdentifier: string): Observable<MultiLanguage | undefined> {
		return this.vocabularyConfigService.nameOf(vocabularyIdentifier);
	}

	private loadTaxonomy(registryEntry: VocabularyEntry): Observable<ThemeTaxonomy> {
		const identifier = registryEntry.code!;

		return this.vocabularyClient.getByIdentifier(identifier).pipe(
			map(response => ({identifier, name: registryEntry.name, entries: response.result ?? []})),
			// A failed vocabulary request must not block the other taxonomies.
			catchError(() => of({identifier, name: registryEntry.name, entries: [] as VocabularyEntry[]}))
		);
	}

	private euFirst(taxonomies: ThemeTaxonomy[]): ThemeTaxonomy[] {
		return [...taxonomies].sort((a, b) => Number(b.identifier === VOCAB_ID_EU_DATA_THEME) - Number(a.identifier === VOCAB_ID_EU_DATA_THEME));
	}
}
