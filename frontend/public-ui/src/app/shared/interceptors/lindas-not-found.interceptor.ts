import {HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse, HttpStatusCode} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {catchError, Observable, of, throwError} from 'rxjs';

@Injectable()
export class LindasNotFoundInterceptor implements HttpInterceptor {
	intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
		return next.handle(request).pipe(
			catchError(error => {
				if (error instanceof HttpErrorResponse && error.status === HttpStatusCode.NotFound && request.url.includes('/api/Lindas/')) {
					return of(
						new HttpResponse({
							body: error.error,
							headers: error.headers,
							status: error.status,
							statusText: error.statusText,
							url: error.url ?? request.url
						})
					);
				}

				return throwError(() => error);
			})
		);
	}
}