import {MappingTableModel, MappingTablesClient, PublicationLevelInfoModel, RegistrationStatusInfoModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {inject, Injectable} from '@angular/core';
import {Observable} from 'rxjs/internal/Observable';
import {ReplaySubject, Subject} from 'rxjs';
import {PagedMappingRelationResult} from '../description/paged-mapping-relation-result';
import {SearchResultPagingInfo} from 'src/app/shared/searchResultPagingInfo';

@Injectable()
export class MappingTableService {
	readonly data$: Observable<MappingTableModel>;
	readonly pagedMappingRelationResult$: Observable<PagedMappingRelationResult | undefined>;
	readonly publicationLevelInfo$: Observable<PublicationLevelInfoModel>;
	readonly registrationStatusInfo$: Observable<RegistrationStatusInfoModel>;

	private last: MappingTableModel | undefined;
	private readonly defaultPage = 1;
	private readonly defaultPageSize = 10;

	private readonly data: Subject<MappingTableModel> = new ReplaySubject<MappingTableModel>(1);
	private readonly pagedMappingRelationResult: Subject<PagedMappingRelationResult | undefined> = new ReplaySubject<PagedMappingRelationResult | undefined>();
	private readonly publicationLevelInfo: Subject<PublicationLevelInfoModel> = new ReplaySubject<PublicationLevelInfoModel>(1);
	private readonly registrationStatusInfo: Subject<RegistrationStatusInfoModel> = new ReplaySubject<RegistrationStatusInfoModel>(1);

	private readonly mappingTableClient = inject(MappingTablesClient);

	constructor() {
		this.data$ = this.data.asObservable();
		this.pagedMappingRelationResult$ = this.pagedMappingRelationResult.asObservable();
		this.publicationLevelInfo$ = this.publicationLevelInfo.asObservable();
		this.registrationStatusInfo$ = this.registrationStatusInfo.asObservable();
	}

	load(id: string, force: boolean = false): void {
		if (force || !this.last || this.last.id !== id) {
			this.pagedMappingRelationResult.next(undefined);
			this.mappingTableClient.getById(id).subscribe(response => {
				this.last = response.result;
				this.data.next(response.result);
			});

			this.mappingTableClient.getPublicationLevelById(id).subscribe(response => this.publicationLevelInfo.next(response.result));
			this.mappingTableClient.getRegistrationStatusById(id).subscribe(response => this.registrationStatusInfo.next(response.result));
			this.mappingTableClient
				.getRelationsByIdAndPageAndPageSize(id, this.defaultPage, this.defaultPageSize)
				.subscribe(response =>
					this.pagedMappingRelationResult.next(new PagedMappingRelationResult(response.result, new SearchResultPagingInfo(response.headers)))
				);
		}
	}

	updateMappingRelations(id: string, page: number, pageSize: number): void {
		if (id === this.last?.id) {
			this.mappingTableClient
				.getRelationsByIdAndPageAndPageSize(id, page, pageSize)
				.subscribe(response =>
					this.pagedMappingRelationResult.next(new PagedMappingRelationResult(response.result, new SearchResultPagingInfo(response.headers)))
				);
		}
	}

	resetMappingRelations(): void {
		this.pagedMappingRelationResult.next(undefined);
	}
}
