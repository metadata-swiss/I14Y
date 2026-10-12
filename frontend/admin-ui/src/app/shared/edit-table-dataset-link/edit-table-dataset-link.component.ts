import {SelectionModel} from '@angular/cdk/collections';
import {
	Component,
	ElementRef,
	EventEmitter,
	inject,
	Input,
	OnChanges,
	OnDestroy,
	OnInit,
	Output,
	QueryList,
	SimpleChanges,
	ViewChild,
	ViewChildren
} from '@angular/core';
import {MatAutocomplete} from '@angular/material/autocomplete';
import {MatSort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {SearchResourceType, DataServiceInputClient, DataServiceModel, Dataset, MultiLanguage} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Subject} from 'rxjs';
import {debounceTime, filter, takeUntil} from 'rxjs/operators';
import {FallbackPipe} from '../fallback/fallback.pipe';
import {PagedCatalogSearch} from '../helper/paged-catalog-search';
import {FormControl, UntypedFormArray, UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {IsIncludedValidators} from '../validators/is-included-validators';

@Component({
	selector: 'app-edit-table-dataset-link',
	templateUrl: './edit-table-dataset-link.component.html',
	styleUrls: ['./edit-table-dataset-link.component.scss'],
	standalone: false
})
export class EditTableDatasetLinkComponent implements OnInit, OnDestroy, OnChanges {
	@ViewChild(MatSort, {static: false}) sort!: MatSort;
	@ViewChildren('focusInputField') focusInputFields!: QueryList<ElementRef>;
	@Input() dataService!: DataServiceModel;
	@Input() parentForm!: any;
	@Input() controlName!: string;
	@Output() edit: EventEmitter<boolean> = new EventEmitter();

	filteredControl = new FormControl();

	dto: Dataset[] = [];
	linkableDatasets = new PagedCatalogSearch([SearchResourceType.Dataset], () => this.dto.map(x => x.id));
	dataSource = new MatTableDataSource<Dataset>(this.dto);
	currentLanguage: string;

	COLUMN_SELECT = 'select';
	COLUMN_TITLE = 'title';
	COLUMN_IDENTIFIER = 'identifiers';
	COLUMN_STATUS = 'registrationStatus';
	COLUMN_PUBLICATION = 'publicationLevel';
	COLUMN_ACTIONS = 'actions';

	displayedColumns: string[] = [
		this.COLUMN_SELECT,
		this.COLUMN_TITLE,
		this.COLUMN_IDENTIFIER,
		this.COLUMN_STATUS,
		this.COLUMN_PUBLICATION,
		this.COLUMN_ACTIONS
	];

	private isEditing = false;
	private isAddMode = false;
	private rowIndex: number | undefined;
	private selection = new SelectionModel<Dataset>(true, []);
	private readonly unsubscribe$ = new Subject();

	private readonly dataServiceInputClient = inject(DataServiceInputClient);
	private readonly fallback = inject(FallbackPipe);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.filteredControl.valueChanges
			.pipe(
				filter(value => typeof value === 'string'),
				debounceTime(300),
				takeUntil(this.unsubscribe$)
			)
			.subscribe(value => this.linkableDatasets.updateQuery(value));

		this.dataSource.sort = this.sort;
		this.getDataSource();
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnDestroy() {
		this.linkableDatasets.destroy();
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	ngOnChanges(changes: SimpleChanges) {
		if (changes.dataService) {
			this.getDataSource();
		}
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
			if (this.dataSource.data[rowIndex].title) {
				return true;
			}
		}
		return false;
	}

	canSelect(): boolean {
		return !this.isEditing;
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

	isEditMode(): boolean {
		return this.isEditing;
	}

	isRowEditMode(index: number): boolean {
		return this.isEditing && this.rowIndex === index;
	}

	isRowSelected(row: Dataset): boolean {
		return this.selection.isSelected(row);
	}

	onAddRow(): void {
		let numRows = this.dataSource.data.push(this.createEmptyDto());
		this.filteredControl.setValue('');
		this.filteredControl.setValidators([Validators.required, IsIncludedValidators.isEntryWithIdIncluded()]);
		this.filteredControl.valueChanges.subscribe(value => {
			this.onChange(this.filteredControl, value, this.COLUMN_TITLE);
		});

		this.isAddMode = true;
		this.UpdateIsEditing(true);
		this.rowIndex = numRows - 1;
		this.refreshDatabinding();
		this.setFocus();
	}

	onOpenedAutoComplete(autocomplete: MatAutocomplete): void {
		this.linkableDatasets.attachScroll(autocomplete);
		const value = this.filteredControl.value;
		this.linkableDatasets.search(typeof value === 'string' ? value : null);
	}

	onChange(ele: any, value: any, val: any) {
		ele[val] = value;
	}

	onClickOnHeader(): void {
		setTimeout(() => {
			this.sortDto();
		}, 200);
	}

	onDiscard() {
		if (this.isAddMode) {
			this.onRemoveRow(this.rowIndex!);
			this.isAddMode = false;
		}

		this.initResourceToTableData();
		this.UpdateIsEditing(false);
		this.refreshDatabinding();
		this.setFocus();
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
			let index: number = this.dataSource.data.findIndex((d: Dataset) => d === item);
			this.onRemoveRow(index);
		});
		this.selection = new SelectionModel<Dataset>(true, []);
	}

	onSave(index: number) {
		if (this.isFormValid) {
			this.dataSource.data[index] = this.filteredControl.value;

			this.resourceControl.push(
				new UntypedFormGroup({
					id: new UntypedFormControl(this.filteredControl.value.id, [Validators.required])
				})
			);

			this.filteredControl.markAsUntouched();
			this.initResourceToTableData();
			this.isAddMode = false;
			this.UpdateIsEditing(false);
			this.refreshDatabinding();
		}
	}

	onToggleSelection(row: Dataset): void {
		this.selection.toggle(row);
	}

	displayValue = (dataset: any) => {
		return this.fallback.transform(dataset?.title, this.currentLanguage) ?? '';
	};

	private initResourceToTableData() {
		this.parentForm.setControl(
			this.controlName,
			new UntypedFormArray(this.dataSource.data.map(item => new UntypedFormGroup({id: new UntypedFormControl(item?.id, [Validators.required])})))
		);
	}

	private createEmptyDto(): Dataset {
		return new Dataset({
			description: new MultiLanguage({
				de: undefined,
				en: undefined,
				fr: undefined,
				it: undefined
			}),
			documents: [],
			id: undefined,
			status: undefined,
			identifiers: [],
			title: new MultiLanguage({
				de: undefined,
				en: undefined,
				fr: undefined,
				it: undefined
			})
		});
	}

	private getDataSource(): void {
		if (this.dataService?.id) {
			this.dataServiceInputClient.getServesDatasetsById(this.dataService.id).subscribe(response => {
				this.dto = response.result ?? [];
				this.dataSource = new MatTableDataSource<Dataset>(this.dto);
				this.initResourceToTableData();
			});
		} else {
			this.dto = [];
			this.dataSource = new MatTableDataSource<Dataset>(this.dto);
			this.initResourceToTableData();
		}
	}

	private hasSelectedItems(): boolean {
		return !this.selection.isEmpty();
	}

	private refreshDatabinding(): void {
		this.dataSource.filter = '';
	}

	private setFocus(): void {
		setTimeout(() => {
			this.focusInputFields.get(this.rowIndex!)?.nativeElement.focus();
		}, 100);
	}

	private get isFormValid(): boolean {
		this.filteredControl.markAllAsTouched();
		this.filteredControl.updateValueAndValidity();
		return this.filteredControl.valid;
	}

	private sortDto() {
		switch (this.sort?.active) {
			case this.COLUMN_TITLE:
				this.dto = [...this.dto].sort((a, b) => {
					const aTitle = this.fallback.transform(a.title, this.currentLanguage);
					const bTitle = this.fallback.transform(b.title, this.currentLanguage);
					return this.sortDtoSub(aTitle!, bTitle!);
				});
				break;
			case this.COLUMN_IDENTIFIER:
				this.dto = [...this.dto].sort((a, b) => {
					return this.sortDtoSub(a.identifiers!.toString(), b.identifiers!.toString());
				});
				break;
			case this.COLUMN_STATUS:
				this.dto = [...this.dto].sort((a, b) => {
					return this.sortDtoSub(a.registrationStatus!.toString(), b.registrationStatus!.toString());
				});
				break;
			case this.COLUMN_PUBLICATION:
				this.dto = [...this.dto].sort((a, b) => {
					return this.sortDtoSub(a.publicationLevel!.toString(), b.publicationLevel!.toString());
				});
		}

		this.dataSource = new MatTableDataSource<Dataset>(this.dto);
	}

	private sortDtoSub(a: string, b: string): number {
		if (this.sort.direction.toString() === 'asc') {
			return a.toLowerCase().localeCompare(b.toLowerCase());
		} else {
			return b.toLowerCase().localeCompare(a.toLowerCase());
		}
	}

	private UpdateIsEditing(value: boolean) {
		this.isEditing = value;
		this.edit.emit(value);
	}

	private get resourceControl(): UntypedFormArray {
		return this.parentForm.get(this.controlName) as UntypedFormArray;
	}
}
