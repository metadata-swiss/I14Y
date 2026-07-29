import {Component, EventEmitter, HostBinding, inject, Input, model, OnInit, Output, ViewChild} from '@angular/core';
import {INode, ISchemaConnector} from '../linked-data-entity';
import {DatasetInputClient, SchemaClass, SchemaPoint, SchemaProperty, SchemaGraph, DcatDatasetModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ActivatedRoute} from '@angular/router';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {EFLayoutDirection, EFMarkerType, FCanvasComponent, FFlowComponent, provideFLayout} from '@foblex/flow';
import {ObNotificationService, ObTColumnState} from '@oblique/oblique';
import {UriHelper} from '../../../shared/helper/uri-helper';
import {Observable, of, Subject, takeUntil} from 'rxjs';
import {EElkLayoutAlgorithm, ElkLayoutEngine} from '@foblex/flow-elk-layout';
import {PointExtensions, SizeExtensions} from '@foblex/2d';
import {IFLayoutConnection} from '@foblex/flow';
import Fuse, {FuseResult} from 'fuse.js';
import {FallbackPipe} from '../../../shared/fallback/fallback.pipe';
import {DatasetService } from '../../services/dataset.service';
import {buildDatasetIri} from '../../../shared/helper/iri-helpers';


@Component({
	selector: 'app-linked-data-graph',
	templateUrl: './linked-data-graph.component.html',
	styleUrl: './linked-data-graph.component.scss',
	standalone: false,
	providers: [provideFLayout(ElkLayoutEngine)]
})
export class LinkedDataGraphComponent implements OnInit {
	@Input() cannotEdit$: Observable<boolean> = of(true);
	@Input() searchTerm: string | undefined;
	@Input() isCreationMode: boolean = false;
	@Input() isGraphView: boolean = false;
	@Output() changeView = new EventEmitter<'graph' | 'table'>();
	@ViewChild('fflow') childFlow: FFlowComponent | undefined;
	@ViewChild('fCanvas') childCanvas: FCanvasComponent | undefined;
	isEditMode = model(false);
	searchNodesResult: FuseResult<INode>[] | undefined;

	public eMarkerType = EFMarkerType;

