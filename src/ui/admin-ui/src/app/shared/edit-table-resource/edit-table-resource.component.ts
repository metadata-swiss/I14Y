import {AfterViewInit, Component, ElementRef, EventEmitter, Input, OnChanges, Output, QueryList, SimpleChanges, ViewChild, ViewChildren} from '@angular/core';
import {MatSort} from '@angular/material/sort';
import {SelectionModel} from '@angular/cdk/collections';
import {MatTableDataSource} from '@angular/material/table';
import {UntypedFormArray, UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {IResource, MultiLanguage} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {URI_PATTERN} from 'src/app/app-constants';

@Component({
	selector: 'app-edit-table-resource',
	templateUrl: './edit-table-resource.component.html',
	styleUrls: ['./edit-table-resource.component.scss'],
	standalone: false
})
export class EditTableResourceComponent implements AfterViewInit, OnChanges {
	@ViewChild(MatSort) sort!: MatSort;
	@ViewChildren('focusInputField') focusInputFields!: QueryList<ElementRef>;
	@Input() parentForm!: UntypedFormGroup;
	@Input() dto: any;
	@Input() controlName!: string;
	@Input() maxEntries: number | undefined;
	@Input() required: boolean = false;
	@Output() edit: EventEmitter<boolean> = new EventEmitter();
	public dataSource = new MatTableDataSource<IResource>([]);

	COLUMN_HREF = 'href';
	COLUMN_GERMAN = 'de';
	COLUMN_FRENCH = 'fr';
	COLUMN_ITALIAN = 'it';
	COLUMN_ENGLISH = 'en';
	COLUMN_SELECT = 'select';
	COLUMN_ACTIONS = 'actions';

	displayedColumns: string[] = [
		this.COLUMN_SELECT,
		this.COLUMN_HREF,
		this.COLUMN_GERMAN,
		this.COLUMN_FRENCH,
		this.COLUMN_ITALIAN,
		this.COLUMN_ENGLISH,
		this.COLUMN_ACTIONS
	];

	private initialRowValue: IResource;
	private isEditing = false;
	private isAddMode = false;
	private rowIndex: number | undefined;
	private selection = new SelectionModel<IResource>(true, []);

	constructor() {
		this.initialRowValue = this.newEntry();
	}

	ngAfterViewInit(): void {
		this.dataSource.sort = this.sort;
	}

	ngOnChanges(changes: SimpleChanges) {
		const change = changes.dto;
		if (change && change.currentValue !== undefined) {
			this.dataSource = new MatTableDataSource<IResource>(this.dto);
			this.initResourceToTableData();
		}
	}

	onMasterToggle(): void {
		if (this.isAllSelected()) {
			this.selection.clear();
		} else {
			this.dataSource.data.forEach(row => this.selection.select(row));
		}
	}

	onRemoveRow(index: number): void {
		this.dataSource.data.splice(index, 1);
		this.resourceControl.removeAt(index);
		if (!this.isAddMode) {
			this.parentForm.markAsDirty();
		}
		this.refreshDatabinding();
	}

	onRemoveSelectedRows(): void {
		this.selection.selected.forEach(item => {
			const index: number = this.dataSource.data.findIndex((d: IResource) => d === item);
			this.onRemoveRow(index);
		});
		this.selection = new SelectionModel<IResource>(true, []);
	}

	onAddRow(): void {
		const numRows = this.dataSource.data.push(this.newEntry());

		const formArray = this.parentForm.get(this.controlName) as UntypedFormArray;
		if (formArray) {
			formArray.push(
				new UntypedFormGroup({
					href: new UntypedFormControl('', [Validators.required, Validators.pattern(URI_PATTERN)]),
					label: new UntypedFormGroup({
						de: new UntypedFormControl(''),
						fr: new UntypedFormControl(''),
						it: new UntypedFormControl(''),
						en: new UntypedFormControl('')
					})
				})
			);

			this.isAddMode = true;
			this.UpdateIsEditing(true);
			this.rowIndex = numRows - 1;
			this.refreshDatabinding();
			this.setFocus();
		}
	}

	type(ele: any, value: any, val: any) {
		ele[val] = value;
	}

	onDiscard() {
		if (this.isAddMode) {
			this.onRemoveRow(this.rowIndex!);
			this.isAddMode = false;
		} else {
			this.dataSource.data[this.rowIndex!] = this.copyRowValue(this.initialRowValue);
		}

		this.initResourceToTableData();
		this.refreshDatabinding();
		this.initialRowValue = this.newEntry();
		this.UpdateIsEditing(false);
	}

	onEditRow(rowIndex: number): void {
		this.initialRowValue = this.copyRowValue(this.dataSource.data[rowIndex]);
		this.UpdateIsEditing(true);
		this.rowIndex = +rowIndex;
		this.setFocus();
	}

	onSave() {
		this.initResourceToTableData();
		this.isAddMode = false;
		this.UpdateIsEditing(false);
	}

	canAdd(): boolean {
		let canAdd = !this.isEditing && !this.hasSelectedItems();
		if (this.maxEntries) {
			return canAdd && this.dataSource.data.length < this.maxEntries;
		}
		return canAdd;
	}

	canEdit(): boolean {
		return !this.isEditing;
	}

	canRemove(): boolean {
		return !this.isEditing;
	}

	canSave(rowIndex: number): boolean {
		if (this.isRowEditMode(rowIndex)) {
			if (this.dataSource.data[rowIndex].href) {
				return true;
			}
		}
		return false;
	}

	canSelect(): boolean {
		return !this.isEditing;
	}

	hasSelectedRows(): boolean {
		return this.selection.hasValue();
	}

	isAllSelected(): boolean {
		const numSelected = this.selection.selected.length;
		const numRows = this.dataSource.data.length;
		return numSelected === numRows;
	}

	isEditMode(): boolean {
		return this.isEditing;
	}

	isRowEditMode(index: number): boolean {
		return this.isEditing && this.rowIndex === index;
	}

	isRowSelected(row: IResource): boolean {
		return this.selection.isSelected(row);
	}

	isControlInvalid(index: number): boolean {
		const relationsArray = this.parentForm.get(this.controlName) as UntypedFormArray;
		if (relationsArray) {
			if (relationsArray.at(index)) {
				const control = relationsArray.at(index) as UntypedFormControl;
				if (control) {
					return control.invalid;
				}
			}
		}
		return false;
	}

	onToggleSelection(row: IResource): void {
		this.selection.toggle(row);
	}

	private copyRowValue(source: IResource): IResource {
		return {
			href: source.href,
			label: new MultiLanguage({
				de: source?.label?.de,
				en: source?.label?.en,
				fr: source?.label?.fr,
				it: source?.label?.it
			})
		};
	}

	private hasSelectedItems(): boolean {
		return !this.selection.isEmpty();
	}

	private initResourceToTableData(): void {
		this.parentForm.setControl(
			this.controlName,
			new UntypedFormArray(
				this.dataSource.data.map(
					item =>
						new UntypedFormGroup({
							href: new UntypedFormControl(item?.href, [Validators.required, Validators.pattern(URI_PATTERN)]),
							label: new UntypedFormGroup({
								de: new UntypedFormControl(item?.label?.de),
								fr: new UntypedFormControl(item?.label?.fr),
								it: new UntypedFormControl(item?.label?.it),
								en: new UntypedFormControl(item?.label?.en)
							})
						})
				),
				this.required ? [Validators.required] : null
			)
		);
	}

	private newEntry(): IResource {
		return {
			href: undefined,
			label: new MultiLanguage({
				de: undefined,
				en: undefined,
				fr: undefined,
				it: undefined
			})
		};
	}

	private refreshDatabinding(): void {
		this.dataSource.filter = '';
	}

	private setFocus(): void {
		setTimeout(() => {
			this.focusInputFields.get(this.rowIndex!)?.nativeElement.focus();
		});
	}

	private UpdateIsEditing(value: boolean) {
		this.isEditing = value;
		this.edit.emit(value);
	}

	private get resourceControl(): any {
		return this.parentForm.get(this.controlName) as UntypedFormArray;
	}
}
