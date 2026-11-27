import {Component, input, TemplateRef} from '@angular/core';
import {MultiLanguage, VocabularyEntry} from '@I14Y-ch/bfs-iop-admin-web-api-client';

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
	/** Shows chips, each led by this name when it returns one. */
	chipPrefix = input<(entry: VocabularyEntry) => MultiLanguage | undefined>();

	prefixOf(entry: VocabularyEntry): MultiLanguage | undefined {
		return this.chipPrefix()?.(entry);
	}

	getEntryRouterLink(entry: VocabularyEntry): string[] {
		return [this.getSearchUrl()(entry).split('?')[0]];
	}

	getEntryQueryParams(entry: VocabularyEntry): Record<string, string> {
		const qs = this.getSearchUrl()(entry).split('?')[1] ?? '';
		return Object.fromEntries(qs.split('&').filter(Boolean).map(p => p.split('=').map(decodeURIComponent)));
	}
}
