import {Component, EventEmitter, inject, Input, model, OnChanges, OnDestroy, OnInit, Output, SimpleChanges} from '@angular/core';
import {SchemaClass, SchemaProperty} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {ObNotificationService} from '@oblique/oblique';
import {Observable, of, Subject, takeUntil} from 'rxjs';
import {MatDialog} from '@angular/material/dialog';
import {DialogComponent, DialogType} from '../../../../shared/dialog/dialog.component';
import {DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY} from '../../../../app-constants';
import {LinkedDataModelWriteService} from '../../services/linked-data-model-write.service';

@Component({
	selector: 'app-linked-data-sidebar',
	templateUrl: './linked-data-sidebar.component.html',
	styleUrl: './linked-data-sidebar.component.scss',
	standalone: false
})
export class LinkedDataSidebarComponent implements OnChanges, OnInit, OnDestroy {
	@Input() selectedClass: SchemaClass | undefined;
	@Input() selectedProperty: SchemaProperty | undefined;
	@Input() isPropertySelected: boolean | undefined;
	@Input() selectedClassUri: string | undefined;
	@Input() datasetId: string | undefined;
	@Input() cannotEdit$: Observable<boolean> = of(true);
	@Output() updateDto = new EventEmitter<SchemaClass | SchemaProperty>();
	@Output() deleteDto = new EventEmitter<SchemaClass | SchemaProperty>();

	isEditMode = model<boolean>();
	selectedDto: SchemaClass | SchemaProperty | undefined;

	type: string | undefined;
	currentLanguage: string;

	TRANSLATION_PREFIX = 'i18n.datasets.linkeddatamodel.sidebar';

	private readonly unsubscribe$ = new Subject<void>();
	private readonly translate = inject(TranslateService);
	private readonly dialog = inject(MatDialog);
	private readonly writeService = inject(LinkedDataModelWriteService);
	private readonly notification = inject(ObNotificationService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		// Watch for language changes
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnChanges(changes: SimpleChanges): void {
		//change the property
		if (changes.selectedProperty && changes.selectedProperty.previousValue !== changes.selectedProperty.currentValue && this.isPropertySelected) {
			this.type = changes.selectedProperty.currentValue.toClassUri && changes.selectedProperty.currentValue.toClassUri.length > 0 ? 'association' : 'property';
			this.selectedDto = changes.selectedProperty.currentValue;
			// change the between class
		} else if (changes.selectedClass && changes.selectedClass.previousValue !== changes.selectedClass.currentValue && !this.isPropertySelected) {
			this.type = 'class';
			this.selectedDto = changes.selectedClass.currentValue;
			// change the selection from property to class of same class
		} else if (changes.isPropertySelected && changes.isPropertySelected.currentValue === false && this.selectedClass) {
			this.type = 'class';
			this.selectedDto = this.selectedClass;
			// change the selection from class to property of the same class
		} else if (changes.isPropertySelected && changes.isPropertySelected.currentValue === true && this.selectedProperty) {
			const property = this.selectedProperty;
			const hasAssociation = property?.toClassUri?.length ?? 0 > 0;
			this.type = hasAssociation ? 'association' : 'property';
			this.selectedDto = this.selectedProperty;
		}
	}

	toggleEditMode(): void {
		this.isEditMode.set(true);
	}

	ngOnDestroy(): void {
		this.unsubscribe$.next();
		this.unsubscribe$.complete();
	}

	updateDtoToGraph(dtoToUpdate: SchemaClass | SchemaProperty) {
		this.updateDto.emit(dtoToUpdate);
	}

	onDeleteClick(): void {
		if (!this.selectedDto) {
			return;
		}
		const headertextKey = 'i18n.delete_dialog.header';
		const bodytextKey = 'i18n.delete_dialog.body';
		const confirmButtontextKey = 'i18n.delete_dialog.confirmbutton';

		this.translate
			.get([headertextKey, bodytextKey, confirmButtontextKey, DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY])
			.subscribe(result => {
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
					if (!this.selectedDto) {
						return;
					}

					const dto = this.selectedDto;
					const isProperty = 'path' in dto;

					if (isProperty && this.datasetId) {
						const property = dto as SchemaProperty;
						const propertyUri = property.uriComplete ?? property.path;
						if (!propertyUri) {
							return;
						}
						this.writeService.deleteProperty(this.datasetId, propertyUri).subscribe(() => {
							this.notification.success('i18n.notification.deleted');
							this.deleteDto.emit(dto);
						});
					} else {
						this.deleteDto.emit(dto);
					}
				});
				dialogRef.afterClosed().subscribe(() => {
					dialogConfirm.unsubscribe();
				});
			});
	}
}
