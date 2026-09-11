import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {inject, Injectable} from '@angular/core';
import {ObHttpApiInterceptorEvents} from '@oblique/oblique';
import {Observable} from 'rxjs';

/**
 * Marks HTTP calls as background enrichment so that {@link BackgroundRequestInterceptor} keeps the
 * global Oblique master loader down for them and the surrounding page stays usable.
 *
 * Oblique's own `deactivateSpinnerOnNextAPICalls` cannot be used directly: it silences the next
 * request that reaches the interceptor, whichever one that is. The concept page dispatches seven
 * calls in the same tick, so the deactivation lands on an arbitrary one of them.
 */
@Injectable({providedIn: 'root'})
export class BackgroundRequestService {
	private depth = 0;

	get isDispatchingBackgroundRequest(): boolean {
		return this.depth > 0;
	}

	/**
	 * Wraps `source` so every request it fires on subscription is treated as a background call. The
	 * flag is only raised for the synchronous dispatch, which is all the generated API client needs,
	 * so unrelated requests are never affected.
	 */
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

/**
 * Must be registered *before* `ObHttpApiInterceptor`: `next.handle()` invokes the Oblique
 * interceptor synchronously, and that interceptor broadcasts the request as its very first action.
 * The deactivation therefore always lands on this exact request and can never be claimed by another
 * call dispatched in the same tick.
 */
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
