import {Pipe, PipeTransform} from '@angular/core';
import {FormatFunctions} from '../format-functions';

/*
 * Join string array or display default value
 * Usage:
 *   myarray | arraytostring:separator:defaultValue
 * Example:
 *   {{ ['a', 'b'] | arraytostring }}
 *   formats to: a, b
 *   {{ [] | arraytostring }}
 *   formats to:
 *   {{ [] | arraytostring: ", ": "-" }}
 *  formats to: -
 */
@Pipe({
	name: 'arraytostring',
	pure: true,
	standalone: false
})
export class ArrayToStringPipe implements PipeTransform {
	transform(input: string[] | undefined, separator: string = ', ', defaultValue: string = ''): string {
		return FormatFunctions.convertArrayToString(input, separator, defaultValue);
	}
}
