import {Injectable} from '@angular/core';
import {format} from 'date-fns';

@Injectable()
export class DateFormatService {
	formatLongDate(date: Date): string {
		return format(date, 'dd. MMMM yyyy');
	}

	public formatShortDate(date: Date): string {
		return format(date, 'dd.MM.yyyy');
	}
}
