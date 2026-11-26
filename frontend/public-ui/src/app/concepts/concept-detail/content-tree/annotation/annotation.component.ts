import {Component, inject, Input, OnDestroy, OnInit} from '@angular/core';
import {Annotation} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService, LangChangeEvent} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';

@Component({
	selector: 'app-annotation',
	templateUrl: './annotation.component.html',
	styleUrls: ['./annotation.component.scss'],
	standalone: false
})
export class AnnotationComponent implements OnInit, OnDestroy {
	@Input() annotation: Annotation | undefined;
	currentLanguage: string;

	private readonly unsubscribe$ = new Subject();

	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}
}
