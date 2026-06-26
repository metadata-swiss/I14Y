import {Component, EventEmitter, inject, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges} from '@angular/core';
import {AllowActionType, DcatCatalogInputClient, DcatCatalogRecordInput, DcatVocabularyEntry} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {map, Observable, of, startWith, Subject, takeUntil} from 'rxjs';
import {AllowActionService} from 'src/app/services/allow.action.service';

@Component({
	selector: 'app-themes-select',
	templateUrl: './themes-select.component.html',
	standalone: false
})
export class ThemesSelectComponent implements OnInit, OnChanges, OnDestroy {
	@Input() catalog: DcatCatalogRecordInput = new DcatCatalogRecordInput({themes: []});
	@Output() updateCatalogs: EventEmitter<void> = new EventEmitter();

	cannotEdit$: Observable<boolean> = of<boolean>(true);
	catalogThemes: DcatVocabularyEntry[] | undefined;
	currentLanguage: string;

	private readonly unsubscribe$ = new Subject();

	private readonly allowActionService = inject(AllowActionService);
	private readonly dcatCatalogInputClient = inject(DcatCatalogInputClient);
	private readonly notification = inject(ObNotificationService);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnInit() {
		this.cannotEdit$ = this.allowActionService.childAllowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.actionType === AllowActionType.Edit)?.value),
			startWith(true)
		);
	}

	ngOnChanges(changes: SimpleChanges): void {
		if (changes.catalog?.currentValue?.catalogId) {
			this.dcatCatalogInputClient.getThemesById(changes.catalog?.currentValue.catalogId).subscribe(response => {
				this.catalogThemes = response.result;
			});
		}
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	compareType(prev: any, next: any): boolean {
		return prev.code === next.code;
	}

	addThemes(catalog: DcatCatalogRecordInput | undefined): void {
		if (catalog) {
			this.dcatCatalogInputClient.putRecordsByBody(catalog).subscribe(_ => {
				this.showSuccessNotification('i18n.datasets.description.new.theme.added');
				this.updateCatalogs.emit();
			});
		}
	}

	private showSuccessNotification(translation: string): void {
		this.notification.success(translation);
	}
}
