import {IPoint, ISize, PointExtensions, SizeExtensions} from '@foblex/2d';
import {IFLayoutConnection, IFLayoutNode} from '@foblex/flow';
import {SchemaClass} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export interface ISchemaConnector extends IFLayoutConnection {
	id: number; // Unique identifier for the connector only use for tracking
	cardinalityFrom: string | undefined;
	cardinalityTo: string;
}

// A structure class is the class itself plus the layout data the flow needs to draw it.
// Extending SchemaClass keeps `instanceof SchemaClass` working in the sidebar and keeps
// `toJSON()`, so `id`, `position` and `size` are never sent to the backend.
export class StructureClass extends SchemaClass implements IFLayoutNode {
	id: string;
	position: IPoint;
	size: ISize;

	// `previous` carries the position over when a class is rebuilt after an edit.
	constructor(schemaClass: SchemaClass, previous?: StructureClass) {
		super(schemaClass);

		const point = schemaClass.point;
		const hasPoint = point?.x !== undefined && point?.y !== undefined;

		this.id = schemaClass.uriComplete!;
		this.position =
			previous?.position ??
			(hasPoint
				? PointExtensions.initialize(point!.x, point!.y)
				: PointExtensions.initialize(Math.floor(Math.random() * 100), Math.floor(Math.random() * 100)));
		this.size = SizeExtensions.initialize(200, 40 + (this.properties?.length ?? 0) * 45); // Set a default size
	}
}

export interface IGraph {
	nodes: StructureClass[];
	connections: ISchemaConnector[];
}
