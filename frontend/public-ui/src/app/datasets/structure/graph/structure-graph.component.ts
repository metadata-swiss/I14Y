import {Component, EventEmitter, inject, Input, OnDestroy, OnInit, Output, ViewChild} from '@angular/core';
import {DatasetInputClient, SchemaClass, SchemaGraph, SchemaProperty} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {PointExtensions, SizeExtensions} from '@foblex/2d';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {EFLayoutDirection, EFMarkerType, FCanvasComponent, FFlowComponent, IFLayoutConnection, provideFLayout} from '@foblex/flow';
import {EElkLayoutAlgorithm, ElkLayoutEngine} from '@foblex/flow-elk-layout';
import {DcatDatasetService} from '../../services/dcat-dataset.service';
import {distinctUntilChanged, Observable, of, Subject, switchMap, takeUntil} from 'rxjs';
import {UriHelper} from '../../../shared/helper/uri-helper';
import {ActivatedRoute, Router} from '@angular/router';
import {Location} from '@angular/common';
import {INode, ISchemaConnector} from '../structure-entity';
import Fuse, {FuseResult} from 'fuse.js';
import {ObTColumnState} from '@oblique/oblique';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
@Component({
	selector: 'app-structure-graph',
	templateUrl: './structure-graph.component.html',
	styleUrl: './structure-graph.component.scss',
	standalone: false,
	providers: [provideFLayout(ElkLayoutEngine)]
})
export class StructureGraphComponent implements OnInit, OnDestroy {
	@Input() isGraphView: boolean = false;
	@Output() changeView = new EventEmitter<'graph' | 'table'>();
	@ViewChild('fCanvas') childCanvas: FCanvasComponent | undefined;
	@ViewChild('fFlow') childFlow: FFlowComponent | undefined;

	schemaGraph: SchemaGraph | undefined;
	schemaGraphClasses: INode[] = [];
	schemaConnectors: ISchemaConnector[] = [];
	supportConnection: IFLayoutConnection[] = [];
	currentLanguage: string;
	selectedClass: SchemaClass | undefined;
	selectedProperty: SchemaProperty | undefined;
	isPropertySelected: boolean = true;
	isSidebarOpen: ObTColumnState = 'NONE';
	searchNodesResult: FuseResult<INode>[] | undefined;
	fuse: Fuse<INode> | undefined;
	isBigGraph = true;
	connectionType = 'bezier';

	public eMarkerType = EFMarkerType;

	private readonly unsubscribe$ = new Subject<void>();
	private readonly _layout = inject(ElkLayoutEngine);
	private readonly fallback = inject(FallbackPipe);

