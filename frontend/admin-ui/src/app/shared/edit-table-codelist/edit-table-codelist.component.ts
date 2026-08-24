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
import {CodeListEntryDetail, ConceptInputClient, MultiLanguage} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {SelectionModel} from '@angular/cdk/collections';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {Languages} from 'src/app/shared/ApplicationLanguage.enum';
import {MultiLanguageMapper} from '../mappers/multilanguagemapper';
import {MatDialog, MatDialogConfig} from '@angular/material/dialog';
import {ModalDialogCodeListComponent} from './modal-dialog/modal-dialog.component';
import {DialogComponent, DialogType} from '../dialog/dialog.component';
import {DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY} from '../../app-constants';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';
import {FormatFunctions} from '../format-functions';
import {CodelistEntryDialogData} from './modal-dialog/codelistentry.dialog.data';

@Component({
	selector: 'app-edit-table-codelist',
	templateUrl: './edit-table-codelist.component.html',
	styleUrls: ['./edit-table-codelist.component.scss'],
	standalone: false
})
export class EditTableCodelistComponent implements AfterViewInit, OnChanges, OnDestroy {
	@ViewChild(MatSort) sort!: MatSort;
	@ViewChildren('focusInputField') focusInputFields!: QueryList<ElementRef>;
	@Input() conceptId: string | undefined;
	@Input() isNewCodeList: boolean = false;
	@Input() showAllLanguages: boolean = false;
	@Input() currentLanguage: string;
	@Input() controlName!: string;
	@Output() updateCodeListEntries: EventEmitter<void> = new EventEmitter();
	@Input() dto!: CodeListEntryDetail[];
	public dataSource = new MatTableDataSource<CodeListEntryDetail>([]);

	contentLanguages: readonly string[] = Languages.ContentLanguagesRm;

	COLUMN_VALUE = 'value';
	COLUMN_PARENTCODE = 'parentCode';
	COLUMN_SELECT = 'select';
	COLUMN_ACTIONS = 'actions';

	columnsToDisplayWithExpand = [this.COLUMN_SELECT, this.COLUMN_VALUE, this.COLUMN_PARENTCODE, ...this.contentLanguages, 'expand', this.COLUMN_ACTIONS];
	expandedElement = signal<CodeListEntryDetail | null>(null);

	private selection = new SelectionModel<CodeListEntryDetail>(true, []);
	private readonly unsubscribe$ = new Subject();

