import {Component, inject, Input, OnDestroy, OnInit} from '@angular/core';
import {ChannelModel} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService, LangChangeEvent} from '@ngx-translate/core';
import {Subject, takeUntil} from 'rxjs';
import {ExtendedFallbackPipe} from 'src/app/shared/fallback/extendedfallback.pipe';
import {FallBackModel} from 'src/app/shared/fallback/fallbackmodel';

// eslint-disable-next-line no-shadow
export enum ArtType {
	Post = 'Post',
	Email = 'E-Mail',
	MobilePhone = 'Mobile Phone',
	Phone = 'Phone',
	Fax = 'Fax',
	Web = 'Web'
}

@Component({
	selector: 'app-channel-detail',
	templateUrl: './channel-detail.component.html',
	styleUrls: ['./channel-detail.component.scss'],
	standalone: false
})
export class ChannelDetailComponent implements OnInit, OnDestroy {
	@Input() channel: ChannelModel | undefined;
	channelType: string = '';
	buttonTranslationText: string = '';
	currentLanguage: string;

	readonly emptyPlaceHolder: string = '-';

	readonly channelTypeCodes = {
		post: '0c84394663',
		email: '1fc1caefa8',
		mobile: '5c12931e3f',
		fax: 'a1c5444664',
		web: 'b37115f83e',
		phone: 'c05134f579'
	};

	readonly EU_Channel_Types = [
		{
			code: '0c84394663',
			name: {
				de: 'Post',
				en: 'Post',
				fr: 'Courrier postal',
				it: 'Posta ordinaria',
				rm: null
			}
		},
		{
			code: '1fc1caefa8',
			name: {
				de: 'E-Mail-Adresse',
				en: 'E-Mail',
				fr: 'Adresse électronique',
				it: 'E-mail',
				rm: null
			}
		},
		{
			code: '5c12931e3f',
			name: {
				de: 'Mobiltelefon',
				en: 'Mobile Phone',
				fr: 'Téléphone portable',
				it: 'Telefono cellulare',
				rm: null
			}
		},
		{
			code: 'a1c5444664',
			name: {
				de: 'Fax',
				en: 'Fax',
				fr: 'Télécopie',
				it: 'Fax',
				rm: null
			}
		},
		{
			code: 'b37115f83e',
			name: {
				de: 'Internet',
				en: 'Web',
				fr: 'Web',
				it: 'Sito web',
				rm: null
			}
		},
		{
			code: 'c05134f579',
			name: {
				de: 'Telefon',
				en: 'Phone',
				fr: 'Téléphone',
				it: 'Telefono',
				rm: null
			}
		}
	];

	private readonly unsubscribe$ = new Subject();

	private readonly extendedFallbackPipe = inject(ExtendedFallbackPipe);
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

	showOpeninghours(): boolean {
		return (
			this.channel?.type?.code === this.channelTypeCodes.post ||
			this.channel?.type?.code === this.channelTypeCodes.phone ||
			this.channel?.type?.code === this.channelTypeCodes.mobile
		);
	}

	getOwendBy(): FallBackModel[] | undefined {
		return this.channel?.ownedBy?.map(o => this.extendedFallbackPipe.transform(o.name, this.currentLanguage)).filter(f => f !== undefined) ?? undefined;
	}

	showButton(): boolean {
		const channelType = this.EU_Channel_Types.find(query => {
			return (
				query.name.en === this.channel?.type?.name?.en ||
				query.name.de === this.channel?.type?.name?.de ||
				query.name.fr === this.channel?.type?.name?.fr ||
				query.name.it === this.channel?.type?.name?.it
			);
		});

		if (channelType?.code === this.channelTypeCodes.email) {
			this.channelType = 'email';
		}

		if (channelType?.code === this.channelTypeCodes.web) {
			this.channelType = 'web';
		}

		if (this.channelType === 'email') {
			this.setButtonTranslation('email');
			return true;
		}

		if (this.channelType === 'web') {
			this.setButtonTranslation('web');
			return true;
		}

		return false;
	}

	setButtonTranslation(channelType: string): void {
		if (channelType === 'email') {
			this.buttonTranslationText = 'i18n.channel.button.email.text';
		}
		if (channelType === 'web') {
			this.buttonTranslationText = 'i18n.channel.button.url.text';
		}
	}

	generateButtonLink(): string {
		if (this.channel?.url) {
			return this.channel.url;
		}
		if (this.channel?.email) {
			return `mailto: ${this.channel?.email}`;
		}
		return '#';
	}
}
