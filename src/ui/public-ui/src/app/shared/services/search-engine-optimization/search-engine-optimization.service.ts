import {inject, Injectable} from '@angular/core';
import {Meta, Title, MetaDefinition} from '@angular/platform-browser';

@Injectable()
export class SearchEngineOptimizationService {
	private readonly metaService = inject(Meta);
	private readonly titleService = inject(Title);

	public UpdateMetaTitle(titleText: string) {
		this.titleService.setTitle(titleText);
		if (this.metaService.getTag('name="title"')) {
			this.metaService.updateTag(this.createMetaDefinition(titleText, 'title', 'og:title'), 'name="title"');
		} else {
			this.metaService.addTag(this.createMetaDefinition(titleText, 'title', 'og:title'));
		}
	}

	public UpdateMetaDescrition(descritionText: string) {
		if (this.metaService.getTag('name="description"')) {
			this.metaService.updateTag(this.createMetaDefinition(descritionText, 'description', 'og:description'), 'name="description"');
		} else {
			this.metaService.addTag(this.createMetaDefinition(descritionText, 'description', 'og:description'));
		}
	}

	private createMetaDefinition(content: string, name: string, property: string): MetaDefinition {
		return {content: content, name: name, property: property};
	}
}
