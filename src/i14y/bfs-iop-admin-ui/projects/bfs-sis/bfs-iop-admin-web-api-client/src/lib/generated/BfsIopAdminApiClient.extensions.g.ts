import { HttpHeaders, HttpResponseBase } from '@angular/common/http';
import { mergeMap as _observableMergeMap } from 'rxjs/operators';
import { Observable, of as _observableOf } from 'rxjs';

export interface IApiClientOptionsBuilder {
    addPrepareRequest(transform: (options: IRequestOptions) => void): void;
    
    addProcessResponse(process: (response: HttpResponseBase) => void): void;
}

export interface IApiClientSupport {
    SetupApiClientOptions(optionsBuilder: IApiClientOptionsBuilder): void;
}

export interface IRequestOptions {
    headers: HttpHeaders
}

export class ApiClientBase {
    private readonly requestPreparations: ((options: IRequestOptions) => void)[] = [];
    private readonly responseProcessings: ((response: HttpResponseBase) => void)[] = [];

    constructor(private readonly support: IApiClientSupport) {
        let requestPreparations = this.requestPreparations;
        let responseProcessings = this.responseProcessings;
        support.SetupApiClientOptions({
            addPrepareRequest(transform: (options: IRequestOptions) => void) {
                requestPreparations.push(transform);
            },
            addProcessResponse(process: (response: HttpResponseBase) => void) {
                responseProcessings.push(process);
            }
        })
    }
    
    protected transformResult(
        url: string,
        response: HttpResponseBase,
        processor: (response: HttpResponseBase) => Observable<any>
    ) {
        const status = response.status;

        // creation responses may be empty, but we still need their headers
        if (status === 201 && response.headers) {
            // recreate headers dictionary
            let _headers: any = {};
            for (let key of response.headers.keys()) {
                _headers[key] = response.headers.get(key);
            }
            
            this.responseProcessings.forEach(process => {
                process(response);
            });

            return processor(response).pipe(
                _observableMergeMap(baseResponse => {
                    // get response or create empty object if empty
                    let enrichedResponse: any = baseResponse ? baseResponse : {};
                    // create headers property or enrich it
                    if (!enrichedResponse.headers) {
                        enrichedResponse.headers = _headers;
                    } else {
                        enrichedResponse.headers = Object.assign(
                            enrichedResponse.headers,
                            _headers
                        );
                    }
                    return _observableOf<any>(enrichedResponse);
                })
            );
        }

        return processor(response);
    }
    
    protected transformOptions(options: IRequestOptions): Observable<IRequestOptions> {
        this.requestPreparations.forEach(transform => {
            transform(options);
        })
        return _observableOf<any>(options);
    }
}
