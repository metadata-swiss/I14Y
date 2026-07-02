import {inject, Injectable} from '@angular/core';
import {Observable} from 'rxjs/internal/Observable';
import {ReplaySubject, Subject} from 'rxjs';
import {
	Dataset,
	PublicationLevelInfoModel,
	PublicServiceInputClient,
	PublicServiceModel,
	PublicServicesClient,
	PublicServiceView,
	PublicServiceViewClient,
	RegistrationStatusInfoModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Injectable()
export class PublicServiceService {
	readonly data$: Observable<PublicServiceModel>;
	readonly publicationLevelInfo$: Observable<PublicationLevelInfoModel>;
	readonly registrationStatusInfo$: Observable<RegistrationStatusInfoModel>;
	readonly isDescribedAt$: Observable<Dataset[]>;
	readonly relations$: Observable<PublicServiceView[]>;
	readonly requires$: Observable<PublicServiceView[]>;
	private last: PublicServiceModel | undefined;

	private readonly data: Subject<PublicServiceModel> = new ReplaySubject<PublicServiceModel>(1);
	private readonly publicationLevelInfo: Subject<PublicationLevelInfoModel> = new ReplaySubject<PublicationLevelInfoModel>(1);
	private readonly registrationStatusInfo: Subject<RegistrationStatusInfoModel> = new ReplaySubject<RegistrationStatusInfoModel>(1);
	private readonly isDescribedAt: Subject<Dataset[]> = new ReplaySubject<Dataset[]>(1);
	private readonly relations: Subject<PublicServiceView[]> = new ReplaySubject<PublicServiceView[]>(1);
	private readonly requires: Subject<PublicServiceView[]> = new ReplaySubject<PublicServiceView[]>(1);

	private readonly publicserviceInputClient = inject(PublicServiceInputClient);
	private readonly publicServicesClient = inject(PublicServicesClient);
	private readonly publicserviceViewClient = inject(PublicServiceViewClient);

	constructor() {
		this.data$ = this.data.asObservable();
		this.publicationLevelInfo$ = this.publicationLevelInfo.asObservable();
		this.registrationStatusInfo$ = this.registrationStatusInfo.asObservable();
		this.isDescribedAt$ = this.isDescribedAt.asObservable();
		this.relations$ = this.relations.asObservable();
		this.requires$ = this.requires.asObservable();
	}

	load(id: string, force: boolean = false) {
		if (force || !this.last || this.last.id !== id) {
			this.publicServicesClient.getById(id).subscribe(response => {
				this.last = response.result;
				this.data.next(response.result);
			});

			this.publicserviceViewClient.getPublicationLevelById(id).subscribe(response => this.publicationLevelInfo.next(response.result));
			this.publicserviceViewClient.getRegistrationStatusById(id).subscribe(response => this.registrationStatusInfo.next(response.result));
			this.publicserviceInputClient.getIsDescribedAtById(id).subscribe(response => this.isDescribedAt.next(response.result));
			this.publicserviceInputClient.getRelationById(id).subscribe(response => this.relations.next(response.result));
			this.publicserviceInputClient.getRequiresById(id).subscribe(response => this.requires.next(response.result));
		}
	}
}
