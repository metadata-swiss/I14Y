import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {Observable} from 'rxjs';

export class ApiAuthInterceptor implements HttpInterceptor {
	intercept(request: HttpRequest<any>, handler: HttpHandler) {
		return this.filterApiCalls(request, handler);
	}

	private addAuthorization(request: HttpRequest<any>, handler: HttpHandler): Observable<HttpEvent<any>> {
		const accessToken: string | null = localStorage.getItem('access_token');

		if (accessToken) {
			return handler.handle(request.clone({setHeaders: {Authorization: `Bearer ${accessToken}`}}));
		} else {
			return handler.handle(request);
		}
	}

	private filterApiCalls(request: HttpRequest<any>, handler: HttpHandler): Observable<HttpEvent<any>> {
		const apiBaseUrl: string | null = localStorage.getItem('api_base_url');

		if (apiBaseUrl) {
			if (request.url.startsWith(apiBaseUrl)) {
				return this.addAuthorization(request, handler);
			} else {
				return handler.handle(request);
			}
		}

		return this.addAuthorization(request, handler);
	}
}
