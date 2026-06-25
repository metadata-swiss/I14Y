import {Pipe, PipeTransform} from '@angular/core';
import {EMAIL_PATTERN, URL_PATTERN} from 'src/app/app-constants';
import {KeyValue} from '@angular/common';

@Pipe({
	name: 'texttolink',
	standalone: false
})
export class TextToLinkPipe implements PipeTransform {
	transform(source: string | undefined): string | undefined {
		let result = source;

		result = this.escapeHtml(result);
		result = this.replaceUrl(result);
		result = this.replaceEmail(result);

		return result;
	}

	private escapeHtml(result: string | undefined): string | undefined {
		return result?.replace(/<\//g, '&lt;&#47;').replace(/</g, '&lt;')?.replace(/>/g, '&gt;');
	}

	private replaceUrl(result: string | undefined): string | undefined {
		let matches = result?.match(new RegExp(URL_PATTERN, 'g'))?.map(x => x);
		let replaceTable: KeyValue<string, string>[] = [];

		matches?.forEach((x: string, index: number) => {
			let replaceKey = `[urlPlaceholder${index}]`;
			replaceTable.push({key: replaceKey, value: this.createUrlLink(x)});

			result = result?.replace(x, replaceKey);
		});

		replaceTable.forEach(x => {
			result = result?.replace(x.key, x.value);
		});

		return result;
	}

	private createUrlLink(x: string): string {
		return `<a href="${x}" target="_blank" rel="noopener noreferrer" icon="none">${x}</a>`;
	}

	private replaceEmail(result: string | undefined): string | undefined {
		let matches = result?.match(new RegExp(EMAIL_PATTERN, 'g'))?.map(x => x);
		let replaceTable: KeyValue<string, string>[] = [];

		matches?.forEach((x: string, index: number) => {
			let replaceKey = `[emailPlaceholder${index}]`;
			replaceTable.push({key: replaceKey, value: this.createMailtoLink(x)});

			result = result?.replace(x, replaceKey);
		});

		replaceTable.forEach(x => {
			result = result?.replace(x.key, x.value);
		});

		return result;
	}

	private createMailtoLink(x: string): string {
		return `<a href="mailto:${x}" icon="none">${x}</a>`;
	}
}
