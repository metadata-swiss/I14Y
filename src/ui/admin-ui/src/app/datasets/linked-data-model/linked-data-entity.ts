import {IPoint} from '@foblex/2d';
import {IFLayoutConnection, IFLayoutNode} from '@foblex/flow';
import {SchemaClass} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export interface ISchemaConnector extends IFLayoutConnection {
	id: number; // Unique identifier for the connector only use for tracking
	cardinalityFrom: string | undefined;
	cardinalityTo: string;
}

export interface INode extends IFLayoutNode {
	node: SchemaClass;
	position: IPoint;
}

export interface IGraph {
	nodes: INode[];
	connections: ISchemaConnector[];
}
