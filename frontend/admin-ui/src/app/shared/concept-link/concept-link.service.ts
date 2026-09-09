import {inject, Injectable} from '@angular/core';
import {CatalogClient, CatalogEntry, SearchResourceType} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';

/**
 * Resolves a concept IRI (identifier + version) to its internal catalog entry, exposing the internal
 * GUID (`CatalogEntry.id`) needed to build an in-app link to `/catalog/concepts/{id}`.
 */
@Injectable({providedIn: 'root'})
export class ConceptLinkService {
	private readonly catalogClient = inject(CatalogClient);

	resolveConceptEntry(identifier: string, version: string): Observable<CatalogEntry | undefined> {
		return this.catalogClient
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
			.pipe(map(response => response.result.find(e => e.identifiers?.[0] === identifier && e.version === version)));
	}
}
