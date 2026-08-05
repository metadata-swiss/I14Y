import {Component, inject, Inject, OnInit} from '@angular/core';
import {UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {MatDialogRef, MAT_DIALOG_DATA, MatDialog} from '@angular/material/dialog';
import {MultiLanguage, VocabularyClient, VocabularyEntry} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService} from '@ngx-translate/core';
import {map, Observable} from 'rxjs';
import {DIALOG_CANCEL_BUTTON_KEY, DIALOG_CONFIRM_BUTTON_KEY, IDENTIFIER_PATTERN, URI_PATTERN} from 'src/app/app-constants';
import {Languages} from 'src/app/shared/ApplicationLanguage.enum';
import {DialogComponent, DialogType} from 'src/app/shared/dialog/dialog.component';
import {FormatFunctions} from 'src/app/shared/format-functions';
import {MultiLanguageMapper} from 'src/app/shared/mappers/multilanguagemapper';
import {ChannelIdentifierValidator} from 'src/app/shared/validators/channel-identifier.validator';
import {createMultilangValidator} from 'src/app/shared/validators/multilang.validator';
import {createWhitespaceValidator} from 'src/app/shared/validators/whitespace.validator';

@Component({
	selector: 'app-modal-dialog-channel',
	templateUrl: './modal-dialog.component.html',
	standalone: false
})
export class ModalDialogChannelComponent implements OnInit {
	form!: UntypedFormGroup;
	title: string = '';
	channelTypes$: Observable<VocabularyEntry[]>;
	selectedChannelType: VocabularyEntry | undefined;
	changeChannelTypeConfirmed: boolean = false;
	currentLanguage: string;

	get showOpeningHour(): boolean {
		return (
			this.selectedChannelType?.code === this.channelTypeCodes.post ||
			this.selectedChannelType?.code === this.channelTypeCodes.mobile ||
			this.selectedChannelType?.code === this.channelTypeCodes.phone
		);
	}

	get isSaveDisabled(): boolean {
		return !this.form.dirty;
	}

	readonly channelTypeCodes = {
		post: '0c84394663',
		email: '1fc1caefa8',
		mobile: '5c12931e3f',
		fax: 'a1c5444664',
		web: 'b37115f83e',
		phone: 'c05134f579'
	};

	private readonly channelTypesIdentifier = 'EU_Channel_Types';
	private readonly contentLanguages: readonly string[] = Languages.ContentLanguages;
	private readonly editingChildren: string[] = [];

	private readonly channelIdentifierValidator = inject(ChannelIdentifierValidator);
	private readonly dialog = inject(MatDialog);
	private readonly dialogRef = inject(MatDialogRef<ModalDialogChannelComponent>);
	private readonly translate = inject(TranslateService);
	private readonly vocabularyClient = inject(VocabularyClient);

	constructor(@Inject(MAT_DIALOG_DATA) public data: any) {
		this.currentLanguage = this.translate.getCurrentLang();
		this.channelTypes$ = this.vocabularyClient.getByIdentifier(this.channelTypesIdentifier).pipe(map(response => response.result));
	}

	ngOnInit(): void {
		this.selectedChannelType = this.data.channel.type;
		this.form = this.createEmptyForm();
		this.updateTypeSpecificValidators(this.selectedChannelType);

		if (this.data.isEditMode) {
			this.channelIdentifierValidator.setInitialValue(this.data.channel?.identifier);
			this.mapDataToForm();
		} else {
			this.channelIdentifierValidator.setInitialValue(undefined);
		}

		this.updateTitle();
	}

	onConceptTypeSelected(type: VocabularyEntry, event: any) {
		if (event.isUserInput) {
			this.selectedChannelType = type;
		}
	}

