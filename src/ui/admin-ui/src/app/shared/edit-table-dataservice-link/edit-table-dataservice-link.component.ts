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
import {MatSort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {
	CatalogClient,
	CatalogEntry,
	SearchResourceType,
	DataServiceModel,
	DatasetsClient,
	DcatDistributionModel,
	MultiLanguage,
	Resource
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Observable, of, Subject} from 'rxjs';
import {map, startWith, takeUntil} from 'rxjs/operators';
import {FallbackPipe} from '../fallback/fallback.pipe';
import {FormControl, UntypedFormArray, UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {ActivatedRoute} from '@angular/router';
import {IsIncludedValidators} from '../validators/is-included-validators';

@Component({
	selector: 'app-edit-table-dataservice-link',
	templateUrl: './edit-table-dataservice-link.component.html',
	styleUrls: ['./edit-table-dataservice-link.component.scss'],
	standalone: false
})
export class EditTableDataserviceLinkComponent implements OnInit, OnDestroy, OnChanges {
	@ViewChild(MatSort, {static: false}) sort!: MatSort;
	@ViewChildren('focusInputField') focusInputFields!: QueryList<ElementRef>;
	@Input() distribution!: DcatDistributionModel;
	@Input() parentForm!: any;
	@Input() controlName!: string;
	@Output() edit: EventEmitter<boolean> = new EventEmitter();

	linkableDataServices: CatalogEntry[] = [];
	filteredlinkableDataServices$: Observable<CatalogEntry[]> = of([]);
	filteredControl = new FormControl();

	datasetId: string;
	dto: DataServiceModel[] = [];
	dataSource = new MatTableDataSource<DataServiceModel>(this.dto);
	currentLanguage: string;

	COLUMN_SELECT = 'select';
	COLUMN_TITLE = 'title';
	COLUMN_ENDPOINT_URL = 'endpointUrls';
	COLUMN_STATUS = 'registrationStatus';
	COLUMN_PUBLICATION = 'publicationLevel';
	COLUMN_ACTIONS = 'actions';

	displayedColumns: string[] = [
		this.COLUMN_SELECT,
		this.COLUMN_TITLE,
		this.COLUMN_ENDPOINT_URL,
		this.COLUMN_STATUS,
		this.COLUMN_PUBLICATION,
		this.COLUMN_ACTIONS
	];

	private isEditing = false;
	private isAddMode = false;
	private rowIndex: number | undefined;
	private selection = new SelectionModel<DataServiceModel>(true, []);
	private readonly unsubscribe$ = new Subject();

	private readonly catalogClient = inject(CatalogClient);
	private readonly datasetsClient = inject(DatasetsClient);
	private readonly fallback = inject(FallbackPipe);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.datasetId = this.route.snapshot.params.id;
	}

	ngOnInit(): void {
		this.getLinkableDataServices().subscribe(x => (this.linkableDataServices = x));
		if (this.filteredControl) {
			this.filteredlinkableDataServices$ = this.filteredControl.valueChanges.pipe(
				startWith(''),
				map(value => this.mapValue(value)),
				map(term =>
					term
						? this.filterLinkableDataServices(
								this.linkableDataServices.filter(x => !this.dto.find(a => a.id === x.id)),
								term
							)
						: this.linkableDataServices.filter(x => !this.dto.find(a => a.id === x.id))
				)
			);
		}

		this.dataSource.sort = this.sort;
		this.getDataSource();
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	ngOnChanges(changes: SimpleChanges) {
		if (changes.distribution) {
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

	convertArrayToString(input: Resource[]): string {
		let result = '';
		result += input
			?.filter(x => Boolean(x.href))
			.map(x => x.href)
			.join(', ');

		return result;
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

	isRowSelected(row: DataServiceModel): boolean {
		return this.selection.isSelected(row);
	}

	onAddRow(): void {
		let numRows = this.dataSource.data.push(this.createEmptyDto());
		this.filteredControl.setValue('');
		this.filteredControl.setValidators([
			Validators.required,
			IsIncludedValidators.isEquivalentValueIncluded(this.linkableDataServices, (x: any, y: any) => x.id === y.id)
		]);
		this.filteredControl.valueChanges.subscribe(value => {
			this.onChange(this.filteredControl, value, this.COLUMN_TITLE);
		});

		this.isAddMode = true;
		this.UpdateIsEditing(true);
		this.rowIndex = numRows - 1;
		this.refreshDatabinding();
		this.setFocus();
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
			let index: number = this.dataSource.data.findIndex((d: DataServiceModel) => d === item);
			this.onRemoveRow(index);
		});
		this.selection = new SelectionModel<DataServiceModel>(true, []);
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

	onToggleSelection(row: DataServiceModel): void {
		this.selection.toggle(row);
	}

	displayValue = (dataService: any) => {
		return this.fallback.transform(dataService?.title, this.currentLanguage) ?? '';
	};

	private getLinkableDataServices(): Observable<CatalogEntry[]> {
		return this.catalogClient // eslint-disable-next-line max-len
			.getSearchByQueryAndAccessRightsAndConceptValueTypesAndFormatsAndBusinessEventsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypesAndPageAndPageSize(
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				undefined,
				[SearchResourceType.DataService],
				undefined,
				undefined
			)
			.pipe(map(response => response.result));
	}

	private initResourceToTableData() {
		this.parentForm.setControl(
			this.controlName,
			new UntypedFormArray(this.dataSource.data.map(item => new UntypedFormGroup({id: new UntypedFormControl(item?.id, [Validators.required])})))
		);
	}

	private createEmptyDto(): DataServiceModel {
		return new DataServiceModel({
			description: new MultiLanguage({
				de: undefined,
				en: undefined,
				fr: undefined,
				it: undefined
			}),
			documentation: [],
			id: undefined,
			registrationStatus: undefined,
			system: undefined,
			endpointUrls: [],
			title: new MultiLanguage({
				de: undefined,
				en: undefined,
				fr: undefined,
				it: undefined
			})
		});
	}

	private getDataSource(): void {
		if (this.distribution?.id) {
			this.datasetsClient.getDistributionsAccessServicesByDatasetIdAndDistributionId(this.datasetId, this.distribution.id).subscribe(response => {
				this.dto = response.result ?? [];
				this.dataSource = new MatTableDataSource<DataServiceModel>(this.dto);
				this.initResourceToTableData();
			});
		} else {
			this.dto = [];
			this.dataSource = new MatTableDataSource<DataServiceModel>(this.dto);
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
			case this.COLUMN_ENDPOINT_URL:
				this.dto = [...this.dto].sort((a, b) => {
					return this.sortDtoSub(a.endpointUrls!.toString(), b.endpointUrls!.toString());
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

		this.dataSource = new MatTableDataSource<DataServiceModel>(this.dto);
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

	private filterLinkableDataServices(list: CatalogEntry[], term: string): CatalogEntry[] {
		return list.filter((option: CatalogEntry) => this.getTitle(option).toLowerCase().includes(term.toLowerCase()));
	}

	private getTitle(option: CatalogEntry): string {
		return this.fallback.transform(option.title, this.currentLanguage) ?? '';
	}

	private mapValue(value: any): any {
		if (value) {
			return typeof value === 'string' ? value : value.title[this.currentLanguage];
		}
		return '';
	}
}
