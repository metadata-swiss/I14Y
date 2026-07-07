import {Dataset} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs/internal/Observable';
import {ReplaySubject, Subject} from 'rxjs';

@Injectable()
export class DcatDatasetService {
	readonly dataset$: Observable<Dataset>;

	private readonly dataset: Subject<Dataset> = new ReplaySubject<Dataset>(1);

	constructor() {
		this.dataset$ = this.dataset.asObservable();
	}

	setDataset(dataset: Dataset) {
		this.dataset.next(dataset);
	}
}
