import {Component, Input, OnInit} from '@angular/core';
import {UntypedFormArray, UntypedFormControl, UntypedFormGroup} from '@angular/forms';
import {DcatDatasetModel, PeriodOfTimeModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Component({
	selector: 'app-temporal-coverage',
	templateUrl: './temporal-coverage.component.html',
	styleUrls: ['./temporal-coverage.component.scss'],
	standalone: false
})
export class TemporalCoverageComponent implements OnInit {
	@Input() form!: UntypedFormGroup;
	@Input() dto: DcatDatasetModel = new DcatDatasetModel();
	public temporalCoverageForm: UntypedFormGroup;

	constructor() {
		this.temporalCoverageForm = new UntypedFormGroup({
			temporalCoverages: new UntypedFormArray([])
		});
	}

	ngOnInit() {
		if (!!this.dto?.temporalCoverage?.length) {
			this.initControls();
		} else {
			this.addControl();
		}
		if (this.form.dirty || this.form.pristine) {
			this.form.setControl('temporalCoverage', this.temporalCoverageForm);
		}
	}

	items(): UntypedFormArray {
		return this.temporalCoverageForm.get('temporalCoverages') as UntypedFormArray;
	}

	newItem(item?: PeriodOfTimeModel): UntypedFormGroup {
		return new UntypedFormGroup({
			coverageFrom: new UntypedFormControl(item?.start ? item?.start : ''),
			coverageTo: new UntypedFormControl(item?.end ? item?.end : ''),
		});
	}

	initControls() {
		this.dto.temporalCoverage?.forEach((item: PeriodOfTimeModel) => {
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
