import {Component, EventEmitter, Inject, OnInit, Output} from '@angular/core';
import {UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {MAT_DIALOG_DATA} from '@angular/material/dialog';
import {MultiLanguage} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {AnnotationDialogData} from './annnotation.dialog.data';
import {MultiLanguageMapper} from '../../mappers/multilanguagemapper';
import {URI_PATTERN} from 'src/app/app-constants';

@Component({
	selector: 'app-modal-dialog-annotation',
	templateUrl: './modal-dialog.component.html',
	standalone: false
})
export class ModalDialogAnnotationComponent implements OnInit {
	@Output() save: EventEmitter<AnnotationDialogData> = new EventEmitter();

	form!: UntypedFormGroup;

	constructor(@Inject(MAT_DIALOG_DATA) public data: AnnotationDialogData) {}

	ngOnInit(): void {
		this.form = new UntypedFormGroup({
			type: new UntypedFormControl('', [Validators.required]),
			title: new UntypedFormControl(''),
			identifier: new UntypedFormControl(''),
			text: new UntypedFormGroup(this.getObjectFromKeys(this.data.contentLanguages, () => new UntypedFormControl())),
			uri: new UntypedFormControl('', [Validators.pattern(URI_PATTERN)])
		});

		this.mapDataToForm();
	}

	get isSaveDisabled(): boolean {
		return !this.form.dirty;
	}

	safeClick() {
		if (this.isFormValid) {
			this.mapFormToData();
			this.save.emit(this.data);
		}
	}

	private get isFormValid(): boolean {
		this.form.markAllAsTouched();
		this.form.updateValueAndValidity();
		return this.form.valid;
	}

	private getObjectFromKeys<Type>(keys: readonly string[], initialValue: (key: string) => Type) {
		return Object.assign({}, ...keys.map(x => ({[x]: initialValue(x)})));
	}

	private mapDataToForm(): void {
		this.form.patchValue({
			text: new MultiLanguage(this.data.dto.text),
			title: this.data.dto.title,
			type: this.data.dto.type,
			identifier: this.data.dto.identifier,
			uri: this.data.dto.uri
		});
	}

	private mapFormToData(): void {
		this.data.dto.title = this.form.value.title || undefined;
		this.data.dto.identifier = this.form.value.identifier || undefined;
		this.data.dto.uri = this.form.value.uri || undefined;
		this.data.dto.type = this.form.value.type;
		const text = new MultiLanguage();
		this.data.contentLanguages.forEach((l: string) => {
			text[l as keyof MultiLanguage] = this.form.value.text[l] || undefined;
		});
		this.data.dto.text = MultiLanguageMapper.cloneOrUndefined(text);
	}
}
