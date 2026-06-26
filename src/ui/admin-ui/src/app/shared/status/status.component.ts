import {Component, EventEmitter, inject, Input, OnChanges, Output, SimpleChanges} from '@angular/core';
import {PublicationLevel, PublicationLevelInfoModel, RegistrationStatus, RegistrationStatusInfoModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, of} from 'rxjs';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ActivatedRoute} from '@angular/router';
import {AppConfig} from '../../app.config';
import {IAppConfig} from '../../app.config.interface';
import {MatSlideToggleChange} from '@angular/material/slide-toggle';
import {MatDialog} from '@angular/material/dialog';
import {DialogComponent, DialogType} from '../dialog/dialog.component';
import {DIALOG_CANCEL_BUTTON_KEY, DIALOG_LOCK_BUTTON_KEY} from 'src/app/app-constants';

@Component({
	selector: 'app-status',
	templateUrl: './status.component.html',
	styleUrls: ['./status.component.scss'],
	standalone: false
})
export class StatusComponent implements OnChanges {
	@Input() version: string | undefined;
	@Input() enableVersioning: boolean = false;
	@Input() disbaleNewVersion$: Observable<boolean> = of(true);
	@Input() registrationStatusInfo: RegistrationStatusInfoModel | undefined;
	@Input() publicationLevelInfo: PublicationLevelInfoModel | undefined;
	@Input() identifier: string | undefined;
	@Input() showWarning: boolean = false;
	@Input() warningText: string = '';
	@Input() showInfo: boolean = false;
	@Input() infoText: string = '';
	@Input() showAutomatedWarning: boolean = false;
	@Input() isLocked: boolean = false;
	@Input() enableLock: boolean = false;
	@Input() disableLock: boolean = true;
	@Input() showPublicLink: boolean = false;
	@Input() lockDialogHeaderKey = '';
	@Input() lockDialogTextKey = '';
	@Output() setRegistrationStatus: EventEmitter<RegistrationStatus> = new EventEmitter();
	@Output() proposeRegistrationStatus: EventEmitter<RegistrationStatus> = new EventEmitter();
	@Output() setPublicationLevel: EventEmitter<PublicationLevel> = new EventEmitter();
	@Output() proposePublicationLevel: EventEmitter<PublicationLevel> = new EventEmitter();
	@Output() lock: EventEmitter<boolean> = new EventEmitter();
	currentLanguage: string;
	combinedStatus: RegistrationStatus[] = [];
	combinedLevel: PublicationLevel[] = [];

	menuItemsRegistrationStatus = [
		{text: 'i18n.status.accept', icon: 'checkmark', action: false, disabled: this.isSetStatusProposalAllowed()},
		{text: 'i18n.status.decline', icon: 'arrow_clockwise', action: true, disabled: this.canUserRevertStatusProposal()}
	];

	menuItemsPublicationLevel = [
		{text: 'i18n.status.accept', icon: 'checkmark', action: false, disabled: this.isSetLevelProposalAllowed()},
		{text: 'i18n.status.decline', icon: 'arrow_clockwise', action: true, disabled: this.canUserRevertLevelProposal()}
	];

	readonly publicationLevelEnum = PublicationLevel;

