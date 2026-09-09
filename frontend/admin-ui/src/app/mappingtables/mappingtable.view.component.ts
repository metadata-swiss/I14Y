import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute, NavigationEnd, Router} from '@angular/router';
import {
	AllowActionResourceType,
	AllowActionType,
	DataFormat,
	MappingTableModel,
	MappingTablesClient,
	PublicationLevel,
	PublicationLevelInfoModel,
	RegistrationStatus,
	RegistrationStatusInfoModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObINotification, ObNotificationService} from '@oblique/oblique';
import {Observable, of, Subject} from 'rxjs';
import {catchError, filter, map, startWith, takeUntil} from 'rxjs/operators';
import {AllowActionService} from '../services/allow.action.service';
import {NAV_VALUE_EDIT, DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY} from '../app-constants';
import {ViewType} from '../shared/templates/viewtype';
import {MatDialog} from '@angular/material/dialog';
import {DialogComponent, DialogType} from '../shared/dialog/dialog.component';
import {MappingTableService} from './services/mappingtable.service';
import {isAutomatedCreation} from '../shared/system-helpers';
import {HttpErrorResponse} from '@angular/common/http';
import {MessageHelperFunctions} from '../shared/message-helper-functions';

@Component({
	selector: 'app-mappingtable-view',
	templateUrl: './mappingtable.view.component.html',
	styleUrls: ['./mappingtable.view.component.scss'],
	standalone: false
})
export class MappingTableViewComponent implements OnInit, OnDestroy {
	cannotCreateVersion$: Observable<boolean> = of(true);
	cannotEdit$: Observable<boolean> = of(true);
	allowActionEditMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionEditMessage$: Observable<string> = of('');
	cannotDelete$: Observable<boolean> = of(true);
	allowActionDeleteMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionDeleteMessage$: Observable<string> = of('');
	currentLanguage: string;
	mappingTable: MappingTableModel = new MappingTableModel();
	mappingTableId: string;
	registrationStatusInfo: RegistrationStatusInfoModel | undefined;
	publicationLevelInfo: PublicationLevelInfoModel | undefined;
	activeTab: string;
	tabs: string[] = ['description'];
	readonly nav_value_edit: string = NAV_VALUE_EDIT;
	readonly publicationLevelEnum = PublicationLevel;
	readonly viewTypeEnum = ViewType;
	readonly dataFormat = DataFormat;
	readonly isAutomatedCreation = isAutomatedCreation;

	private readonly unsubscribe$ = new Subject();

	private readonly allowActionService = inject(AllowActionService);
	private readonly mappingTablesClient = inject(MappingTablesClient);
	private readonly mappingTableService = inject(MappingTableService);
	private readonly dialog = inject(MatDialog);
	private readonly notification = inject(ObNotificationService);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.activeTab = this.route.snapshot.children[0].url[0].path;
		this.currentLanguage = this.translate.getCurrentLang();
		this.mappingTableId = this.route.snapshot.params.id;
	}

	ngOnInit(): void {
		this.route.params.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
			this.mappingTableService.load(params.id, true);
			this.allowActionService.load(params.id, AllowActionResourceType.MappingTable);
			this.mappingTableId = params.id;
		});

		this.mappingTableService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.mappingTable = x));
		this.mappingTableService.registrationStatusInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(status => (this.registrationStatusInfo = status));
		this.mappingTableService.publicationLevelInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(level => (this.publicationLevelInfo = level));

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
		this.mappingTablesClient.putRegistrationStatusByIdAndStatus(this.mappingTableId, status).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onProposeRegistrationStatus(proposal: RegistrationStatus) {
		this.mappingTablesClient.putRegistrationStatusProposalByIdAndProposal(this.mappingTableId, proposal).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onSetPublicationLevel(level: PublicationLevel) {
		this.mappingTablesClient.putPublicationLevelByIdAndLevel(this.mappingTableId, level).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onProposePublicationLevel(proposal: PublicationLevel) {
		this.mappingTablesClient.putPublicationLevelProposalByIdAndProposal(this.mappingTableId, proposal).subscribe(() => {
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
				this.mappingTablesClient.deleteById(this.mappingTable?.id as string).subscribe(() => {
					this.notification.success('i18n.notification.deleted');
					this.router.navigate(['../../'], {relativeTo: this.route});
				});
			});
			dialogRef.afterClosed().subscribe(() => {
				dialogConfirm.unsubscribe();
			});
		});
	}

	exportMappingTable(format: DataFormat): void {
		const skippedErrorNotifications = 1;
		this.obHttpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(skippedErrorNotifications);
		this.mappingTablesClient
			.getExportByIdAndFormat(this.mappingTableId, format)
			.pipe(
				catchError((error: HttpErrorResponse) => {
					this.notification.error(MessageHelperFunctions.getExportErrorMessage(error));
					return of();
				})
			)
			.subscribe(response => {
				const a = document.createElement('a');
				const objectUrl = URL.createObjectURL(response.result.data);

				const fileName = `DataService_${this.mappingTable?.identifiers![0]}${this.mappingTable?.version ? '-' + this.mappingTable.version : ''}.${format}`;

				a.href = objectUrl;
				a.download = response.result.fileName ?? fileName;
				a.click();

				URL.revokeObjectURL(objectUrl);
				a.remove();
			});
	}

	private updateAfterSave() {
		this.mappingTableService.load(this.mappingTableId, true);
		this.allowActionService.load(this.mappingTableId, AllowActionResourceType.MappingTable, true);
		this.notification.success('i18n.status.change.success');
	}
}
