import {Component, inject, Input} from '@angular/core';
import {Annotation} from '@I14Y-ch/bfs-iop-admin-web-api-client';
import {TranslateService} from '@ngx-translate/core';
import {ObNavTreeItemModelPlus} from 'src/app/datasets/content/ObNavTreeItemModelPlus';
import {FormatFunctions} from 'src/app/shared/format-functions';
import {buildConceptCodeIri} from 'src/app/shared/helper/iri-helpers';
import {hasAnyLangText} from 'src/app/shared/helper/assert-helper';

@Component({
	selector: 'app-codelistentry-detail',
	templateUrl: './codelistentry-detail.component.html',
	styleUrls: ['./codelistentry-detail.component.scss'],
	standalone: false
})
export class CodeListEntryDetailComponent {
	@Input()
	set currentNode(node: ObNavTreeItemModelPlus) {
		this.node = node;
		this.updateFields();
	}
	nodeSelected = false;
	hasAnnotations = false;
	annotations: Annotation[];
	code: string;
	title: string;
	validFrom: Date | undefined;
	validTo: Date | undefined;
	description: string | undefined;
	conceptIdentifier: string;
	conceptVersion: string;

	private node: ObNavTreeItemModelPlus;

	private readonly translate = inject(TranslateService);

	getFormattedDate(date: Date | undefined): string | null {
		return FormatFunctions.getFormattedDate(date);
	}

	getFormattedConceptCodeIriPattern(): string | undefined {
		const identifier = this.conceptIdentifier;
		const version = this.conceptVersion;
		const code = encodeURIComponent(this.code);

		if (!identifier || !version) {
			return undefined;
		}

		return buildConceptCodeIri(identifier, code, version);
	}

	private updateFields() {
		this.code = undefined;
		this.title = undefined;
		this.description = undefined;
		this.nodeSelected = false;
		if (this.node) {
			this.nodeSelected = true;
			this.code = this.node.code;
			this.title = this.node.title;
			this.description = this.node.description;
			this.validFrom = this.node.validFrom;
			this.validTo = this.node.validTo;
			this.annotations = (this.node.annotations ?? [])
				.filter(
					a => (a.text && hasAnyLangText(a.text)) || (a.type && a.type.trim() !== '') || (a.title && a.title.trim() !== '') || (a.uri && a.uri.trim() !== '')
				)
				.sort((a, b) => a.position - b.position);
			this.hasAnnotations = this.annotations.length > 0;
			this.conceptIdentifier = this.node.conceptIdentifier ?? '';
			this.conceptVersion = this.node.conceptVersion ?? '';
		}
	}
}
