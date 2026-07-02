import {DatasetQualityInformationData} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs/internal/Observable';
import {ReplaySubject} from 'rxjs';

@Injectable()
export class DatasetQualityInformationDataService {
	readonly data$: Observable<DatasetQualityInformationData>;

	// prettier-ignore
	private readonly data: ReplaySubject<DatasetQualityInformationData> =
		new ReplaySubject<DatasetQualityInformationData>(1);

	constructor() {
		this.data$ = this.data.asObservable();
	}

	setData(data: DatasetQualityInformationData) {
		this.data.next(data);
	}
}
