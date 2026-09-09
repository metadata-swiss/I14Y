import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute, NavigationEnd, Router} from '@angular/router';
import {
	AllowActionResourceType,
	AllowActionType,
	DataFormat,
	DatasetClient,
	DatasetInputClient,
	DcatDatasetModel,
	PublicationLevel,
	PublicationLevelInfoModel,
	RegistrationStatus,
	RegistrationStatusInfoModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObINotification, ObNotificationService} from '@oblique/oblique';
import {Observable, of, Subject} from 'rxjs';
import {catchError, filter, map, startWith, takeUntil} from 'rxjs/operators';
import {DatasetService} from './services/dataset.service';
import {AllowActionService} from '../services/allow.action.service';
import {ViewType} from '../shared/templates/viewtype';
import {NAV_VALUE_EDIT, DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY} from '../app-constants';
import {DialogComponent, DialogType} from '../shared/dialog/dialog.component';
import {MatDialog} from '@angular/material/dialog';
import {HttpErrorResponse} from '@angular/common/http';
import {isAutomatedCreation} from '../shared/system-helpers';
import {MessageHelperFunctions} from '../shared/message-helper-functions';

@Component({
	selector: 'app-datasets-view',
	templateUrl: './datasets.view.component.html',
	styleUrls: ['./datasets.view.component.scss'],
	standalone: false
})
export class DatasetsViewComponent implements OnInit, OnDestroy {
	cannotCreateVersion$: Observable<boolean> = of(true);
	cannotEdit$: Observable<boolean> = of(true);
	allowActionEditMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionEditMessage$: Observable<string> = of('');
	cannotDelete$: Observable<boolean> = of(true);
	allowActionDeleteMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionDeleteMessage$: Observable<string> = of('');
	currentLanguage: string;
	dataset: DcatDatasetModel = new DcatDatasetModel();
	datasetId: string;
	registrationStatusInfo: RegistrationStatusInfoModel | undefined;
	publicationLevelInfo: PublicationLevelInfoModel | undefined;
	activeTab: string;
	tabs: string[] = ['description', 'distributions', 'structure', 'qualityinfo'];
	readonly nav_value_edit: string = NAV_VALUE_EDIT;
	readonly publicationLevelEnum = PublicationLevel;
	readonly viewTypeEnum = ViewType;
	readonly dataFormat = DataFormat;
	readonly isAutomatedCreation = isAutomatedCreation;
	readonly accessRightsPublicCode = 'PUBLIC';

	private readonly unsubscribe$ = new Subject();

	private readonly allowActionService = inject(AllowActionService);
	private readonly datasetClient = inject(DatasetClient);
	private readonly datasetInputClient = inject(DatasetInputClient);
	private readonly datasetService = inject(DatasetService);
	private readonly dialog = inject(MatDialog);
	private readonly notification = inject(ObNotificationService);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.activeTab = this.route.snapshot.children[0].url[0].path;
		this.currentLanguage = this.translate.getCurrentLang();
		this.datasetId = this.route.snapshot.params.id;
	}

	ngOnInit(): void {
		this.route.params.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
			this.datasetService.load(params.id, true);
			this.datasetId = params.id;
		});
		this.datasetService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.dataset = x));
		this.datasetService.registrationStatusInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(status => (this.registrationStatusInfo = status));
		this.datasetService.publicationLevelInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(level => (this.publicationLevelInfo = level));

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
		this.router.events
			.pipe(
				takeUntil(this.unsubscribe$),
				filter((e): e is NavigationEnd => e instanceof NavigationEnd)
			)
			.subscribe(_ => {
				this.activeTab = this.route.snapshot.children[0].url[0].path;
			});

		this.allowActionService.load(this.datasetId, AllowActionResourceType.Dataset);
		this.cannotCreateVersion$ = this.allowActionService.allowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.actionType === AllowActionType.Version)?.value),
			startWith(true)
		);
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
		this.cannotDelete$ = this.allowActionService.allowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.actionType === AllowActionType.Delete)?.value),
			startWith(true)
		);
		this.allowActionDeleteMessageDetailCode$ = this.allowActionService.allowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => result.find(x => x.actionType === AllowActionType.Delete)?.messageDetailsCode?.toString() ?? undefined),
			startWith('')
		);
		this.defaultAllowActionDeleteMessage$ = this.allowActionService.allowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => result.find(x => x.actionType === AllowActionType.Delete)?.message ?? ''),
			startWith('')
		);
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	onSetRegistrationStatus(status: RegistrationStatus) {
		this.datasetInputClient.putRegistrationStatusByIdAndStatus(this.datasetId, status).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onProposeRegistrationStatus(proposal: RegistrationStatus) {
		this.datasetInputClient.putRegistrationStatusProposalByIdAndProposal(this.datasetId, proposal).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onSetPublicationLevel(level: PublicationLevel) {
		this.datasetInputClient.putPublicationLevelByIdAndLevel(this.datasetId, level).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onProposePublicationLevel(proposal: PublicationLevel) {
		this.datasetInputClient.putPublicationLevelProposalByIdAndProposal(this.datasetId, proposal).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onDeleteClick(): void {
		const headertextKey = 'i18n.dialog.delete.header_text';
		const bodytextKey = 'i18n.dialog.delete.body_text';
		const confirmButtontextKey = 'i18n.button.confirm';

		this.translate.get([headertextKey, bodytextKey, confirmButtontextKey, DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY]).subscribe(result => {
			const dialogRef = this.dialog.open(DialogComponent, {
				data: {
					showHeader: true,
					headerText: result[headertextKey],
					bodyText: result[bodytextKey],
					dialogType: DialogType.confirm,
					cancelButtonText: result[DIALOG_CANCEL_BUTTON_KEY],
					confirmButtonText: result[DIALOG_CONFIRM_BUTTON_KEY]
				},
				disableClose: true
			});
			const dialogConfirm = dialogRef.componentInstance.confirm.subscribe(() => {
				this.datasetInputClient.deleteById(this.dataset?.id as string).subscribe(() => {
					this.notification.success('i18n.notification.deleted');
					this.router.navigate(['../../'], {relativeTo: this.route});
				});
			});
			dialogRef.afterClosed().subscribe(() => {
				dialogConfirm.unsubscribe();
			});
		});
	}

	exportDataset(format: DataFormat): void {
		const skippedErrorNotifications = 1;
		this.obHttpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(skippedErrorNotifications);
		this.datasetClient
			.getExportByIdAndFormat(this.datasetId, format)
			.pipe(
				catchError((error: HttpErrorResponse) => {
					this.notification.error(MessageHelperFunctions.getExportErrorMessage(error));
					return of();
				})
			)
			.subscribe(response => {
				const a = document.createElement('a');
				const objectUrl = URL.createObjectURL(response.result.data);

				const fileName = `Dataset_${this.dataset?.identifiers![0]}${this.dataset.version ? '-' + this.dataset.version : ''}.${format}`;

				a.href = objectUrl;
				a.download = response.result.fileName ?? fileName;
				a.click();

				URL.revokeObjectURL(objectUrl);
				a.remove();
			});
	}

	private updateAfterSave() {
		this.datasetService.load(this.datasetId, true);
		this.allowActionService.load(this.datasetId, AllowActionResourceType.Dataset, true);
		this.notification.success('i18n.status.change.success');
	}
}
