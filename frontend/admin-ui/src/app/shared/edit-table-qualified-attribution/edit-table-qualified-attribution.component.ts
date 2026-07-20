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
import {FormControl, UntypedFormArray, UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {
	IAgent,
	IDcatQualifiedAttributionInputModel,
	VocabularyClient,
	VocabularyEntry,
	CodeInputModel,
	IdentifierInputModel,
	AgentClient
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, Subject} from 'rxjs';
import {map, shareReplay, startWith, takeUntil} from 'rxjs/operators';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ActivatedRoute} from '@angular/router';
import {FallbackPipe} from '../fallback/fallback.pipe';

@Component({
	selector: 'app-edit-table-qualified-attribution',
	templateUrl: './edit-table-qualified-attribution.component.html',
	styleUrls: ['./edit-table-qualified-attribution.component.scss'],
	standalone: false
})
export class EditTableQualifiedAttributionComponent implements AfterViewInit, OnChanges, OnInit, OnDestroy {
	@ViewChild(MatSort) sort!: MatSort;
	@ViewChildren('focusInputField') focusInputFields!: QueryList<ElementRef>;
	@Input() parentForm!: any;
	@Input() dto: any;
	@Input() controlName!: string;
	@Output() edit: EventEmitter<boolean> = new EventEmitter();
	public dataSource = new MatTableDataSource<IDcatQualifiedAttributionInputModel>([]);
	public attributionRole$: Observable<VocabularyEntry[]> | undefined;
	public currentLanguage: string;
	public datasetId: string | undefined;
	public filteredAgents$!: Observable<IAgent[]>;
	public filteredAgentsControl = new FormControl();
	public agents: IAgent[] = [];

	COLUMN_HAD_ROLE = 'hadRole';
	COLUMN_AGENT = 'agent';
	COLUMN_ACTIONS = 'actions';

	displayedColumns: string[] = [this.COLUMN_HAD_ROLE, this.COLUMN_AGENT, this.COLUMN_ACTIONS];

	private readonly attributionRole: string = 'VOCAB_I14Y_ATTRIBUTION_ROLE';
	private initialRowValue: IDcatQualifiedAttributionInputModel;
	private isEditing = false;
	private isAddMode = false;
	private rowIndex: number | undefined;
	private selection = new SelectionModel<IDcatQualifiedAttributionInputModel>(true, []);
	private readonly unsubscribe$ = new Subject();

	private readonly agentClient = inject(AgentClient);
	private readonly fallback = inject(FallbackPipe);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyClient = inject(VocabularyClient);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.initialRowValue = this.newEntry();
		this.attributionRole$ = this.vocabularyClient.getByIdentifier(this.attributionRole).pipe(
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
			this.dataSource = new MatTableDataSource<IDcatQualifiedAttributionInputModel>(this.dto);
			this.initResourceToTableData();
		}
	}

	ngOnInit() {
		this.datasetId = this.route.snapshot.params.id;

		if (this.filteredAgentsControl) {
			this.filteredAgents$ = this.filteredAgentsControl.valueChanges.pipe(
				startWith(''),
				map(value => (typeof value === 'string' ? value : value.name[this.currentLanguage])),
				map(term => (term ? this.filterAgents(term) : this.agents))
			);
		}

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	getAgents(): Promise<void> {
		return new Promise<void>(resolve => {
			this.agentClient
				.get()
				.pipe(shareReplay(1))
				.subscribe(response => {
					this.agents = response.result;
					resolve();
				});
		});
	}

	getAgent(agent: IdentifierInputModel | undefined): IAgent | undefined {
		return this.agents.find(x => x.identifier === agent?.identifier);
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
			const index: number = this.dataSource.data.findIndex((d: IDcatQualifiedAttributionInputModel) => d === item);
			this.onRemoveRow(index);
		});
		this.selection = new SelectionModel<IDcatQualifiedAttributionInputModel>(true, []);
	}

	onAddRow(): void {
		const numRows = this.dataSource.data.push(this.newEntry());
		this.filteredAgentsControl.setValue('');
		this.filteredAgentsControl.valueChanges.subscribe(value => {
			this.type(this.filteredAgentsControl, value, this.COLUMN_AGENT);
		});

		this.parentForm.get(this.controlName).push(
			new UntypedFormGroup({
				hadRole: new UntypedFormControl({}, [Validators.required]),
				agent: new UntypedFormControl({}, [Validators.required])
			})
		);

		this.isAddMode = true;
		this.UpdateIsEditing(true);
		this.rowIndex = numRows - 1;
		this.refreshDatabinding();
		this.setFocus();
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
		this.filteredAgentsControl.setValue(this.parentForm.controls.qualifiedAttributions.get(rowIndex.toString()).get('agent')?.value);
		this.UpdateIsEditing(true);
		this.rowIndex = +rowIndex;
		this.setFocus();
	}

	onSave(): void {
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
			if (this.dataSource.data[rowIndex].hadRole?.code && this.dataSource.data[rowIndex].agent?.identifier) {
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

	isRowSelected(row: IDcatQualifiedAttributionInputModel): boolean {
		return this.selection.isSelected(row);
	}

	onToggleSelection(row: IDcatQualifiedAttributionInputModel): void {
		this.selection.toggle(row);
	}

	compareType(prev: any, next: any): boolean {
		return prev.code === next.code;
	}

	displayValue = (user: any) => {
		return this.fallback.transform(user?.name, this.currentLanguage) ?? '';
	};

	private filterAgents(term: string): IAgent[] {
		const filterTerm = term.toLowerCase();

		// eslint-disable-next-line max-len
		return this.agents.filter((option: IAgent) => this.fallback.transform(option.name, this.currentLanguage)?.toString()?.toLowerCase()?.includes(filterTerm));
	}

	private copyRowValue(source: IDcatQualifiedAttributionInputModel): IDcatQualifiedAttributionInputModel {
		return {
			hadRole: source?.hadRole,
			agent: source?.agent
		};
	}

	private hasSelectedItems(): boolean {
		return !this.selection.isEmpty();
	}

	private initResourceToTableData(): void {
		if (this.agents.length > 0) {
			this.initControls();
		} else {
			this.getAgents().then(() => {
				this.initControls();
			});
		}
	}

	private initControls() {
		this.parentForm.setControl(
			this.controlName,
			new UntypedFormArray(
				this.dataSource.data.map(
					item =>
						new UntypedFormGroup({
							agent: new UntypedFormControl(
								this.agents.find(x => x.identifier === item?.agent?.identifier),
								[Validators.required]
							),
							hadRole: new UntypedFormControl(item?.hadRole, [Validators.required])
						})
				)
			)
		);
	}

	private newEntry(): IDcatQualifiedAttributionInputModel {
		return {
			hadRole: new CodeInputModel({
				code: undefined
			}),
			agent: new IdentifierInputModel({
				identifier: undefined
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
