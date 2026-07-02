import {Component, EventEmitter, inject, Input, Output} from '@angular/core';
import {MatDialog} from '@angular/material/dialog';
import {DialogComponent, DialogType, IDialogConfig} from '../dialog/dialog.component';

@Component({
	selector: 'app-modal-dialog',
	templateUrl: './modal-dialog.component.html',
	standalone: false
})
export class ModalDialogComponent {
	@Input() openDialogButtonText = 'open Dialog';
	@Input() buttonType: 'primary' | 'secondary' | 'tertiary' = 'primary';
	@Input() dialogConfig: IDialogConfig = {
		showHeader: true,
		enableSave: true,
		headerText: '',
		bodyText: '',
		dialogType: DialogType.info,
		okButtonText: '',
		cancelButtonText: '',
		confirmButtonText: '',
		saveChangesButtonText: '',
		discardChangesButtonText: ''
	};
	@Output() beforeOpenDialog: EventEmitter<void> = new EventEmitter();
	@Output() cancel: EventEmitter<void> = new EventEmitter();
	@Output() confirm: EventEmitter<void> = new EventEmitter();
	@Output() discardChanges: EventEmitter<void> = new EventEmitter();
	@Output() saveChanges: EventEmitter<void> = new EventEmitter();

	private readonly dialog = inject(MatDialog);

	onClick() {
		this.beforeOpenDialog.emit();
	}

	openDialog() {
		const dialogRef = this.dialog.open(DialogComponent, {
			data: this.dialogConfig,
			disableClose: true
		});
		const dialogCancel = dialogRef.componentInstance.cancel.subscribe(() => {
			this.cancel.emit();
		});
		const dialogConfirm = dialogRef.componentInstance.confirm.subscribe(() => {
			this.confirm.emit();
		});
		const dialogDiscardChanges = dialogRef.componentInstance.discardChanges.subscribe(() => {
			this.discardChanges.emit();
		});
		const dialogSaveChanges = dialogRef.componentInstance.saveChanges.subscribe(() => {
			this.saveChanges.emit();
		});
		dialogRef.afterClosed().subscribe(() => {
			dialogCancel.unsubscribe();
			dialogConfirm.unsubscribe();
			dialogDiscardChanges.unsubscribe();
			dialogSaveChanges.unsubscribe();
		});
	}
}
