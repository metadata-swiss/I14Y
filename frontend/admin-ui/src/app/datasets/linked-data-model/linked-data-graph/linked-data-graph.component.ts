import {Component, effect, EventEmitter, HostBinding, inject, Input, model, OnInit, Output, ViewChild} from '@angular/core';
import {StructureClass, ISchemaConnector} from '../linked-data-entity';
import {DatasetInputClient, SchemaClass, SchemaPoint, SchemaProperty, DcatDatasetModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {ActivatedRoute} from '@angular/router';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {EFLayoutDirection, EFMarkerType, FCanvasComponent, FFlowComponent, provideFLayout} from '@foblex/flow';
import {ObNotificationService, ObTColumnState} from '@oblique/oblique';
import {UriHelper} from '../../../shared/helper/uri-helper';
import {Observable, of, Subject, takeUntil} from 'rxjs';
import {EElkLayoutAlgorithm, ElkLayoutEngine} from '@foblex/flow-elk-layout';
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
	searchNodesResult: FuseResult<StructureClass>[] | undefined;

	public eMarkerType = EFMarkerType;

	schemaGraphClasses: StructureClass[] = []; // the classes displayed in the graph
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
	fuse: Fuse<StructureClass> | undefined;
	isBigGraph = true;
	connectionType = 'bezier';

	// True while a freshly added (not-yet-saved) class/property is being edited
	// in the sidebar. Used to close the sidebar if the user cancels the creation.
	private pendingCreation = false;

	// True once the graph has been built, so a later dataset emission does not rebuild it.
	private isGraphInitialized = false;

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

				const persistedClassCount = this.schemaGraphClasses.filter(c => (c.identifier ?? '').length > 0).length;
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
				if (!this.isGraphInitialized && (dataset?.identifiers?.length ?? 0) > 0) {
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
		return [...this.schemaGraphClasses].sort((a, b) => (this.getIdentifier(a) ?? '').localeCompare(this.getIdentifier(b) ?? ''));
	}

	getIdentifier(schemaClass: SchemaClass): string {
		return UriHelper.GetUriFragment(schemaClass.uriComplete ?? '');
	}

	getUniquePath(schemaClass: SchemaClass, property: SchemaProperty | undefined): string {
		return property?.path ?? UriHelper.completePathUriForUnique(schemaClass?.uriComplete!, property?.identifier ?? '');
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
				if (item.uriComplete !== classUriForProperty) {
					return item;
				}
				const filtered = (item.properties ?? []).filter(p => this.getUniquePath(item, p) !== propertyIdToDelete);
				return new StructureClass(this.withReplacedProperties(item, filtered), item);
			});
		} else {
			const classUriToDelete = dto.uriComplete;
			if (!classUriToDelete) {
				return;
			}
			this.schemaGraphClasses = this.schemaGraphClasses.filter(item => item.uriComplete !== classUriToDelete);
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

	selectNode(node: StructureClass): void {
		if (this.childCanvas) {
			this.childCanvas.setScale(0.8);
			this.childCanvas.centerGroupOrNode(node.id, true);
		}
	}

	searchNodes(searchTerm: string): Observable<StructureClass[]> {
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
			item.uriComplete === classUri ? new StructureClass(this.upsertedPropertyInClass(item, updatedProperty), item) : item
		);
	}

	// Copy of the class where the saved property replaces the one with the same path,
	// or is appended when the class does not hold it yet.
	private upsertedPropertyInClass(item: StructureClass, updatedProperty: SchemaProperty): SchemaClass {
		const updatedPropertyId = this.getUniquePath(item, updatedProperty);
		const currentProperties = item.properties ?? [];
		const hasProperty = currentProperties.some(property => this.getUniquePath(item, property) === updatedPropertyId);

		const properties = hasProperty
			? currentProperties.map(property =>
					this.getUniquePath(item, property) === updatedPropertyId ? this.createUpdatedNewProperty(updatedProperty) : property
				)
			: [...currentProperties, new SchemaProperty(updatedProperty)];

		return this.withReplacedProperties(item, properties);
	}

	// Copy of the class's schema fields with `properties` replaced, excluding the
	// StructureClass-only layout fields (id, position, size) so they never leak into
	// the intermediate SchemaClass before the caller re-wraps it into a StructureClass.
	private withReplacedProperties(item: StructureClass, properties: SchemaProperty[]): SchemaClass {
		const {id, position, size, ...schemaFields} = item;
		return new SchemaClass({...schemaFields, properties});
	}

	// Update the class when the graph already holds it, add it otherwise.
	private applyClassUpdate(updatedClass: SchemaClass): void {
		const updatedClassUri = updatedClass.uriComplete;
		if (!updatedClassUri) {
			return;
		}
		const classExists = this.schemaGraphClasses.some(item => item.uriComplete === updatedClassUri);

		this.schemaGraphClasses = classExists
			? this.schemaGraphClasses.map(item =>
					item.uriComplete === updatedClassUri ? new StructureClass(this.createUpdatedNewClass(updatedClass), item) : item
				)
			: [...this.schemaGraphClasses, new StructureClass(updatedClass)];
	}

	private loadGraph(): void {
		this.datasetInputClient
			.getModelGraphById(this.datasetId)
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(async response => {
				const classes = response.result?.classes;
				if (classes?.length) {
					this.isGraphInitialized = true;
					this.isBigGraph = classes.length > 20;
					this.positionCalculation(classes);
					let hasPosition = this.hasPosition(classes[0]);
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
		const placeholder = new StructureClass(
			new SchemaClass({
				uriComplete: this.createUriForNewClass(),
				identifier: ''
			})
		);
		this.selectedClass = placeholder;
		this.schemaGraphClasses = [placeholder];
		this.isGraphInitialized = true;
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

	private searchListBuild(): Fuse<StructureClass> | undefined {
		if (this.schemaGraphClasses && this.schemaGraphClasses.length > 1) {
			return new Fuse(this.schemaGraphClasses, {
				useExtendedSearch: false,
				threshold: 0.1,
				keys: ['searchText'],
				getFn: (obj, path) => {
					const key = Array.isArray(path) ? path.join('.') : path;

					if (key === 'searchText') {
						return this.fallback.transform(obj.label, this.currentLanguage) ?? UriHelper.GetUriFragment(obj.uriComplete) ?? '';
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

		for (const schemaClass of this.schemaGraphClasses) {
			const id = schemaClass.uriComplete!;
			const inputId = UriHelper.RemoveHash(schemaClass.targetClass) ?? UriHelper.GetUriFragment(schemaClass.uriComplete);
			classIdMap.set(id, id);
			classIdMap.set(inputId, id);
		}

		let index = 0;
		for (const schemaClass of this.schemaGraphClasses) {
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
			this.schemaGraphClasses.push(new StructureClass(schemaClass));
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

		for (const node of this.schemaGraphClasses) {
			const layoutNode = result.nodes.find(n => n.id === node.id);
			if (layoutNode?.position) {
				node.position = {...layoutNode.position};
			}
		}
		this.schemaGraphClasses = [...this.schemaGraphClasses];

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
