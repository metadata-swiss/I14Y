import {Component, inject, OnInit, ViewChild} from '@angular/core';
import {Dataset, DatasetInputClient, LinkedDataFormat} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ObNotificationService} from '@oblique/oblique';
import {DcatDatasetService} from '../services/dcat-dataset.service';
import {Observable, of, Subject, switchMap, takeUntil, tap} from 'rxjs';
import {INode} from './structure-entity';
import {MatAutocompleteSelectedEvent} from '@angular/material/autocomplete';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {StructureGraphComponent} from './graph/structure-graph.component';
import {UriHelper} from 'src/app/shared/helper/uri-helper';
import {ActivatedRoute} from '@angular/router';

@Component({
	selector: 'app-structure',
	templateUrl: './structure.component.html',
	styleUrl: './structure.component.scss',
	standalone: false
})
export class StructureComponent implements OnInit {
	@ViewChild('graph') childGraph: StructureGraphComponent | undefined;
	acceptedFileExtensions: string[] = ['.ttl', '.rdf'];
	isGraphView: boolean = true;
	searchTerm: string | undefined;
	searchNodesResults$: Observable<INode[]> | undefined;
	currentLanguage: string;

	private readonly unsubscribe$ = new Subject<void>();

	private readonly dcatDatasetService = inject(DcatDatasetService);
	private readonly datasetInputClient = inject(DatasetInputClient);
	private readonly fallback = inject(FallbackPipe);
	private readonly notification = inject(ObNotificationService);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnDestroy(): void {
		this.unsubscribe$.next();
		this.unsubscribe$.complete();
		this.searchNodesResults$ = of([]);
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
		this.route.queryParams.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
			if (params['view'] === 'table') {
				this.setTableView();
			} else {
				this.setGraphView();
			}
		});
		let format = Object.values(LinkedDataFormat);
		this.acceptedFileExtensions = format.map(f => '.' + f.toLowerCase());
	}

	onSelect(event: MatAutocompleteSelectedEvent): void {
		let node = event.option.value as INode;
		this.childGraph?.selectNode(node);
	}

	onSearch(): void {
		if (this.searchTerm && this.searchTerm.trim() !== '') {
			this.searchNodesResults$ = this.childGraph?.searchNodes(this.searchTerm);
		} else {
			this.searchNodesResults$ = of([]);
		}
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
				this.notification.error('i18n.notification.download.format.error');
				return;
		}
		let datasetId: string;

		this.dcatDatasetService.dataset$
			.pipe(
				takeUntil(this.unsubscribe$),
				tap((dataset: Dataset) => (datasetId = dataset.id!)),
				switchMap(dataset => this.datasetInputClient.getModelExportByIdAndFormat(dataset.id!, linkedDataFormat))
			)
			.subscribe(response => {
				const link = document.createElement('a');
				const url = window.URL.createObjectURL(response.result.data);
				link.href = url;
				link.download = this.getFileName(format, datasetId);
				link.click();

				window.URL.revokeObjectURL(url);
			});
	}

	getFileName(format: string, datasetId: string): string {
		let fileExtension = `${format.toLowerCase()}`;
		return datasetId + fileExtension;
	}

	getNodeCount(): number {
		return this.childGraph?.schemaGraphClasses.length ?? 0;
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

	getCleanNameFormat(format: string): string {
		let cleanName = format.replace('.', '').toUpperCase();
		return cleanName;
	}
}
