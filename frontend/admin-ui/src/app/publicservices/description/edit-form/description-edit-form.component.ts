import {
	AfterViewInit,
	ChangeDetectionStrategy,
	Component,
	EventEmitter,
	inject,
	Input,
	OnChanges,
	OnDestroy,
	Output,
	SimpleChanges,
	ViewChild
} from '@angular/core';
import {AbstractControl, FormControl, UntypedFormGroup} from '@angular/forms';
import {
	IActiveDirectoryUser,
	IAgent,
	IPerson,
	IVocabularyEntry,
	PublicationLevel,
	PublicationLevelInfoModel,
	PublicServiceModel,
	VocabularyClient,
	VocabularyEntry
} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Observable, Subject} from 'rxjs';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {IDialogConfig} from 'src/app/shared/dialog/dialog.component';
import {map, startWith, takeUntil} from 'rxjs/operators';
import {MatAccordion} from '@angular/material/expansion';
import {ModalDialogComponent} from 'src/app/shared/modal-dialog/modal-dialog.component';
import {FallbackPipe} from 'src/app/shared/fallback/fallback.pipe';
import {Languages} from 'src/app/shared/ApplicationLanguage.enum';
import {VocabularyConfigService} from 'src/app/services/vocabulary-config.service';

@Component({
	selector: 'app-description-edit-form',
	templateUrl: './description-edit-form.component.html',
	styleUrls: ['./description-edit-form.component.scss'],
	changeDetection: ChangeDetectionStrategy.OnPush,
	standalone: false
})
export class DescriptionEditFormComponent implements AfterViewInit, OnDestroy, OnChanges {
	@Output() cancel: EventEmitter<void> = new EventEmitter();
	@Output() saveAndClose: EventEmitter<void> = new EventEmitter();
	@Output() save: EventEmitter<void> = new EventEmitter();
	@Output() formChanges: EventEmitter<SimpleChanges> = new EventEmitter();
	@Input() dto: PublicServiceModel = new PublicServiceModel();
	@Input() form!: UntypedFormGroup;
	@Input() cancelDialogConfig!: IDialogConfig;
	@Input() isEditMode!: boolean;
	@Input() publicationLevelInfo: PublicationLevelInfoModel | undefined;
	@Input()
	public set organisations(input: IAgent[]) {
		this._organisations.splice(0, this._organisations.length, ...input);
	}
	@Input() public set spatialCHCodes(input: VocabularyEntry[]) {
		this.allSpatialCHCodes.splice(0, this.allSpatialCHCodes.length, ...input);
	}

	@ViewChild(MatAccordion) accordion!: MatAccordion;
	@ViewChild(ModalDialogComponent) modalDialog!: ModalDialogComponent;

	showAllLanguages: boolean;
	filteredOrganisations$!: Observable<IAgent[]>;
	filteredSpatialCHCodes$!: Observable<IVocabularyEntry[]>;
	spatialQueryControl = new FormControl<string | any>('');

	currentLanguage: string;
	sectors$: Observable<VocabularyEntry[]>;
	formatSectors$: Observable<VocabularyEntry[]>;
	businessEventsCodes$: Observable<VocabularyEntry[]>;
	formatBusinessEventsCodes$: Observable<VocabularyEntry[]>;
	lifeEventsCodes$: Observable<VocabularyEntry[]>;
	formatLifeEventsCodes$: Observable<VocabularyEntry[]>;
	thematicAreas$: Observable<VocabularyEntry[]>;
	formatThematicAreas$: Observable<VocabularyEntry[]>;
	languages$: Observable<VocabularyEntry[]>;
	formatLanguages$: Observable<VocabularyEntry[]>;
	contentLanguages: readonly string[] = Languages.ContentLanguagesRm;
	themesConceptPageIri: string | undefined = undefined;
	businessEventsConceptPageIri: string | undefined = undefined;
	lifeEventsConceptPageIri: string | undefined = undefined;
	readonly publicationLevelEnum = PublicationLevel;

