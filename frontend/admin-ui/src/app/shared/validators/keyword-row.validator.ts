import {AbstractControl, UntypedFormGroup, ValidationErrors, ValidatorFn} from '@angular/forms';

/**
 * Cross-field validator for keyword rows.
 * Ensures that at least a URI or one label (in any language) is provided.
 * If neither URI nor any label is filled, marks both the uri control
 * and all label controls as invalid and touched so mat-error displays.
 */
export function createKeywordRowValidator(): ValidatorFn {
	return (group: AbstractControl): ValidationErrors | null => {
		if (!(group instanceof UntypedFormGroup)) {
			return null;
		}

		const uriControl = group.get('uri');
		const labelGroup = group.get('label') as UntypedFormGroup;

		const uriValue = uriControl?.value?.trim() || '';
		const hasUri = uriValue.length > 0;

		const langKeys = labelGroup ? Object.keys(labelGroup.controls) : [];
		const hasAnyLabel = langKeys.some(lang => {
			const val = labelGroup.get(lang)?.value?.trim() || '';
			return val.length > 0;
		});

		if (!hasUri && !hasAnyLabel) {
			// Set errors and mark touched on URI
			if (uriControl) {
				uriControl.setErrors({...(uriControl.errors || {}), keywordEmpty: true});
				uriControl.markAsTouched();
			}

			// Set errors and mark touched on each label control
			langKeys.forEach(lang => {
				const ctrl = labelGroup.get(lang);
				if (ctrl) {
					ctrl.setErrors({...(ctrl.errors || {}), keywordEmpty: true});
					ctrl.markAsTouched();
				}
			});

			return {keywordEmpty: true};
		}

		// Clear only the keywordEmpty error, preserve other errors (e.g. pattern on URI)
		if (uriControl?.errors?.keywordEmpty) {
			const {keywordEmpty, ...remaining} = uriControl.errors;
			uriControl.setErrors(Object.keys(remaining).length > 0 ? remaining : null);
		}

		langKeys.forEach(lang => {
			const ctrl = labelGroup.get(lang);
			if (ctrl?.errors?.keywordEmpty) {
				const {keywordEmpty, ...remaining} = ctrl.errors;
				ctrl.setErrors(Object.keys(remaining).length > 0 ? remaining : null);
			}
		});

		return null;
	};
}
