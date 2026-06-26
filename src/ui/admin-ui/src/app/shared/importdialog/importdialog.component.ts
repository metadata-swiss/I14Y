import {Component, EventEmitter, Inject, Output} from '@angular/core';
import {MAT_DIALOG_DATA} from '@angular/material/dialog';
import {ObIUploadEvent} from '@oblique/oblique';

export interface IImportDialogConfig {
	accept: string[];
	headerTextKey: string;
}

@Component({
	selector: 'app-dialog',
	templateUrl: './importdialog.component.html',
	standalone: false
})
export class ImportDialogComponent {
	@Output() cancel: EventEmitter<void> = new EventEmitter();
	@Output() upload: EventEmitter<ObIUploadEvent> = new EventEmitter<ObIUploadEvent>();

	constructor(@Inject(MAT_DIALOG_DATA) public dialogConfig: IImportDialogConfig) {}

	onCancelClick() {
		this.cancel.emit();
	}

	uploadEvent(event: ObIUploadEvent) {
		this.upload.emit(event);
	}
}