	private readonly businessEvents = 'VOCAB_BK_BUSINESSEVENTS';
	private readonly lifeEvents = 'VOCAB_BK_LIFEEVENTS';
	private readonly datasetTheme = 'Concept_DATASET_THEME';
	private readonly languageCodes = 'Languages_Iso_639';
	private readonly allSpatialCHCodes: VocabularyEntry[] = [];
	private readonly editingChildren: string[] = [];
	private readonly _organisations: IAgent[] = [];
	private readonly unsubscribe$ = new Subject();
	private hasUpdatedControls: boolean = false;
	private readonly INVALID = 'INVALID';

	private readonly fallback = inject(FallbackPipe);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyClient = inject(VocabularyClient);
	private readonly vocabularyConfigService = inject(VocabularyConfigService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.languages$ = this.vocabularyClient.getByIdentifier(this.languageCodes).pipe(map(response => response.result));
		this.formatLanguages$ = this.languages$;
		this.sectors$ = this.vocabularyClient.getByIdentifier(this.datasetTheme).pipe(map(response => response.result));
		this.formatSectors$ = this.sectors$;
		this.businessEventsCodes$ = this.vocabularyClient.getByIdentifier(this.businessEvents).pipe(map(response => response.result));
		this.lifeEventsCodes$ = this.vocabularyClient.getByIdentifier(this.lifeEvents).pipe(map(response => response.result));
		this.formatBusinessEventsCodes$ = this.businessEventsCodes$;
		this.formatLifeEventsCodes$ = this.lifeEventsCodes$;
		this.thematicAreas$ = this.vocabularyClient.getByIdentifier(this.datasetTheme).pipe(map(response => response.result));
		this.formatThematicAreas$ = this.thematicAreas$;
		this.showAllLanguages = true;
		this.formatThemes();
		this.formatBusinessEvents();
		this.formatLifeEvents();
		this.formatThematicAreas();
		this.formatLanguages();
	}

