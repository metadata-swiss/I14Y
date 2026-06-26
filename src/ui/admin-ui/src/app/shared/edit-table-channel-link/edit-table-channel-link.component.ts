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
import {MultiLanguage, Agent, AgentClient, ChannelModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {MatTableDataSource} from '@angular/material/table';
import {SelectionModel} from '@angular/cdk/collections';
import {Observable, of, Subject} from 'rxjs';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {map, startWith, takeUntil} from 'rxjs/operators';
import {FormControl, UntypedFormArray, UntypedFormControl, Validators} from '@angular/forms';
import {FallbackPipe} from '../fallback/fallback.pipe';
import {AgentMapper} from '../mappers/agentmapper';
import {IsIncludedValidators} from '../validators/is-included-validators';

@Component({
	selector: 'app-edit-table-channel-link',
	templateUrl: './edit-table-channel-link.component.html',
	styleUrls: ['./edit-table-channel-link.component.scss'],
	standalone: false
})
export class EditTableChannelLinkComponent implements OnInit, OnChanges, OnDestroy {
	@ViewChild(MatSort, {static: false}) sort!: MatSort;
	@ViewChildren('focusInputFi-eld') focusInputFields!: QueryList<ElementRef>;
	@Input() channel: ChannelModel | undefined;
	@Input() parentForm!: any;
	@Input() controlName!: string;
	@Output() edit: EventEmitter<boolean> = new EventEmitter();

	linkableAgents: Agent[] = [];
	filteredlinkableAgents$: Observable<Agent[]> = of([]);
	filteredControl = new FormControl();

	dto: Agent[] = [];
	dataSource = new MatTableDataSource<Agent>(this.dto);
	currentLanguage: string;

	COLUMN_SELECT = 'select';
	COLUMN_NAME = 'name';
	COLUMN_ACTIONS = 'actions';

	displayedColumns: string[] = [this.COLUMN_SELECT, this.COLUMN_NAME, this.COLUMN_ACTIONS];

	private isEditing = false;
	private isAddMode = false;
	private rowIndex: number | undefined;
	private selection = new SelectionModel<Agent>(true, []);
	private readonly unsubscribe$ = new Subject();

	private readonly agentClient = inject(AgentClient);
	private readonly fallback = inject(FallbackPipe);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.getLinkableAgents().subscribe(x => (this.linkableAgents = x));
		if (this.filteredControl) {
			this.filteredlinkableAgents$ = this.filteredControl.valueChanges.pipe(
				startWith(''),
				map(value => this.mapValue(value)),
				map(term =>
					term
						? this.filterLinkableAgents(
								this.linkableAgents.filter(x => !this.dto.find(a => a.id === x.id)),
								term
						  )
						: this.linkableAgents.filter(x => !this.dto.find(a => a.id === x.id))
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
		if (changes.channel) {
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
			if (this.dataSource.data[rowIndex].name) {
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

	isRowSelected(row: Agent): boolean {
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
			IsIncludedValidators.isEquivalentValueIncluded(this.linkableAgents, (x: any, y: any) => x.id === y.id)
		]);
		this.filteredControl.valueChanges.subscribe(value => {
			this.onChange(this.filteredControl, value, this.COLUMN_NAME);
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

	onDiscard() {
		if (this.isAddMode) {
			this.onRemoveRow(this.rowIndex!);
			this.isAddMode = false;
		}

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
		this.resourceControl.markAsDirty();
		if (!this.isAddMode) {
			this.parentForm.markAsDirty();
		}
		this.refreshDatabinding();
	}

	onRemoveSelectedRows(): void {
		this.selection.selected.forEach(item => {
			let index: number = this.dataSource.data.findIndex((d: Agent) => d === item);
			this.onRemoveRow(index);
		});
		this.selection = new SelectionModel<Agent>(true, []);
	}

	onSave(index: number) {
		if (this.isFormValid) {
			this.dataSource.data[index] = this.filteredControl.value;
			this.resourceControl.push(new UntypedFormControl(this.filteredControl.value));
			this.resourceControl.markAsDirty();

			this.filteredControl.markAsUntouched();
			this.isAddMode = false;
			this.UpdateIsEditing(false);
			this.refreshDatabinding();
		}
	}

	onToggleSelection(row: Agent): void {
		this.selection.toggle(row);
	}

	displayValue = (agent: any) => {
		return this.fallback.transform(agent?.name, this.currentLanguage) ?? '';
	};

	private getLinkableAgents(): Observable<Agent[]> {
		return this.agentClient.get().pipe(map(response => response.result));
	}

	private createEmptyDto(): Agent {
		return new Agent({
			id: undefined,
			spatial: undefined,
			name: new MultiLanguage({
				de: undefined,
				en: undefined,
				fr: undefined,
				it: undefined
			}),
			prefLabel: new MultiLanguage({
				de: undefined,
				en: undefined,
				fr: undefined,
				it: undefined
			})
		});
	}

	private getDataSource(): void {
		this.dto = this.channel?.ownedBy?.map(x => AgentMapper.mapToAgent(x)) ?? [];
		this.dataSource = new MatTableDataSource<Agent>(this.dto);
		this.parentForm.setControl(this.controlName, new UntypedFormArray(this.dataSource.data.map(item => new UntypedFormControl(item))));
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

	private UpdateIsEditing(value: boolean) {
		this.isEditing = value;
		this.edit.emit(value);
	}

	private get resourceControl(): UntypedFormArray {
		return this.parentForm.get(this.controlName) as UntypedFormArray;
	}

	private filterLinkableAgents(list: Agent[], term: string): Agent[] {
		return list.filter((option: Agent) => this.getName(option).toLowerCase().includes(term.toLowerCase()));
	}

	private getName(option: Agent): string {
		return this.fallback.transform(option.name, this.currentLanguage) ?? '';
	}

	private mapValue(value: any): any {
		if (value) {
			return typeof value === 'string' ? value : value.name[this.currentLanguage];
		}
		return '';
	}
}
