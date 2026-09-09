import {
	AfterViewInit,
	Component,
	ElementRef,
	EventEmitter,
	inject,
	Input,
	OnChanges,
	OnDestroy,
	Output,
	QueryList,
	signal,
	SimpleChanges,
	ViewChild,
	ViewChildren
} from '@angular/core';
import {MatSort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {SelectionModel} from '@angular/cdk/collections';
import {ObNotificationService} from '@oblique/oblique';
import {HttpStatusCode} from '@angular/common/http';
import {Languages} from 'src/app/shared/ApplicationLanguage.enum';
import {MatDialog, MatDialogConfig} from '@angular/material/dialog';
import {ModalDialogMappingRelationComponent} from './modal-dialog/modal-dialog.component';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';
import {MappingRelationModel, MappingTablesClient, SwaggerResponse} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {DialogComponent, DialogType} from 'src/app/shared/dialog/dialog.component';
import {DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY} from 'src/app/app-constants';
import {MappingRelationInputMapper} from 'src/app/shared/mappers/mappingrelationinputmapper';
import {MappingRelationDialogData} from './modal-dialog/mapping-relation.dialog.data';
import {isLocalIri} from 'src/app/shared/helper/iri-helpers';

@Component({
	selector: 'app-edit-table-relations',
	templateUrl: './edit-table-relations.component.html',
	styleUrls: ['./edit-table-relations.component.scss'],
	standalone: false
})
export class EditTableRelationsComponent implements AfterViewInit, OnChanges, OnDestroy {
	@ViewChild(MatSort) sort!: MatSort;
	@ViewChildren('focusInputField') focusInputFields!: QueryList<ElementRef>;
	@Input() mappingTableId: string | undefined;
	@Input() isNewMappingTable: boolean = false;
	@Input() currentLanguage: string;
	@Input() controlName!: string;
	@Output() updateMappingRelations: EventEmitter<void> = new EventEmitter();
	@Input() dto!: MappingRelationModel[];
	public dataSource = new MatTableDataSource<MappingRelationModel>([]);

	contentLanguages: readonly string[] = Languages.ContentLanguagesRm;

	COLUMN_ACTIONS = 'actions';
	COLUMN_ADDITIONALINFO = 'additionalInfo';
	COLUMN_SELECT = 'select';
	COLUMN_RELATIONTYPE = 'relationType';
	COLUMN_SOURCECODE = 'sourceCode';
	COLUMN_TARGETCODE = 'tragetCode';

	displayedColumns = [this.COLUMN_SELECT, this.COLUMN_SOURCECODE, this.COLUMN_RELATIONTYPE, this.COLUMN_TARGETCODE, this.COLUMN_ACTIONS];
	expandedElement = signal<MappingRelationModel | null>(null);
	isExpanded = false;

	private selection = new SelectionModel<MappingRelationModel>(true, []);
	private readonly unsubscribe$ = new Subject();

	private readonly dialog = inject(MatDialog);
	private readonly mappingTableClient = inject(MappingTablesClient);
	private readonly notification = inject(ObNotificationService);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => (this.currentLanguage = language.lang));
	}

	ngAfterViewInit(): void {
		this.dataSource.sort = this.sort;
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	ngOnChanges(changes: SimpleChanges) {
		if (changes.dto) {
			let change = changes.dto;
			if (change.currentValue !== undefined) {
				this.dataSource = new MatTableDataSource<MappingRelationModel>(this.dto);
				this.dataSource.filter = '';
			}
		}
	}

	onMasterToggle(): void {
		if (this.isAllSelected()) {
			this.selection.clear();
		} else {
			this.dataSource.data.forEach(row => this.selection.select(row));
		}
	}

	removeRow(index: number): void {
		const dialogRef = this.dialog.open(DialogComponent, {
			data: {
				showHeader: true,
				enableSave: false,
				headerText: this.translate.instant('i18n.dialog.delete.header_text'),
				bodyText: this.translate.instant('i18n.dialog.delete.body_text'),
				dialogType: DialogType.confirm,
				okButtonText: '',
				cancelButtonText: this.translate.instant(DIALOG_CANCEL_BUTTON_KEY),
				confirmButtonText: this.translate.instant(DIALOG_CONFIRM_BUTTON_KEY),
				discardChangesButtonText: '',
				saveChangesButtonText: ''
			},
			disableClose: true
		});

		const dialogConfirm = dialogRef.componentInstance.confirm.subscribe(() => {
			const row = this.dataSource.data.splice(index, 1);
			if (row) {
				this.deleteRequest(row[0]).then(success => {
					if (success) {
						this.showSuccessNotification();
						this.getMappingRelations();
						this.refreshDatabinding();
					} else {
						this.showErrorNotification();
					}
				});
			}
		});

		dialogRef.afterClosed().subscribe(() => {
			dialogConfirm.unsubscribe();
		});
	}

	onRemoveSelectedRows(): void {
		const dialogRef = this.dialog.open(DialogComponent, {
			data: {
				showHeader: true,
				enableSave: false,
				headerText: this.translate.instant('i18n.dialog.delete.header_text'),
				bodyText: this.translate.instant('i18n.dialog.delete.body_text'),
				dialogType: DialogType.confirm,
				okButtonText: '',
				cancelButtonText: this.translate.instant(DIALOG_CANCEL_BUTTON_KEY),
				confirmButtonText: this.translate.instant(DIALOG_CONFIRM_BUTTON_KEY),
				discardChangesButtonText: '',
				saveChangesButtonText: ''
			},
			disableClose: true
		});
		const dialogConfirm = dialogRef.componentInstance.confirm.subscribe(() => {
			const promises = this.selection.selected.map((item: MappingRelationModel): Promise<boolean> => {
				let index = this.dataSource.data.findIndex((d: MappingRelationModel) => d === item);
				const row = this.dataSource.data.splice(index, 1);
				if (row) {
					return this.deleteRequest(row[0]);
				}
				return Promise.resolve(false);
			});

			Promise.all(promises).then(results => {
				if (results.every(success => success)) {
					this.showSuccessNotification();
					this.getMappingRelations();
					this.refreshDatabinding();
				} else {
					this.showErrorNotification();
				}
			});

			this.selection = new SelectionModel<MappingRelationModel>(true, []);
		});
		dialogRef.afterClosed().subscribe(() => {
			dialogConfirm.unsubscribe();
		});
	}

	addRow(): void {
		this.showDialog(this.newEntry(), false);
	}

	editRow(row: MappingRelationModel): void {
		this.showDialog(row, true);
	}

	onFindIndex(row: MappingRelationModel): number {
		return this.dataSource.data.findIndex(x => x === row);
	}

	canAdd(): boolean {
		return !this.hasSelectedItems();
	}

	toggleDetail(element: MappingRelationModel) {
		this.expandedElement.update(e => (e !== element ? element : null));
	}

	/**
	 * Returns true when the URI belongs to the current environment AND the backend
	 * resolved a code for it. Gates the "code | label" display; cross-environment
	 * URIs always show the raw IRI.
	 */
	showResolvedCode(entry: {uri?: string; code?: string} | undefined): boolean {
		return !!entry?.uri && !!entry.code && isLocalIri(entry.uri);
	}

	canCreateMappingRelations(): boolean {
		return this.isNewMappingTable;
	}

	hasRows(): boolean {
		return this.dataSource?.data?.length > 0;
	}

	hasSelectedRows(): boolean {
		return this.selection.hasValue();
	}

	isAllSelected(): boolean {
		const numSelected = this.selection.selected.length;
		const numRows = this.dataSource.data.length;
		return numSelected === numRows;
	}

	isRowSelected(row: MappingRelationModel): boolean {
		return this.selection.isSelected(row);
	}

	reloadEntries() {
		this.getMappingRelations();
	}

	onToggleSelection(row: MappingRelationModel): void {
		this.selection.toggle(row);
	}

	private updateAfterSave(success: boolean) {
		if (success) {
			this.showSuccessNotification();
			this.getMappingRelations();
			this.refreshDatabinding();
		} else {
			this.showErrorNotification();
		}
	}

	private hasSelectedItems(): boolean {
		return !this.selection.isEmpty();
	}

	private newEntry(): MappingRelationModel {
		return new MappingRelationModel({
			source: undefined,
			target: undefined,
			relationType: undefined
		});
	}

	private getMappingRelations(): void {
		this.updateMappingRelations.emit();
	}

	private refreshDatabinding(): void {
		this.dataSource.filter = '';
	}

	private validateHttpStatus<T>(response: SwaggerResponse<T>, resolve: (value: boolean | PromiseLike<boolean>) => void, code: HttpStatusCode) {
		if (response.status === code) {
			resolve(true);
		} else {
			resolve(false);
		}
	}

	private putRequest(mappingRelation: MappingRelationModel): Promise<boolean> {
		return new Promise<boolean>(resolve => {
			if (this.mappingTableId && mappingRelation.id) {
				this.mappingTableClient
					.putRelationsByIdAndRelationIdAndBody(this.mappingTableId, mappingRelation.id, MappingRelationInputMapper.mapToInputModel(mappingRelation))
					.subscribe(response => {
						this.validateHttpStatus(response, resolve, HttpStatusCode.NoContent);
					});
			}
		});
	}

	private deleteRequest(mappingRelation: MappingRelationModel): Promise<boolean> {
		return new Promise<boolean>(resolve => {
			if (this.mappingTableId && mappingRelation.id) {
				this.mappingTableClient.deleteRelationsByIdAndRelationId(this.mappingTableId, mappingRelation.id).subscribe(response => {
					this.validateHttpStatus(response, resolve, HttpStatusCode.NoContent);
				});
			}
		});
	}

	private postRequest(mappingRelation: MappingRelationModel): Promise<boolean> {
		return new Promise<boolean>(resolve => {
			if (this.mappingTableId) {
				this.mappingTableClient
					.postRelationsByIdAndBody(this.mappingTableId, [MappingRelationInputMapper.mapToInputModel(mappingRelation)])
					.subscribe(response => {
						this.validateHttpStatus(response, resolve, HttpStatusCode.Created);
					});
			}
		});
	}

	private showDialog(entry: MappingRelationModel, isEdit: boolean) {
		const dialogRef = this.dialog.open(ModalDialogMappingRelationComponent, this.createDialogConfig(entry, isEdit));

		dialogRef.afterClosed().subscribe(data => {
			if (data as MappingRelationDialogData) {
				if (data.dto.id) {
					this.putRequest(data.dto).then(success => {
						this.updateAfterSave(success);
					});
				} else {
					this.postRequest(data.dto).then(success => {
						this.updateAfterSave(success);
					});
				}
			}
		});
	}

	private showSuccessNotification(): void {
		this.notification.success('i18n.notification.save_succeeded');
	}

	private showErrorNotification(): void {
		this.notification.error('i18n.notification.save_error');
	}

	private createDialogConfig(mappingRelation: MappingRelationModel, isEdit: boolean) {
		let dialogConfig = new MatDialogConfig<MappingRelationDialogData>();
		dialogConfig.data = new MappingRelationDialogData(mappingRelation, isEdit ? 'i18n.mapping_relations.edit.title' : 'i18n.title.create_mapping_relation');
		dialogConfig.width = '70%';
		dialogConfig.maxWidth = '1200px';
		dialogConfig.minWidth = '600px';
		dialogConfig.disableClose = true;
		dialogConfig.autoFocus = true;
		return dialogConfig;
	}
}