	ngAfterViewInit(): void {
		this.setPublisher();
		this.setSpatialCHCodes();

		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
			this.showAllLanguages = false;
			this.formatThemes();
			this.formatThematicAreas();
			this.formatLanguages();
		});

		this.vocabularyConfigService
			.resolveConceptPageIris([this.datasetTheme, this.businessEvents, this.lifeEvents])
			.pipe(takeUntil(this.unsubscribe$))
			.subscribe(iris => {
				this.themesConceptPageIri         = iris[this.datasetTheme];
				this.businessEventsConceptPageIri = iris[this.businessEvents];
				this.lifeEventsConceptPageIri     = iris[this.lifeEvents];
			});
	}

	ngOnChanges(changes: SimpleChanges): void {
		this.formChanges.emit(changes);
		if (this.dto.id && this._organisations.length > 0) {
			setTimeout(() => {
				this.checkIfFormChangesIsInvalid();
			}, 100);
		}
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	canSave(): boolean {
		return this.editingChildren.length === 0;
	}

	isIdentifierEditionDisabled(): boolean {
		return this.publicationLevelInfo?.level === this.publicationLevelEnum.Public;
	}

	onBeforeOpenDialog(): void {
		this.cancelDialogConfig.enableSave = this.canSave();

		this.modalDialog.openDialog();
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
	}

	displayVocabularyEntry = (format: VocabularyEntry) => {
		return this.fallback.transform(format?.name, this.currentLanguage) ?? '';
	};

	onCancel(): void {
		this.cancel.emit();
	}

	onSave(): void {
		if (this.isFormValid) {
			this.save.emit();
		}
	}

	onSaveAndClose(): void {
		if (this.isFormValid) {
			this.saveAndClose.emit();
		}
	}

	displayValue = (organisation: IAgent): string => {
		return this.fallback.transform(organisation?.name, this.currentLanguage) ?? '';
	};

	displayPersonName = (user: IPerson & IActiveDirectoryUser): string => {
		if (user) {
			return (user?.displayName ? user?.displayName : (user?.name as string)) ?? user.email;
		}
		return '';
	};

	compareType(prev: any, next: any): boolean {
		return prev.code === next.code;
	}

	private filterOrganisations(term: string): IAgent[] {
		return this._organisations.filter((option: IAgent) => this.getName(option).toLowerCase().includes(term.toLowerCase()));
	}

	private get isFormValid(): boolean {
		this.form.markAllAsTouched();
		this.form.updateValueAndValidity();
		return this.form.valid;
	}

	private formatLanguages(): void {
		this.formatLanguages$ = this.languages$.pipe(map(x => this.sortLanguageEntries(x)));
	}

	private formatThemes(): void {
		this.formatSectors$ = this.sectors$.pipe(map(x => this.sortVocabularyEntries(x)));
	}

	private formatBusinessEvents(): void {
		this.formatBusinessEventsCodes$ = this.businessEventsCodes$.pipe(map(x => this.sortVocabularyEntries(x)));
	}

	private formatLifeEvents(): void {
		this.formatLifeEventsCodes$ = this.lifeEventsCodes$.pipe(map(x => this.sortVocabularyEntries(x)));
	}

	private formatThematicAreas(): void {
		this.formatThematicAreas$ = this.thematicAreas$.pipe(map(x => this.sortVocabularyEntries(x)));
	}

	private sortVocabularyEntries(x: VocabularyEntry[]): VocabularyEntry[] {
		return [...x].sort((a, b) => {
			const aName = this.fallback.transform(a.name, this.currentLanguage);
			const bName = this.fallback.transform(b.name, this.currentLanguage);
			return aName!.localeCompare(bName!);
		});
	}

	private sortLanguageEntries(x: VocabularyEntry[]): VocabularyEntry[] {
		let prefLangs: VocabularyEntry[] = [];
		let otherLangs: VocabularyEntry[] = [];

		x.forEach(e => {
			if (Languages.ContentLanguagesRm.find(l => l === e.code)) {
				prefLangs.push(e);
			} else {
				otherLangs.push(e);
			}
		});

		return [...this.sortVocabularyEntries(prefLangs), ...this.sortVocabularyEntries(otherLangs)];
	}

	private checkIfFormChangesIsInvalid(): void {
		if (this.form.status === this.INVALID && !this.hasUpdatedControls) {
			this.hasUpdatedControls = true;
			Object.keys(this.form.controls).forEach(controlName => {
				const control = this.form.get(controlName) as AbstractControl;
				if (control && control.status === this.INVALID) {
					control.updateValueAndValidity();
					control.markAsTouched();
				}
			});
		}
	}

	private setPublisher(): void {
		let publisher = this.form.get('publisher');
		if (publisher) {
			this.filteredOrganisations$ = publisher.valueChanges.pipe(
				startWith(''),
				map(value => (typeof value === 'string' ? value : value?.name[this.currentLanguage])),
				map(term => (term ? this.filterOrganisations(term) : this._organisations))
			);
		}
	}

	private mapValue(value: any): any {
		if (value) {
			return typeof value === 'string' ? value : value.name[this.currentLanguage];
		}
		return '';
	}

	private getName(option: VocabularyEntry | IAgent): string {
		return this.fallback.transform(option.name, this.currentLanguage) ?? '';
	}

	private filterSpatialCHCodes(term: string): VocabularyEntry[] {
		const filterTerm = term.toLowerCase();

		const filteredAllSpatialCHCodes = this.allSpatialCHCodes.filter((option: VocabularyEntry) => this.getName(option).toLowerCase().includes(filterTerm));

		return [...this.form.get('spatialCH')?.value, ...filteredAllSpatialCHCodes];
	}

	private setSpatialCHCodes(): void {
		this.filteredSpatialCHCodes$ = this.spatialQueryControl.valueChanges.pipe(
			startWith(''),
			map(value => this.mapValue(value)),
			map(term => (term ? this.filterSpatialCHCodes(term) : this.allSpatialCHCodes))
		);
	}
}
