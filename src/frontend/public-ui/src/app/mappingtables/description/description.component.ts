import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {ViewType} from 'src/app/shared/templates/viewtype';
import {MappingTableModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {MappingTableService} from '../services/mappingtable.service';

@Component({
	selector: 'app-description',
	templateUrl: './description.component.html',
	styleUrls: ['./description.component.scss'],
	standalone: false
})
export class DescriptionComponent implements OnInit, OnDestroy {
	mappingTable: MappingTableModel;
	readonly viewTypeEnum = ViewType;

	private readonly unsubscribe$ = new Subject();

	private readonly mappingTableService = inject(MappingTableService);

	ngOnInit() {
		this.mappingTableService.mappingtable$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.mappingTable = x;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(null);
		this.unsubscribe$.complete();
	}
}
