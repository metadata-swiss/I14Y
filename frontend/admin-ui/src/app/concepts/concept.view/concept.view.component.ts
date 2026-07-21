import {catchError, filter, startWith} from 'rxjs/operators';
import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute, NavigationEnd, Router} from '@angular/router';
import {
	AllowActionResourceType,
	AllowActionType,
	ConceptInputClient,
	ConceptView,
	ConceptViewClient,
	DataFormat,
	PublicationLevel,
	PublicationLevelInfoModel,
	RegistrationStatus,
	RegistrationStatusInfoModel
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ObHttpApiInterceptorEvents, ObINotification, ObNotificationService} from '@oblique/oblique';
import {map, Observable, of, Subject, takeUntil} from 'rxjs';
import {NAV_VALUE_DETAIL, NAV_VALUE_EDIT, DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY} from 'src/app/app-constants';
import {MatDialog} from '@angular/material/dialog';
import {AllowActionService} from 'src/app/services/allow.action.service';
import {DialogComponent, DialogType} from 'src/app/shared/dialog/dialog.component';
import {ConceptService} from '../services/concept.service';
import {ViewType} from 'src/app/shared/templates/viewtype';
import {isAutomatedCreation} from 'src/app/shared/system-helpers';
import {HttpErrorResponse} from '@angular/common/http';
import {MessageHelperFunctions} from 'src/app/shared/message-helper-functions';

@Component({
	selector: 'app-concept-view',
	templateUrl: './concept.view.component.html',
	styleUrls: ['./concept.view.component.scss'],
	standalone: false
})
export class ConceptViewComponent implements OnInit, OnDestroy {
	cannotCreateVersion$: Observable<boolean> = of(true);
	cannotChangeLock$: Observable<boolean> = of(true);
	cannotEdit$: Observable<boolean> = of(true);
	allowActionEditMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionEditMessage$: Observable<string> = of('');
	cannotDelete$: Observable<boolean> = of(true);
	allowActionDeleteMessageDetailCode$: Observable<string | undefined> = of(undefined);
	defaultAllowActionDeleteMessage$: Observable<string> = of('');
	currentLanguage: string;
	concept: ConceptView | undefined;
	conceptId = '';
	registrationStatusInfo: RegistrationStatusInfoModel | undefined;
	publicationLevelInfo: PublicationLevelInfoModel | undefined;
	activeTab: string;
	tabs: string[] = ['description'];
	isLocked: boolean = false;
	readonly publicationLevelEnum = PublicationLevel;
	readonly viewTypeEnum = ViewType;
	readonly dataFormat = DataFormat;
	readonly from: string = NAV_VALUE_DETAIL;
	readonly nav_value_edit: string = NAV_VALUE_EDIT;
	readonly isAutomatedCreation = isAutomatedCreation;

	private readonly unsubscribe$ = new Subject();

