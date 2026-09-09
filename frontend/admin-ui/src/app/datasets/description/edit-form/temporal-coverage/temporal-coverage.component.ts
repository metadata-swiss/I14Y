import {Component, Input, OnInit} from '@angular/core';
import {UntypedFormArray, UntypedFormControl, UntypedFormGroup} from '@angular/forms';
import {DcatDatasetModel, PeriodOfTimeModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {createCoverageValidator} from 'src/app/shared/validators/coverage.validator';

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

		this.form.setControl('temporalCoverage', this.temporalCoverageForm);
	}

	items(): UntypedFormArray {
		return this.temporalCoverageForm.get('temporalCoverages') as UntypedFormArray;
	}

	newItem(item?: PeriodOfTimeModel): UntypedFormGroup {
		const formGroup = new UntypedFormGroup(
			{
				coverageFrom: new UntypedFormControl(item?.start ? item?.start : ''),
				coverageTo: new UntypedFormControl(item?.end ? item?.end : '')
			},
			{validators: [createCoverageValidator()], updateOn: 'blur'}
		);

		formGroup.get('coverageFrom')?.valueChanges.subscribe(() => {
			formGroup.get('coverageTo')?.updateValueAndValidity({emitEvent: false});
		});

		formGroup.get('coverageTo')?.valueChanges.subscribe(() => {
			formGroup.get('coverageFrom')?.updateValueAndValidity({emitEvent: false});
		});
		
		return formGroup
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
