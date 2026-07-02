import {
	AfterViewInit,
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
import {SelectionModel} from '@angular/cdk/collections';
import {MatTableDataSource} from '@angular/material/table';
import {UntypedFormArray, UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {
	DatasetsClient,
	DcatQualifiedRelationModel,
	MultiLanguage,
	ResourceModel,
	VocabularyClient,
	VocabularyEntry
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {map, shareReplay, takeUntil} from 'rxjs/operators';
import {Observable, Subject} from 'rxjs';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ActivatedRoute} from '@angular/router';
import {URI_PATTERN} from 'src/app/app-constants';

@Component({
	selector: 'app-edit-table-qualified-relation',
	templateUrl: './edit-table-qualified-relation.component.html',
	styleUrls: ['./edit-table-qualified-relation.component.scss'],
	standalone: false
})
export class EditTableQualifiedRelationComponent implements OnInit, AfterViewInit, OnChanges, OnDestroy {
	@ViewChild(MatSort) sort!: MatSort;
	@ViewChildren('focusInputField') focusInputFields!: QueryList<ElementRef>;
	@Input() parentForm!: any;
	@Input() dto: DcatQualifiedRelationModel[] = [];
	@Input() controlName!: string;
	@Output() edit: EventEmitter<boolean> = new EventEmitter();
	public dataSource = new MatTableDataSource<DcatQualifiedRelationModel>([]);
	public relationshipRole$: Observable<VocabularyEntry[]> | undefined;
	public currentLanguage: string;
	public datasetId: string | undefined;

	COLUMN_HAD_ROLE = 'hadRole';
	COLUMN_URI = 'uri';
	COLUMN_GERMAN = 'de';
	COLUMN_FRENCH = 'fr';
	COLUMN_ITALIAN = 'it';
	COLUMN_ENGLISH = 'en';
	COLUMN_ACTIONS = 'actions';

	displayedColumns: string[] = [
		this.COLUMN_HAD_ROLE,
		this.COLUMN_URI,
		this.COLUMN_GERMAN,
		this.COLUMN_FRENCH,
		this.COLUMN_ITALIAN,
		this.COLUMN_ENGLISH,
		this.COLUMN_ACTIONS
	];

	private readonly relationshipRole: string = 'VOCAB_I14Y_RELATIONSHIP_ROLE';
	private initialRowValue: DcatQualifiedRelationModel;
	private isEditing = false;
	private isAddMode = false;
	private rowIndex: number | undefined;
	private selection = new SelectionModel<DcatQualifiedRelationModel>(true, []);
	private readonly unsubscribe$ = new Subject();

	private readonly datasetsClient = inject(DatasetsClient);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyClient = inject(VocabularyClient);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.initialRowValue = this.newEntry();
		this.relationshipRole$ = this.vocabularyClient.getByIdentifier(this.relationshipRole).pipe(
			map(response => response.result),
			shareReplay(1)
		);
	}

	ngAfterViewInit(): void {
		this.dataSource.sort = this.sort;
	}

	ngOnChanges(changes: SimpleChanges) {
		const change = changes.dto;
		if (change.currentValue !== undefined) {
			this.dataSource = new MatTableDataSource<DcatQualifiedRelationModel>(this.dto);
			// Ensure that the relation label is initialized
			this.dataSource.data.forEach(qualifiedRelation => {
				if (qualifiedRelation.relation) {
					if (!qualifiedRelation.relation.label) {
						qualifiedRelation.relation.label = new MultiLanguage({});
					}
				}
			});
			this.initResourceToTableData();
		}
	}

	ngOnInit() {
		this.datasetId = this.route.snapshot.params.id;

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	onMasterToggle(): void {
		if (this.isAllSelected()) {
			this.selection.clear();
		} else {
			this.dataSource.data.forEach(row => this.selection.select(row));
		}
	}

	updateDto() {
		const id = this.route.snapshot.params.id;
		this.datasetsClient.getById(id).subscribe(response => {
			this.dto = response.result.qualifiedRelations ?? [];
			if (this.dto) {
				this.dataSource.data = this.dto;
			}
		});
	}

	onRemoveRow(index: number, row?: DcatQualifiedRelationModel): void {
		this.dataSource.data.splice(index, 1);
		this.resourceControl.removeAt(index);
		if (!this.isAddMode) {
			this.parentForm.markAsDirty();
		}
		this.refreshDatabinding();
	}

	onRemoveSelectedRows(): void {
		this.selection.selected.forEach(item => {
			const index: number = this.dataSource.data.findIndex((d: DcatQualifiedRelationModel) => d === item);
			this.onRemoveRow(index);
		});
		this.selection = new SelectionModel<DcatQualifiedRelationModel>(true, []);
	}

	onAddRow(): void {
		const numRows = this.dataSource.data.push(this.newEntry());

		this.parentForm.get(this.controlName).push(
			new UntypedFormGroup({
				id: new UntypedFormControl(''),
				datasetQualifiedRelationId: new UntypedFormControl(''),
				hadRole: new UntypedFormControl('', [Validators.required]),
				relation: new UntypedFormGroup({
					uri: new UntypedFormControl('', [Validators.required, Validators.pattern(URI_PATTERN)]),
					label: new UntypedFormGroup({
						de: new UntypedFormControl(''),
						fr: new UntypedFormControl(''),
						it: new UntypedFormControl(''),
						en: new UntypedFormControl('')
					})
				})
			})
		);

		this.isAddMode = true;
		this.UpdateIsEditing(true);
		this.rowIndex = numRows - 1;
		this.refreshDatabinding();
		this.setFocus();
	}

	type(ele: any, value: any, val: any): void {
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

	save(): void {
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
			if (this.dataSource.data[rowIndex].relation?.uri && this.dataSource.data[rowIndex].hadRole?.code) {
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

	isRowSelected(row: DcatQualifiedRelationModel): boolean {
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

	onToggleSelection(row: DcatQualifiedRelationModel): void {
		this.selection.toggle(row);
	}

	compareType(prev: any, next: any): boolean {
		return prev.code === next.code;
	}

	private copyRowValue(source: DcatQualifiedRelationModel): DcatQualifiedRelationModel {
		return new DcatQualifiedRelationModel({
			hadRole: source?.hadRole,
			relation: new ResourceModel({
				uri: source.relation?.uri,
				label: new MultiLanguage({
					de: source.relation?.label?.de,
					fr: source.relation?.label?.fr,
					it: source.relation?.label?.it,
					en: source.relation?.label?.en
				})
			})
		});
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
							hadRole: new UntypedFormControl(item?.hadRole, [Validators.required]),
							relation: new UntypedFormGroup({
								uri: new UntypedFormControl(item?.relation?.uri, [Validators.required, Validators.pattern(URI_PATTERN)]),
								label: new UntypedFormGroup({
									de: new UntypedFormControl(item?.relation?.label?.de),
									fr: new UntypedFormControl(item?.relation?.label?.fr),
									it: new UntypedFormControl(item?.relation?.label?.it),
									en: new UntypedFormControl(item?.relation?.label?.en)
								})
							})
						})
				)
			)
		);
	}

	private newEntry(): DcatQualifiedRelationModel {
		return new DcatQualifiedRelationModel({
			hadRole: undefined,
			relation: new ResourceModel({
				uri: undefined,
				label: new MultiLanguage({
					de: undefined,
					fr: undefined,
					it: undefined,
					en: undefined
				})
			})
		});
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
