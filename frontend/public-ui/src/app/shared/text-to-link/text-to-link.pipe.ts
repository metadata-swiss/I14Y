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
		const icon = // eslint-disable-next-line max-len
			'<span class="mat-icon" aria-hidden="true"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fit="" height="100%" width="100%" preserveAspectRatio="xMidYMid meet" focusable="false"><path d="M20.25,14.11621h1.5v7.63403H2.25V2.25025h7.63385v1.5H3.75v16.5h16.5v-6.13403ZM15.95605.78833v1.5h4.69531l-7.7002,7.7002,1.06055,1.06055,7.7002-7.7002v4.69531h1.5V.78833h-7.25586Z"></path></svg></span> ';

		return `<a href="${x}" target="_blank" rel="noopener noreferrer" class="ob-external-link">${x.includes(window.location.origin) ? x : x + icon}</a>`;
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
