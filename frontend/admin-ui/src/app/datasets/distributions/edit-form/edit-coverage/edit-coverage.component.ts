import {Component, Input, OnInit} from '@angular/core';
import {UntypedFormArray, UntypedFormControl, UntypedFormGroup} from '@angular/forms';
import {DcatDistributionModel, PeriodOfTimeModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {createCoverageValidator} from 'src/app/shared/validators/coverage.validator';

@Component({
	selector: 'app-edit-coverage',
	templateUrl: './edit-coverage.component.html',
	styleUrls: ['./edit-coverage.component.scss'],
	standalone: false
})
export class EditCoverageComponent implements OnInit {
	@Input() form!: UntypedFormGroup;
	@Input() dto: DcatDistributionModel = new DcatDistributionModel();
	public coverageForm: UntypedFormGroup;

	constructor() {
		this.coverageForm = new UntypedFormGroup({
			coverages: new UntypedFormArray([])
		});
	}

	ngOnInit() {
		if (!!this.dto?.coverage?.length) {
			this.initControls();
		} else {
			this.addControl();
		}

		this.form.setControl('coverage', this.coverageForm);
	}

	items(): UntypedFormArray {
		return this.coverageForm.get('coverages') as UntypedFormArray;
	}

	newItem(item?: PeriodOfTimeModel): UntypedFormGroup {
		return new UntypedFormGroup(
			{
				coverageFrom: new UntypedFormControl(item?.start ? item?.start : ''),
				coverageTo: new UntypedFormControl(item?.end ? item?.end : '')
			},
			{validators: [createCoverageValidator()], updateOn: 'blur'}
		);
	}

	initControls() {
		this.dto.coverage?.forEach((item: PeriodOfTimeModel) => {
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
