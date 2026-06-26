import {inject, Injectable} from '@angular/core';
import {VocabularyClient, VocabularyConfigModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {catchError, map, Observable, of, shareReplay} from 'rxjs';
import {buildConceptIri, buildConceptPageIri} from '../shared/iri-helpers';

@Injectable({providedIn: 'root'})
export class VocabularyConfigService {
	private readonly vocabularyClient = inject(VocabularyClient);

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
}
