import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {Subject, takeUntil} from 'rxjs';
import {TranslateService} from '@ngx-translate/core';
import {
	DatasetQualityAnswerOption,
	DatasetQualityInformation,
	DatasetQualityInformationClient,
	DatasetQualityInformationData,
	DatasetQualityQuestion
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {DatasetQualityInformationDataService} from '../services/dataset-quality-information-data.service';

@Component({
	selector: 'app-qualityinfo',
	templateUrl: './qualityinfo.component.html',
	styleUrls: ['./qualityinfo.component.scss'],
	standalone: false
})
export class QualityInfoComponent implements OnInit, OnDestroy {
	currentLanguage: string;
	questions: DatasetQualityQuestion[] = Array();
	datasetQualityInformations: DatasetQualityInformationData;

	private readonly unsubscribe$ = new Subject();

	private readonly datasetQualityInformationDataService = inject(DatasetQualityInformationDataService);
	private readonly client = inject(DatasetQualityInformationClient);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.datasetQualityInformationDataService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(data => {
			this.datasetQualityInformations = data;
			this.loadQuestions();
		});
	}

	loadQuestions() {
		this.client.getDefinition().subscribe(reponse => {
			this.questions = reponse.result.sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
		});
	}

	getAnswer(question: DatasetQualityQuestion): DatasetQualityInformation | undefined {
		let answer: DatasetQualityInformation | undefined;
		if (question.id) {
			answer = this.datasetQualityInformations.qualityInformations?.find(q => q.questionId === question.id);
		}

		return answer;
	}

	getAnswerOption(question: DatasetQualityQuestion): DatasetQualityAnswerOption | undefined {
		let answerOption: DatasetQualityAnswerOption | undefined;
		if (question.id) {
			const answer = this.datasetQualityInformations.qualityInformations?.find(q => q.questionId === question.id);
			if (answer) {
				answerOption = question.answerOptions?.find(aw => aw.id === answer?.answerId);
			}
		}

		return answerOption;
	}

	isAnswerOptionMandatory(question: DatasetQualityQuestion): boolean {
		const answerOption = this.getAnswerOption(question);
		return answerOption?.detailMandatory ?? false;
	}

	ngOnDestroy() {
		this.unsubscribe$.next(null);
		this.unsubscribe$.complete();
	}
}
