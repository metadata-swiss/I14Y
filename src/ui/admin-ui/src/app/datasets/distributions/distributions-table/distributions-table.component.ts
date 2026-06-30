import {ActivatedRoute} from '@angular/router';
import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {
	AllowActionResourceType,
	AllowActionType,
	DatasetInputClient,
	DatasetsClient,
	DcatDatasetModel,
	DcatDistributionModel,
	DistributionSummary,
	VocabularyEntryModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {FormatFunctions} from 'src/app/shared/format-functions';
import {LangChangeEvent} from '@ngx-translate/core';
import {NAV_VALUE_EDIT, NAV_VALUE_OVERVIEW, DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY} from 'src/app/app-constants';
import {Observable, of, Subject} from 'rxjs';
import {SortableListViewComponent} from 'src/app/shared/sortable-list-view/sortable-list-view.component';
import {map, startWith, takeUntil} from 'rxjs/operators';
import {MatDialog} from '@angular/material/dialog';
import {DialogComponent, DialogType} from 'src/app/shared/dialog/dialog.component';
import {ObNotificationService} from '@oblique/oblique';
import {AllowActionService} from 'src/app/services/allow.action.service';
import {DatasetService} from '../../services/dataset.service';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {DcatDatasetInputModelMapper} from 'src/app/shared/mappers/dcatdatasetinputmodelmapper';

type SortableKeys = 'title' | 'format' | 'byteSize' | 'issued' | 'modified';

@Component({
	selector: 'app-distributions-table',
	templateUrl: './distributions-table.component.html',
	styleUrls: ['./distributions-table.component.scss'],
	standalone: false
})
export class DistributionsTableComponent extends SortableListViewComponent<DcatDistributionModel, SortableKeys> implements OnInit, OnDestroy {
	readonly COLUMN_TITLE = 'title';
	readonly COLUMN_FORMAT = 'format';
	readonly COLUMN_BYTESIZE = 'byteSize';
	readonly COLUMN_LANGUAGES = 'languages';
	readonly COLUMN_ISSUED = 'issued';
	readonly COLUMN_MODIFIED = 'modified';
	readonly COLUMN_ACTIONS = 'actions';

	readonly from = NAV_VALUE_OVERVIEW;
	readonly nav_value_edit: string = NAV_VALUE_EDIT;

	displayedColumns: string[] = [
		this.COLUMN_TITLE,
		this.COLUMN_FORMAT,
		this.COLUMN_BYTESIZE,
		this.COLUMN_LANGUAGES,
		this.COLUMN_ISSUED,
		this.COLUMN_MODIFIED,
		this.COLUMN_ACTIONS
	];

	datasetId = '';
	currentLanguage: string;
	cannotEdit$: Observable<boolean> = of(true);
	allowActionEditMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionEditMessage$: Observable<string> = of('');

	private readonly unsubscribe$ = new Subject();

	private readonly allowActionService = inject(AllowActionService);
	private readonly datasetInputClient = inject(DatasetInputClient);
	private readonly datasetsClient = inject(DatasetsClient);
	private readonly datasetService = inject(DatasetService);
	private readonly dialog = inject(MatDialog);
	private readonly fallback = inject(FallbackPipe);
	private readonly notification = inject(ObNotificationService);
	private readonly route = inject(ActivatedRoute);

	constructor() {
		super();

		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => (this.currentLanguage = language.lang));

		this.datasetId = this.route.parent?.snapshot.params.id;
		this.datasetService.load(this.datasetId);
		this.datasetService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.data = x.distributions ?? [];
		});

		this.allowActionService.load(this.datasetId, AllowActionResourceType.Dataset);
		this.cannotEdit$ = this.allowActionService.allowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.actionType === AllowActionType.Edit)?.value),
			startWith(true)
		);
		this.allowActionEditMessageDetailCode$ = this.allowActionService.allowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => result.find(x => x.actionType === AllowActionType.Edit)?.messageDetailsCode?.toString() ?? undefined),
			startWith('')
		);
		this.defaultAllowActionEditMessage$ = this.allowActionService.allowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => result.find(x => x.actionType === AllowActionType.Edit)?.message ?? ''),
			startWith('')
		);
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	getFormattedDate(date: Date): string | null {
		return FormatFunctions.getFormattedDate(date);
	}

	getLanguages(languages: VocabularyEntryModel[]) {
		return FormatFunctions.convertArrayToString(
			languages.map(l => this.mapVocabularyName(l)).filter(l => l.length > 0),
			', ',
			'-'
		).toLocaleUpperCase();
	}

	mapVocabularyName(vocabulary: VocabularyEntryModel): string {
		return this.fallback.transform(vocabulary.name, this.currentLanguage) ?? '';
	}

	onDelete(distribution: DistributionSummary): void {
		const dialogRef = this.dialog.open(DialogComponent, {
			data: {
				showHeader: true,
				enableSave: false,
				headerText: this.translate.instant('i18n.distribution.deletedialog.headertext'),
				bodyText: this.translate.instant('i18n.distribution.deletedialog.bodytext'),
				dialogType: DialogType.confirm,
				okButtonText: '',
				cancelButtonText: this.translate.instant(DIALOG_CANCEL_BUTTON_KEY),
				confirmButtonText: this.translate.instant(DIALOG_CONFIRM_BUTTON_KEY),
				discardChangesButtonText: '',
				saveChangesButtonText: ''
			},
			disableClose: true
		});
		const dialogConfirm = dialogRef.componentInstance.confirm.subscribe(() => {
			this.datasetsClient.getById(this.datasetId).subscribe(response => {
				this.deleteDistribution(response.result, distribution.id!);
			});
		});
		dialogRef.afterClosed().subscribe(() => {
			dialogConfirm.unsubscribe();
		});
	}

	private deleteDistribution(dto: DcatDatasetModel, distributionId: string) {
		if (dto.distributions) {
			let distributionTodelete = dto?.distributions?.find(c => c.id === distributionId);
			if (distributionTodelete) {
				let index = dto.distributions?.indexOf(distributionTodelete);

				if (index !== -1) {
					dto.distributions?.splice(index, 1);

					this.datasetInputClient.putByIdAndBody(this.datasetId, DcatDatasetInputModelMapper.mapToInputModel(dto)).subscribe(_ => {
						this.datasetService.load(this.datasetId, true);
						this.notification.success('i18n.notification.deleted');
					});
				}
			}
		}
	}
}
