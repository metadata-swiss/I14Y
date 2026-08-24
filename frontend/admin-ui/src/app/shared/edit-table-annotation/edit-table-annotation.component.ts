import {SelectionModel} from '@angular/cdk/collections';
import {AfterViewInit, Component, EventEmitter, inject, Input, OnChanges, OnDestroy, Output, SimpleChanges, ViewChild} from '@angular/core';
import {MatSort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {Annotation, ConceptInputClient, MultiLanguage} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Languages} from '../ApplicationLanguage.enum';
import {FallbackPipe} from '../fallback/fallback.pipe';
import {MultiLanguageMapper} from '../mappers/multilanguagemapper';
import {MatDialog, MatDialogConfig} from '@angular/material/dialog';
import {ModalDialogAnnotationComponent} from './modal-dialog/modal-dialog.component';
import {ObHttpApiInterceptorEvents, ObNotificationService} from '@oblique/oblique';
import {Subject, takeUntil} from 'rxjs';
import {AnnotationDialogData} from './modal-dialog/annnotation.dialog.data';
import {AnnotationInputModelMapper} from '../mappers/annotationinputmodelmapper';
import {DialogComponent, DialogType} from '../dialog/dialog.component';
import {DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY} from 'src/app/app-constants';

@Component({
	selector: 'app-edit-table-annotation',
	templateUrl: './edit-table-annotation.component.html',
	styleUrls: ['./edit-table-annotation.component.scss'],
	standalone: false
})
export class EditTableAnnotationComponent implements AfterViewInit, OnChanges, OnDestroy {
	@ViewChild(MatSort, {static: false}) sort!: MatSort;
	@Input() conceptId: string | undefined;
	@Input() codelistEntryId!: string;
	@Input() annotations: Annotation[] = [];
	@Input() currentLanguage!: string;
	@Input() controlName!: string;
	@Output() reloadEntriesEvent = new EventEmitter();

	dataSource = new MatTableDataSource<Annotation>(this.annotations);

	COLUMN_SELECT = 'select';
	COLUMN_TYPE = 'type';
	COLUMN_TITLE = 'title';
	COLUMN_TEXT = 'text';
	COLUMN_URI = 'uri';
	COLUMN_IDENTIFIER = 'identifier';
	COLUMN_ACTIONS = 'actions';

	contentLanguages: readonly string[] = Languages.ContentLanguagesRm;

	displayedColumns: string[] = [
		this.COLUMN_SELECT,
		this.COLUMN_TYPE,
		this.COLUMN_TITLE,
		this.COLUMN_TEXT,
		this.COLUMN_URI,
		this.COLUMN_IDENTIFIER,
		this.COLUMN_ACTIONS
	];

	private selection = new SelectionModel<Annotation>(true, []);
	private readonly unsubscribe$ = new Subject();

