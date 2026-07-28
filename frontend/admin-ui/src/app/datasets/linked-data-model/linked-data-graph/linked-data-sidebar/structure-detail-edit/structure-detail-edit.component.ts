import {Component, EventEmitter, inject, Input, model, Output, ViewChild} from '@angular/core';
import {
	CatalogClient,
	CatalogEntry,
	SearchResourceType,
	MultiLanguage,
	SchemaClass,
	SchemaProperty
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {StructureDetailEditFormComponent} from './structure-detail-edit-form/structure-detail-edit-form.component';
import {FormControl, FormGroup, Validators} from '@angular/forms';
import {Languages} from '../../../../../shared/ApplicationLanguage.enum';
import {ObNotificationService} from '@oblique/oblique';
import {ActivatedRoute} from '@angular/router';
import {filter, map, Observable, Subject, switchMap, tap, EMPTY} from 'rxjs';
import {ArrayHelper} from 'src/app/shared/helper/array-helper';
import {DialogComponent, DialogType} from 'src/app/shared/dialog/dialog.component';
import {TranslateService} from '@ngx-translate/core';
import {DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY} from 'src/app/app-constants';
import {MatDialog} from '@angular/material/dialog';
import {UriHelper} from 'src/app/shared/helper/uri-helper';
import {LinkedDataModelWriteService} from '../../../services/linked-data-model-write.service';


@Component({
	selector: 'app-structure-detail-edit',
	templateUrl: './structure-detail-edit.component.html',
	standalone: false
})
export class StructureDetailEditComponent {
	@ViewChild(StructureDetailEditFormComponent, {static: true}) structureEditForm!: StructureDetailEditFormComponent;
	@Input() selectedClassUri?: string;
	@Input() selectedDto!: SchemaClass | SchemaProperty;
	@Output() updateDto = new EventEmitter<SchemaClass | SchemaProperty>();

	isEditMode = model<boolean>();
	form!: FormGroup;
	autoCompleteItems: CatalogEntry[] = [];
	loading = false;
	hasMore = true;

	private datasetId;
	private search$ = new Subject<string | null>();
	private loadMore$ = new Subject<void>();
	private currentQuery: string | null | undefined;
	private page = 1;
	private pageSize = 20;
	private isCreationMode = false;

	private readonly unsubscribe$ = new Subject<void>();
	private readonly writeService = inject(LinkedDataModelWriteService);
	private readonly catalogClient = inject(CatalogClient);
	private readonly route = inject(ActivatedRoute);
	private readonly dialog = inject(MatDialog);
	private readonly notification = inject(ObNotificationService);
	private readonly contentLanguages: readonly string[] = Languages.ContentLanguages;
	private readonly translate = inject(TranslateService);

	constructor() {
		this.datasetId = this.route.parent?.snapshot.params.id;
	}

	ngOnInit(): void {
		this.form = this.createEmtpyForm();
		this.mapDataToForm();
		this.initSearchStream();
		this.isCreationMode = this.selectedDto.identifier === undefined || this.selectedDto.identifier.length === 0;
	}

	createEmtpyForm(): FormGroup {
		if (this.selectedDto instanceof SchemaProperty) {
			return (this.selectedDto.toClassUri?.length ?? 0) > 0 ? this.createAssociationsForm() : this.createSchemaPropertyForm();
		} else {
			return this.createSchemaClassForm();
		}
	}

	onCancel(): void {
		if (!this.form.dirty) {
			this.structureEditForm.form.reset();
			this.isEditMode.set(false);
		} else {
			const isEdit = this.isEditMode();
			const headertextKey: string = isEdit ? 'i18n.edit.cancel_dialog.headertext' : 'i18n.create.cancel_dialog.headertext';
			const bodytextKey: string = isEdit ? 'i18n.edit.cancel_dialog.bodytext' : 'i18n.create.cancel_dialog.bodytext';

			this.translate.get([headertextKey, bodytextKey, DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY]).subscribe(result => {
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
					this.structureEditForm.form.reset();
					this.isEditMode.set(false);
				});
				dialogRef.afterClosed().subscribe(() => {
					dialogConfirm.unsubscribe();
				});
			});
		}
	}

	onSave(): void {
		this.structureEditForm.form.markAllAsTouched();
		if (this.structureEditForm.form.valid) {
			this.mapFormToData();
			this.save$(this.selectedDto).subscribe({
				next: () => {
					this.notification.success('i18n.notification.save_succeeded');
					if (UriHelper.GetUriFragment(this.selectedDto.uriComplete) !== this.selectedDto.identifier) {
						this.selectedDto.uriComplete = UriHelper.replaceLastSegment(this.selectedDto.uriComplete!, this.selectedDto.identifier!);
					}
					this.isCreationMode = false;
					this.updateDto.emit(this.selectedDto);
				},
				error: () => {
					this.notification.error('i18n.notification.save_failed');
				}
			});
		}
	}

	onSaveAndClose(): void {
		this.structureEditForm.form.markAllAsTouched();
		if (this.structureEditForm.form.valid) {
			this.mapFormToData();
			this.save$(this.selectedDto).subscribe({
				next: () => {
					this.notification.success('i18n.notification.save_succeeded');
					if (UriHelper.GetUriFragment(this.selectedDto.uriComplete) !== this.selectedDto.identifier) {
						this.selectedDto.uriComplete = UriHelper.replaceLastSegment(this.selectedDto.uriComplete!, this.selectedDto.identifier!);
					}
					this.isEditMode.set(false);
					this.isCreationMode = false;
					this.updateDto.emit(this.selectedDto);
				},
				error: () => {
					this.notification.error('i18n.notification.save_failed');
				}
			});
		}
	}

	ngOnDestroy(): void {
		this.unsubscribe$.next();
		this.unsubscribe$.complete();
	}

	searchAutoComplete(query: string | null): void {
		this.search$.next(query);
	}

	loadNextPage(): void {
		this.loadMore$.next();
	}

	private initSearchStream(): void {
		this.search$
			.pipe(
				tap(query => {
					this.currentQuery = query;
					// reset state
					this.page = 1;
					this.hasMore = true;
					this.autoCompleteItems = [];
					this.loading = true;
				}),
				switchMap(() => this.searchConcepts(this.currentQuery, this.page, this.pageSize)),
				map(res => res.result) // CatalogEntry[]
			)
			.subscribe(items => {
				this.autoCompleteItems = items;
				this.page++;
				this.loading = false;
				this.hasMore = items.length === this.pageSize;
			});

		this.loadMore$
			.pipe(
				filter(() => !this.loading && this.hasMore),
				tap(() => (this.loading = true)),
				switchMap(() => this.searchConcepts(this.currentQuery, this.page, this.pageSize)),
				map(res => res.result)
			)
			.subscribe(items => {
				this.autoCompleteItems = [...this.autoCompleteItems, ...items];
				this.page++;
				this.loading = false;
				this.hasMore = items.length === this.pageSize;
			});
	}

	private searchConcepts(query: string | null | undefined, page: number, pageSize: number) {
		return this.catalogClient.getSearchByQueryAndAccessRightsAndConceptValueTypesAndFormatsAndBusinessEventsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypesAndPageAndPageSize(
			query ?? undefined,
			undefined,
			undefined,
			undefined,
			undefined,
			undefined,
			undefined,
			undefined,
			undefined,
			undefined,
			undefined,
			undefined,
			undefined,
			[SearchResourceType.Concept],
			page,
			pageSize
		);
	}

	private save$(dto: SchemaClass | SchemaProperty): Observable<unknown> {
		if (dto instanceof SchemaProperty && (this.selectedClassUri?.length ?? 0) > 0) {
			return this.writeService.saveProperty(this.datasetId, this.selectedClassUri!, dto, this.isCreationMode);
		}
		if (dto instanceof SchemaClass) {
			return this.writeService.saveClass(this.datasetId, dto, this.isCreationMode);
		}
		return EMPTY;
	}

	private mapDataToForm(): void {
		this.form.patchValue({
			uri : this.selectedDto.uriComplete,
			title: this.selectedDto.label,
			description: this.selectedDto.description,
			identifier: this.selectedDto.identifier
		});
		if (this.selectedDto instanceof SchemaProperty) {
			this.form.patchValue({
				uri : this.selectedDto.path,
				identifier: this.selectedDto.identifier,
				dataType: this.selectedDto.dataType,
				pattern: this.selectedDto.pattern,
				minCount: this.selectedDto.minCardinality,
				maxCount: this.selectedDto.maxCardinality,
				conformsTo: this.selectedDto.conformsTo,
				minLength: this.selectedDto.minLength,
				maxLength: this.selectedDto.maxLength,
				unit: this.selectedDto.unit,
				order: this.selectedDto.order,
				allowedValues: ArrayHelper.convertArrayToString(this.selectedDto.allowedValues)
			});
		}
	}

	private ensureMultiLanguage(value?: MultiLanguage): MultiLanguage {
		if (value instanceof MultiLanguage) {
			return value;
		}

		return new MultiLanguage(value ?? {});
	}

	private mapFormToData() {
		if (this.selectedDto.uriComplete === undefined || this.selectedDto.uriComplete.length === 0) {
			this.selectedDto.uriComplete = this.form.value.uri;
		}
		this.selectedDto.label = this.ensureMultiLanguage(this.selectedDto.label);
		this.selectedDto.description = this.ensureMultiLanguage(this.selectedDto.description);
		this.contentLanguages.forEach(l => {
			this.selectedDto.label![l as keyof MultiLanguage] = this.form.value.title?.[l] || undefined;
			this.selectedDto.description![l as keyof MultiLanguage] = this.form.value.description?.[l] || undefined;
		});

		this.selectedDto.identifier = this.form.value.identifier;

		// Symmetric IRI-finalization for both SchemaClass and SchemaProperty:
		// when the placeholder IRI initialized on creation ends with '/', append the
		// identifier the user just entered to produce the final IRI.
		if (this.selectedDto instanceof SchemaClass) {
			if (this.selectedDto.uriComplete?.endsWith('/') && this.selectedDto.identifier) {
				this.selectedDto.uriComplete = `${this.selectedDto.uriComplete}${this.selectedDto.identifier}`;
			}
		}

		if (this.selectedDto instanceof SchemaProperty) {
			if (this.selectedDto.path?.endsWith('/') && this.selectedDto.identifier) {
				this.selectedDto.path = `${this.selectedDto.path}${this.selectedDto.identifier}`;
			}
			this.selectedDto.dataType = this.form.value.dataType;
			this.selectedDto.pattern = this.form.value.pattern;
			this.selectedDto.conformsTo = this.form.value.conformsTo;
			this.selectedDto.minCardinality = this.form.value.minCount;
			this.selectedDto.maxCardinality = this.form.value.maxCount;
			this.selectedDto.minLength = this.form.value.minLength;
			this.selectedDto.maxLength = this.form.value.maxLength;
			this.selectedDto.order = this.form.value.order;
			this.selectedDto.unit = this.form.value.unit;
			this.selectedDto.allowedValues = ArrayHelper.convertStringToArray(this.form.value.allowedValues);
		}
	}

	private getObjectFromKeys<Type>(keys: readonly string[], initialValue: (key: string) => Type) {
		return Object.assign({}, ...keys.map(x => ({[x]: initialValue(x)})));
	}

	private createSchemaPropertyForm(): FormGroup {
		return new FormGroup({
			uri: new FormControl([]),
			title: new FormGroup(this.getObjectFromKeys(this.contentLanguages, () => new FormControl(''))),
			description: new FormGroup(this.getObjectFromKeys(this.contentLanguages, () => new FormControl(''))),
			identifier: new FormControl([], Validators.required),
			dataType: new FormControl([]),
			pattern: new FormControl([]),
			conformsTo: new FormControl([]),
			minCount: new FormControl([]),
			maxCount: new FormControl([]),
			minLength: new FormControl([]),
			maxLength: new FormControl([]),
			order: new FormControl([]),
			unit: new FormControl([]),
			allowedValues: new FormControl([])
		});
	}

	private createSchemaClassForm(): FormGroup {
		return new FormGroup({
			uri: new FormControl([]),
			title: new FormGroup(this.getObjectFromKeys(this.contentLanguages, () => new FormControl(''))),
			description: new FormGroup(this.getObjectFromKeys(this.contentLanguages, () => new FormControl(''))),
			identifier: new FormControl([], Validators.required)
		});
	}

	private createAssociationsForm(): FormGroup {
		return new FormGroup({
			title: new FormGroup(this.getObjectFromKeys(this.contentLanguages, () => new FormControl(''))),
			description: new FormGroup(this.getObjectFromKeys(this.contentLanguages, () => new FormControl(''))),
			identifier: new FormControl([], Validators.required),
			minCount: new FormControl([]),
			maxCount: new FormControl([])
		});
	}
}
