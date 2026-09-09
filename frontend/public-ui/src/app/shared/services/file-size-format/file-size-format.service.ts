import {Injectable} from '@angular/core';

@Injectable()
export class FileSizeFormatService {
	public formatBytes(numBytes: number | undefined, decPlaces): string | undefined {
		if (numBytes) {
			const oneKByte = 1000;

			if (decPlaces === undefined || decPlaces === '') {
				decPlaces = 2;
			}

			const byteMtrcs = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
			const mtrcNumbFactor = Math.floor(Math.log(numBytes) / Math.log(oneKByte));
			return `${parseFloat((numBytes / oneKByte ** mtrcNumbFactor).toFixed(decPlaces))} ${byteMtrcs[mtrcNumbFactor]}`;
		}
		return undefined;
	}
}
