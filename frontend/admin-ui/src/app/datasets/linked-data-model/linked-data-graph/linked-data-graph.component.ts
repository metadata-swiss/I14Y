import {Component, effect, EventEmitter, HostBinding, inject, Input, model, OnInit, Output, ViewChild} from '@angular/core';
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
import {DatasetService} from '../../services/dataset.service';
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
	schemaGraph: SchemaGraph | undefined; //SchemaGraph.class  is the class get from the backend
	schemaGraphClasses: INode[] = []; // the classes displayed in the graph
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

	// True while a freshly added (not-yet-saved) class/property is being edited
	// in the sidebar. Used to close the sidebar if the user cancels the creation.
	private pendingCreation = false;

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

		// When editing is cancelled on a freshly added item:
		// - if no classes exist yet reload the page to restore the last persisted state;
		// - otherwise  just close the sidebar.
		effect(() => {
			if (!this.isEditMode() && this.pendingCreation) {
				this.pendingCreation = false;

				const persistedClassCount = (this.schemaGraph?.classes ?? []).filter(c => (c.identifier ?? '').length > 0).length;
				if (persistedClassCount === 0) {
					window.location.reload();
					return;
				}

				this.selectedProperty = undefined;
				this.selectedClass = undefined;
				this.selectedClassUri = undefined;
				this.isSidebarOpen = 'NONE';
			}
		});
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
		return property?.path ?? UriHelper.completePathUriForUnique(schemaClass?.uriComplete!, property?.identifier ?? '');
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
		this.pendingCreation = true;
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

			const propertyIdToDelete = dto.path ?? UriHelper.completePathUriForUnique(classUriForProperty, dto.identifier ?? '');

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
		// Item has been saved, it is no longer a pending creation.
		this.pendingCreation = false;

		if (dto instanceof SchemaProperty) {
			const propertyClassUri = classUri ?? this.selectedClassUri;
			if (propertyClassUri) {
				this.applyPropertyUpdate(dto, propertyClassUri);
			}
		} else if (dto instanceof SchemaClass) {
			this.applyClassUpdate(dto);
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

	private applyPropertyUpdate(updatedProperty: SchemaProperty, classUri: string): void {
		this.schemaGraphClasses = this.schemaGraphClasses.map(item =>
			item.node.uriComplete === classUri ? {...item, node: this.upsertedPropertyInClass(item.node, updatedProperty)} : item
		);

		if (this.schemaGraph?.classes) {
			this.schemaGraph.classes = this.schemaGraph.classes.map(schemaClass =>
				schemaClass.uriComplete === classUri ? this.upsertedPropertyInClass(schemaClass, updatedProperty) : schemaClass
			);
		}
	}

	// Copy of the class where the saved property replaces the one with the same path,
	// or is appended when the class does not hold it yet.
	private upsertedPropertyInClass(schemaClass: SchemaClass, updatedProperty: SchemaProperty): SchemaClass {
		const updatedPropertyId = this.getUniquePath(schemaClass, updatedProperty);
		const currentProperties = schemaClass.properties ?? [];
		const hasProperty = currentProperties.some(property => this.getUniquePath(schemaClass, property) === updatedPropertyId);

		const properties = hasProperty
			? currentProperties.map(property =>
					this.getUniquePath(schemaClass, property) === updatedPropertyId ? this.createUpdatedNewProperty(updatedProperty) : property
				)
			: [...currentProperties, new SchemaProperty(updatedProperty)];

		return new SchemaClass({...schemaClass, properties});
	}

	private applyClassUpdate(updatedClass: SchemaClass): void {
		const updatedClassUri = updatedClass.uriComplete;

		this.upsertGraphNode(updatedClass, updatedClassUri!);
		this.upsertSchemaGraphClass(updatedClass, updatedClassUri!);
	}

	private upsertGraphNode(updatedClass: SchemaClass, updatedClassUri: string): void {
		const nodeExists = this.schemaGraphClasses.some(item => item.node.uriComplete === updatedClassUri);

		this.schemaGraphClasses = nodeExists
			? this.schemaGraphClasses.map(item =>
					// `id` follows the IRI: the placeholder node is built before the identifier
					// completes it, and a rename changes it as well.
					item.node.uriComplete === updatedClassUri ? {...item, node: this.createUpdatedNewClass(updatedClass), id: updatedClassUri} : item
				)
			: [...this.schemaGraphClasses, this.createNodeForClass(updatedClass, updatedClassUri)];
	}

	private upsertSchemaGraphClass(updatedClass: SchemaClass, updatedClassUri: string): void {
		if (!this.schemaGraph) {
			return;
		}

		const classes = this.schemaGraph.classes ?? [];
		const classExists = classes.some(schemaClass => schemaClass.uriComplete === updatedClassUri);

		this.schemaGraph.classes = classExists
			? classes.map(schemaClass => (schemaClass.uriComplete === updatedClassUri ? this.createUpdatedNewClass(updatedClass) : schemaClass))
			: [...classes, updatedClass];
	}

	// Graph node for a class, placed at its saved position when it has one, otherwise at random.
	private createNodeForClass(schemaClass: SchemaClass, classUri: string): INode {
		const point = schemaClass.point;
		const hasPoint = point?.x !== undefined && point?.y !== undefined;

		return {
			node: schemaClass,
			position: hasPoint
				? PointExtensions.initialize(point!.x, point!.y)
				: PointExtensions.initialize(Math.floor(Math.random() * 100), Math.floor(Math.random() * 100)),
			size: SizeExtensions.initialize(200, 40 + (schemaClass.properties?.length ?? 0) * 45), // Set a default size
			id: classUri
		};
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
		// Seed both lists with the placeholder: the edit form completes this very object's IRI
		// on save, so `applyClassUpdate()` finds it in each list and updates it instead of
		// appending it a second time.
		this.schemaGraph.classes = [this.selectedClass];
		this.schemaGraphClasses = [this.createNodeForClass(this.selectedClass, this.selectedClass.uriComplete!)];
		this.isSidebarOpen = 'OPENED';
		this.isPropertySelected = false;
		this.pendingCreation = true;
		this.isEditMode.set(true);
	}

	private createUriForNewClass(): string {
		const datasetIdentifier = this.dataset?.identifiers?.[0];
		var newClassUri = buildDatasetIri(datasetIdentifier!) + '/structure/';
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
		for (const schemaClass of schemaClasses) {
			this.schemaGraphClasses.push(this.createNodeForClass(schemaClass, schemaClass.uriComplete!));
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

	private createUpdatedNewProperty(property: SchemaProperty): SchemaProperty {
		const updatedProperty = new SchemaProperty(property);
		updatedProperty.path = updatedProperty.uriComplete;
		return updatedProperty;
	}

	private createUpdatedNewClass(schemaClass: SchemaClass): SchemaClass {
		const updatedClass = new SchemaClass(schemaClass);
		updatedClass.uriComplete = UriHelper.replaceLastSegment(updatedClass.uriComplete!, updatedClass.identifier!);
		return updatedClass;
	}
}
