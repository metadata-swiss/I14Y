import {DcatPublicServiceService} from '../services/dcat-publicservice.service';
import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {Subject, takeUntil} from 'rxjs';
import {IdLabel, PublicServiceView, VocabularyEntry} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {FormatFunctions} from '../../shared/format-functions';
import {TranslateService} from '@ngx-translate/core';
import {ViewType} from 'src/app/shared/templates/viewtype';

@Component({
	selector: 'app-public-service-description',
	templateUrl: './public-service-description.component.html',
	styleUrls: ['./public-service-description.component.scss'],
	standalone: false
})
export class PublicServiceDescriptionComponent implements OnInit, OnDestroy {
	publicService: PublicServiceView;
	hasChannels: boolean = false;
	currentLanguage: string;
	readonly emptyPlaceHolder: string = '-';
	readonly viewTypeEnum = ViewType;

	private readonly unsubscribe$ = new Subject();

	private readonly dcatPublicServiceService = inject(DcatPublicServiceService);
	private readonly translate = inject(TranslateService);

	constructor() {
		this.currentLanguage = this.translate.getCurrentLang();
	}

	ngOnInit(): void {
		this.dcatPublicServiceService.publicService$.pipe(takeUntil(this.unsubscribe$)).subscribe(x => {
			this.publicService = x;
			this.hasChannels = x.channels?.length > 0;
		});
	}

	ngOnDestroy() {
		this.unsubscribe$.next(null);
		this.unsubscribe$.complete();
	}

	getIsDescribedAt(): IdLabel[] | undefined {
		return this.publicService?.isDescribedAt ? this.publicService.isDescribedAt : undefined;
	}

	getRelations(): IdLabel[] | undefined {
		return this.publicService?.relation ? this.publicService.relation : undefined;
	}

	getRequires(): IdLabel[] | undefined {
		return this.publicService?.requires ? this.publicService.requires : undefined;
	}

	getVocabularyEntriesWithCode(entries: VocabularyEntry[] | undefined): string[] | undefined {
		return FormatFunctions.getTranslatedVocabularyEntriesWithCode(entries, this.currentLanguage);
	}
}
