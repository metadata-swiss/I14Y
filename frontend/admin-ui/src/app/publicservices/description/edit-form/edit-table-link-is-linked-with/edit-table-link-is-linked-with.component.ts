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
import {FormControl, UntypedFormArray, UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {MatSort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {
	CatalogClient,
	CatalogEntry,
	SearchResourceType,
	MultiLanguage,
	PublicServiceInputClient,
	PublicServiceModel,
	PublicServiceView
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Observable, of, Subject} from 'rxjs';
import {map, startWith, takeUntil} from 'rxjs/operators';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {IsIncludedValidators} from 'src/app/shared/validators/is-included-validators';

@Component({
	selector: 'app-edit-table-link-is-linked-with',
	templateUrl: './edit-table-link-is-linked-with.component.html',
	styleUrls: ['./edit-table-link-is-linked-with.component.scss'],
	standalone: false
})
export class EditTableLinkIsLinkedWithComponent implements OnInit, OnDestroy, OnChanges {
	@ViewChild(MatSort, {static: false}) sort!: MatSort;
	@ViewChildren('focusInputField') focusInputFields!: QueryList<ElementRef>;
	@Input() publicService!: PublicServiceModel;
	@Input() parentForm!: any;
	@Input() controlName!: string;
	@Output() edit: EventEmitter<boolean> = new EventEmitter();

	linkablePublicServices: CatalogEntry[] = [];
	filteredlinkablePublicServices$: Observable<CatalogEntry[]> = of([]);
	filteredControl = new FormControl();

	dto: PublicServiceView[] = [];
	dataSource = new MatTableDataSource<PublicServiceView>(this.dto);
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
	private selection = new SelectionModel<PublicServiceView>(true, []);
	private readonly unsubscribe$ = new Subject();

	private readonly catalogClient = inject(CatalogClient);
	private readonly fallback = inject(FallbackPipe);
	private readonly publicServiceInputClient = inject(PublicServiceInputClient);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.getLinkablePublicServices().subscribe(x => (this.linkablePublicServices = x));
		if (this.filteredControl) {
			this.filteredlinkablePublicServices$ = this.filteredControl.valueChanges.pipe(
				startWith(''),
				map(value => this.mapValue(value)),
				map(term =>
					term
						? this.filterLinkableDatasets(
								this.linkablePublicServices.filter(x => !this.dto.find(a => a.id === x.id)),
								term
							)
						: this.linkablePublicServices.filter(x => !this.dto.find(a => a.id === x.id))
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
		if (changes.publicService) {
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

	isRowSelected(row: PublicServiceView): boolean {
		return this.selection.isSelected(row);
	}

	reloadEntries(): void {
		this.getDataSource();
	}

	onAddRow(): void {
		let numRows = this.dataSource.data.push(this.createEmptyDto());
		this.filteredControl.setValue('');
		this.filteredControl.setValidators([
			Validators.required,
			IsIncludedValidators.isEquivalentValueIncluded(this.linkablePublicServices, (x: any, y: any) => x.id === y.id)
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
			let index: number = this.dataSource.data.findIndex((d: PublicServiceView) => d === item);
			this.onRemoveRow(index);
		});
		this.selection = new SelectionModel<PublicServiceView>(true, []);
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

	onToggleSelection(row: PublicServiceView): void {
		this.selection.toggle(row);
	}

	displayValue = (dataService: any) => {
		return this.fallback.transform(dataService?.title, this.currentLanguage) ?? '';
	};

	private getLinkablePublicServices(): Observable<CatalogEntry[]> {
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
				[SearchResourceType.PublicService],
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

	private createEmptyDto(): PublicServiceView {
		return new PublicServiceView({
			description: new MultiLanguage({
				de: undefined,
				en: undefined,
				fr: undefined,
				it: undefined
			}),
			id: undefined,
			title: new MultiLanguage({
				de: undefined,
				en: undefined,
				fr: undefined,
				it: undefined
			})
		});
	}

	private getDataSource(): void {
		if (this.publicService?.id) {
			this.publicServiceInputClient.getRelationById(this.publicService.id).subscribe(response => {
				this.dto = response.result ?? [];
				this.dataSource = new MatTableDataSource<PublicServiceView>(this.dto);
				this.initResourceToTableData();
			});
		} else {
			this.dto = [];
			this.dataSource = new MatTableDataSource<PublicServiceView>(this.dto);
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
		if (this.sort?.active === this.COLUMN_TITLE) {
			this.dto = [...this.dto].sort((a, b) => {
				const aTitle = this.fallback.transform(a.title, this.currentLanguage);
				const bTitle = this.fallback.transform(b.title, this.currentLanguage);
				return this.sortDtoSub(aTitle!, bTitle!);
			});
		}

		this.dataSource = new MatTableDataSource<PublicServiceView>(this.dto);
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

	private filterLinkableDatasets(list: CatalogEntry[], term: string): CatalogEntry[] {
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
