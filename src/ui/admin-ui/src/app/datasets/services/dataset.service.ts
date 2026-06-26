import {DatasetClient, DatasetsClient, DcatDatasetModel, PublicationLevelInfoModel, RegistrationStatusInfoModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {inject, Injectable} from '@angular/core';
import {Observable} from 'rxjs/internal/Observable';
import {ReplaySubject, Subject} from 'rxjs';

@Injectable()
export class DatasetService {
	readonly data$: Observable<DcatDatasetModel>;
	readonly publicationLevelInfo$: Observable<PublicationLevelInfoModel>;
	readonly registrationStatusInfo$: Observable<RegistrationStatusInfoModel>;
	private last: DcatDatasetModel | undefined;

	private readonly data: Subject<DcatDatasetModel> = new ReplaySubject<DcatDatasetModel>(1);
	private readonly publicationLevelInfo: Subject<PublicationLevelInfoModel> = new ReplaySubject<PublicationLevelInfoModel>(1);
	private readonly status: Subject<RegistrationStatusInfoModel> = new ReplaySubject<RegistrationStatusInfoModel>(1);

	private readonly datasetClient = inject(DatasetClient);
	private readonly datasetsClient = inject(DatasetsClient);

	constructor() {
		this.data$ = this.data.asObservable();
		this.publicationLevelInfo$ = this.publicationLevelInfo.asObservable();
		this.registrationStatusInfo$ = this.status.asObservable();
	}

	load(id: string, force: boolean = false) {
		if (force || !this.last || this.last.id !== id) {
			this.datasetsClient.getById(id).subscribe(response => {
				this.last = response.result;
				this.data.next(response.result);
			});

			this.datasetClient.getPublicationLevelById(id).subscribe(response => this.publicationLevelInfo.next(response.result));
			this.datasetClient.getRegistrationStatusById(id).subscribe(response => this.status.next(response.result));
		}
	}
}
