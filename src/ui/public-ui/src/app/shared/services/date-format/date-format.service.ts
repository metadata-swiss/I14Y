import {inject, Injectable} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import moment from 'moment';

@Injectable()
export class DateFormatService {
	private readonly translate = inject(TranslateService);

	formatLongDate(date?: Date): string {
		return moment(date).locale(this.translate.getCurrentLang()).format('DD. MMMM yyyy');
	}

	public formatShortDate(date?: Date): string {
		return moment(date).locale(this.translate.getCurrentLang()).format('DD.MM.yyyy');
	}
}
