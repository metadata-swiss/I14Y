import {Component, input} from '@angular/core';
import {VocabularyEntry} from '@I14Y-ch/bfs-iop-admin-web-api-client';

@Component({
	selector: 'app-vocabulary-entry-list',
	templateUrl: './vocabulary-entry-list.component.html',
	standalone: false
})
export class VocabularyEntryListComponent {
	label = input.required<string>();
	vocabularyIri = input<string | undefined>();
	entries = input<VocabularyEntry[] | undefined>();
	getSearchUrl = input.required<(code: string) => string>();
	valueId = input.required<string>();
	currentLanguage = input.required<string>();
}
