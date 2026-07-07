import {Component, ElementRef, EventEmitter, inject, Input, OnChanges, OnDestroy, OnInit, Output, QueryList, SimpleChanges, ViewChildren} from '@angular/core';
import {CatalogClient, CatalogEntry, ConceptReferenceModel, MultiLanguage, SearchResourceType} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {UntypedFormControl, UntypedFormGroup} from '@angular/forms';
import {MatTableDataSource} from '@angular/material/table';
import {catchError, map, of, Subject, switchMap, takeUntil} from 'rxjs';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {FallbackPipe} from '../fallback/fallback.pipe';
import {buildConceptIri, extractIriVersion} from '../iri-helpers';

@Component({
	selector: 'app-edit-concept-references',
	templateUrl: './edit-concept-references.component.html',
	styleUrls: ['./edit-concept-references.component.scss'],
	standalone: false
})
export class EditConceptReferencesComponent implements OnInit, OnChanges, OnDestroy {
	@ViewChildren('focusInputField') focusInputFields!: QueryList<ElementRef>;
	@Input() parentForm!: UntypedFormGroup;
	@Input() controlName!: string;
	@Input() dto: any;
	@Output() edit: EventEmitter<boolean> = new EventEmitter();

	public dataSource = new MatTableDataSource<ConceptReferenceModel>([]);

	searchControl = new UntypedFormControl('');
	autoCompleteItems: CatalogEntry[] = [];
	currentLanguage: string;
	loading = false;

	COLUMN_CONCEPT = 'concept';
	COLUMN_ACTIONS = 'actions';
	displayedColumns: string[] = [this.COLUMN_CONCEPT, this.COLUMN_ACTIONS];

	private isEditing = false;
	private isAddMode = false;
	private rowIndex: number | undefined;
	private readonly unsubscribe$ = new Subject<void>();
	private readonly searchTerms$ = new Subject<string>();
	private readonly catalogClient = inject(CatalogClient);
	private readonly fallback = inject(FallbackPipe);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((e: LangChangeEvent) => {
			this.currentLanguage = e.lang;
		});

		this.searchTerms$
			.pipe(
				map(q => (typeof q === 'string' ? q.trim() : '')),
				switchMap(q => {
					if (!q) {
						this.loading = false;
						return of<CatalogEntry[]>([]);
					}
					this.loading = true;
					return this.catalogClient
						.getSearchByQueryAndAccessRightsAndConceptValueTypesAndFormatsAndBusinessEventsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypesAndPageAndPageSize(
							q,
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
							1,
							20
						)
						.pipe(
							map(res => res.result ?? []),
							catchError(() => of<CatalogEntry[]>([]))
						);
				}),
				takeUntil(this.unsubscribe$)
			)
			.subscribe(items => {
				this.autoCompleteItems = items;
				this.loading = false;
			});
	}

	ngOnChanges(changes: SimpleChanges): void {
		const change = changes.dto;
		if (change && change.currentValue !== undefined) {
			this.dataSource = new MatTableDataSource<ConceptReferenceModel>([...(this.dto ?? [])]);
		}
	}

	ngOnDestroy(): void {
		this.unsubscribe$.next();
		this.unsubscribe$.complete();
	}

	onAddRow(): void {
		const numRows = this.dataSource.data.push(new ConceptReferenceModel({uri: '', name: new MultiLanguage(), conceptId: undefined}));
		this.isAddMode = true;
		this.setEditing(true);
		this.rowIndex = numRows - 1;
		this.searchControl.setValue('');
		this.autoCompleteItems = [];
		this.refreshDatabinding();
		this.setFocus();
	}

	onSearch(query: string): void {
		this.searchTerms$.next(query);
	}

	onSelect(entry: CatalogEntry): void {
		this.autoCompleteItems = [];
		if (!entry.id || this.dataSource.data.some((x, i) => i !== this.rowIndex && x.conceptId === entry.id)) {
			// already listed (or unusable) – leave the in-progress row empty so it can't be saved
			this.searchControl.setValue('');
			return;
		}
		const identifier = entry.identifiers?.[0];
		const version = entry.version;
		const uri = identifier && version ? buildConceptIri(identifier, version) : entry.id!;
		// Mutate the existing row in place so the already-rendered mat-row reflects the pick.
		const row = this.dataSource.data[this.rowIndex!];
		row.uri = uri;
		row.name = entry.title ?? new MultiLanguage();
		row.conceptId = entry.id;
		this.searchControl.setValue(this.fallback.transform(entry.title, this.currentLanguage) ?? '');
	}

	onSave(): void {
		this.isAddMode = false;
		this.setEditing(false);
		this.updateFormControl();
		this.parentForm.markAsDirty();
	}

	onDiscard(): void {
		if (this.isAddMode) {
			this.dataSource.data.splice(this.rowIndex!, 1);
			this.isAddMode = false;
			this.refreshDatabinding();
		}
		this.searchControl.setValue('');
		this.autoCompleteItems = [];
		this.setEditing(false);
	}

	onRemoveRow(index: number): void {
		this.dataSource.data.splice(index, 1);
		this.refreshDatabinding();
		this.updateFormControl();
		this.parentForm.markAsDirty();
	}

	displayName(item: ConceptReferenceModel): string {
		const text = this.fallback.transform(item.name, this.currentLanguage);
		if (!text) {
			return item.uri ?? '';
		}
		const version = extractIriVersion(item.uri ?? '');
		return version ? `${text} (${version})` : text;
	}

	displayFn = (entry: CatalogEntry | string): string => {
		if (typeof entry === 'string') return entry;
		return this.fallback.transform(entry?.title, this.currentLanguage) ?? '';
	};

	canAdd(): boolean {
		return !this.isEditing;
	}

	canRemove(): boolean {
		return !this.isEditing;
	}

	canSave(index: number): boolean {
		return this.isRowEditMode(index) && !!this.dataSource.data[index]?.conceptId;
	}

	isRowEditMode(index: number): boolean {
		return this.isEditing && this.rowIndex === index;
	}

	private setEditing(value: boolean): void {
		this.isEditing = value;
		this.edit.emit(value);
	}

	private updateFormControl(): void {
		this.parentForm.get(this.controlName)?.setValue([...this.dataSource.data]);
	}

	private refreshDatabinding(): void {
		this.dataSource.filter = '';
	}

	private setFocus(): void {
		setTimeout(() => {
			this.focusInputFields.get(this.rowIndex!)?.nativeElement.focus();
		});
	}
}
