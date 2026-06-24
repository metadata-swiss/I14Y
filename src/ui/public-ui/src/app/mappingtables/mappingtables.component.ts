import {Component, OnInit, OnDestroy, inject} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {SearchEngineOptimizationService} from '../shared/services/search-engine-optimization/search-engine-optimization.service';
import {MappingTableModel, MappingTablesClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ViewType} from '../shared/templates/viewtype';
import {TranslateService, LangChangeEvent} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';
import {FallbackPipe} from '../shared/fallback/fallback.pipe';
import {MappingTableService} from './services/mappingtable.service';

@Component({
	selector: 'app-mappingtables',
	templateUrl: './mappingtables.component.html',
	styleUrls: [],
	standalone: false
})
export class MappingTableComponent implements OnInit, OnDestroy {
	mappingTableId: string;
	mappingTable: MappingTableModel;
	registrationStatus: string;
	tabs: string[] = ['description', 'content'];
	currentLanguage: string;
	readonly viewTypeEnum = ViewType;

	private readonly unsubscribe$ = new Subject();

	private readonly mappingTableClient = inject(MappingTablesClient);
	private readonly mappingTableServuice = inject(MappingTableService);
	private readonly fallback = inject(FallbackPipe);
	private readonly route = inject(ActivatedRoute);
	private readonly searchEngineOptimizationService = inject(SearchEngineOptimizationService);
	private readonly translate = inject(TranslateService);

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
		this.route.params.subscribe(params => {
			this.mappingTableId = params.mappingTableId;

			this.loadDataService(params.mappingTableId);
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	public loadDataService(id: string) {
		this.mappingTableClient.getById(id).subscribe(response => {
			this.mappingTable = response.result;

			this.mappingTableServuice.setMappingTable(response.result);

			this.registrationStatus = this.mappingTable.registrationStatus.toString();
			this.updateMetaData();
		});
	}

	private updateMetaData() {
		this.searchEngineOptimizationService.UpdateMetaTitle(this.fallback.transform(this.mappingTable?.name, this.currentLanguage));
		this.searchEngineOptimizationService.UpdateMetaDescrition(this.fallback.transform(this.mappingTable?.description, this.currentLanguage));
	}
}
