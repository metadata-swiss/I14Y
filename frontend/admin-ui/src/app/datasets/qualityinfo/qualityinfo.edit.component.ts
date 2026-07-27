import {AfterViewInit, Component, inject, OnDestroy, OnInit, SimpleChanges} from '@angular/core';
import {UntypedFormControl, UntypedFormGroup} from '@angular/forms';
import {MatDialog} from '@angular/material/dialog';
import {ActivatedRoute, Router} from '@angular/router';
import {
	DatasetQualityInformation,
	DatasetQualityInformationClient,
	DatasetQualityInformationData,
	DatasetQualityQuestion,
	MultiLanguage
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {Observable, Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';
import {
	NAV_PARAM_FROM,
	NAV_VALUE_EDIT,
	DIALOG_CANCEL_BUTTON_KEY,
	DIALOG_CREATE_BUTTON_KEY,
	DIALOG_DISCARD_CHANGES_BUTTON_KEY,
	DIALOG_SAVE_CHANGES_BUTTON_KEY
} from 'src/app/app-constants';
import {DeactivationGuarded} from 'src/app/shared/deactivationguarded.interface';
import {DialogComponent, DialogType, IDialogConfig} from 'src/app/shared/dialog/dialog.component';
import {ResourceMapper} from 'src/app/shared/mappers/resourcemapper';
import {DatasetService} from '../services/dataset.service';

@Component({
	selector: 'app-qualityinfo.edit',
	templateUrl: './qualityinfo.edit.component.html',
	styleUrls: [],
	standalone: false
})
export class QualityInfoEditComponent implements OnInit, AfterViewInit, OnDestroy, DeactivationGuarded {
	currentLanguage: string;
	dto: DatasetQualityInformationData = new DatasetQualityInformationData();
	from = '';
	mode = '';
	title: MultiLanguage | undefined;

	cancelDialogConfig: IDialogConfig = {
		showHeader: true,
		enableSave: true,
		headerText: '',
		bodyText: '',
		dialogType: DialogType.save,
		okButtonText: '',
		cancelButtonText: '',
		confirmButtonText: '',
		discardChangesButtonText: '',
		saveChangesButtonText: ''
	};

	questions: DatasetQualityQuestion[] = [];

	form: UntypedFormGroup = new UntypedFormGroup({
		documentation: new UntypedFormControl('')
	});

	private readonly unsubscribe$ = new Subject();

	private readonly client = inject(DatasetQualityInformationClient);
	private readonly datasetService = inject(DatasetService);
	private readonly dialog = inject(MatDialog);
	private readonly notification = inject(ObNotificationService);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.from = this.route.snapshot.queryParams[NAV_PARAM_FROM];
		this.mode = this.route.snapshot.url[this.route.snapshot.url.length - 1].path;

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
			this.UpdateModalDialog();
		});
		const datasetId = this.route.snapshot.params.id;

		if (this.mode === NAV_VALUE_EDIT) {
			this.client.getByDatasetId(datasetId).subscribe(response => (this.dto = response.result));
		} else {
			this.dto = this.createEmptyDto(datasetId);
		}
		this.client.getDefinition().subscribe(response => (this.questions = response.result));
		this.datasetService.load(datasetId);
		this.datasetService.data$.subscribe(x => (this.title = x.title));
	}

	ngAfterViewInit() {
		this.form.valueChanges.pipe(takeUntil(this.unsubscribe$)).subscribe(() => {
			this.UpdateModalDialog();
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	onAnswerChanged(answer: DatasetQualityInformation) {
		var qIndex = this.dto.qualityInformations!.findIndex(q => q.questionId === answer.questionId);
		if (qIndex >= 0) {
			this.dto.qualityInformations?.splice(qIndex, 1, answer);
		} else {
			this.dto.qualityInformations?.push(answer);
		}
	}

	onFormChanges(changes: SimpleChanges): void {
		if (changes.dto) {
			const change = changes.dto;
			if (change.currentValue) {
				this.dto = Object.assign(change.currentValue);
				this.mapDataToForm();
			}
		}
	}

	onCancel(): void {
		this.form.markAsPristine();
		this.navigateBack();
	}

	onSave(): void {
		this.save();
	}

	onSaveAndClose(): void {
		this.mapFormToData();
		if (this.isEditMode()) {
			this.client.putByBody(this.dto).subscribe(_ => {
				this.showSuccessNotification();
				this.form.markAsPristine();
				this.navigateBack();
			});
		} else {
			this.client.postByBody(this.dto).subscribe(_ => {
				this.showSuccessNotification();
				this.form.markAsPristine();
				this.navigateBack();
			});
		}
	}

	canDeactivate(): boolean | Observable<boolean> | Promise<boolean> {
		return this.isNavigationAllowed();
	}

	isEditMode(): boolean {
		return this.mode === NAV_VALUE_EDIT;
	}

	private isNavigationAllowed(beforeunloadEvent = false): Promise<boolean> {
		return new Promise<boolean>(resolve => {
			if (!this.form.dirty) {
				resolve(true);
			} else {
				if (beforeunloadEvent) {
					resolve(false);
				} else {
					const dialogRef = this.dialog.open(DialogComponent, {
						data: this.cancelDialogConfig,
						disableClose: true
					});
					const dialogCancel = dialogRef.componentInstance.cancel.subscribe(() => {
						resolve(false);
					});
					const dialogDiscardChanges = dialogRef.componentInstance.discardChanges.subscribe(() => {
						resolve(true);
					});
					const dialogSaveChanges = dialogRef.componentInstance.saveChanges.subscribe(() => {
						this.save().then(() => {
							resolve(true);
						});
					});
					dialogRef.afterClosed().subscribe(() => {
						dialogCancel.unsubscribe();
						dialogDiscardChanges.unsubscribe();
						dialogSaveChanges.unsubscribe();
					});
				}
			}
		});
	}

	private save(): Promise<void> {
		return new Promise<void>(resolve => {
			this.mapFormToData();
			if (this.isEditMode()) {
				this.client.putByBody(this.dto).subscribe(_ => {
					this.showSuccessNotification();
					this.form.markAsPristine();
					resolve();
				});
			} else {
				this.client.postByBody(this.dto).subscribe(_ => {
					this.showSuccessNotification();
					this.form.markAsPristine();
					resolve();
				});
			}
		});
	}

	private navigateBack(): void {
		this.router.navigate(['../'], {relativeTo: this.route});
	}

	private showSuccessNotification(): void {
		this.notification.success('i18n.notification.save_succeeded');
	}

	private UpdateModalDialog(): void {
		const headertextKey: string = this.isEditMode() ? 'i18n.create.cancel_dialog.headertext' : 'i18n.datasets.qualityinfo.create.canceldialog.headertext';
		const bodytextKey: string = this.isEditMode() ? 'i18n.edit.cancel_dialog.bodytext' : 'i18n.create.cancel_dialog.bodytext';

		this.translate // eslint-disable-next-line max-len
			.get([headertextKey, bodytextKey, DIALOG_CANCEL_BUTTON_KEY, DIALOG_DISCARD_CHANGES_BUTTON_KEY, DIALOG_SAVE_CHANGES_BUTTON_KEY, DIALOG_CREATE_BUTTON_KEY])
			.subscribe(result => {
				this.cancelDialogConfig = {
					showHeader: true,
					enableSave: this.form.valid,
					headerText: result[headertextKey],
					bodyText: result[bodytextKey],
					dialogType: DialogType.save,
					okButtonText: '',
					cancelButtonText: result[DIALOG_CANCEL_BUTTON_KEY],
					confirmButtonText: '',
					discardChangesButtonText: result[DIALOG_DISCARD_CHANGES_BUTTON_KEY],
					saveChangesButtonText: this.isEditMode() ? result[DIALOG_SAVE_CHANGES_BUTTON_KEY] : result[DIALOG_CREATE_BUTTON_KEY]
				};
			});
	}

	private mapDataToForm(): void {
		this.form.patchValue({emitEvent: false, onlySelf: true});

		this.form.markAsUntouched();
	}

	private mapFormToData() {
		this.dto.documentation = ResourceMapper.mapElements(this.form.value.documentation);

		this.questions.forEach(question => {
			var qIndex = this.dto.qualityInformations?.findIndex(q => q.questionId === question.id);
			if (qIndex === undefined || qIndex < 0) {
				return;
			}

			var questionInfo = this.dto.qualityInformations?.find(q => q.questionId === question.id);
			if (questionInfo === undefined) {
				return;
			}

			var answer = question.answerOptions?.find(a => a.id === questionInfo?.answerId);
			if (answer === undefined) {
				return;
			}

			var answerControl = this.form.controls['details' + answer.id];
			if (answer.detailMandatory && answerControl !== undefined) {
				questionInfo.detail = answerControl.value;
			} else {
				questionInfo.detail = '';
			}

			if (answerControl && answer.detailMandatory === false) {
				answerControl.setValue('');
			}

			this.dto.qualityInformations?.splice(qIndex, 1);
			this.dto.qualityInformations?.push(questionInfo);
		});
	}

	private createEmptyDto(datasetId: string): DatasetQualityInformationData {
		return new DatasetQualityInformationData({
			datasetId: datasetId,
			documentation: [],
			qualityInformations: []
		});
	}
}

function resolve(arg0: boolean) {
	throw new Error('Function not implemented.');
}
