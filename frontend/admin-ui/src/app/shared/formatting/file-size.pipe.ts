import {Pipe, PipeTransform} from '@angular/core';

/*
 * Display filesize in a reasonably formatted way
 * Usage:
 *   bytesize | filesize
 * Example:
 *   {{ 1024 | filesize }}
 *   formats to: 1 KB
 */
@Pipe({
	name: 'filesize',
	pure: true,
	standalone: false
})
export class FileSizePipe implements PipeTransform {
	transform(byteSize: number | undefined): string {
		const input = byteSize ?? 0;
		if (input > 0) {
			const oneKByte = 1000;
			const byteMtrcs = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
			const mtrcNumbFactor = Math.floor(Math.log(input) / Math.log(oneKByte));
			return `${parseFloat((input / oneKByte ** mtrcNumbFactor).toFixed(0))} ${byteMtrcs[mtrcNumbFactor]}`;
		}
		return '-';
	}
}
