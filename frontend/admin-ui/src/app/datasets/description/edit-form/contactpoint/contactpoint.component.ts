import {Component, Input, OnInit} from '@angular/core';
import {UntypedFormArray, UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {DcatDatasetModel, IVCardModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {Languages} from 'src/app/shared/ApplicationLanguage.enum';

@Component({
	selector: 'app-contactpoint',
	templateUrl: './contactpoint.component.html',
	styleUrls: ['./contactpoint.component.scss'],
	standalone: false
})
export class ContactpointComponent implements OnInit {
	@Input() form!: UntypedFormGroup;
	@Input() dto: DcatDatasetModel = new DcatDatasetModel();
	@Input() contactPointTitle = '';
	public contactPointForm: UntypedFormGroup;

	contentLanguages: readonly string[] = Languages.ContentLanguagesRm;

	constructor() {
		this.contactPointForm = new UntypedFormGroup({
			contactPoint: new UntypedFormArray([])
		});
	}

	ngOnInit() {
		if (!!this.dto?.contactPoints?.length) {
			this.initControls();
		} else {
			this.addControl();
		}
		if (this.form.dirty || this.form.pristine) {
			this.form.setControl('contactPoint', this.contactPointForm);
		}
	}

	items(): UntypedFormArray {
		return this.contactPointForm.get('contactPoint') as UntypedFormArray;
	}

	newItem(item?: IVCardModel): UntypedFormGroup {
		return new UntypedFormGroup({
			hasEmail: new UntypedFormControl(item?.hasEmail ? item?.hasEmail : '', [Validators.required, Validators.email]),
			hasTelephone: new UntypedFormControl(item?.hasTelephone ? item?.hasTelephone : ''),
			fn: new UntypedFormGroup({
				de: new UntypedFormControl(item?.fn?.de),
				en: new UntypedFormControl(item?.fn?.en),
				it: new UntypedFormControl(item?.fn?.it),
				fr: new UntypedFormControl(item?.fn?.fr),
				rm: new UntypedFormControl(item?.fn?.rm)
			}),
			note: new UntypedFormGroup({
				de: new UntypedFormControl(item?.note?.de),
				en: new UntypedFormControl(item?.note?.en),
				it: new UntypedFormControl(item?.note?.it),
				fr: new UntypedFormControl(item?.note?.fr),
				rm: new UntypedFormControl(item?.note?.rm)
			}),
			hasAddress: new UntypedFormGroup({
				de: new UntypedFormControl(item?.hasAddress?.de),
				en: new UntypedFormControl(item?.hasAddress?.en),
				it: new UntypedFormControl(item?.hasAddress?.it),
				fr: new UntypedFormControl(item?.hasAddress?.fr),
				rm: new UntypedFormControl(item?.hasAddress?.rm)
			})
		});
	}

	initControls() {
		this.dto.contactPoints?.forEach((item: IVCardModel) => {
			this.items().push(this.newItem(item));
		});
	}

	addControl() {
		this.items().push(this.newItem());
	}

	removeControl(i: number) {
		this.items().removeAt(i);
	}
}
