import {Component, inject, Input, OnDestroy, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
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
	displayLanguage: string;

	private readonly unsubscribe$ = new Subject();

	private readonly route = inject(ActivatedRoute);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
		this.displayLanguage = this.route.snapshot.queryParamMap.get('lang') ?? this.currentLanguage;
	}

	ngOnInit(): void {
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLanguage = language.lang;
			this.displayLanguage = this.route.snapshot.queryParamMap.get('lang') ?? language.lang;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(1);
		this.unsubscribe$.complete();
	}
}
