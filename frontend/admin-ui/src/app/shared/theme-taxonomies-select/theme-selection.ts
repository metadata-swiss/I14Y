import {AbstractControl} from '@angular/forms';
import {ThemeInputModel, VocabularyEntry} from '@I14Y-ch/bfs-iop-admin-web-api-client';

/** Updates one vocabulary in the shared themes control, preserving selections from other editors. */
export class ThemeSelection {
	constructor(private readonly control: AbstractControl | null) {}

	get themes(): ThemeInputModel[] {
		return (this.control?.value ?? []) as ThemeInputModel[];
	}

	vocabularies(): string[] {
		return [...new Set(this.themes.map(theme => theme.vocabularyIdentifier).filter((x): x is string => !!x))];
	}

	codesOf(vocabularyIdentifier: string): string[] {
		return this.themes.filter(theme => theme.vocabularyIdentifier === vocabularyIdentifier).map(theme => theme.code!);
	}

	replace(vocabularyIdentifier: string, codes: string[], entries: VocabularyEntry[]): void {
		const kept = this.themes.filter(theme => theme.vocabularyIdentifier !== vocabularyIdentifier);

		const added = codes.map(
			code =>
				new ThemeInputModel({
					code,
					uri: entries.find(entry => entry.code === code)?.uri,
					vocabularyIdentifier
				})
		);

		this.write([...kept, ...added]);
	}

	remove(vocabularyIdentifier: string): void {
		this.write(this.themes.filter(theme => theme.vocabularyIdentifier !== vocabularyIdentifier));
	}

	private write(themes: ThemeInputModel[]): void {
		this.control?.setValue(themes);
		this.control?.markAsDirty();
	}
}