	private readonly conceptInputClient = inject(ConceptInputClient);
	private readonly dialog = inject(MatDialog);
	private readonly notification = inject(ObNotificationService);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);
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
				this.dataSource = new MatTableDataSource<CodeListEntryDetail>(this.dto);
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
			if (index >= 0) {
				const row = this.dataSource.data.splice(index, 1);
				this.deleteRequest(row[0]).then(success => {
					if (success) {
						this.showSuccessNotification();
						this.getCodeListEntries();
						this.refreshDatabinding();
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
			const promises = this.selection.selected.map(async (item: CodeListEntryDetail) => {
				const index = this.dataSource.data.findIndex((d: CodeListEntryDetail) => d === item);
				if (index >= 0) {
					const row = this.dataSource.data.splice(index, 1);
					return this.deleteRequest(row[0]);
				}

				return Promise.resolve(false);
			});

			Promise.all(promises).then(results => {
				if (results.every(success => success)) {
					this.showSuccessNotification();
				}

				this.getCodeListEntries();
				this.refreshDatabinding();
			});

			this.selection = new SelectionModel<CodeListEntryDetail>(true, []);
		});
		dialogRef.afterClosed().subscribe(() => {
			dialogConfirm.unsubscribe();
		});
	}

	toggleDetail(element: CodeListEntryDetail) {
		this.expandedElement.update(e => (e !== element ? element : null));
	}

	addRow(): void {
		this.showDialog(this.newEntry(), false);
	}

	editRow(row: CodeListEntryDetail): void {
		row.description = row.description ?? MultiLanguageMapper.fixEmptyValues(new MultiLanguage());

		this.showDialog(row, true);
	}

	onFindIndex(row: CodeListEntryDetail): number {
		return this.dataSource.data.findIndex(x => x === row);
	}

	displayParentCode(code: CodeListEntryDetail) {
		return code?.value ?? '';
	}

	canAdd(): boolean {
		return !this.hasSelectedItems();
	}

	canCreateCodeValues(): boolean {
		return this.isNewCodeList;
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

	isRowSelected(row: CodeListEntryDetail): boolean {
		return this.selection.isSelected(row);
	}

	reloadEntries() {
		this.getCodeListEntries();
	}

	onToggleSelection(row: CodeListEntryDetail): void {
		this.selection.toggle(row);
	}

	getFormattedDate(date: Date | undefined): string | null {
		return FormatFunctions.getFormattedDate(date);
	}

	private hasSelectedItems(): boolean {
		return !this.selection.isEmpty();
	}

	private newEntry(): CodeListEntryDetail {
		return new CodeListEntryDetail({
			conceptId: this.conceptId,
			value: undefined,
			parentCode: undefined,
			name: MultiLanguageMapper.fixEmptyValues(new MultiLanguage()),
			description: MultiLanguageMapper.fixEmptyValues(new MultiLanguage())
		});
	}

	private getCodeListEntries(): void {
		this.updateCodeListEntries.emit();
	}

	private refreshDatabinding(): void {
		this.dataSource.filter = '';
	}

	private putRequest(codelistEntry: CodeListEntryDetail): Promise<boolean> {
		return new Promise<boolean>(resolve => {
			this.conceptInputClient.putCodelistEntriesByIdAndCodeListEntryIdAndBody(this.conceptId!, codelistEntry.id!, codelistEntry).subscribe({
				next: () => {
					resolve(true);
				},
				error: (error: {detail?: string}) => {
					resolve(false);
					this.showErrorNotification(error);
				}
			});
		});
	}

	private deleteRequest(codelistEntry: CodeListEntryDetail): Promise<boolean> {
		return new Promise<boolean>(resolve => {
			if (codelistEntry.id) {
				const skippedErrorNotifications = 1;
				this.obHttpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(skippedErrorNotifications);
				this.conceptInputClient.deleteCodelistEntriesByIdAndCodeListEntryId(this.conceptId!, codelistEntry.id).subscribe({
					next: () => {
						resolve(true);
					},
					error: (error: {detail?: string}) => {
						resolve(false);
						this.showErrorNotification(error);
					}
				});
			}
		});
	}

	private postRequest(codelistEntry: CodeListEntryDetail): Promise<boolean> {
		return new Promise<boolean>(resolve => {
			this.conceptInputClient.postCodelistEntriesByIdAndBody(this.conceptId!, codelistEntry).subscribe({
				next: () => {
					resolve(true);
				},
				error: (error: {detail?: string}) => {
					resolve(false);
					this.showErrorNotification(error);
				}
			});
		});
	}

	private showDialog(entry: CodeListEntryDetail, isEdit: boolean) {
		const dialogRef = this.dialog.open(ModalDialogCodeListComponent, this.createDialogConfig(entry, isEdit));

		const dialogSave = dialogRef.componentInstance.save.subscribe((data: CodelistEntryDialogData) => {
			if (data as CodelistEntryDialogData) {
				if (data.dto.id) {
					this.putRequest(data.dto).then(success => {
						if (success) {
							dialogRef.close();
							this.showSuccessNotification();
							this.getCodeListEntries();
							this.refreshDatabinding();
						}
					});
				} else {
					this.postRequest(data.dto).then(success => {
						if (success) {
							dialogRef.close();
							this.showSuccessNotification();
							this.getCodeListEntries();
							this.refreshDatabinding();
						}
					});
				}
			}
		});

		dialogRef.afterClosed().subscribe(() => {
			dialogSave.unsubscribe();
		});
	}

	private showSuccessNotification(): void {
		this.notification.success('i18n.notification.save_succeeded');
	}

	private showErrorNotification(error: {detail?: string}): void {
		this.notification.error(
			error?.detail ? {message: 'i18n.notification.error_detail', messageParams: {error: error.detail}, sticky: true} : 'i18n.notification.save_error'
		);
	}

	private createDialogConfig(entry: CodeListEntryDetail, isEdit: boolean): MatDialogConfig<CodelistEntryDialogData> {
		let dialogConfig = new MatDialogConfig<CodelistEntryDialogData>();
		dialogConfig.data = new CodelistEntryDialogData(
			this.contentLanguages,
			this.conceptId,
			entry,
			isEdit ? 'i18n.title.edit_code_value' : 'i18n.title.add_code_value'
		);
		dialogConfig.width = '70%';
		dialogConfig.maxWidth = '1200px';
		dialogConfig.minWidth = '600px';
		dialogConfig.disableClose = true;
		dialogConfig.autoFocus = true;
		return dialogConfig;
	}
}
