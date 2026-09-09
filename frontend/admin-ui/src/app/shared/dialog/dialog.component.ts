import {Component, EventEmitter, Inject, Output} from '@angular/core';
import {MAT_DIALOG_DATA} from '@angular/material/dialog';

export interface IDialogConfig {
	showHeader: boolean;
	enableSave: boolean;
	headerText: string;
	bodyText: string;
	dialogType: DialogType;
	okButtonText: string;
	confirmButtonText: string;
	cancelButtonText: string;
	saveChangesButtonText: string;
	discardChangesButtonText: string;
}

// eslint-disable-next-line no-shadow
export enum DialogType {
	info,
	confirm,
	save
}

@Component({
	selector: 'app-dialog',
	templateUrl: './dialog.component.html',
	standalone: false
})
export class DialogComponent {
	dialogType = DialogType;
	@Output() cancel: EventEmitter<void> = new EventEmitter();
	@Output() confirm: EventEmitter<void> = new EventEmitter();
	@Output() discardChanges: EventEmitter<void> = new EventEmitter();
	@Output() saveChanges: EventEmitter<void> = new EventEmitter();

	constructor(@Inject(MAT_DIALOG_DATA) public dialogConfig: IDialogConfig) {}

	onCancelClick() {
		this.cancel.emit();
	}

	onConfirmClick() {
		this.confirm.emit();
	}

	onDiscardChangesClick() {
		this.discardChanges.emit();
	}

	onSaveChangesClick() {
		this.saveChanges.emit();
	}
}
