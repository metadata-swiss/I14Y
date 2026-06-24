// Matches a query that is a single email address (no surrounding whitespace/quotes).
const EMAIL_QUERY_REGEX = /^[^\s@"]+@[^\s@"]+\.[^\s@"]+$/;

/**
 * Wraps the query in double quotes when it is a single email address, so the search backend matches
 * the address exactly (against the responsible-person / deputy / contact-point email fields) instead
 * of tokenizing it (foo.bar@x.admin.ch -> foo/bar/x/admin/ch) and over-matching every object that
 * merely shares one of those tokens. Mirrors the admin UI's "My Data" quoting. The backend also
 * tolerates the quotes, so this is consistent whether or not the backend email fix is deployed.
 *
 * Non-email queries are returned unchanged; null/empty becomes undefined to match the API client's
 * optional `query` parameter.
 */
export function quoteQueryIfEmail(query: string | null | undefined): string | undefined {
	if (!query) {
		return undefined;
	}

	const trimmed = query.trim();
	return EMAIL_QUERY_REGEX.test(trimmed) ? `"${trimmed}"` : query;
}
