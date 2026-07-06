import {HttpErrorResponse} from '@angular/common/http';
import {ObINotification} from '@oblique/oblique';

export class MessageHelperFunctions {
	public static getExportErrorMessage(error: HttpErrorResponse): ObINotification {
		let message: string = '';
		let title: string = '';
		switch (error.status) {
			case 400:
			case 403:
			case 404:
			case 500:
			case 501:
			case 502:
			case 503:
			case 504:
				title = `i18n.http_error.${error.status}.title`;
				message = `i18n.http_error.${error.status}.export`;
				break;
			default:
				title = 'i18n.oblique.notification.type.error';
				message = error.message;
				break;
		}

		return {message: message, title: title};
	}
}
