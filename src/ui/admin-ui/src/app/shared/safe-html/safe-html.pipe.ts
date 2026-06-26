import {inject, Pipe, PipeTransform} from '@angular/core';
import {DomSanitizer} from '@angular/platform-browser';

@Pipe({
	name: 'safeHtml',
	standalone: false
})
export class SafeHtmlPipe implements PipeTransform {
	private readonly sanitizer = inject(DomSanitizer);

	transform(value: string) {
		return this.sanitizer.bypassSecurityTrustHtml(value);
	}
}
