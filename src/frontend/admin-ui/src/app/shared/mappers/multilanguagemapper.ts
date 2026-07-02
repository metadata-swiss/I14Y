import {MultiLanguage} from '@I14Y-ch/bfs-iop-admin-web-api-client';

export class MultiLanguageMapper {
	public static mapElements(elements: MultiLanguage[]): MultiLanguage[] {
		return elements?.map(x => MultiLanguageMapper.clone(x));
	}

	/**
	 * Clone a MultiLanguage instance and set empty values to undefined
	 * @param element The instance to clone
	 * @returns A cloned instance with undefined properties instead of empty strings
	 */
	public static clone(element: MultiLanguage | undefined): MultiLanguage {
		return new MultiLanguage({
			de: element?.de || undefined,
			fr: element?.fr || undefined,
			it: element?.it || undefined,
			en: element?.en || undefined,
			rm: element?.rm || undefined
		});
	}

	/**
	 * Clone a MultiLanguage instance, returning undefined if all language fields are empty.
	 * Use this for optional label fields where the API expects null instead of an empty object.
	 * @param element The instance to clone
	 * @returns A cloned instance, or undefined if no language has a value
	 */
	public static cloneOrUndefined(element: MultiLanguage | null | undefined): MultiLanguage | undefined {
		if (!element) {
			return undefined;
		}
		const cloned = MultiLanguageMapper.clone(element);
		if (!cloned.de && !cloned.en && !cloned.fr && !cloned.it && !cloned.rm) {
			return undefined;
		}
		return cloned;
	}

	/**
	 * Set empty string properties to undefined
	 * @param element The instance to fix
	 */
	public static fixEmptyValues(element: MultiLanguage | undefined): MultiLanguage | undefined {
		if (element) {
			element.de ||= undefined;
			element.fr ||= undefined;
			element.it ||= undefined;
			element.en ||= undefined;
			element.rm ||= undefined;
		}
		return element;
	}
}
