import {ActivatedRoute} from '@angular/router';
import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {
	AllowActionResourceType,
	AllowActionType,
	DatasetQualityAnswerOption,
	DatasetQualityInformation,
	DatasetQualityInformationClient,
	DatasetQualityInformationData,
	DatasetQualityInformationLink,
	DatasetQualityQuestion,
	ResourceModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {map, startWith, takeUntil} from 'rxjs/operators';
import {NAV_VALUE_CREATE, NAV_VALUE_EDIT} from 'src/app/app-constants';
import {Observable, of, Subject} from 'rxjs';
import {AllowActionService} from 'src/app/services/allow.action.service';

@Component({
	selector: 'app-qualityinfo',
	templateUrl: './qualityinfo.component.html',
	styleUrls: ['./qualityinfo.component.scss'],
	standalone: false
})
export class QualityInfoComponent implements OnInit, OnDestroy {
	modeCreate = NAV_VALUE_CREATE;
	modeEdit = NAV_VALUE_EDIT;
	questions: DatasetQualityQuestion[] = Array();
	answers: DatasetQualityInformationData = new DatasetQualityInformationData();
	cannotEdit$: Observable<boolean> = of(true);
	currentLanguage: string;
	target = '_blank';
	rel = 'noopener noreferrer';

	private datasetId = '';
	private readonly unsubscribe$ = new Subject();

	private readonly allowActionService = inject(AllowActionService);
	private readonly client = inject(DatasetQualityInformationClient);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.datasetId = this.route.parent?.snapshot.params.id;

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => (this.currentLanguage = language.lang));

		this.loadData();
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	hasAnswers(): boolean {
		if (this.answers.qualityInformations && this.answers.qualityInformations?.length > 0) {
			return true;
		}
		return false;
	}

	isAnswerOptionMandatory(question: DatasetQualityQuestion): boolean {
		const answerOption = this.getAnswerOption(question);
		return answerOption?.detailMandatory ?? false;
	}

	getAnswer(question: DatasetQualityQuestion): DatasetQualityInformation | undefined {
		let answer: DatasetQualityInformation | undefined;
		if (question.id) {
			answer = this.answers.qualityInformations?.find(q => q.questionId === question.id);
		}
		return answer;
	}

	getAnswerOption(question: DatasetQualityQuestion): DatasetQualityAnswerOption | undefined {
		let answerOption: DatasetQualityAnswerOption | undefined;
		if (question.id) {
			const answer = this.answers.qualityInformations?.find(q => q.questionId === question.id);
			if (answer) {
				answerOption = question.answerOptions?.find(aw => aw.id === answer?.answerId);
			}
		}

		return answerOption;
	}

	mapRessources(resources: DatasetQualityInformationLink[] | undefined): ResourceModel[] | undefined {
		return resources?.map(r => new ResourceModel({uri: r.href, label: r.label})) ?? undefined;
	}

	private loadData(): void {
		this.client.getByDatasetId(this.datasetId).subscribe(ra => {
			this.answers = ra.result;
			this.client.getDefinition().subscribe(rd => {
				this.questions = rd.result.sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
			});
		});

		this.allowActionService.load(this.datasetId, AllowActionResourceType.Dataset);
		this.cannotEdit$ = this.allowActionService.allowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.actionType === AllowActionType.Edit)?.value),
			startWith(true)
		);
	}
}
