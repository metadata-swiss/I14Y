import {Component, inject, Input, OnChanges, OnDestroy, OnInit, SimpleChanges} from '@angular/core';
import {AgentModel, ChannelModel, MultiLanguage, PublicServiceModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent} from '@ngx-translate/core';
import {Subject} from 'rxjs';
import {SortableListViewComponent} from 'src/app/shared/sortable-list-view/sortable-list-view.component';
import {takeUntil} from 'rxjs/operators';
import {DIALOG_CANCEL_BUTTON_KEY} from '../../../../app-constants';
import {ArrayToStringPipe} from 'src/app/shared/formatting/array-to-string.pipe';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {SelectionModel} from '@angular/cdk/collections';
import {MatDialog, MatDialogConfig} from '@angular/material/dialog';
import {Languages} from 'src/app/shared/ApplicationLanguage.enum';
import {DialogComponent, DialogType} from 'src/app/shared/dialog/dialog.component';
import {ModalDialogChannelComponent} from './modal-dialog/modal-dialog.component';

type SortableKeys = 'identifier' | 'type' | 'ownedBy';

@Component({
	selector: 'app-edit-table-channel',
	templateUrl: './edit-table-channel.component.html',
	styleUrls: ['./edit-table-channel.component.scss'],
	standalone: false
})
export class EditTableChannelComponent extends SortableListViewComponent<ChannelModel, SortableKeys> implements OnInit, OnDestroy, OnChanges {
	@Input() publicService!: PublicServiceModel;
	readonly COLUMN_SELECT = 'select';
	readonly COLUMN_IDENTIFIER = 'identifier';
	readonly COLUMN_TYPE = 'type';
	readonly COLUMN_OWNEDBY = 'ownedBy';
	readonly COLUMN_ACTIONS = 'actions';

	displayedColumns: string[] = [this.COLUMN_SELECT, this.COLUMN_IDENTIFIER, this.COLUMN_TYPE, this.COLUMN_OWNEDBY, this.COLUMN_ACTIONS];
	currentLanguage: string;
	contentLanguages: readonly string[] = Languages.ContentLanguagesRm;

	private readonly selection = new SelectionModel<ChannelModel>(true, []);
	private readonly unsubscribe$ = new Subject();
	private readonly arrayToString = inject(ArrayToStringPipe);
	private readonly fallback = inject(FallbackPipe);
	private readonly dialog = inject(MatDialog);

	constructor() {
		super();

		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit() {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => (this.currentLanguage = language.lang));
	}

	ngOnChanges(changes: SimpleChanges): void {
		let change = changes.publicService;
		if (change.currentValue && change.currentValue.channels) {
			this.dataSource.data = change.currentValue.channels;
		}
	}

	onMasterToggle(): void {
		if (this.isAllSelected()) {
			this.selection.clear();
		} else {
			this.dataSource.data.forEach(row => this.selection.select(row));
		}
	}

	onToggleSelection(row: ChannelModel): void {
		this.selection.toggle(row);
	}

	onRemoveSelectedRows() {
		const headertextKey = 'i18n.dialog.delete_channel.header_text';
		const bodytextKey = 'i18n.dialog.delete_channel.body_text';
		const confirmButtontextKey = 'i18n.button.confirm_delete';
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
				this.deleteChannel(this.selection.selected);
				this.selection.clear();
			});
			dialogRef.afterClosed().subscribe(() => {
				dialogConfirm.unsubscribe();
			});
		});
	}

	onEditRow(channel: ChannelModel) {
		this.showDialog(channel, true);
	}

	onRemoveRow(channel: ChannelModel) {
		const headertextKey = 'i18n.dialog.delete_channel.header_text';
		const bodytextKey = 'i18n.dialog.delete_channel.body_text';
		const confirmButtontextKey = 'i18n.button.confirm_delete';
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
				this.deleteChannel([channel]);
			});
			dialogRef.afterClosed().subscribe(() => {
				dialogConfirm.unsubscribe();
			});
		});
	}

	onAddRow() {
		this.showDialog(this.createChannel(), false);
	}

	hasSelectedRows(): boolean {
		return this.selection.hasValue();
	}

	isAllSelected(): boolean {
		const numSelected = this.selection.selected.length;
		const numRows = this.dataSource.data.length;

		return numSelected === numRows;
	}

	isRowSelected(row: ChannelModel): boolean {
		return this.selection.isSelected(row);
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	getOwnedBy(element: ChannelModel): string {
		return this.arrayToString.transform(
			element.ownedBy?.map(o => this.mapOwnedByName(o)).filter(o => o.length > 0),
			', ',
			'-'
		);
	}

	mapOwnedByName(agent: AgentModel): string {
		return this.fallback.transform(agent.name, this.currentLanguage) ?? '';
	}

	private refreshDatabinding(): void {
		this.dataSource.filter = '';
	}

	private createChannel(): ChannelModel {
		return new ChannelModel({
			description: new MultiLanguage(),
			id: '',
			type: undefined,
			identifier: undefined,
			openingHours: undefined,
			address: new MultiLanguage(),
			email: undefined,
			fax: undefined,
			mobile: undefined,
			phone: undefined,
			url: undefined,
			ownedBy: undefined
		});
	}

	private deleteChannel(channelsTodelete: ChannelModel[]) {
		if (this.publicService.channels && channelsTodelete.length > 0) {
			channelsTodelete.forEach(element => {
				if (element) {
					const index = this.publicService.channels!.indexOf(element);
					if (index !== -1) {
						this.publicService.channels?.splice(index, 1);
					}
				}
			});
			this.dataSource.data = this.publicService.channels ?? [];
		}
	}

	private showDialog(channel: ChannelModel, isEditMode: boolean) {
		let isEdit = channel.identifier !== undefined && channel.identifier.length > 0;
		const dialogConfig = new MatDialogConfig();
		this.updateDialogConfig(channel, dialogConfig, isEditMode);

		const dialogRef = this.dialog.open(ModalDialogChannelComponent, dialogConfig);

		dialogRef.afterClosed().subscribe(data => {
			if (data?.channel) {
				if (!isEdit) {
					if (!this.publicService.channels) {
						this.publicService.channels = [];
					}
					this.publicService.channels.push(data?.channel);
				} else {
					const index = this.publicService.channels?.findIndex(c => c.id === data?.channel.id);
					if (index !== undefined && index > -1) {
						this.publicService.channels![index] = data?.channel;
					}
				}
				this.dataSource.data = this.publicService.channels ?? [];
				this.refreshDatabinding();
			}
		});
	}

	private updateDialogConfig(channel: ChannelModel, dialogConfig: MatDialogConfig<any>, isEditMode: boolean) {
		dialogConfig.data = {contentLanguages: this.contentLanguages, channel: channel, isEditMode: isEditMode};
		dialogConfig.width = '70%';
		dialogConfig.maxWidth = '1200px';
		dialogConfig.minWidth = '600px';
		dialogConfig.disableClose = true;
		dialogConfig.autoFocus = true;
	}
}
