// Every request of the generated iop-core client (api-client/generated, `npm run api:generate`) goes through iopCoreFetch.

let baseURL = ''

/** Sets the iop-core URL. Called by app/plugins/iop-core-client.ts. */
export function setIopCoreBaseUrl(url: string) {
  baseURL = url
}

/** What a call throws for a non-2xx response ($fetch's FetchError): the HTTP status and iop-core's ProblemDetails. */
export type ErrorType<TError> = Error & { statusCode?: number, data?: TError }

export function iopCoreFetch<T>(url: string, options: RequestInit): Promise<T> {
  return $fetch<T>(url, {
    baseURL,
    method: options.method as 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE',
    headers: options.headers,
    body: options.body,
    signal: options.signal,
  })
}