	private readonly allowActionService = inject(AllowActionService);
	private readonly conceptViewClient = inject(ConceptViewClient);
	private readonly conceptInputClient = inject(ConceptInputClient);
	private readonly conceptService = inject(ConceptService);
	private readonly dialog = inject(MatDialog);
	private readonly notification = inject(ObNotificationService);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);
	private readonly route = inject(ActivatedRoute);
	private readonly router = inject(Router);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.activeTab = this.route.snapshot.children[0].url[0].path;
		this.conceptId = this.route.snapshot.params.conceptId;
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});

		this.route.params.pipe(takeUntil(this.unsubscribe$)).subscribe(params => {
			this.conceptService.load(params.conceptId, true);
			this.allowActionService.load(params.conceptId, AllowActionResourceType.Concept);
			this.conceptId = params.conceptId;
		});
		this.conceptService.data$.pipe(takeUntil(this.unsubscribe$)).subscribe(result => {
			this.concept = result;
			this.isLocked = result?.isLocked ?? false;
		});
		this.conceptService.registrationStatusInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(status => (this.registrationStatusInfo = status));
		this.conceptService.publicationLevelInfo$.pipe(takeUntil(this.unsubscribe$)).subscribe(level => (this.publicationLevelInfo = level));

		this.router.events
			.pipe(
				takeUntil(this.unsubscribe$),
				filter((e): e is NavigationEnd => e instanceof NavigationEnd)
			)
			.subscribe(_ => {
				this.activeTab = this.route.snapshot.children[0].url[0].path;
			});

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
		this.cannotCreateVersion$ = this.allowActionService.allowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => !result.find(x => x.actionType === AllowActionType.Version)?.value),
			startWith(true)
		);
		this.cannotChangeLock$ = this.allowActionService.allowActions$.pipe(
			takeUntil(this.unsubscribe$),
			map(result => {
				const canLock = result.find(x => x.actionType === AllowActionType.Lock)?.value ?? false;
				const canUnlock = result.find(x => x.actionType === AllowActionType.Unlock)?.value ?? false;
				return !(canLock || canUnlock);
			}),
			startWith(true)
		);
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	onSetRegistrationStatus(status: RegistrationStatus): void {
		this.conceptInputClient.putRegistrationStatusByIdAndStatus(this.conceptId, status).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onProposeRegistrationStatus(proposal: RegistrationStatus): void {
		this.conceptInputClient.putRegistrationStatusProposalByIdAndProposal(this.conceptId, proposal).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onSetPublicationLevel(level: PublicationLevel): void {
		this.conceptInputClient.putPublicationLevelByIdAndLevel(this.conceptId, level).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onProposePublicationLevel(proposal: PublicationLevel): void {
		this.conceptInputClient.putPublicationLevelProposalByIdAndProposal(this.conceptId, proposal).subscribe(() => {
			this.updateAfterSave();
		});
	}

	onDeleteClick(): void {
		const headertextKey = 'i18n.delete_dialog.header';
		const bodytextKey = 'i18n.delete_dialog.body';
		const confirmButtontextKey = 'i18n.delete_dialog.confirmbutton';

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
				this.conceptInputClient.deleteById(this.concept?.id as string).subscribe(() => {
					this.notification.success('i18n.notification.deleted');
					this.router.navigate(['../../'], {relativeTo: this.route});
				});
			});
			dialogRef.afterClosed().subscribe(() => {
				dialogConfirm.unsubscribe();
			});
		});
	}

	onLock(event: boolean): void {
		this.isLocked = true;
		if (this.concept?.id) {
			this.conceptInputClient.putLockedByIdAndLocked(this.concept?.id, event).subscribe({
				next: () => {
					this.conceptService.load(this.conceptId, true);
					this.allowActionService.load(this.conceptId, AllowActionResourceType.Concept, true);
				},
				error: () => {
					this.isLocked = false;
				}
			});
		}
	}

	exportConcept(format: DataFormat): void {
		const skippedErrorNotifications = 1;
		this.obHttpApiInterceptorEvents.deactivateNotificationOnNextAPICalls(skippedErrorNotifications);
		this.conceptViewClient
			.getExportByIdAndFormat(this.conceptId, format)
			.pipe(
				catchError((error: HttpErrorResponse) => {
					this.notification.error(MessageHelperFunctions.getExportErrorMessage(error));
					return of();
				})
			)
			.subscribe(response => {
				const a = document.createElement('a');
				const objectUrl = URL.createObjectURL(response.result.data);

				const fileName = `DataService_${this.concept?.identifiers![0]}${this.concept?.version ? '-' + this.concept.version : ''}.${format}`;

				a.href = objectUrl;
				a.download = response.result.fileName ?? fileName;
				a.click();

				URL.revokeObjectURL(objectUrl);
				a.remove();
			});
	}

	private updateAfterSave() {
		this.conceptService.load(this.conceptId, true);
		this.allowActionService.load(this.conceptId, AllowActionResourceType.Concept, true);
		this.notification.success('i18n.status.change.success');
	}
}
