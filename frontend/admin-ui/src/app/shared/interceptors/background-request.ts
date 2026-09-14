import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {inject, Injectable} from '@angular/core';
import {ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {Observable} from 'rxjs';

/** Marks HTTP calls as background enrichment, so they do not raise the global Oblique spinner. */
@Injectable({providedIn: 'root'})
export class BackgroundRequestService {
	private depth = 0;

	get isDispatchingBackgroundRequest(): boolean {
		return this.depth > 0;
	}

	/** Requests fired synchronously on subscription to `source` are treated as background calls. */
	withoutGlobalSpinner<T>(source: Observable<T>): Observable<T> {
		return new Observable<T>(subscriber => {
			this.depth++;

			try {
				return source.subscribe(subscriber);
			} finally {
				this.depth--;
			}
		});
	}
}

/** Must be registered before `ObHttpApiInterceptor`, otherwise the deactivation hits another request. */
@Injectable()
export class BackgroundRequestInterceptor implements HttpInterceptor {
	private readonly backgroundRequests = inject(BackgroundRequestService);
	private readonly obHttpApiInterceptorEvents = inject(ObHttpApiInterceptorEvents);

	intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
		if (this.backgroundRequests.isDispatchingBackgroundRequest) {
			this.obHttpApiInterceptorEvents.deactivateSpinnerOnNextAPICalls(1);
		}

		return next.handle(request);
	}
}
