import {Injectable} from '@angular/core';
import {MappingTableModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, ReplaySubject, Subject} from 'rxjs';

@Injectable()
export class MappingTableService {
	readonly mappingtable$: Observable<MappingTableModel>;

	private readonly mappingtable: Subject<MappingTableModel> = new ReplaySubject<MappingTableModel>(1);

	constructor() {
		this.mappingtable$ = this.mappingtable.asObservable();
	}

	setMappingTable(mappingtable: MappingTableModel) {
		this.mappingtable.next(mappingtable);
	}
}