	private readonly conceptInputClient = inject(ConceptInputClient);
	private readonly dialog = inject(MatDialog);
	private readonly fallbackPipe = inject(FallbackPipe);
	private readonly notification = inject(ObNotificationService);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => (this.currentLanguage = language.lang));
	}

	ngAfterViewInit(): void {
		this.dataSource.sort = this.sort;
		this.sort.sortChange.subscribe(() => {
			this.sortpage();
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	ngOnChanges(changes: SimpleChanges): void {
		if (changes.annotations) {
			if (this.annotations) {
				this.dataSource = new MatTableDataSource<Annotation>(this.annotations);
			}
		}
	}

	canAdd(): boolean {
		return !this.hasSelectedItems();
	}

	onEditRow(row: Annotation): void {
		this.showDialog(row);
	}

	onAddRow(): void {
		this.showDialog(this.newEntry());
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
			const promises = this.selection.selected.map(async (item: Annotation) => {
				const index = this.dataSource.data.findIndex((d: Annotation) => d === item);
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
				
				this.reloadEntriesEvent.emit();
			});			

			this.selection = new SelectionModel<Annotation>(true, []);
		});

		dialogRef.afterClosed().subscribe(() => {
			dialogConfirm.unsubscribe();
		});
	}

	onRemoveRow(index: number): void {
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
						this.reloadEntriesEvent.emit();
					}
				});
			}
		});

		dialogRef.afterClosed().subscribe(() => {
			dialogConfirm.unsubscribe();
		});
	}

	onMasterToggle(): void {
		if (this.isAllSelected()) {
			this.selection.clear();
		} else {
			this.dataSource.data.forEach(row => this.selection.select(row));
		}
	}

	onToggleSelection(row: Annotation): void {
		this.selection.toggle(row);
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

	isRowSelected(row: Annotation): boolean {
		return this.selection.isSelected(row);
	}

	private newEntry(): Annotation {
		return new Annotation({
			identifier: undefined,
			text: MultiLanguageMapper.fixEmptyValues(new MultiLanguage()),
			title: undefined,
			type: undefined,
			uri: undefined
		});
	}
	private sortpage() {
		switch (this.sort?.active) {
			case this.COLUMN_TEXT:
				this.annotations.sort((a, b) => {
					const aText = this.fallbackPipe.transform(a.text, this.currentLanguage);
					const bText = this.fallbackPipe.transform(b.text, this.currentLanguage);
					return this.sortSub(aText!, bText!);
				});
				break;
			case this.COLUMN_TYPE:
				this.annotations.sort((a, b) => {
					return this.sortSub(a.type!.toString(), b.type!.toString());
				});

				break;
			case this.COLUMN_TITLE:
				this.annotations.sort((a, b) => {
					return this.sortSub(a.title!.toString(), b.title!.toString());
				});
				break;
			case this.COLUMN_URI:
				this.annotations.sort((a, b) => {
					return this.sortSub(a.uri!.toString(), b.uri!.toString());
				});
				break;
			case this.COLUMN_IDENTIFIER:
				this.annotations.sort((a, b) => {
					return this.sortSub(a.identifier!.toString(), b.identifier!.toString());
				});
		}

		this.dataSource = new MatTableDataSource<Annotation>(this.annotations);
	}

	private sortSub(a: string, b: string): number {
		if (this.sort.direction === 'asc') {
			return a.toLowerCase().localeCompare(b.toLowerCase());
		} else {
			return b.toLowerCase().localeCompare(a.toLowerCase());
		}
	}

	private hasSelectedItems(): boolean {
		return !this.selection.isEmpty();
	}

	private showDialog(entry: Annotation) {
		this.updateDialogConfig(entry);

		const dialogRef = this.dialog.open(ModalDialogAnnotationComponent, this.updateDialogConfig(entry));

		const dialogSave = dialogRef.componentInstance.save.subscribe((data: AnnotationDialogData) => {
			if (data as AnnotationDialogData) {
				dialogRef.componentInstance.disableSave = true;
				if (data.dto.id) {
					this.putRequest(data.dto).then(success => {
						if (success) {
							dialogRef.close();
							this.showSuccessNotification();
							this.reloadEntriesEvent.emit();
						}
						dialogRef.componentInstance.disableSave = false;
					});
				} else {
					this.postRequest(data.dto).then(success => {
						if (success) {
							dialogRef.close();
							this.showSuccessNotification();
							this.reloadEntriesEvent.emit();
						}
						dialogRef.componentInstance.disableSave = false;
					});
				}
			}
		});

		dialogRef.afterClosed().subscribe(() => {
			dialogSave.unsubscribe();
		});
	}

	private putRequest(annotation: Annotation): Promise<boolean> {
		return new Promise<boolean>(resolve => {
			this.conceptInputClient
				.putCodelistEntriesAnnotationsByIdAndCodeListEntryIdAndAnnotationIdAndBody(
					this.conceptId!,
					this.codelistEntryId,
					annotation.id!,
					AnnotationInputModelMapper.mapToInputModel(annotation)
				)
				.subscribe({
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

	private deleteRequest(annotation: Annotation): Promise<boolean> {
		return new Promise<boolean>(resolve => {
			if (annotation.id) {
				const skippedErrorNotifications = 1;
				this.obHttpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(skippedErrorNotifications);
				this.conceptInputClient
					.deleteCodelistEntriesAnnotationsByIdAndCodeListEntryIdAndAnnotationId(this.conceptId!, this.codelistEntryId, annotation.id)
					.subscribe({
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

	private postRequest(annotation: Annotation): Promise<boolean> {
		return new Promise<boolean>(resolve => {
			this.conceptInputClient
				.postCodelistEntriesAnnotationsByIdAndCodeListEntryIdAndBody(
					this.conceptId!,
					this.codelistEntryId,
					AnnotationInputModelMapper.mapToInputModel(annotation)
				)
				.subscribe({
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

	private updateDialogConfig(entry: Annotation): MatDialogConfig<AnnotationDialogData> {
		let dialogConfig = new MatDialogConfig<AnnotationDialogData>();
		dialogConfig.data = new AnnotationDialogData(this.contentLanguages, entry, entry.id ? 'i18n.title.edit_annotation' : 'i18n.title.add_annotation');
		dialogConfig.width = '70%';
		dialogConfig.maxWidth = '1200px';
		dialogConfig.minWidth = '600px';
		dialogConfig.disableClose = true;
		dialogConfig.autoFocus = true;
		return dialogConfig;
	}

	private showSuccessNotification(): void {
		this.notification.success('i18n.notification.save_succeeded');
	}

	private showErrorNotification(error: {detail?: string}): void {
		this.notification.error(
			error?.detail ? {message: 'i18n.notification.error_detail', messageParams: {error: error.detail}, sticky: true} : 'i18n.notification.save_error'
		);
	}
}
