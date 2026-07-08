import {AfterViewInit, Component, EventEmitter, inject, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, ViewChild} from '@angular/core';
import {
	CodeListEntryValueTypeEnum,
	CodeListEntryDetail,
	ConceptInputClient,
	ConceptType,
	ConceptView,
	FileParameter,
	IActiveDirectoryUser,
	IAgent,
	IPerson,
	VocabularyClient,
	VocabularyEntry,
	CodeListEntrySortProperty,
	ConceptViewClient,
	CodeListEntriesDataFormat,
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {UntypedFormGroup, Validators} from '@angular/forms';
import {Languages} from '../../../shared/ApplicationLanguage.enum';
import {map, Observable, startWith, Subject, takeUntil} from 'rxjs';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {CustomErrorStateMatcherVersion} from 'src/app/shared/validators/custom-state-matcher-version';
import {ObNotificationService} from '@oblique/oblique';
import {EditTableCodelistComponent} from 'src/app/shared/edit-table-codelist/edit-table-codelist.component';
import {CodelistIdentifierValidator} from 'src/app/shared/validators/codelist-identifier.validator';
import {MatDialog} from '@angular/material/dialog';
import {DialogComponent, DialogType} from 'src/app/shared/dialog/dialog.component';
import {DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY} from 'src/app/app-constants';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {SearchResultPagingInfo} from 'src/app/shared/searchResultPagingInfo';
import {PageEvent} from '@angular/material/paginator';

@Component({
	selector: 'app-concept-edit-form',
	templateUrl: './concept-edit-form.component.html',
	styleUrls: ['./concept-edit-form.component.scss'],
	standalone: false
})
export class ConceptEditFormComponent implements OnInit, OnDestroy, OnChanges, AfterViewInit {
	@Input() isEditMode: boolean = false;
	@Input() isVersionMode: boolean = false;
	@Input() isFirstIdentifierLocked: boolean = false;
	// Read model (ConceptView) passed down from the parent; the parent maps it to the ConceptInput write model on save.
	@Input() dto: ConceptView = new ConceptView();
	@Input() initialConceptType: ConceptType | undefined;
	@Input() form!: UntypedFormGroup;
	@Input() public set agents(input: IAgent[]) {
		this.allAgents.splice(0, this.allAgents.length, ...input);
	}
	@Output() disableSave: EventEmitter<boolean> = new EventEmitter();
	@ViewChild(EditTableCodelistComponent, {static: true}) editableTable!: EditTableCodelistComponent;

	conceptTypes = Object.values(ConceptType);
	codeListEntryValueTypes = Object.values(CodeListEntryValueTypeEnum);
	codeListEntryDefaultSortProperty = Object.values(CodeListEntrySortProperty);
	conceptTypeEnum = ConceptType;
	codeListEntryValueTypeEnum = CodeListEntryValueTypeEnum;
	selectedConceptType: ConceptType | undefined;
	changeContentTypeConfirmed: boolean = false;
	currentLanguage: string;
	showAllLanguages: boolean;
	title: string;
	filteredAgents$!: Observable<IAgent[]>;
	themes$: Observable<VocabularyEntry[]>;
	formatThemes$: Observable<VocabularyEntry[]>;
	contentLanguages: readonly string[] = Languages.ContentLanguages;
	errorStateMatcherVersion = new CustomErrorStateMatcherVersion();
	codelistEntries: CodeListEntryDetail[] = [];
	pagingInfo: SearchResultPagingInfo = new SearchResultPagingInfo(undefined);

	private readonly allAgents: IAgent[] = [];
	private readonly datasetTheme = 'Concept_DATASET_THEME';
	private readonly defaultPage: number = 1;
	private readonly editingChildren: string[] = [];
	private readonly unsubscribe$ = new Subject();

	private readonly codelistIdentifierValidator = inject(CodelistIdentifierValidator);
	private readonly conceptInputClient = inject(ConceptInputClient);
	private readonly conceptViewClient = inject(ConceptViewClient);
	private readonly dialog = inject(MatDialog);
	private readonly fallback = inject(FallbackPipe);
	private readonly notification = inject(ObNotificationService);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyClient = inject(VocabularyClient);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.showAllLanguages = true;
		this.title = '';
		this.themes$ = this.vocabularyClient.getByIdentifier(this.datasetTheme).pipe(map(response => response.result));
		this.formatThemes$ = this.themes$.pipe(
			map(x =>
				[...x].sort((a, b) => {
					const aName = this.fallback.transform(a.name, this.currentLanguage);
					const bName = this.fallback.transform(b.name, this.currentLanguage);
					return aName!.localeCompare(bName!);
				})
			)
		);
	}

	ngOnInit(): void {
		let publisher = this.form.get('publisher');
		if (publisher) {
			this.filteredAgents$ = publisher.valueChanges.pipe(
				startWith(''),
				map(value => this.mapValue(value)),
				map(term => (term ? this.filterAgents(term) : this.allAgents))
			);
		}

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
			this.showAllLanguages = false;
			this.updateTitle();
		});
		this.codelistIdentifierValidator.conceptId = this.dto.id;
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	ngOnChanges(changes: SimpleChanges): void {
		this.updateTitle();
		if (changes.dto?.currentValue) {
			this.updateTypeSpecificValidators(this.dto.conceptType?.toString());
			this.selectedConceptType = this.dto.conceptType;
			this.updateCodeListEntries(this.defaultPage);
		}
	}

	ngAfterViewInit() {
		this.form.get('agencyName')?.markAsTouched();
	}

	onChangePage(pageEvent: PageEvent) {
		this.updateCodeListEntries(pageEvent.pageIndex + 1);
	}

	canCreateCodeValues(): boolean {
		return this.isEditMode;
	}

	canImport(): boolean {
		return this.isEditMode && this.codelistEntries.length === 0;
	}

	onConceptTypeSelected(type: string, event: any) {
		if (event.isUserInput) {
			if (this.isEditMode && !this.changeContentTypeConfirmed) {
				const headertextKey = 'i18n.datasets.concept.type.dialog.headertext';
				const bodytextKey = 'i18n.datasets.concept.type.dialog.bodytext';

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
					const dialogCancel = dialogRef.componentInstance.cancel.subscribe(() => {
						this.form.get('conceptType')?.setValue(this.selectedConceptType);
						this.form.get('conceptType')?.markAsPristine();
					});
					const dialogConfirm = dialogRef.componentInstance.confirm.subscribe(() => {
						this.changeConceptType(type);
						this.changeContentTypeConfirmed = true;
					});
					dialogRef.afterClosed().subscribe(() => {
						dialogCancel.unsubscribe();
						dialogConfirm.unsubscribe();
					});
				});
			} else {
				this.changeConceptType(type);
			}
		}
	}

	onChildIsEditing(name: string, isEditing: boolean): void {
		if (isEditing) {
			this.editingChildren.push(name);
		} else {
			const index = this.editingChildren.indexOf(name, 0);
			if (index > -1) {
				this.editingChildren.splice(index, 1);
			}
		}

		this.disableSave.emit(this.editingChildren.length > 0);
	}

	handleImportJson(files: FileList, input: HTMLInputElement): void {
		// only take first file
		const file: File = files[0];
		if (file && this.dto.id) {
			this.notification.info({
				title: 'i18n.datasets.content.import.notifications.title',
				message: 'i18n.datasets.content.import.notifications.started',
				messageParams: {fileName: file.name}
			});
			const fileParameter: FileParameter = {fileName: file.name, data: file};

			this.conceptInputClient.postCodelistEntriesImportsByIdAndFormatAndBody(this.dto.id, CodeListEntriesDataFormat.Json, fileParameter).subscribe({
				next: _ => {
					this.notification.success({
						title: 'i18n.datasets.content.import.notifications.title',
						message: 'i18n.datasets.content.import.notifications.success',
						messageParams: {fileName: file.name}
					});
					this.updateCodeListEntries(this.defaultPage);
				},
				error: error => {
					this.notification.error({
						title: 'i18n.datasets.content.import.notifications.title',
						message: 'i18n.datasets.content.import.notifications.error',
						messageParams: {error: error.detail, fileName: file.name},
						sticky: true
					});
				},
				complete: () => {
					// empty input upon complete
					input.value = '';
				}
			});
		}
	}

	handleImportCsv(files: FileList, input: HTMLInputElement): void {
		// only take first file
		const file: File = files[0];
		if (file && this.dto.id) {
			this.notification.info({
				title: 'i18n.datasets.content.import.notifications.title',
				message: 'i18n.datasets.content.import.notifications.started',
				messageParams: {fileName: file.name}
			});
			const fileParameter: FileParameter = {fileName: file.name, data: file};

			this.conceptInputClient.postCodelistEntriesImportsByIdAndFormatAndBody(this.dto.id, CodeListEntriesDataFormat.Csv, fileParameter).subscribe({
				next: _ => {
					this.notification.success({
						title: 'i18n.datasets.content.import.notifications.title',
						message: 'i18n.datasets.content.import.notifications.success',
						messageParams: {fileName: file.name}
					});
					this.updateCodeListEntries(this.defaultPage);
				},
				error: error => {
					this.notification.error({
						title: 'i18n.datasets.content.import.notifications.title',
						message: 'i18n.datasets.content.import.notifications.error',
						messageParams: {error: error.detail, fileName: file.name},
						sticky: true
					});
				},
				complete: () => {
					// empty input upon complete
					input.value = '';
				}
			});
		}
	}

	deleteAllCodeList(): void {
		const headertextKey = 'i18n.datasets.concept.delete.codelist.dialog.header';
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
				this.conceptInputClient.deleteCodelistEntriesById(this.dto.id as string).subscribe(() => {
					this.notification.success('i18n.notification.deleted');
					this.updateCodeListEntries(this.pagingInfo.page);
				});
			});
			dialogRef.afterClosed().subscribe(() => {
				dialogConfirm.unsubscribe();
			});
		});
	}

	displayFn = (user: IPerson & IActiveDirectoryUser): string => {
		return user?.displayName ? user?.displayName : (user?.name as string);
	};

	displayValue = (user: any) => {
		return this.fallback.transform(user?.name, this.currentLanguage) ?? '';
	};

	public updateCodeListEntries(page: number): void {
		if (this.dto.id) {
			this.conceptViewClient.getCodelistEntriesByIdAndPageAndPageSize(this.dto.id, page, this.pagingInfo.pageSize).subscribe(response => {
				this.pagingInfo = new SearchResultPagingInfo(response.headers);
				this.codelistEntries = response.result;

				if (this.codelistEntries.length === 0 && page > 1) {
					this.updateCodeListEntries(this.defaultPage);
				}
			});
		}
	}

	private updateTitle(): void {
		const titleKey = this.isEditMode ? 'i18n.datasets.concept.edit.new.title' : 'i18n.datasets.concept.create.new.title';
		const firstIdentifier = this.dto.identifiers?.[0];
		this.translate.get(titleKey, {identifier: firstIdentifier}).subscribe(result => {
			this.title = result;
		});
	}

	private changeConceptType(type: string) {
		this.resetMandatoryFields();
		this.markMandatoryFieldsAsUntouched();

		this.selectedConceptType = this.conceptTypes.find(x => x === type);

		this.updateTypeSpecificValidators(type);
	}

	private resetMandatoryFields(): void {
		this.form.get('pattern')?.setValue('');
		this.form.get('maxLength')?.setValue('');
		this.form.get('minLength')?.setValue('');
		this.form.get('minValue')?.setValue('');
		this.form.get('maxValue')?.setValue('');
		this.form.get('nbDecimal')?.setValue('');
		this.form.get('measurementUnit')?.setValue('');
		this.form.get('codeListEntryValueTyp')?.setValue('');
		this.form.get('codelistEntryValueMaxLength')?.setValue('');
	}

	private markMandatoryFieldsAsUntouched(): void {
		this.form.get('pattern')?.markAsUntouched();
		this.form.get('maxLength')?.markAsUntouched();
		this.form.get('minLength')?.markAsUntouched();
		this.form.get('minValue')?.markAsUntouched();
		this.form.get('maxValue')?.markAsUntouched();
		this.form.get('nbDecimal')?.markAsUntouched();
		this.form.get('measurementUnit')?.markAsUntouched();
		this.form.get('codeListEntryValueTyp')?.markAsUntouched();
		this.form.get('codelistEntryValueMaxLength')?.markAsUntouched();
	}

	private updateTypeSpecificValidators(type: string | undefined) {
		this.removeErrors();
		this.clearValidators();

		switch (type) {
			case this.conceptTypeEnum.String:
				this.form.get('maxLength')?.setValidators([Validators.required, Validators.pattern('^-?([0-9]+)$')]);
				this.form.get('minLength')?.setValidators([Validators.required, Validators.pattern('^-?([0-9]+)$')]);
				break;

			case this.conceptTypeEnum.Numeric:
				this.form.get('minValue')?.setValidators([Validators.required, Validators.pattern('^-?([0-9]+)$')]);
				this.form.get('maxValue')?.setValidators([Validators.required, Validators.pattern('^-?([0-9]+)$')]);
				this.form.get('nbDecimal')?.setValidators([Validators.required, Validators.pattern('^-?([0-9]+)$')]);
				break;

			case this.conceptTypeEnum.CodeList:
				this.form.get('codeListEntryValueType')?.setValidators([Validators.required]);
				this.form.get('codelistEntryValueMaxLength')?.setValidators([Validators.required, Validators.pattern('^-?([0-9]+)$')]);
				break;
		}

		this.form.updateValueAndValidity();
	}

	private removeErrors(): void {
		this.form.get('minValue')?.setErrors(null);
		this.form.get('maxValue')?.setErrors(null);
		this.form.get('nbDecimal')?.setErrors(null);
		this.form.get('codeListEntryValueType')?.setErrors(null);
		this.form.get('codelistEntryValueMaxLength')?.setErrors(null);
		this.form.get('maxLength')?.setErrors(null);
		this.form.get('minLength')?.setErrors(null);
	}

	private clearValidators(): void {
		this.form.get('minValue')?.clearValidators();
		this.form.get('maxValue')?.clearValidators();
		this.form.get('nbDecimal')?.clearValidators();
		this.form.get('codeListEntryValueType')?.clearValidators();
		this.form.get('codelistEntryValueMaxLength')?.clearValidators();
		this.form.get('maxLength')?.clearValidators();
		this.form.get('minLength')?.clearValidators();
	}

	private mapValue(value: any): any {
		if (value) {
			return typeof value === 'string' ? value : value[this.currentLanguage];
		}
		return '';
	}

	private filterAgents(term: string): IAgent[] {
		return this.allAgents.filter((option: IAgent) => this.getName(option).toLowerCase().includes(term.toLowerCase()));
	}

	private getName(option: IAgent): string {
		return this.fallback.transform(option.name, this.currentLanguage) ?? '';
	}
}
