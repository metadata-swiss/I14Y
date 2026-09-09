import {Component, inject, Input, OnDestroy} from '@angular/core';
import {IMultiLanguage, ResourceModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {FallbackPipe} from '../fallback/fallback.pipe';
import {LangChangeEvent, TranslateService} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';

@Component({
	selector: 'app-resource-model',
	templateUrl: './resourcemodel.component.html',
	standalone: false
})
export class ResourceModelComponent implements OnDestroy {
	@Input() resources: ResourceModel[];
	@Input() label: string;
	@Input() forceDisplay = false;
	@Input() id: string;
	currentLang: string;

	private readonly unsubscribe$ = new Subject();

	private readonly fallback = inject(FallbackPipe);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLang = this.translate.getCurrentLang();
		this.translate.onLangChange.pipe(takeUntil(this.unsubscribe$)).subscribe((language: LangChangeEvent) => {
			this.currentLang = language.lang;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(null);
		this.unsubscribe$.complete();
	}

	getOrderedRessources(): ResourceModel[] {
		const shallowCopy = [...(this.resources ?? [])];
		shallowCopy.sort((a, b) => {
			if (this.getTranslated(a.label) === this.getTranslated(b.label)) {
				return a.uri.localeCompare(b.uri);
			}
			return this.getTranslated(a.label).localeCompare(this.getTranslated(b.label));
		});
		return shallowCopy;
	}

	private getTranslated(source: IMultiLanguage | undefined): string | undefined {
		return this.fallback.transform(source, this.currentLang) ?? undefined;
	}
}
