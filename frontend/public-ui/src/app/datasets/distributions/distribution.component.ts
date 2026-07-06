import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {Subject} from 'rxjs';
import {combineLatestWith, takeUntil} from 'rxjs/operators';
import {ActivatedRoute} from '@angular/router';
import {DatasetsClient, DcatDistributionModel, DataServiceModel, Dataset} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {DcatDatasetService} from '../services/dcat-dataset.service';

@Component({
	selector: 'app-distribution',
	templateUrl: './distribution.component.html',
	styleUrls: [],
	standalone: false
})
export class DistributionComponent implements OnInit, OnDestroy {
	distribution: DcatDistributionModel;
	dataServiceLinks: DataServiceModel[];
	dataset: Dataset;
	datasetId: string | undefined;

	private readonly unsubscribe$ = new Subject();

	private readonly datasetsClient = inject(DatasetsClient);
	private readonly dcatDatasetService = inject(DcatDatasetService);
	private readonly route = inject(ActivatedRoute);

	ngOnInit() {
		this.dcatDatasetService.dataset$.pipe(takeUntil(this.unsubscribe$), combineLatestWith(this.route.params)).subscribe(([res, params]) => {
			this.dataset = res;
			this.datasetId = res.id;
			this.setDistribution(res.distributions, params.distributionId);
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}

	private setDistribution(distributions: DcatDistributionModel[], distributionId: string) {
		if (distributionId && distributions) {
			const f = distributions.filter(x => x.id === distributionId);
			this.distribution = f.length > 0 ? f[0] : null;
			// access services
			if (this.distribution) {
				this.loadAccessServices(distributionId);
			}
		}
	}

	private loadAccessServices(distributionId: string) {
		if (this.datasetId) {
			this.datasetsClient.getDistributionsAccessServicesByDatasetIdAndDistributionId(this.datasetId, distributionId).subscribe(response => {
				this.dataServiceLinks = response.result;
			});
		}
	}
}
