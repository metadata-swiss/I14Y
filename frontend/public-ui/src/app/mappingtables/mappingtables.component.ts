import {Component, OnInit, OnDestroy, inject} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {SearchEngineOptimizationService} from '../shared/services/search-engine-optimization/search-engine-optimization.service';
import {DataFormat, MappingTableModel, MappingTablesClient} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ViewType} from '../shared/templates/viewtype';
import {TranslateService, LangChangeEvent} from '@ngx-translate/core';
import {catchError, of, Subject, takeUntil} from 'rxjs';
import {FallbackPipe} from '../shared/fallback/fallback.pipe';
import {MappingTableService} from './services/mappingtable.service';
import { ObHttpApiInterceptorEvents, ObNotificationService } from '@oblique/oblique';
import { HttpErrorResponse } from '@angular/common/http';
import { MessageHelperFunctions } from '../shared/message-helper-functions';

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
	private readonly notification = inject(ObNotificationService);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);
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

	exportMappingTable(formatSelected: DataFormat) {
		const skippedErrorNotifications = 1;
		this.obHttpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(skippedErrorNotifications);
		this.mappingTableClient
			.getExportByIdAndFormat(this.mappingTableId, formatSelected)
			.pipe(
				catchError((error: HttpErrorResponse) => {
					this.notification.error(MessageHelperFunctions.getExportErrorMessage(error));
					return of();
				})
			)
			.subscribe(response => {
				const a = document.createElement('a');
				const objectUrl = URL.createObjectURL(response.result.data);
				// eslint-disable-next-line max-len
				const fileName = `MappingTable_${this.mappingTable?.identifiers![0]}${this.mappingTable.version ? '-' + this.mappingTable.version : ''}.${formatSelected.toLocaleLowerCase()}`;

				a.href = objectUrl;
				a.download = response.result.fileName ?? fileName;
				a.click();

				URL.revokeObjectURL(objectUrl);
				a.remove();
			});
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
