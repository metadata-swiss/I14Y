import {inject, Injectable} from '@angular/core';
import {DatasetInputClient, SchemaClass, SchemaProperty} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {EMPTY, Observable} from 'rxjs';

/**
 * Centralised write operations (create / update / delete) for classes and
 * properties inside a dataset's linked data model. Keeps HTTP calls out of
 * dumb UI components so both the sidebar (delete) and the edit form (save)
 * can share the same client and behaviour.
 */
@Injectable({providedIn: 'root'})
export class LinkedDataModelWriteService {
	private readonly datasetInputClient = inject(DatasetInputClient);

	saveProperty(datasetId: string, classUri: string, property: SchemaProperty, isCreationMode: boolean): Observable<unknown> {
		if (!datasetId || !classUri) {
			return EMPTY;
		}
		return isCreationMode
			? this.datasetInputClient.postModelPropertyByIdAndClassUriAndBody(datasetId, classUri, property)
			: this.datasetInputClient.putModelPropertyByIdAndClassUriAndBody(datasetId, classUri, property);
	}

	saveClass(datasetId: string, schemaClass: SchemaClass, isCreationMode: boolean): Observable<unknown> {
		if (!datasetId) {
			return EMPTY;
		}
		return isCreationMode
			? this.datasetInputClient.postModelClassByIdAndBody(datasetId, schemaClass)
			: this.datasetInputClient.putModelClassByIdAndBody(datasetId, schemaClass);
	}

	deleteProperty(datasetId: string, propertyUri: string): Observable<unknown> {
		if (!datasetId || !propertyUri) {
			return EMPTY;
		}
		return this.datasetInputClient.deleteModelPropertyByIdAndPropertyUri(datasetId, propertyUri);
	}
}
