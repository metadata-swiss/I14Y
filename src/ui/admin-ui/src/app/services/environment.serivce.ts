import {Injectable} from '@angular/core';
import {AppConfig} from '../app.config';
import {IAppConfig} from '../app.config.interface';

@Injectable({
	providedIn: 'root'
})
export class EnvironmentService {
	public text: string;
	private readonly appConfig = AppConfig.getConfig<IAppConfig>();

	constructor() {
		this.text = this.appConfig.ENV_NAME === 'PRD' ? '' : this.appConfig.ENV_NAME;
	}
}
