import {Injectable} from '@angular/core';
import {Observable} from 'rxjs/internal/Observable';
import {ReplaySubject, Subject} from 'rxjs';
import {DataService} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Injectable()
export class DataServiceService {
	readonly dataService$: Observable<DataService>;

	private readonly dataService: Subject<DataService> = new ReplaySubject<DataService>(1);

	constructor() {
		this.dataService$ = this.dataService.asObservable();
	}

	setDataService(dataService: DataService) {
		this.dataService.next(dataService);
	}
}
