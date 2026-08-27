import {Component, inject, OnInit, ViewChild} from '@angular/core';
import {AllowActionResourceType, AllowActionType, DatasetInputClient, FileParameter, LinkedDataFormat} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ActivatedRoute} from '@angular/router';
import {ObIUploadEvent, ObNavTreeItemModel, ObNotificationService} from '@oblique/oblique';
import {LinkedDataGraphComponent} from './linked-data-graph/linked-data-graph.component';
import {AllowActionService} from 'src/app/services/allow.action.service';
import {map, Observable, of, startWith, Subject, takeUntil} from 'rxjs';
import {INode} from './linked-data-entity';
import {TranslateService} from '@ngx-translate/core';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {MatAutocompleteSelectedEvent} from '@angular/material/autocomplete';
import {UriHelper} from 'src/app/shared/helper/uri-helper';

@Component({
	selector: 'app-linked-data-model',
	templateUrl: './linked-data-model.component.html',
	styleUrls: ['./linked-data-model.component.scss'],
	standalone: false
})
export class LinkedDataModelComponent implements OnInit {
	@ViewChild('graph') childGraph: LinkedDataGraphComponent | undefined;
	datasetId: string;
	acceptedFileExtension: string[] = ['.ttl', '.rdf'];
	createModel: boolean = false;
	modelExists: boolean = false;
	isGraphView: boolean = true;
	navItems: ObNavTreeItemModel[] = [];
	searchTerm: string | undefined;
	cannotEdit$: Observable<boolean> = of(true);
	searchNodesResults$: Observable<INode[]> | undefined;
	currentLanguage: string;

	private readonly allowActionService = inject(AllowActionService);
	private readonly datasetInputClient = inject(DatasetInputClient);
	private readonly notification = inject(ObNotificationService);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);
	private readonly unsubscribe$ = new Subject<void>();
	private readonly fallback = inject(FallbackPipe);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.datasetId = this.route.parent?.snapshot.params.id;
	}

	ngOnInit(): void {
		this.datasetInputClient.getModelExistsById(this.datasetId).subscribe(response => {
			this.modelExists = response.result;
			let format = Object.values(LinkedDataFormat);
			this.acceptedFileExtension = format.map(f => '.' + f.toLowerCase());
		});
		this.route.queryParams.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
			if (params['view'] === 'table') {
				this.setTableView();
			} else {
				this.setGraphView();
			}
		});

		this.allowActionService.load(this.datasetId, AllowActionResourceType.Dataset, true);
		this.cannotEdit$ = this.allowActionService.allowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.actionType === AllowActionType.Edit)?.value),
			startWith(true)
		);
	}

	ngOnDestroy() {
		this.unsubscribe$.complete();
	}

	uploadsLinkedDataModel(event: ObIUploadEvent): void {
		let files = event.files;
		if (files.length > 0) {
			let file = files[0] as File;
			let fileToUpload: FileParameter = {data: file, fileName: file!.name};
			this.datasetInputClient.postModelImportByIdAndBody(this.datasetId, fileToUpload).subscribe(
				_ => {
					this.modelExists = true;
					this.notification.success('i18n.notification.save_succeeded');
				},
				(error: {detail?: string}) => {
					// The generated client throws the problem details itself for declared statuses,
					// so `detail` carries the reason the backend rejected the file.
					this.notification.error(
						error?.detail
							? {message: 'i18n.notification.error_detail', messageParams: {error: error.detail}, sticky: true}
							: 'i18n.notification.save_error'
					);
				}
			);
		}
	}

	displayFn = (item: INode | null): string => {
		if (!item) return '';
		return this.fallback.transform(item.node.label, this.currentLanguage) ?? UriHelper.GetUriFragment(item.node.uriComplete) ?? '';
	};

	setTableView(): void {
		this.childGraph?.setSidebarState('NONE');
		this.isGraphView = false;
	}

	setGraphView(): void {
		this.isGraphView = true;
	}

	onSearch(): void {
		if (this.searchTerm && this.searchTerm.trim() !== '') {
			this.searchNodesResults$ = this.childGraph?.searchNodes(this.searchTerm);
		} else {
			this.searchNodesResults$ = of([]);
		}
	}

	onSelect(event: MatAutocompleteSelectedEvent): void {
		let node = event.option.value as INode;
		this.childGraph?.selectNode(node);
	}

	deleteLinkedDataModel(): void {
		this.datasetInputClient.deleteModelDeleteById(this.datasetId).subscribe(() => {
			this.notification.success('i18n.notification.deleted');
			this.modelExists = false;
			this.createModel = false;
		});
	}

	createLinkedDataModel() {
		this.createModel = true;
	}

	downloadLinkedDataModel(format: string): void {
		let linkedDataFormat: LinkedDataFormat;
		switch (format) {
			case '.ttl':
				linkedDataFormat = LinkedDataFormat.Ttl;
				break;
			case '.rdf':
				linkedDataFormat = LinkedDataFormat.Rdf;
				break;
			case '.jsonld':
				linkedDataFormat = LinkedDataFormat.JsonLd;
				break;
			default:
				this.notification.error('i18n.notification.download_format_error');
				return;
		}
		this.datasetInputClient.getModelExportByIdAndFormat(this.datasetId, linkedDataFormat).subscribe(async response => {
			let link = document.createElement('a');
			link.href = window.URL.createObjectURL(response.result.data);
			link.download = this.getFileName(format);
			link.click();
		});
	}

	savePosition(): void {
		this.childGraph?.savePosition();
	}

	getCleanNameFormat(format: string): string {
		let cleanName = format.replace('.', '').toUpperCase();
		return cleanName;
	}

	getNodeCount(): number {
		return this.childGraph?.schemaGraphClasses.length ?? 0;
	}

	private getFileName(format: string): string {
		let fileExtension = `${format.toLowerCase()}`;
		return this.datasetId + fileExtension;
	}
}