	private readonly dialog = inject(MatDialog);
	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.translate.onLangChange.subscribe((language: LangChangeEvent) => (this.currentLanguage = language.lang));
	}

	ngOnChanges(changes: SimpleChanges) {
		const changeRegistrationStatus = changes.registrationStatusInfo;
		const changePublicationLevel = changes.publicationLevelInfo;
		const changeRegistrationStatusInfo = changes.registrationStatusInfo;
		const changePublicationLevelInfo = changes.publicationLevelInfo;

		if (changeRegistrationStatus?.currentValue || changePublicationLevel?.currentValue) {
			this.combinedStatus = this.mergeArrays(this.registrationStatusInfo?.allowedProposals ?? [], this.registrationStatusInfo?.allowedStatuses ?? []);
			this.combinedLevel = this.mergeArrays(this.publicationLevelInfo?.allowedProposals ?? [], this.publicationLevelInfo?.allowedLevels ?? []);
		}

		if (changeRegistrationStatusInfo?.currentValue) {
			this.menuItemsRegistrationStatus[0].disabled = !this.isSetStatusProposalAllowed();
			this.menuItemsRegistrationStatus[1].disabled = !this.canUserRevertStatusProposal();
		}

		if (changePublicationLevelInfo?.currentValue) {
			this.menuItemsPublicationLevel[0].disabled = !this.isSetLevelProposalAllowed();
			this.menuItemsPublicationLevel[1].disabled = !this.canUserRevertLevelProposal();
		}
	}

	isStatusAllowed(entry: RegistrationStatus): boolean | undefined {
		if (this.registrationStatusInfo?.allowedStatuses) {
			return this.registrationStatusInfo?.allowedStatuses.includes(entry);
		}
		return undefined;
	}

	isSetLevelAllowed(entry: PublicationLevel): boolean | undefined {
		if (this.publicationLevelInfo?.allowedLevels) {
			return this.publicationLevelInfo?.allowedLevels?.includes(entry);
		}
		return undefined;
	}

	isSetStatusProposalAllowed(): boolean | undefined {
		return this.registrationStatusInfo?.allowedStatuses?.includes(<RegistrationStatus>this.registrationStatusInfo?.proposal);
	}

	canUserRevertStatusProposal(): boolean | undefined {
		return this.registrationStatusInfo?.canUserRevertProposal;
	}

	isSetLevelProposalAllowed(): boolean | undefined {
		return this.publicationLevelInfo?.allowedLevels?.includes(<PublicationLevel>this.publicationLevelInfo?.proposal);
	}

	canUserRevertLevelProposal(): boolean | undefined {
		return this.publicationLevelInfo?.canUserRevertProposal;
	}

	onSetRegistrationStatusClick(status: RegistrationStatus) {
		this.setRegistrationStatus.emit(status);
	}

	onProposeRegistrationStatusClick(proposal: RegistrationStatus) {
		this.proposeRegistrationStatus.emit(proposal);
	}

	onProposeRegistrationStatusCancel() {
		this.proposeRegistrationStatus.emit(undefined);
	}

	onProposePublicationLevelCancel() {
		this.proposePublicationLevel.emit(undefined);
	}

	onSetPublicationLevelClick(level: PublicationLevel) {
		this.setPublicationLevel.emit(level);
	}

	onProposePublicationLevelClick(proposal: PublicationLevel) {
		this.proposePublicationLevel.emit(proposal);
	}

	onChangeLockToggle(event: MatSlideToggleChange) {
		// eslint-disable-next-line max-len
		this.translate.get([this.lockDialogHeaderKey, this.lockDialogTextKey, DIALOG_CANCEL_BUTTON_KEY, DIALOG_LOCK_BUTTON_KEY]).subscribe(result => {
			const dialogRef = this.dialog.open(DialogComponent, {
				data: {
					showHeader: true,
					headerText: result[this.lockDialogHeaderKey],
					bodyText: result[this.lockDialogTextKey],
					dialogType: DialogType.confirm,
					cancelButtonText: result[DIALOG_CANCEL_BUTTON_KEY],
					confirmButtonText: result[DIALOG_LOCK_BUTTON_KEY]
				},
				disableClose: true
			});
			const dialogCancel = dialogRef.componentInstance.cancel.subscribe(() => {
				this.isLocked = !event.checked;
			});
			const dialogConfirm = dialogRef.componentInstance.confirm.subscribe(() => {
				this.lock.emit(event.checked);
			});
			dialogRef.afterClosed().subscribe(() => {
				dialogCancel.unsubscribe();
				dialogConfirm.unsubscribe();
			});
		});
	}

	redirectToExternalResource(): string {
		let concepts = this.route?.parent?.snapshot?.url[1]?.path === 'concepts';
		let datasets = this.route?.parent?.snapshot?.url[1]?.path === 'datasets';
		let dataservices = this.route?.parent?.snapshot?.url[1]?.path === 'dataservices';
		let publicservices = this.route?.parent?.snapshot?.url[1]?.path === 'publicservices';
		let mappingtables = this.route?.parent?.snapshot?.url[1]?.path === 'mappingtables';

		const id = this.route.snapshot.params.id || this.route.snapshot.params.conceptId;
		const i14yPublicRoute = AppConfig.getConfig<IAppConfig>().I14Y_PUBLIC_ROUTE;

		const routes = {
			concepts: `${i14yPublicRoute}/${this.currentLanguage}/concepts/${id}/description`,
			dataservices: `${i14yPublicRoute}/${this.currentLanguage}/catalog/dataservices/${id}`,
			publicservices: `${i14yPublicRoute}/${this.currentLanguage}/catalog/publicservices/${this.identifier}/description`,
			datasets: `${i14yPublicRoute}/${this.currentLanguage}/catalog/datasets/${this.identifier}/description`,
			mappingtables: `${i14yPublicRoute}/${this.currentLanguage}/catalog/mappingtables/${id}/description`
		};

		if (concepts) {
			return routes.concepts;
		}
		if (dataservices) {
			return routes.dataservices;
		}
		if (publicservices) {
			return routes.publicservices;
		}
		if (datasets) {
			return routes.datasets;
		}
		if (mappingtables) {
			return routes.mappingtables;
		}
		return '';
	}

	showDropdownButton<T>(menuEntries: T[] | undefined): boolean {
		return menuEntries ? menuEntries.length > 0 : false;
	}

	private mergeArrays<T>(arr1: T[], arr2: T[]): T[] {
		return Array.from(new Set([...arr1, ...arr2]));
	}
}
