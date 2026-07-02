import {Component, inject, Inject, OnInit} from '@angular/core';
import {UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import {MappingRelationUriModel, VocabularyClient, VocabularyEntry} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService} from '@ngx-translate/core';
import {URI_PATTERN} from 'src/app/app-constants';
import {VocabularyEntryMapper} from 'src/app/shared/mappers/vocabularyentrymapper';
import {MappingRelationDialogData} from './mapping-relation.dialog.data';

@Component({
	selector: 'app-modal-dialog-mapping-relation',
	templateUrl: './modal-dialog.component.html',
	standalone: false
})
export class ModalDialogMappingRelationComponent implements OnInit {
	form: UntypedFormGroup = new UntypedFormGroup({
		source: new UntypedFormControl('', {
			validators: [Validators.required, Validators.pattern(URI_PATTERN)]
		}),
		target: new UntypedFormControl('', {
			validators: [Validators.required, Validators.pattern(URI_PATTERN)]
		}),
		relationType: new UntypedFormControl('', {
			validators: [Validators.required]
		})
	});

	relationTypes: VocabularyEntry[] = [];
	currentLanguage: string;

	private readonly mappingPredicate = 'VOCAB_I14Y_MAPPING_PREDICATE';

	private readonly dialogRef = inject(MatDialogRef<ModalDialogMappingRelationComponent>);
	private readonly vocabularyClient = inject(VocabularyClient);
	private readonly translate = inject(TranslateService);

	constructor(@Inject(MAT_DIALOG_DATA) public data: MappingRelationDialogData) {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.getRelationTypes().then(relationTypes => {
			this.relationTypes = relationTypes;
			this.mapDataToForm();
		});
	}

	get isSaveDisabled(): boolean {
		return !this.form.dirty;
	}

	save() {
		if (this.isFormValid) {
			this.mapFormToData();
			this.dialogRef.close(this.data);
		}
	}

	private get isFormValid(): boolean {
		this.form.markAllAsTouched();
		this.form.updateValueAndValidity();
		return this.form.valid;
	}

	private getRelationTypes(): Promise<VocabularyEntry[]> {
		return new Promise<VocabularyEntry[]>(resolve => {
			this.vocabularyClient.getByIdentifier(this.mappingPredicate).subscribe(response => {
				resolve(response.result);
			});
		});
	}

	private mapDataToForm(): void {
		this.form.patchValue({
			source: this.data.dto.source?.uri,
			target: this.data.dto.target?.uri,
			relationType: this.data.dto.relationType?.code
		});
	}

	private mapFormToData(): void {
		this.data.dto.source = new MappingRelationUriModel({uri: this.form.value.source});
		this.data.dto.target = new MappingRelationUriModel({uri: this.form.value.target});
		let relationType = this.relationTypes.find(x => x.code === this.form.value.relationType);
		if (relationType) {
			this.data.dto.relationType = VocabularyEntryMapper.mapToVocabularyEntryModel(relationType);
		}
	}
}
