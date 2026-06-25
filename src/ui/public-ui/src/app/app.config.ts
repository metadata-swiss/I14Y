export class AppConfig {
	private static config: any;
	private static readonly fileName = 'assets/config/appconfig.json';

	static loadConfig<T>(): T {
		const xhr = new XMLHttpRequest();
		xhr.overrideMimeType('application/json');
		xhr.open('GET', AppConfig.fileName, false);
		xhr.send(null);
		if (xhr.status === 200) {
			if (xhr.responseText) {
				try {
					const parsed = JSON.parse(xhr.responseText);
					AppConfig.config = parsed;
				} catch (e) {
					console.error(e);
				}
			}
		} else {
			const errorMessage = `Could not load file '${AppConfig.fileName}': ${xhr.status}`;
			console.error(errorMessage);
		}

		return AppConfig.config as T;
	}

	static getConfig<T>(): T {
		if (!AppConfig.config || AppConfig.config === undefined) {
			return AppConfig.loadConfig();
		}
		return AppConfig.config as T;
	}
}