	schemaGraph: SchemaGraph | undefined;
	schemaGraphClasses: INode[] = [];
	schemaConnectors: ISchemaConnector[] = [];
	supportConnection: IFLayoutConnection[] = [];
	currentLanguage: string;
	selectedClass: SchemaClass | undefined;
	selectedProperty: SchemaProperty | undefined;
	selectedClassUri: string | undefined;
	isPropertySelected: boolean = true;
	isSidebarOpen: ObTColumnState = 'NONE';
	connectionsAuxiliary: IFLayoutConnection[] = [];
	dataset: DcatDatasetModel = new DcatDatasetModel();
	datasetId: string;
	fuse: Fuse<INode> | undefined;
	isBigGraph = true;
	connectionType = 'bezier';

	
	private readonly datasetInputClient = inject(DatasetInputClient);
	private readonly notification = inject(ObNotificationService);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);
	private readonly datasetService = inject(DatasetService);
	private readonly _layout = inject(ElkLayoutEngine);
	private readonly fallback = inject(FallbackPipe);
	private readonly unsubscribe$ = new Subject<void>();
   

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.datasetId = this.route.parent?.snapshot.params.id;
	}

	@HostBinding('style.--sidebar-width')
	get sidebarWidth(): string {
		return this.isEditMode() ? '40%' : '25%';
	}

	@HostBinding('style.--collapsed-sidebar-width')
	get collapsedSidebarWidth(): string {
		return this.isEditMode() ? '-40%' : '-25%';
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
			this.fuse = this.searchListBuild();
		});

		if (this.isCreationMode) {
			this.datasetService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(dataset => {
				this.dataset = dataset;
				if (!this.schemaGraph && (dataset?.identifiers?.length ?? 0) > 0) {
					this.createInitGraph();
				}
			});
		} else {
			this.loadGraph();
		}
	}

	ngOnDestroy(): void {
		this.unsubscribe$.next();
		this.unsubscribe$.complete();
	}

	getNodeId(index: number, graphId: number): string {
		return `${graphId}-node-${index}`;
	}

	getNodeLabel(index: number): string {
		return `Node${index}`;
	}

	getSortedClasses(): SchemaClass[] {
		return [...(this.schemaGraph?.classes ?? [])].sort((a, b) => (this.getIdentifier(a) ?? '').localeCompare(this.getIdentifier(b) ?? ''));
	}

	setSidebarState(state: ObTColumnState) {
		this.isSidebarOpen = state;
	}

	onclassSelected(classSelected: SchemaClass): void {
		this.selectedClass = classSelected;
		this.selectedClassUri = classSelected.uriComplete;
		this.isSidebarOpen = 'OPENED';
		this.isPropertySelected = false;
		this.changeView.emit('graph');
	}

	getIdentifier(schemaClass: SchemaClass): string {
		return UriHelper.GetUriFragment(schemaClass.uriComplete ?? '');
	}

	getUniquePath(schemaClass: SchemaClass, property: SchemaProperty | undefined): string {
		return property?.uriComplete ?? UriHelper.completePathUriForUnique(schemaClass?.uriComplete!, property?.path ?? '');
	}

	onPropertySelected(event: {property: SchemaProperty | undefined; classUri: string | undefined}): void {
		this.selectedProperty = event.property;
		this.isSidebarOpen = 'OPENED';
		this.selectedClassUri = event.classUri;
		this.isPropertySelected = true;
		this.changeView.emit('graph');
	}

	// create a new empty property and open the sidebar to edit it
	onAddProperty(event: {classUri: string}): void {
		const propertyUri = event.classUri.endsWith('/') ? event.classUri : `${event.classUri}/`;
		this.selectedProperty = new SchemaProperty({
			uriComplete: propertyUri,
			path: propertyUri
		});
		this.selectedClassUri = event.classUri;
		this.isPropertySelected = true;
		this.isSidebarOpen = 'OPENED';
		this.isEditMode.set(true);
		this.changeView.emit('graph');
	}

	// remove class or property from graph after confirmation in sidebar
	onDeleteFromSidebar(dto: SchemaClass | SchemaProperty, classUri?: string): void {
		if (dto instanceof SchemaProperty) {
			const classUriForProperty = classUri ?? this.selectedClassUri;
			if (!classUriForProperty) {
				return;
			}

			const propertyIdToDelete = dto.uriComplete ?? UriHelper.completePathUriForUnique(classUriForProperty, dto.path ?? '');

			this.schemaGraphClasses = this.schemaGraphClasses.map(item => {
				if (item.node.uriComplete !== classUriForProperty) {
					return item;
				}
				const filtered = (item.node.properties ?? []).filter(p => this.getUniquePath(item.node, p) !== propertyIdToDelete);
				return {
					...item,
					node: new SchemaClass({...item.node, properties: filtered})
				};
			});

			if (this.schemaGraph?.classes) {
				this.schemaGraph.classes = this.schemaGraph.classes.map(schemaClass => {
					if (schemaClass.uriComplete !== classUriForProperty) {
						return schemaClass;
					}
					const filtered = (schemaClass.properties ?? []).filter(p => this.getUniquePath(schemaClass, p) !== propertyIdToDelete);
					return new SchemaClass({...schemaClass, properties: filtered});
				});
			}
		} else {
			const classUriToDelete = dto.uriComplete;
			if (!classUriToDelete) {
				return;
			}
			this.schemaGraphClasses = this.schemaGraphClasses.filter(item => item.node.uriComplete !== classUriToDelete);
			if (this.schemaGraph?.classes) {
				this.schemaGraph.classes = this.schemaGraph.classes.filter(c => c.uriComplete !== classUriToDelete);
			}
		}

		this.selectedProperty = undefined;
		this.selectedClass = undefined;
		this.selectedClassUri = undefined;
		this.isSidebarOpen = 'NONE';
		this.isEditMode.set(false);
	}

	onUpdateFromSidebar(dto: SchemaClass | SchemaProperty, classUri?: string): void {
		if ('path' in dto) {
			const classUriForProperty = classUri ?? this.selectedClassUri;
			if (!classUriForProperty) {
				return;
			}

			const updatedProperty = dto as SchemaProperty;

			this.schemaGraphClasses = this.schemaGraphClasses.map(item => {
				if (item.node.uriComplete !== classUriForProperty) {
					return item;
				}

				const updatedPropertyId = this.getUniquePath(item.node, updatedProperty);
				const currentProperties = item.node.properties ?? [];
				const hasProperty = currentProperties.some(property => this.getUniquePath(item.node, property) === updatedPropertyId);

				const updatedProperties = hasProperty
					? currentProperties.map(property =>
							this.getUniquePath(item.node, property) === updatedPropertyId ? new SchemaProperty(updatedProperty) : property
						)
					: [...currentProperties, new SchemaProperty(updatedProperty)];

				return {
					...item,
					node: new SchemaClass({
						...item.node,
						properties: updatedProperties
					})
				};
			});

			if (this.schemaGraph?.classes) {
				this.schemaGraph.classes = this.schemaGraph.classes.map(schemaClass => {
					if (schemaClass.uriComplete !== classUriForProperty) {
						return schemaClass;
					}

					const updatedPropertyId = this.getUniquePath(schemaClass, updatedProperty);
					const currentProperties = schemaClass.properties ?? [];
					const hasProperty = currentProperties.some(property => this.getUniquePath(schemaClass, property) === updatedPropertyId);

					const updatedProperties = hasProperty
						? currentProperties.map(property =>
								this.getUniquePath(schemaClass, property) === updatedPropertyId ? new SchemaProperty(updatedProperty) : property
							)
						: [...currentProperties, new SchemaProperty(updatedProperty)];

					return new SchemaClass({
						...schemaClass,
						properties: updatedProperties
					});
				});
			}

			return;
		}

		const updatedClass = new SchemaClass(dto);
		const updatedClassUri = updatedClass.uriComplete;

		if (!updatedClassUri) {
			return;
		}

		const classExists = this.schemaGraphClasses.some(item => item.node.uriComplete === updatedClassUri);

		if (classExists) {
			this.schemaGraphClasses = this.schemaGraphClasses.map(item => {
				if (item.node.uriComplete === updatedClassUri) {
					return {
						...item,
						node: new SchemaClass(dto)
					};
				}

				return item;
			});

			if (this.schemaGraph?.classes) {
				this.schemaGraph.classes = this.schemaGraph.classes.map(schemaClass =>
					schemaClass.uriComplete === updatedClassUri ? new SchemaClass(dto) : schemaClass
				);
			}
			return;
		}

		this.schemaGraphClasses = [
			...this.schemaGraphClasses,
			{
				node: updatedClass,
				position: PointExtensions.initialize(Math.floor(Math.random() * 100), Math.floor(Math.random() * 100)),
				size: SizeExtensions.initialize(200, 40 + (updatedClass.properties?.length ?? 0) * 45),
				id: updatedClassUri
			}
		];

		if (this.schemaGraph?.classes) {
			this.schemaGraph.classes = [...this.schemaGraph.classes, updatedClass];
		}
	}

	selectNode(node: INode): void {
		if (this.childCanvas) {
			this.childCanvas.setScale(0.8);
			this.childCanvas.centerGroupOrNode(node.id, true);
		}
	}

	searchNodes(searchTerm: string): Observable<INode[]> {
		if (this.fuse) {
			let results = this.fuse.search(searchTerm);
			return of(results.map(entry => entry.item));
		}
		return of([]);
	}

	sortProperties(schemaClass: SchemaClass): void {
		// Sort the properties of the class based on their order, handling cases where order is undefined
		(schemaClass.properties ?? []).sort((a, b) => {
			const orderA = a.order ?? Number.MAX_SAFE_INTEGER; // Assign a high value if order is undefined
			const orderB = b.order ?? Number.MAX_SAFE_INTEGER; // Assign a high value if order is undefined
			return orderA - orderB; // Sort by order
		});
	}

	savePosition(): void {
		if (this.childFlow) {
			let classesPosition: {[key: string]: SchemaPoint} = {};
			this.childFlow.getState().nodes.forEach(node => {
				if (node.position) {
					let schemaPosition: SchemaPoint = new SchemaPoint({
						x: node.position.x,
						y: node.position.y
					});
					classesPosition[node.id] = schemaPosition;
				}
			});
			this.datasetInputClient.putModelPositionByIdAndBody(this.datasetId, classesPosition).subscribe(() => {
				this.notification.success('i18n.notification.save_succeeded');
			});
		}
	}

	private loadGraph(): void {
		this.datasetInputClient
			.getModelGraphById(this.datasetId)
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(async response => {
				this.schemaGraph = response.result;
				if (this.schemaGraph?.classes) {
					this.isBigGraph = this.schemaGraph.classes.length > 20;
					this.positionCalculation(this.schemaGraph.classes);
					let hasPosition = this.hasPosition(this.schemaGraph.classes[0]);
					this.connectionCalculation(!hasPosition);
					if (!hasPosition) {
						await this.applyLayout();
					}

					this.fuse = this.searchListBuild();
				}
			});
	}
    // create init graph and a empty class
	private createInitGraph(): void {

		this.schemaGraph = new SchemaGraph();
		this.selectedClass = new SchemaClass({
			uriComplete: this.createUriForNewClass(),
			identifier: ''
		});
		this.schemaGraph.classes = [this.selectedClass];
		this.isSidebarOpen = 'OPENED';
		this.isPropertySelected = false;
		this.isEditMode.set(true);
	}

	private createUriForNewClass(): string {
		const datasetIdentifier = this.dataset?.identifiers?.[0];
		var newClassUri = buildDatasetIri(datasetIdentifier!)+'/structure/';
		return newClassUri;
	}

	private searchListBuild(): Fuse<INode> | undefined {
		if (this.schemaGraphClasses && this.schemaGraphClasses.length > 1) {
			return new Fuse(this.schemaGraphClasses, {
				useExtendedSearch: false,
				threshold: 0.1,
				keys: ['node.searchText'],
				getFn: (obj, path) => {
					const key = Array.isArray(path) ? path.join('.') : path;

					if (key === 'node.searchText') {
						return this.fallback.transform(obj.node?.label, this.currentLanguage) ?? UriHelper.GetUriFragment(obj.node?.uriComplete) ?? '';
					}

					return key.split('.').reduce((acc: any, part: string) => acc?.[part], obj) ?? '';
				}
			});
		} else {
			return undefined;
		}
	}



	private connectionCalculation(isCreateSupportConnection: boolean): void {
		this.schemaConnectors = [];
		this.supportConnection = [];
		const classIdMap = new Map<string, string>();

		for (const schemaClass of this.schemaGraph?.classes || []) {
			const id = schemaClass.uriComplete!;
			const inputId = UriHelper.RemoveHash(schemaClass.targetClass) ?? UriHelper.GetUriFragment(schemaClass.uriComplete);
			classIdMap.set(id, id);
			classIdMap.set(inputId, id);
		}

		let index = 0;
		for (const schemaClass of this.schemaGraph?.classes || []) {
			if (schemaClass.properties && schemaClass.properties.length > 0) {
				for (const property of schemaClass.properties) {
					if (!property.toClassUri) {
						continue;
					}

					const targetClassId = classIdMap.get(UriHelper.GetUriFragment(property.toClassUri));

					if (!targetClassId) {
						continue;
					}

					if (isCreateSupportConnection) {
						const supportConnector: IFLayoutConnection = {
							source: schemaClass.uriComplete!,
							target: targetClassId!
						};
						this.supportConnection.push(supportConnector);
					}

					const connector: ISchemaConnector = {
						id: index, // Assign a unique ID based on the current length of connectors
						source: this.getUniquePath(schemaClass, property)!,
						target: UriHelper.GetUriFragment(property.toClassUri)!,
						cardinalityFrom: property.minCardinality ? property.minCardinality.toString() : '0',
						cardinalityTo: property.maxCardinality ? property.maxCardinality.toString() : 'n'
					};

					this.schemaConnectors.push(connector);
					index++;
				}
			}
		}
	}

	private positionCalculation(schemaClasses: SchemaClass[]) {
		let x: number;
		let y: number;
		for (const schemaClass of schemaClasses) {
			if (schemaClass.point && schemaClass.point.x !== undefined && schemaClass.point.y !== undefined) {
				x = schemaClass.point.x;
				y = schemaClass.point.y;
			} else {
				x = Math.floor(Math.random() * 100);
				y = Math.floor(Math.random() * 100);
			}

			this.schemaGraphClasses.push({
				node: schemaClass,
				position: PointExtensions.initialize(x, y),
				size: SizeExtensions.initialize(200, 40 + schemaClass.properties!.length * 45), // Set a default size
				id: schemaClass.uriComplete!
			});
		}
	}

	private async applyLayout() {
		let layOutOptions = {};
		if (this.isBigGraph) {
			this.connectionType = 'straight';
			layOutOptions = {
				'elk.algorithm': 'layered',
				'elk.direction': 'RIGHT',
				'elk.layered.compaction.postCompaction.strategy': 'NONE',
				'elk.layered.mergeEdges': false,
				'elk.layered.spacing.nodeNodeBetweenLayers': 200,
				'elk.layered.nodePlacement.strategy': 'SIMPLE',
				'elk.layered.crossingMinimization.strategy': 'LAYER_SWEEP',
				'elk.layered.thoroughness': 30,
				'elk.edgeRouting': 'ORTHOGONAL'
			};
		} else {
			layOutOptions = {
				'elk.algorithm': 'layered',
				'elk.layered.mrtree.enabled': 'true',
				'elk.mrtree.nodePlacement': 'NETWORK_SIMPLEX',
				'elk.layered.spacing.nodeNodeBetweenLayers': '600',
				'elk.spacing.nodeNode': '90',
				'elk.layered.considerModelOrder.strategy': 'NODES_AND_EDGES'
			};
			this.connectionType = 'bezier';
		}

		const result = await this._layout.calculate(this.schemaGraphClasses, this.supportConnection, {
			direction: EFLayoutDirection.LEFT_RIGHT,
			algorithm: this.isBigGraph ? EElkLayoutAlgorithm.LAYERED : EElkLayoutAlgorithm.MRTREE,
			nodeGap: 40,
			layerGap: 800,
			layoutOptions: layOutOptions
		});

		this.schemaGraphClasses = this.schemaGraphClasses.map(node => {
			const layoutNode = result.nodes.find(n => n.id === node.id);
			return {
				...node,
				position: layoutNode?.position ? {...layoutNode.position} : node.position
			};
		});

		if (this.childCanvas) {
			this.childCanvas.fitToScreen({x: 30, y: 30});
		}
	}

	private hasPosition(schemaClass: SchemaClass): boolean {
		return schemaClass.point !== undefined && schemaClass.point.x !== undefined && schemaClass.point.y !== undefined;
	}
}
