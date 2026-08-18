import {ArrayToStringPipe} from './../../shared/formating/array-to-string.pipe';
import {Component, inject, Input, OnDestroy, OnInit} from '@angular/core';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {DateFormatService} from '../../shared/services/date-format/date-format.service';
import {FileSizeFormatService} from '../../shared/services/file-size-format/file-size-format.service';
import {takeUntil, take} from 'rxjs/operators';
import {Subject} from 'rxjs';
import {
	DcatDistributionModel,
	PeriodOfTime,
	ResourceModel,
	VocabularyEntryModel,
	DataServiceModel,
	IMultiLanguage
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {FormatFunctions} from 'src/app/shared/format-functions';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {VOCAB_ID_FILE_TYPE, VOCAB_ID_MEDIA_TYPE} from 'src/app/app-constants';
import {VocabularyConfigService} from '../../shared/services/vocabulary-config/vocabulary-config.service';

@Component({
	selector: 'app-distribution-detail',
	templateUrl: './distribution-detail.component.html',
	styleUrls: ['./distribution-detail.component.scss'],
	standalone: false
})
export class DistributionDetailComponent implements OnInit, OnDestroy {
	@Input()
	set distribution(distribution: DcatDistributionModel) {
		this.dist = distribution;
	}
	get distribution() {
		return this.dist;
	}

	@Input()
	dataServiceLinks: DataServiceModel[];
	languages: string[];
	currentLang: string;

	readonly emptyPlaceHolder: string = '-';
	formatConceptPageIri: string | undefined = undefined;
	mediaTypeVocabularyIri: string | undefined = undefined;
	private readonly unsubscribe$ = new Subject();
	private dist: DcatDistributionModel;

	private readonly arrayToString = inject(ArrayToStringPipe);
	private readonly dateFormatService = inject(DateFormatService);
	private readonly fallback = inject(FallbackPipe);
	private readonly fileSizeFormatService = inject(FileSizeFormatService);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyConfigService = inject(VocabularyConfigService);

	constructor() {
		this.currentLang = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.vocabularyConfigService
			.resolveConceptPageIris([VOCAB_ID_FILE_TYPE])
			.pipe(take(1))
			.subscribe(iris => {
				this.formatConceptPageIri = iris[VOCAB_ID_FILE_TYPE];
			});

		this.vocabularyConfigService
			.resolveIris([VOCAB_ID_MEDIA_TYPE])
			.pipe(take(1))
			.subscribe(iris => {
				this.mediaTypeVocabularyIri = iris[VOCAB_ID_MEDIA_TYPE];
			});

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLang = language.lang;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(null);
		this.unsubscribe$.complete();
	}

	hasUrl(resources: ResourceModel[]): boolean {
		if (resources?.length > 0) {
			const url = resources[0];
			return url?.uri && url.uri.length > 0;
		}
		return false;
	}

	getUrl(resources: ResourceModel[]): string {
		if (resources?.length > 0) {
			const url = resources[0];
			if (url?.uri && url.uri.length > 0) {
				return url.uri;
			}
		}
		return null;
	}

	getLanguages(languages: VocabularyEntryModel[] | undefined): string | undefined {
		return this.arrayToString.transform(FormatFunctions.getTranslatedVocabularyEntries(languages, this.currentLang), ', ') ?? undefined;
	}

	getFormattedDate(date: Date | undefined): string | undefined {
		return date ? this.dateFormatService.formatShortDate(date) : undefined;
	}

	formatFileSize(byteSize: number | undefined): string | undefined {
		return this.fileSizeFormatService.formatBytes(byteSize, 0);
	}

	getCoverage(coverage: PeriodOfTime[] | undefined): string[] | undefined {
		return coverage ? coverage.map(e => `${FormatFunctions.getFormattedDate(e.start)} - ${FormatFunctions.getFormattedDate(e.end)}`) : undefined;
	}

	getFormatSearchUrl(code: string | undefined): string | undefined {
		if (!code) {
			return undefined;
		}
		return `/${this.currentLang}/catalog/all?formats=${code}`;
	}

	getOrderedRessources(resources: ResourceModel[]): ResourceModel[] {
		const shallowCopy = [...(resources ?? [])];
		shallowCopy.sort((a, b) => {
			if (this.getTranslated(a.label) === this.getTranslated(b.label)) {
				return a.uri.localeCompare(b.uri);
			}
			return this.getTranslated(a.label).localeCompare(this.getTranslated(b.label));
		});
		return shallowCopy;
	}

	private getTranslated(source: IMultiLanguage | undefined): string | undefined {
		return this.fallback.transform(source, this.currentLang) ?? undefined;
	}
}
