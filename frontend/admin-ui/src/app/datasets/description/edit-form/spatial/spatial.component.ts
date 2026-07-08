import {Component, Input, OnInit} from '@angular/core';
import {UntypedFormArray, UntypedFormControl, UntypedFormGroup} from '@angular/forms';
import {DcatDatasetModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Component({
	selector: 'app-spatial',
	templateUrl: './spatial.component.html',
	styleUrls: ['./spatial.component.scss'],
	standalone: false
})
export class SpatialComponent implements OnInit {
	@Input() form!: UntypedFormGroup;
	@Input() dto: DcatDatasetModel = new DcatDatasetModel();
	public spatialForm: UntypedFormGroup;

	constructor() {
		this.spatialForm = new UntypedFormGroup({
			spatials: new UntypedFormArray([])
		});
	}

	ngOnInit() {
		if (!!this.dto?.spatial?.length) {
			this.initControls();
		} else {
			this.addControl();
		}
		if (this.form.dirty || this.form.pristine) {
			this.form.setControl('spatial', this.spatialForm);
		}
	}

	items(): UntypedFormArray {
		return this.spatialForm.get('spatials') as UntypedFormArray;
	}

	newItem(item?: string): UntypedFormGroup {
		return new UntypedFormGroup({
			spatial: new UntypedFormControl(item ? item : ''),
		});
	}

	initControls() {
		this.dto.spatial?.forEach((item: string) => {
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
