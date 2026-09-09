import {Injectable} from '@angular/core';
import {
  IApiClientOptionsBuilder,
  IApiClientSupport
} from './generated/BfsIopAdminApiClient.extensions.g';

@Injectable({
  providedIn: 'root'
})
export class BfsIopAdminApiClientSupport implements IApiClientSupport {
  SetupApiClientOptions(optionsBuilder: IApiClientOptionsBuilder): void {
    optionsBuilder.addPrepareRequest(requestOptions => {
      requestOptions.headers = requestOptions.headers.append(
        'x-generated-client',
        'true'
      );
    });
    optionsBuilder.addProcessResponse(() => {});
  }
}