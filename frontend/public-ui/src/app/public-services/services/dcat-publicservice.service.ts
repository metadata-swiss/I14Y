import {Injectable} from '@angular/core';
import {Observable} from 'rxjs/internal/Observable';
import {ReplaySubject, Subject} from 'rxjs';
import {PublicServiceView} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Injectable()
export class DcatPublicServiceService {
	readonly publicService$: Observable<PublicServiceView>;

	private readonly publicService: Subject<PublicServiceView> = new ReplaySubject<PublicServiceView>(1);

	constructor() {
		this.publicService$ = this.publicService.asObservable();
	}

	setPublicService(publicService: PublicServiceView) {
		this.publicService.next(publicService);
	}
}
