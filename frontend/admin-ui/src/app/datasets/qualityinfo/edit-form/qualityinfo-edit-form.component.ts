import {ChangeDetectionStrategy, Component, EventEmitter, inject, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, ViewChild} from '@angular/core';
import {UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {
	DatasetQualityAnswerOption,
	DatasetQualityInformation,
	DatasetQualityInformationClient,
	DatasetQualityInformationData,
	DatasetQualityQuestion
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService, LangChangeEvent} from '@ngx-translate/core';
import {IDialogConfig} from 'src/app/shared/dialog/dialog.component';
import {MatAccordion} from '@angular/material/expansion';
import {ActivatedRoute} from '@angular/router';
import {MatSelectChange} from '@angular/material/select';
import {ModalDialogComponent} from 'src/app/shared/modal-dialog/modal-dialog.component';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';

@Component({
	selector: 'app-qualityinfo-edit-form',
	templateUrl: './qualityinfo-edit-form.component.html',
	styleUrls: ['./qualityinfo-edit-form.component.scss'],
	changeDetection: ChangeDetectionStrategy.OnPush,
	standalone: false
})
export class QualityInfoEditFormComponent implements OnInit, OnDestroy, OnChanges {
	id!: string;
	@Output() cancel: EventEmitter<void> = new EventEmitter();
	@Output() saveAndClose: EventEmitter<void> = new EventEmitter();
	@Output() save: EventEmitter<void> = new EventEmitter();
	@Output() formChanges: EventEmitter<SimpleChanges> = new EventEmitter();
	@Output() answerChanged: EventEmitter<DatasetQualityInformation> = new EventEmitter();
	@Input() dto: DatasetQualityInformationData = new DatasetQualityInformationData();
	@Input() form!: UntypedFormGroup;
	@Input() cancelDialogConfig!: IDialogConfig;
	@Input() isEditMode!: boolean;
	@ViewChild(MatAccordion) accordion!: MatAccordion;
	@ViewChild(ModalDialogComponent) modalDialog!: ModalDialogComponent;

	public questions: DatasetQualityQuestion[] = [];
	currentLanguage: string;
	private readonly editingChildren: string[] = [];
	private readonly unsubscribe$ = new Subject();

	private readonly qualityInfoClient = inject(DatasetQualityInformationClient);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.id = this.route.snapshot.params.id;
	}

	ngOnChanges(changes: SimpleChanges): void {
		this.formChanges.emit(changes);
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => (this.currentLanguage = language.lang));
		this.qualityInfoClient.getDefinition().subscribe(response => {
			this.questions = response.result;

			if (this.isEditMode) {
				this.mapDataToForm();
				this.qualityInfoClient.getByDatasetId(this.id).subscribe(infoResult => {
					this.dto = infoResult.result;
					this.dto.qualityInformations?.forEach(q => {
						if (q.detail) {
							this.form.controls['details' + q.answerId].setValue(q.detail);
						}
					});
				});
			} else {
				this.dto = this.createEmptyDto();
			}
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	onAnswerChanged(event: MatSelectChange, questionId: string | undefined): void {
		if (event.value === undefined) {
			return;
		}
		var answer = this.dto.qualityInformations?.find(a => a.questionId === questionId);
		if (answer === undefined) {
			answer = new DatasetQualityInformation({
				answerId: event.value,
				questionId: questionId
			});
		} else {
			answer.answerId = event.value;
		}

		this.answerChanged.emit(answer);
	}

	showAnswerDetails(questionId: string | undefined): boolean {
		var question = this.questions.find(q => q.id === questionId);
		var answerId = this.form.controls[questionId ? questionId : 'question'].value;
		var answer = question?.answerOptions?.find(a => a.id === answerId);
		if (answer !== undefined && answer.detailMandatory) {
			return answer.detailMandatory;
		}
		return false;
	}

	getdAnswerId(question: DatasetQualityQuestion): string | undefined {
		var answer = this.dto.qualityInformations?.find(a => a.questionId === question.id);
		if (answer) {
			return answer.answerId;
		}
		return undefined;
	}

	getSelectedAnswer(question: DatasetQualityQuestion): DatasetQualityAnswerOption | undefined {
		var answerId = this.form.controls[question.id ? question.id : 'question'].value;
		var answer = question?.answerOptions?.find(a => a.id === answerId);
		if (answer !== undefined && answer.detailMandatory) {
			return answer;
		}
		return undefined;
	}

	onBeforeOpenDialog(): void {
		this.modalDialog.openDialog();
	}

	onChildIsEditing(name: string, isEditing: boolean): void {
		if (isEditing) {
			this.editingChildren.push(name);
		} else {
			const index = this.editingChildren.indexOf(name, 0);
			if (index > -1) {
				this.editingChildren.splice(index, 1);
			}
		}
	}

	onSave(): void {
		if (this.isFormValid) {
			this.save.emit();
		}
	}

	onSaveAndClose(): void {
		if (this.isFormValid) {
			this.saveAndClose.emit();
		}
	}

	onCancel(): void {
		this.cancel.emit();
	}

	private get isFormValid(): boolean {
		this.form.markAllAsTouched();
		this.form.updateValueAndValidity();
		return this.form.valid;
	}

	private createEmptyDto(): DatasetQualityInformationData {
		var qualityData = new DatasetQualityInformationData();
		var questions: DatasetQualityInformation[] = [];
		this.questions.forEach(question => {
			var questionControl = question.mandatory ? new UntypedFormControl('', Validators.required) : new UntypedFormControl('');
			this.form.addControl(question.id ? question.id : 'question', questionControl);

			question.answerOptions?.forEach(answerOption => {
				if (answerOption.detailMandatory) {
					var detailControl = new UntypedFormControl('');
					this.form.addControl('details' + answerOption.id, detailControl);
				}
			});

			var info = new DatasetQualityInformation({
				id: undefined,
				questionId: question.id,
				answerId: undefined,
				detail: ''
			});
			questions.push(info);
		});
		qualityData.qualityInformations = questions;

		return qualityData;
	}

	private mapDataToForm(): void {
		this.questions?.forEach(question => {
			var answer = this.dto.qualityInformations?.find(a => a.questionId === question.id);
			var answerId = answer ? answer.answerId : undefined;
			var questionControl = question.mandatory ? new UntypedFormControl(answerId || '', Validators.required) : new UntypedFormControl(answerId || '');
			this.form.addControl(question.id ? question.id : 'question', questionControl);

			question.answerOptions?.forEach(answerOption => {
				if (answerOption.detailMandatory) {
					var detailControl = new UntypedFormControl(answer?.detail);
					this.form.addControl('details' + answerOption.id, detailControl);
				}
			});
		});
	}
}
