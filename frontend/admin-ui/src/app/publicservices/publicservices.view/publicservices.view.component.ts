import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute, NavigationEnd, Router} from '@angular/router';
import {
	AllowActionResourceType,
	AllowActionType,
	DataFormat,
	PublicationLevel,
	PublicationLevelInfoModel,
	PublicServiceInputClient,
	PublicServiceModel,
	PublicServicesClient,
	RegistrationStatus,
	RegistrationStatusInfoModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObINotification, ObNotificationService} from '@oblique/oblique';
import {catchError, filter, map, Observable, of, startWith, Subject, takeUntil} from 'rxjs';
import {PublicServiceService} from '../services/publicservice.service';
import {AllowActionService} from 'src/app/services/allow.action.service';
import {ViewType} from 'src/app/shared/templates/viewtype';
import {NAV_VALUE_EDIT, DIALOG_CANCEL_BUTTON_KEY} from 'src/app/app-constants';
import {MatDialog} from '@angular/material/dialog';
import {DialogComponent, DialogType} from 'src/app/shared/dialog/dialog.component';
import {isAutomatedCreation} from 'src/app/shared/system-helpers';
import {HttpErrorResponse} from '@angular/common/http';
import {MessageHelperFunctions} from 'src/app/shared/message-helper-functions';

@Component({
	selector: 'app-publicservices.view',
	templateUrl: './publicservices.view.component.html',
	styleUrls: ['./publicservices.view.component.scss'],
	standalone: false
})
export class PublicservicesViewComponent implements OnInit, OnDestroy {
	cannotEdit$: Observable<boolean> = of(true);
	allowActionEditMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionEditMessage$: Observable<string> = of('');
	cannotDelete$: Observable<boolean> = of(true);
	allowActionDeleteMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionDeleteMessage$: Observable<string> = of('');
	currentLanguage: string;
	publicservice: PublicServiceModel = new PublicServiceModel();
	publicServiceId: string;
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
	private readonly dialog = inject(MatDialog);
	private readonly notification = inject(ObNotificationService);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);
	private readonly publicServicesClient = inject(PublicServicesClient);
	private readonly publicserviceInputClient = inject(PublicServiceInputClient);
	private readonly publicServiceService = inject(PublicServiceService);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.activeTab = this.route.snapshot.children[0].url[0].path;
		this.currentLanguage = this.translate.getCurrentLang();
		this.publicServiceId = this.route.snapshot.params.id;
	}

	ngOnInit(): void {
		this.route.params.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
			this.publicServiceService.load(params.id, true);
			this.publicServiceId = params.id;
		});
		this.publicServiceService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => (this.publicservice = x));
		this.publicServiceService.registrationStatusInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(status => (this.registrationStatusInfo = status));
		this.publicServiceService.publicationLevelInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(level => (this.publicationLevelInfo = level));

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

		this.allowActionService.load(this.publicServiceId, AllowActionResourceType.PublicService);
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
		this.publicserviceInputClient.putRegistrationStatusByIdAndStatus(this.publicServiceId, status).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onProposeRegistrationStatus(proposal: RegistrationStatus) {
		this.publicserviceInputClient.putRegistrationStatusProposalByIdAndProposal(this.publicServiceId, proposal).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onSetPublicationLevel(level: PublicationLevel) {
		this.publicserviceInputClient.putPublicationLevelByIdAndLevel(this.publicServiceId, level).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onProposePublicationLevel(proposal: PublicationLevel) {
		this.publicserviceInputClient.putPublicationLevelProposalByIdAndProposal(this.publicServiceId, proposal).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onDeletePublicServiceClick() {
		const headertextKey = 'i18n.publicservices.delete.dialog.header';
		const bodytextKey = 'i18n.publicservices.delete.dialog.body';
		const confirmButtontextKey = 'i18n.publicservices.delete.dialog.confirmbutton';
		this.translate.get([headertextKey, bodytextKey, confirmButtontextKey, DIALOG_CANCEL_BUTTON_KEY]).subscribe(result => {
			const dialogRef = this.dialog.open(DialogComponent, {
				data: {
					showHeader: true,
					headerText: result[headertextKey],
					bodyText: result[bodytextKey],
					dialogType: DialogType.confirm,
					cancelButtonText: result[DIALOG_CANCEL_BUTTON_KEY],
					confirmButtonText: result[confirmButtontextKey]
				},
				disableClose: true
			});
			const dialogConfirm = dialogRef.componentInstance.confirm.subscribe(() => {
				this.publicserviceInputClient.deleteById(this.publicservice?.id as string).subscribe(_ => {
					this.showSuccessNotification();
					this.navigateBack();
				});
			});
			dialogRef.afterClosed().subscribe(() => {
				dialogConfirm.unsubscribe();
			});
		});
	}

	exportPublicService(format: DataFormat): void {
		const skippedErrorNotifications = 1;
		this.obHttpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(skippedErrorNotifications);
		this.publicServicesClient
			.getExportByIdAndFormat(this.publicServiceId, format)
			.pipe(
				catchError((error: HttpErrorResponse) => {
					this.notification.error(MessageHelperFunctions.getExportErrorMessage(error));
					return of();
				})
			)
			.subscribe(response => {
				const a = document.createElement('a');
				const objectUrl = URL.createObjectURL(response.result.data);

				const fileName = `DataService_${this.publicservice?.identifiers![0]}.${format}`;

				a.href = objectUrl;
				a.download = response.result.fileName ?? fileName;
				a.click();

				URL.revokeObjectURL(objectUrl);
				a.remove();
			});
	}

	private navigateBack(): void {
		this.router.navigate(['../../../'], {relativeTo: this.route});
	}

	private showSuccessNotification() {
		this.notification.success('i18n.notification.deleted');
	}

	private updateAfterSave() {
		this.publicServiceService.load(this.publicServiceId, true);
		this.allowActionService.load(this.publicServiceId, AllowActionResourceType.PublicService, true);
		this.notification.success('i18n.status.change.success');
	}
}