	constructor(
		private readonly dcatDatasetService: DcatDatasetService,
		private readonly datasetInputClient: DatasetInputClient,
		private readonly translate: TranslateService,
		private readonly route: ActivatedRoute,
		private readonly router: Router,
		private readonly location: Location
	) {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.dcatDatasetService.dataset$
			.pipe(
				takeUntil(this.unsubscribe$),
				distinctUntilChanged((prev, curr) => prev.id === curr.id),
				switchMap(dataset => this.datasetInputClient.getModelGraphById(dataset.id!))
			)
			.subscribe(async response => {
				this.schemaGraph = response.result;
				this.schemaGraphClasses = [];
				this.schemaConnectors = [];

				if (this.schemaGraph.classes) {
					this.isBigGraph = this.schemaGraph.classes.length > 20;
					this.positionCalculation(this.schemaGraph.classes);
					if (this.schemaGraph.classes.length > 0) {
						let hasPosition = this.hasPosition(this.schemaGraph.classes[0]);
						this.connectionCalculation(!hasPosition);
						if (!hasPosition) {
							await this.applyLayout();
						}
						this.selectFromCurrentRoute();
					}
				}

				if (this.childCanvas) {
					this.childCanvas.fitToScreen({x: 30, y: 30});
				}

				this.fuse = this.searchListBuild();
			});

		this.route.paramMap.pipe(takeUntil(this.unsubscribe$)).subscribe(() => {
			this.selectFromCurrentRoute();
		});

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
			this.fuse = this.searchListBuild();
		});
	}

	ngOnDestroy(): void {
		this.unsubscribe$.next();
		this.unsubscribe$.complete();
	}

	getSortedClasses(): SchemaClass[] {
		return [...(this.schemaGraph?.classes ?? [])].sort((a, b) => (this.getIdentifier(a) ?? '').localeCompare(this.getIdentifier(b) ?? ''));
	}

	setSidebarState(state: ObTColumnState) {
		this.isSidebarOpen = state;
	}

	connectionCalculation(isCreateSupportConnection: boolean): void {
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

	sortProperties(schemaClass: SchemaClass): void {
		// Sort the properties of the class based on their order, handling cases where order is undefined
		(schemaClass.properties ?? []).sort((a, b) => {
			const orderA = a.order ?? Number.MAX_SAFE_INTEGER; // Assign a high value if order is undefined
			const orderB = b.order ?? Number.MAX_SAFE_INTEGER; // Assign a high value if order is undefined
			return orderA - orderB; // Sort by order
		});
	}

	onclassSelected(classSelected: SchemaClass, updateUrl: boolean = true): void {
		this.selectedClass = classSelected;
		this.selectedProperty = undefined;
		this.isSidebarOpen = 'OPENED';
		this.isPropertySelected = false;
		this.changeView.emit('graph');

		if (updateUrl) {
			const classId = this.getRouteValue(classSelected.uriComplete);

			if (classId) {
				this.updateStructureUrl(classId);
			}
		}
	}

	onPropertySelected(propertySelected: SchemaProperty, updateUrl: boolean = true): void {
		const classSelected = this.findClassForProperty(propertySelected);

		if (!classSelected) {
			return;
		}
		this.selectProperty(classSelected, propertySelected, updateUrl);
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

	completePathUriForUnique(schemaClass: SchemaClass, property: SchemaProperty | undefined): string {
		return UriHelper.completePathUriForUnique(schemaClass.uriComplete ?? '', property?.path ?? '');
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
	}

	getIdentifier(schemaClass: SchemaClass): string {
		return UriHelper.GetUriFragment(schemaClass.uriComplete ?? '');
	}

	getUniquePath(schemaClass: SchemaClass, property: SchemaProperty | undefined): string {
		return property?.uriComplete ?? UriHelper.completePathUriForUnique(schemaClass?.uriComplete!, property?.path ?? '');
	}

	private searchListBuild(): Fuse<INode> | undefined {
		if (this.schemaGraphClasses && this.schemaGraphClasses.length > 0) {
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

	private selectProperty(classSelected: SchemaClass, propertySelected: SchemaProperty, updateUrl: boolean): void {
		this.selectedClass = classSelected;
		this.selectedProperty = propertySelected;
		this.isSidebarOpen = 'OPENED';
		this.isPropertySelected = true;
		this.changeView.emit('graph');

		if (updateUrl) {
			const classId = this.getRouteValue(classSelected.uriComplete);
			const propertyId = this.getRouteValue(propertySelected.uriComplete);

			if (classId && propertyId) {
				this.updateStructureUrl(classId, propertyId);
			}
		}
	}

	private positionCalculation(schemaClasses: SchemaClass[]): void {
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

	private selectFromCurrentRoute(): void {
		const classId = this.route.snapshot.paramMap.get('classId');
		const propertyId = this.route.snapshot.paramMap.get('propertyId');

		this.selectFromRoute(classId, propertyId);
	}

	private matchesRouteValue(value: string | undefined, routeValue: string): boolean {
		if (!value) {
			return false;
		}

		return UriHelper.GetUriFragment(value) === routeValue;
	}

	private getRouteValue(value: string | undefined): string | undefined {
		if (!value) {
			return undefined;
		}

		return UriHelper.GetUriFragment(value);
	}

	private findClassForProperty(propertySelected: SchemaProperty): SchemaClass | undefined {
		return this.schemaGraphClasses.map(item => item.node).find(schemaClass => schemaClass.properties?.some(property => property === propertySelected));
	}

	private updateStructureUrl(classId: string, propertyId?: string): void {
		const urlTree = this.router.createUrlTree(propertyId ? ['structure', classId, propertyId] : ['structure', classId], {
			relativeTo: this.route.parent
		});

		this.location.go(this.router.serializeUrl(urlTree));
	}

	private selectFromRoute(classId: string | null, propertyId: string | null): void {
		if (!classId || !this.schemaGraphClasses.length) {
			return;
		}

		const schemaClass = this.schemaGraphClasses.map(item => item.node).find(currentClass => this.matchesRouteValue(currentClass.uriComplete, classId));

		if (!schemaClass) {
			return;
		}

		this.onclassSelected(schemaClass, false);

		if (!propertyId) {
			this.selectedProperty = undefined;
			return;
		}

		const property = schemaClass.properties?.find(currentProperty => this.matchesRouteValue(currentProperty.uriComplete, propertyId));

		if (property) {
			this.selectProperty(schemaClass, property, false);
		}
	}

	private hasPosition(schemaClass: SchemaClass): boolean {
		return schemaClass.point !== undefined && schemaClass.point.x !== undefined && schemaClass.point.y !== undefined;
	}
}