	onChannelTypeSelected(type: VocabularyEntry, event: any) {
		if (event.isUserInput) {
			if (this.data.isEditMode && !this.changeChannelTypeConfirmed) {
				const headertextKey = 'i18n.dialog.change_channel_type.header_text';
				const bodytextKey = 'i18n.dialog.change_channel_type.body_text';

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
						this.form.get('type')?.setValue(this.selectedChannelType);
						this.form.get('type')?.markAsPristine();
					});
					const dialogConfirm = dialogRef.componentInstance.confirm.subscribe(() => {
						this.changeChannelType(type);
						this.changeChannelTypeConfirmed = true;
					});
					dialogRef.afterClosed().subscribe(() => {
						dialogCancel.unsubscribe();
						dialogConfirm.unsubscribe();
					});
				});
			} else {
				this.changeChannelType(type);
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
	}

	save() {
		if (this.isFormValid) {
			this.mapFormToData();
			this.dialogRef.close(this.data);
		}
	}

	compareType(prev: any, next: any): boolean {
		return prev.code === next.code;
	}

	canSave(): boolean {
		return this.editingChildren.length === 0;
	}

	getPatternErrorMassage(value: string): Observable<string> {
		return this.translate
			.get('i18n.error.pattern', {
				invalidChars: FormatFunctions.convertArrayToString(this.getInvalidCharaters(value), ', ')
			})
			.pipe(map(x => x as string));
	}

	private getInvalidCharaters(value: string) {
		const pattern = new RegExp(IDENTIFIER_PATTERN);
		let invalidCharacters: string[] = [];

		value.split('').forEach(char => {
			if (!pattern.test(char)) {
				invalidCharacters.push(char);
			}
		});
		return invalidCharacters;
	}

	private get isFormValid(): boolean {
		this.form.markAllAsTouched();
		this.form.updateValueAndValidity();
		return this.form.valid;
	}

	private getObjectFromKeys<Type>(keys: readonly string[], initialValue: (key: string) => Type) {
		return Object.assign({}, ...keys.map(x => ({[x]: initialValue(x)})));
	}

	private updateTitle(): void {
		const titleKey = this.data.isEditMode ? 'i18n.title.edit_channel' : 'i18n.title.create_channel';
		this.translate.get(titleKey, {identifier: this.data.channel.identifier}).subscribe(result => {
			this.title = result;
		});
	}

	private changeChannelType(type: VocabularyEntry) {
		this.selectedChannelType = type;
		this.resetTypeSpecificFields();
		this.markTypeSpecificFieldsAsUntouched();
		this.updateTypeSpecificValidators(this.selectedChannelType);
	}

	private resetTypeSpecificFields() {
		this.form.get('address.de')?.setValue('');
		this.form.get('address.fr')?.setValue('');
		this.form.get('address.it')?.setValue('');
		this.form.get('address.en')?.setValue('');
		this.form.get('email')?.setValue('');
		this.form.get('mobile')?.setValue('');
		this.form.get('fax')?.setValue('');
		this.form.get('url')?.setValue('');
		this.form.get('phone')?.setValue('');
		this.form.get('openingHours')?.setValue('');
	}

	private markTypeSpecificFieldsAsUntouched() {
		this.form.get('address.de')?.markAsUntouched();
		this.form.get('address.fr')?.markAsUntouched();
		this.form.get('address.it')?.markAsUntouched();
		this.form.get('address.en')?.markAsUntouched();
		this.form.get('email')?.markAsUntouched();
		this.form.get('mobile')?.markAsUntouched();
		this.form.get('fax')?.markAsUntouched();
		this.form.get('url')?.markAsUntouched();
		this.form.get('phone')?.markAsUntouched();
		this.form.get('openingHours')?.markAsUntouched();
	}

	private updateTypeSpecificValidators(type: VocabularyEntry | undefined) {
		this.removeErrors();
		this.clearValidators();

		switch (type?.code) {
			case this.channelTypeCodes.email:
				this.form.get('email')?.setValidators([Validators.required, Validators.email]);
				break;
			case this.channelTypeCodes.fax:
				this.form.get('fax')?.setValidators([Validators.required]);
				break;
			case this.channelTypeCodes.mobile:
				this.form.get('mobile')?.setValidators([Validators.required]);
				break;
			case this.channelTypeCodes.phone:
				this.form.get('phone')?.setValidators([Validators.required]);
				break;
			case this.channelTypeCodes.post:
				this.form.get('address')?.setValidators([createMultilangValidator([...this.contentLanguages])]);
				break;
			case this.channelTypeCodes.web:
				this.form.get('url')?.setValidators([Validators.required, Validators.pattern(URI_PATTERN)]);
				break;
			default:
				break;
		}
	}

	private removeErrors() {
		this.form.get('address.de')?.setErrors(null);
		this.form.get('address.fr')?.setErrors(null);
		this.form.get('address.it')?.setErrors(null);
		this.form.get('address.en')?.setErrors(null);
		this.form.get('email')?.setErrors(null);
		this.form.get('mobile')?.setErrors(null);
		this.form.get('fax')?.setErrors(null);
		this.form.get('url')?.setErrors(null);
		this.form.get('phone')?.setErrors(null);
	}

	private clearValidators() {
		this.form.get('address')?.clearValidators();
		this.form.get('email')?.clearValidators();
		this.form.get('mobile')?.clearValidators();
		this.form.get('fax')?.clearValidators();
		this.form.get('url')?.clearValidators();
		this.form.get('phone')?.clearValidators();
	}

	private createEmptyForm() {
		return new UntypedFormGroup({
			identifier: new UntypedFormControl('', {
				validators: [Validators.required, Validators.pattern(IDENTIFIER_PATTERN), createWhitespaceValidator()],
				asyncValidators: [this.channelIdentifierValidator.validate.bind(this.channelIdentifierValidator)],
				updateOn: 'blur'
			}),
			description: new UntypedFormGroup(this.getObjectFromKeys(this.data.contentLanguages, () => new UntypedFormControl(''))),
			type: new UntypedFormControl(''),
			openingHours: new UntypedFormControl(''),
			address: new UntypedFormGroup(this.getObjectFromKeys(this.data.contentLanguages, () => new UntypedFormControl(''))),
			email: new UntypedFormControl('', Validators.email),
			fax: new UntypedFormControl(''),
			mobile: new UntypedFormControl(''),
			phone: new UntypedFormControl(''),
			url: new UntypedFormControl('', Validators.pattern(URI_PATTERN)),
			ownedBy: new UntypedFormControl('')
		});
	}

	private mapDataToForm(): void {
		this.form.patchValue({
			identifier: this.data.channel?.identifier,
			type: this.data.channel?.type,
			description: new MultiLanguage(this.data.channel?.description),
			openingHours: this.data.channel?.openingHours,
			address: new MultiLanguage(this.data.channel?.address),
			email: this.data.channel?.email,
			fax: this.data.channel?.fax,
			mobile: this.data.channel?.mobile,
			phone: this.data.channel?.phone,
			url: this.data.channel?.url,
			ownedBy: this.data.channel?.ownedBy
		});
	}

	private mapFormToData(): void {
		this.data.channel.identifier = this.form.value.identifier || undefined;
		this.data.channel.type = this.form.value.type || undefined;
		this.data.channel.openingHours = this.form.value.openingHours || undefined;
		this.data.channel.email = this.form.value.email || undefined;
		this.data.channel.fax = this.form.value.fax || undefined;
		this.data.channel.mobile = this.form.value.mobile || undefined;
		this.data.channel.phone = this.form.value.phone || undefined;
		this.data.channel.url = this.form.value.url || undefined;
		this.data.channel.description = this.mapMultiLanguage(this.form.value.description);
		this.data.channel.address = this.mapMultiLanguage(this.form.value.address);
		this.data.channel.ownedBy = this.form.value.ownedBy;
	}

	private mapMultiLanguage(multiLanguage: MultiLanguage | undefined): MultiLanguage | undefined {
		if (multiLanguage?.de || multiLanguage?.en || multiLanguage?.fr || multiLanguage?.it || multiLanguage?.rm) {
			return MultiLanguageMapper.clone(multiLanguage);
		}
		return undefined;
	}
}
