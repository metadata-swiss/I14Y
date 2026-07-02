import {
	DataServiceClient,
	DataServiceModel,
	DataServicesClient,
	PublicationLevelInfoModel,
	RegistrationStatusInfoModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {inject, Injectable} from '@angular/core';
import {Observable} from 'rxjs/internal/Observable';
import {ReplaySubject, Subject} from 'rxjs';

@Injectable()
export class DataserviceService {
	readonly data$: Observable<DataServiceModel>;
	readonly publicationLevelInfo$: Observable<PublicationLevelInfoModel>;
	readonly registrationStatusInfo$: Observable<RegistrationStatusInfoModel>;
	private last: DataServiceModel | undefined;

	private readonly data: Subject<DataServiceModel> = new ReplaySubject<DataServiceModel>(1);
	private readonly publicationLevelInfo: Subject<PublicationLevelInfoModel> = new ReplaySubject<PublicationLevelInfoModel>(1);
	private readonly registrationStatusInfo: Subject<RegistrationStatusInfoModel> = new ReplaySubject<RegistrationStatusInfoModel>(1);

	private readonly dataServiceClient = inject(DataServicesClient);
	private readonly dataserviceClient = inject(DataServiceClient);

	constructor() {
		this.data$ = this.data.asObservable();
		this.publicationLevelInfo$ = this.publicationLevelInfo.asObservable();
		this.registrationStatusInfo$ = this.registrationStatusInfo.asObservable();
	}

	load(id: string, force: boolean = false) {
		if (force || !this.last || this.last.id !== id) {
			this.dataServiceClient.getById(id).subscribe(response => {
				this.last = response.result;
				this.data.next(response.result);
			});

			this.dataserviceClient.getPublicationLevelById(id).subscribe(response => this.publicationLevelInfo.next(response.result));
			this.dataserviceClient.getRegistrationStatusById(id).subscribe(response => this.registrationStatusInfo.next(response.result));
		}
	}
}
