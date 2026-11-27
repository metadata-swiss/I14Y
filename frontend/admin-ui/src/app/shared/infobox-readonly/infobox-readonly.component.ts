import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {Subject, takeUntil} from 'rxjs';
import {AllowActionService} from 'src/app/services/allow.action.service';

@Component({
	selector: 'app-infobox-readonly',
	templateUrl: './infobox-readonly.component.html',
	standalone: false
})
export class InfoboxReadOnlyComponent implements OnInit, OnDestroy {
	readonly: boolean = false;

	private readonly unsubscribe$ = new Subject();

	private readonly allowActionService = inject(AllowActionService);

	ngOnInit(): void {
		this.allowActionService.readonly$.pipe(takeUntil(this.unsubscribe$)).subscribe(readonly => {
			this.readonly = readonly;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}
}
