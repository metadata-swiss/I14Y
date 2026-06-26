export class AppConfig {
	private static config: any;

	static loadConfig<T>(arg: string | any): Promise<void> {
		return new Promise((resolve, reject) => {
			if (typeof arg === 'string') {
				// argument is path to config
				startRequest(resolve, reject);
			} else {
				// argument is already object
				this.config = arg;
				resolve();
			}
		});

		function startRequest(resolve: (value: void | PromiseLike<void>) => void, reject: (reason?: any) => void) {
			const xhr = new XMLHttpRequest();
			xhr.overrideMimeType('application/json');
			xhr.open('GET', arg, true);
			xhr.onreadystatechange = () => {
				if (xhr.readyState === 4) {
					checkRequestStatus(xhr, resolve, reject);
				}
			};
			xhr.send(null);
		}

		function checkRequestStatus(xhr: XMLHttpRequest, resolve: (value: void | PromiseLike<void>) => void, reject: (reason?: any) => void) {
			if (xhr.status === 200) {
				if (xhr.responseText) {
					parseConfig(xhr, resolve, reject);
				}
			} else {
				const errorMessage = `Could not load file '${arg}': ${xhr.status}`;
				console.error(errorMessage);
				reject();
			}
		}

		function parseConfig(xhr: XMLHttpRequest, resolve: (value: void | PromiseLike<void>) => void, reject: (reason?: any) => void) {
			try {
				const parsed = JSON.parse(xhr.responseText);
				AppConfig.config = parsed;
				resolve();
			} catch (e) {
				console.error(e);
				reject();
			}
		}
	}

	static getConfig<T>(): T {
		return AppConfig.config as T;
	}
}
