import {Component, input, TemplateRef} from '@angular/core';
import {VocabularyEntry} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Component({
	selector: 'app-vocabulary-entry-list',
	templateUrl: './vocabulary-entry-list.component.html',
	styleUrl: './vocabulary-entry-list.component.scss',
	standalone: false
})
export class VocabularyEntryListComponent {
	label = input.required<string>();
	vocabularyIri = input<string | undefined>();
	labelInfoTemplate = input<TemplateRef<any> | undefined>(undefined);
	entries = input<VocabularyEntry[] | undefined>();
	getSearchUrl = input.required<(entry: VocabularyEntry) => string>();
	valueId = input.required<string>();
	currentLanguage = input.required<string>();

	getEntryRouterLink(entry: VocabularyEntry): string[] {
		return [this.getSearchUrl()(entry).split('?')[0]];
	}

	getEntryQueryParams(entry: VocabularyEntry): Record<string, string> {
		const qs = this.getSearchUrl()(entry).split('?')[1] ?? '';
		return Object.fromEntries(qs.split('&').filter(Boolean).map(p => p.split('=')));
	}
}
