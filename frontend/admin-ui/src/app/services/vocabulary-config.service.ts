import {inject, Injectable} from '@angular/core';
import {ConceptViewClient, MultiLanguage, VocabularyClient, VocabularyConfigModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {catchError, map, Observable, of, shareReplay, switchMap} from 'rxjs';
import {buildConceptIri} from '../shared/helper/iri-helpers';

@Injectable({providedIn: 'root'})
export class VocabularyConfigService {
	private readonly vocabularyClient = inject(VocabularyClient);
	private readonly conceptViewClient = inject(ConceptViewClient);
	private readonly namesByIdentifier = new Map<string, Observable<MultiLanguage | undefined>>();

	private readonly configs$: Observable<Map<string, VocabularyConfigModel>> = this.vocabularyClient
		.getConfigurations()
		.pipe(
			map(response => new Map(response.result.filter(c => c.vocabularyIdentifier != null).map(c => [c.vocabularyIdentifier!, c]))),
			catchError(() => of(new Map<string, VocabularyConfigModel>())),
			shareReplay(1)
		);

	resolveIris(ids: string[]): Observable<Record<string, string | undefined>> {
		return this.configs$.pipe(
			map(configs => {
				const result: Record<string, string | undefined> = {};
				for (const id of ids) {
					const config = configs.get(id);
					result[id] = config
						? buildConceptIri(config.conceptIdentifier!, config.conceptVersion!)
						: undefined;
				}
				return result;
			})
		);
	}

	resolveConceptPageIris(ids: string[]): Observable<Record<string, string | undefined>> {
		return this.configs$.pipe(
			map(configs => {
				const result: Record<string, string | undefined> = {};
				for (const id of ids) {
					const config = configs.get(id);
					result[id] = config ? buildConceptIri(config.conceptIdentifier!, config.conceptVersion!) : undefined;
				}
				return result;
			})
		);
	}

	nameOf(vocabularyIdentifier: string): Observable<MultiLanguage | undefined> {
		let name$ = this.namesByIdentifier.get(vocabularyIdentifier);

		if (!name$) {
			name$ = this.configs$.pipe(
				switchMap(configs => {
					const config = configs.get(vocabularyIdentifier);

					return config?.conceptIdentifier
						? this.conceptViewClient
								.getByConceptIdentifierAndPublisherIdentifierAndVersionAndPublicationLevelAndRegistrationStatusAndPageAndPageSize(
									config.conceptIdentifier,
									undefined,
									config.conceptVersion,
									undefined,
									undefined,
									1,
									1
								)
								.pipe(map(response => response.result?.[0]?.name))
						: of(undefined);
				}),
				catchError(() => of(undefined)),
				shareReplay(1)
			);

			this.namesByIdentifier.set(vocabularyIdentifier, name$);
		}

		return name$;
	}
}
