import {AfterViewInit, Component, ElementRef, EventEmitter, Input, OnChanges, Output, QueryList, SimpleChanges, ViewChild, ViewChildren} from '@angular/core';
import {MatSort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {IKeywordModel, MultiLanguage} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {SelectionModel} from '@angular/cdk/collections';
import {UntypedFormArray, UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {URI_PATTERN} from 'src/app/app-constants';
import {createKeywordRowValidator} from 'src/app/shared/validators/keyword-row.validator';

@Component({
	selector: 'app-edit-table-keywords',
	templateUrl: './edit-table-keywords.component.html',
	styleUrls: ['./edit-table-keywords.component.scss'],
	standalone: false
})
export class EditTableKeywordsComponent implements AfterViewInit, OnChanges {
	@ViewChild(MatSort) sort!: MatSort;
	@ViewChildren('focusInputField') focusInputFields!: QueryList<ElementRef>;
	@Input() parentForm!: UntypedFormGroup;
	@Input() dto: any;
	@Input() controlName!: string;
	@Output() edit: EventEmitter<boolean> = new EventEmitter();
	public dataSource = new MatTableDataSource<IKeywordModel>([]);

	COLUMN_URI = 'uri';
	COLUMN_GERMAN = 'de';
	COLUMN_FRENCH = 'fr';
	COLUMN_ITALIAN = 'it';
	COLUMN_ENGLISH = 'en';
	COLUMN_ROMANCH = 'rm'
	COLUMN_SELECT = 'select';
	COLUMN_ACTIONS = 'actions';

	displayedColumns: string[] = [
		this.COLUMN_SELECT,
		this.COLUMN_URI,
		this.COLUMN_GERMAN,
		this.COLUMN_FRENCH,
		this.COLUMN_ITALIAN,
		this.COLUMN_ENGLISH,
		this.COLUMN_ROMANCH,
		this.COLUMN_ACTIONS
	];

	private initialRowValue: IKeywordModel;
	private isEditing = false;
	private isAddMode = false;
	private rowIndex: number | undefined;
	private selection = new SelectionModel<IKeywordModel>(true, []);

	constructor() {
		this.initialRowValue = this.newEntry();
	}

	ngAfterViewInit(): void {
		this.dataSource.sort = this.sort;
	}

	ngOnChanges(changes: SimpleChanges) {
		const change = changes.dto;
		if (change.currentValue !== undefined) {
			this.dataSource = new MatTableDataSource<IKeywordModel>(this.dto);
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
			const index: number = this.dataSource.data.findIndex((d: IKeywordModel) => d === item);
			this.onRemoveRow(index);
		});
		this.selection = new SelectionModel<IKeywordModel>(true, []);
	}

	onAddRow(): void {
		const numRows = this.dataSource.data.push(this.newEntry());

		const formArray = this.parentForm.get(this.controlName) as UntypedFormArray;
		if (formArray) {
			formArray.push(this.createKeywordFormGroup());

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
		const formArray = this.parentForm.get(this.controlName) as UntypedFormArray;
		if (formArray && this.rowIndex !== undefined) {
			const rowGroup = formArray.at(this.rowIndex) as UntypedFormGroup;
			rowGroup.updateValueAndValidity();
			rowGroup.markAllAsTouched();

			if (rowGroup.invalid) {
				return; // Don't save — fields will be marked red
			}
		}

		this.initResourceToTableData();
		this.isAddMode = false;
		this.UpdateIsEditing(false);
	}

	canAdd(): boolean {
		return !this.isEditing && !this.hasSelectedItems();
	}

	canEdit(): boolean {
		return !this.isEditing;
	}

	canRemove(): boolean {
		return !this.isEditing;
	}

	canSave(rowIndex: number): boolean {
		if (this.isRowEditMode(rowIndex)) {
			// Trigger cross-field validation before checking
			const formArray = this.parentForm.get(this.controlName) as UntypedFormArray;
			if (formArray?.at(rowIndex)) {
				const rowGroup = formArray.at(rowIndex) as UntypedFormGroup;
				rowGroup.updateValueAndValidity();
			}
			return !this.isControlInvalid(rowIndex);
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

	isRowSelected(row: IKeywordModel): boolean {
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

	onToggleSelection(row: IKeywordModel): void {
		this.selection.toggle(row);
	}

	getUriControl(index: number): UntypedFormControl | null {
		const formArray = this.parentForm.get(this.controlName) as UntypedFormArray;
		const rowGroup = formArray?.at(index) as UntypedFormGroup;
		return (rowGroup?.get('uri') as UntypedFormControl) || null;
	}

	getLabelControl(index: number, lang: string): UntypedFormControl | null {
		const formArray = this.parentForm.get(this.controlName) as UntypedFormArray;
		const rowGroup = formArray?.at(index) as UntypedFormGroup;
		const labelGroup = rowGroup?.get('label') as UntypedFormGroup;
		return (labelGroup?.get(lang) as UntypedFormControl) || null;
	}

	private createKeywordFormGroup(item?: IKeywordModel): UntypedFormGroup {
		return new UntypedFormGroup(
			{
				label: new UntypedFormGroup({
					de: new UntypedFormControl(item?.label?.de ?? ''),
					fr: new UntypedFormControl(item?.label?.fr ?? ''),
					it: new UntypedFormControl(item?.label?.it ?? ''),
					en: new UntypedFormControl(item?.label?.en ?? ''),
					rm: new UntypedFormControl(item?.label?.rm ?? '')
				}),
				uri: new UntypedFormControl(item?.uri ?? '', [Validators.pattern(URI_PATTERN)])
			},
			{validators: [createKeywordRowValidator()]}
		);
	}

	private copyRowValue(source: IKeywordModel): IKeywordModel {
		return {
			label: new MultiLanguage({
				de: source?.label?.de,
				en: source?.label?.en,
				fr: source?.label?.fr,
				it: source?.label?.it,
				rm: source?.label?.rm
			}),
			uri: source?.uri
		};
	}

	private hasSelectedItems(): boolean {
		return !this.selection.isEmpty();
	}

	private initResourceToTableData(): void {
		this.parentForm.setControl(this.controlName, new UntypedFormArray(this.dataSource.data.map(item => this.createKeywordFormGroup(item))));
	}

	private newEntry(): IKeywordModel {
		return {
			label: new MultiLanguage({
				de: '',
				en: '',
				fr: '',
				it: '',
				rm: ''
			}),
			uri: ''
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
