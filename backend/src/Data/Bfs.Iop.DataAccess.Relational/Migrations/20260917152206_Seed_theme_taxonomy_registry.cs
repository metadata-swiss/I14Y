using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bfs.Iop.DataAccess.Relational.Migrations
{
    // Everything is resolved by identifier and every statement is guarded, so no id is assumed to exist
    // and re-running this on a database the samples already seeded is a no-op.
    public partial class Seed_theme_taxonomy_registry : Migration
    {
        private const string SwissThemeVocabulary = "Concept_DATASET_THEME";
        private const string EuThemeVocabulary = "VOCAB_EU_DATA_THEME";
        private const string RegistryIdentifier = "VOCAB_I14Y_THEME_TAXONOMY";
        private const string RegistryVersion = "1.0.0";
        private const string SwissThemeBaseUri = "https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME";

        // Every column holding theme values. Sectors and thematic areas are in here because they have
        // always drawn on the Swiss theme vocabulary too.
        private static readonly (string Table, string Column)[] _themeColumns =
        [
            ("dataset", "theme"),
            ("data_service", "theme"),
            ("mapping_tables", "themes"),
            ("iop_concepts", "themes"),
            ("public_services", "sector"),
            ("public_services", "thematic_area")
        ];

        // The resource types a catalog record can point at, with the table holding their themes.
        private static readonly (string Table, string ResourceType)[] _themedResources =
        [
            ("dataset", "Dataset"),
            ("data_service", "DataService")
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Annotate every entry of the Swiss theme vocabulary with its canonical URI.
            // e.g. 101 -> https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/101
            migrationBuilder.Sql($"""
                WITH swiss_theme_concept AS (
                    SELECT c.*
                    FROM data.iop_concepts c
                    JOIN data.vocabulary_config vc
                      ON vc.vocabulary_identifier = '{SwissThemeVocabulary}'
                     AND c.identifiers @> ARRAY[vc.concept_identifier]
                     AND c.version = vc.concept_version
                )
                INSERT INTO data.annotations (id, code_list_entry_id, identifier, position, type, uri, created_at)
                SELECT
                    gen_random_uuid()                  AS id,
                    e.id                               AS code_list_entry_id,
                    ''                                 AS identifier,
                    0                                  AS position,
                    'EXT_RESOURCE'                     AS type,
                    '{SwissThemeBaseUri}/' || e.code   AS uri,
                    now()                              AS created_at
                FROM data.code_list_entries e
                JOIN swiss_theme_concept c ON c.id = e.iop_concept_id
                WHERE NOT EXISTS (
                    SELECT 1 FROM data.annotations a
                    WHERE a.code_list_entry_id = e.id AND a.type = 'EXT_RESOURCE');
                """);

            // 2. The registry concept (vocabulary of theme vocabularies). Publisher and responsible person are inherited from the Swiss theme
            //    concept.
            migrationBuilder.Sql($"""
                WITH swiss_theme_concept AS (
                    SELECT c.*
                    FROM data.iop_concepts c
                    JOIN data.vocabulary_config vc
                      ON vc.vocabulary_identifier = '{SwissThemeVocabulary}'
                     AND c.identifiers @> ARRAY[vc.concept_identifier]
                     AND c.version = vc.concept_version
                )
                INSERT INTO data.iop_concepts (
                    id, concept_type, identifiers, is_locked, themes, version, created_at,
                    publication_level, publisher_id, registration_status, responsible_person_id,
                    code_list_entry_value_type, code_list_entry_value_max_length,
                    name_de, name_en, name_fr, name_it,
                    description_de, description_en, description_fr, description_it)
                SELECT
                    gen_random_uuid()                  AS id,
                    1                                  AS concept_type,                    -- CodeList
                    ARRAY['{RegistryIdentifier}']      AS identifiers,
                    true                               AS is_locked,
                    ARRAY[]::text[]                    AS themes,
                    '{RegistryVersion}'                AS version,
                    now()                              AS created_at,
                    2                                  AS publication_level,               -- Public
                    c.publisher_id                     AS publisher_id,
                    5                                  AS registration_status,             -- Standard
                    c.responsible_person_id            AS responsible_person_id,
                    1                                  AS code_list_entry_value_type,      -- String
                    64                                 AS code_list_entry_value_max_length,
                    'I14Y Themen-Taxonomien'           AS name_de,
                    'I14Y theme taxonomies'            AS name_en,
                    'Taxonomies de thèmes I14Y'        AS name_fr,
                    'Tassonomie dei temi I14Y'         AS name_it,
                    'Dieses Konzept listet die auf der I14Y-Plattform registrierten Themenvokabulare auf.' AS description_de,
                    'This concept lists the theme vocabularies registered on the I14Y platform.'           AS description_en,
                    'Ce concept énumère les vocabulaires de thèmes enregistrés sur la plateforme I14Y.'    AS description_fr,
                    'Questo concetto elenca i vocabolari dei temi registrati sulla piattaforma I14Y.'      AS description_it
                FROM swiss_theme_concept c
                WHERE NOT EXISTS (
                    SELECT 1 FROM data.iop_concepts x WHERE x.identifiers @> ARRAY['{RegistryIdentifier}'])
                LIMIT 1;
                """);

            // 3. Its entries: one per vocabulary usable as a theme source, keyed by vocabulary identifier.
            migrationBuilder.Sql($"""
                WITH registry_concept AS (
                    SELECT c.*
                    FROM data.iop_concepts c
                    WHERE c.identifiers @> ARRAY['{RegistryIdentifier}']
                )
                INSERT INTO data.code_list_entries (id, code, iop_concept_id, position, created_at, name_de, name_en, name_fr, name_it)
                SELECT
                    gen_random_uuid()                  AS id,
                    v.code                             AS code,
                    c.id                               AS iop_concept_id,
                    v.position                         AS position,
                    now()                              AS created_at,
                    v.name_de                          AS name_de,
                    v.name_en                          AS name_en,
                    v.name_fr                          AS name_fr,
                    v.name_it                          AS name_it
                FROM registry_concept c
                CROSS JOIN (VALUES
                    ('{SwissThemeVocabulary}', 0,
                     'I14Y Themenvokabular', 'I14Y themes vocabulary', 'Vocabulaire des thèmes I14Y', 'Vocabolario dei temi I14Y'),
                    ('{EuThemeVocabulary}', 1,
                     'EU Themenvokabular', 'EU thematic vocabulary', 'Vocabulaire thématique de l''UE', 'Vocabolario tematico dell''UE')
                ) AS v(code, position, name_de, name_en, name_fr, name_it)
                WHERE NOT EXISTS (
                    SELECT 1 FROM data.code_list_entries e
                    WHERE e.iop_concept_id = c.id AND e.code = v.code);
                """);

            // 4. Registering it as a vocabulary is what turns the concept into the registry.
            migrationBuilder.Sql($"""
                INSERT INTO data.vocabulary_config (id, vocabulary_identifier, concept_identifier, concept_version, created_at)
                SELECT
                    gen_random_uuid()                  AS id,
                    '{RegistryIdentifier}'             AS vocabulary_identifier,
                    '{RegistryIdentifier}'             AS concept_identifier,
                    '{RegistryVersion}'                AS concept_version,
                    now()                              AS created_at
                WHERE EXISTS (
                    SELECT 1 FROM data.iop_concepts c WHERE c.identifiers @> ARRAY['{RegistryIdentifier}'])
                  AND NOT EXISTS (
                    SELECT 1 FROM data.vocabulary_config vc WHERE vc.vocabulary_identifier = '{RegistryIdentifier}');
                """);

            // 5. Existing rows hold bare theme codes; themes are now stored as URIs. Values that already
            //    are URIs are left alone, so re-running this changes nothing.
            foreach (var (table, column) in _themeColumns)
            {
                migrationBuilder.Sql($"""
                    UPDATE data.{table}
                    SET {column} = ARRAY(
                        SELECT CASE WHEN t.value LIKE 'http%' THEN t.value ELSE '{SwissThemeBaseUri}/' || t.value END
                        FROM unnest({column}) WITH ORDINALITY AS t(value, ord)
                        ORDER BY t.ord)
                    WHERE EXISTS (SELECT 1 FROM unnest({column}) AS v WHERE v NOT LIKE 'http%');
                    """);
            }

            // 6. Themes recorded on catalog records used to be what the RDF export published; the export
            //    now reads them from the resource, so they are moved there. Their taxonomy is whatever
            //    vocabulary the row names, which is how EU themes recorded this way are carried over.
            //    dcat_catalog_theme is left untouched, so this is re-runnable and nothing is lost.
            foreach (var (table, resourceType) in _themedResources)
            {
                migrationBuilder.Sql($"""
                    -- Every (vocabulary, code) that has a URI, across all registered vocabularies.
                    WITH theme_uri AS (
                        SELECT vc.vocabulary_identifier, entry.code, annotation.uri
                        FROM data.vocabulary_config vc
                        JOIN data.iop_concepts concept
                          ON concept.identifiers @> ARRAY[vc.concept_identifier]
                         AND concept.version = vc.concept_version
                        JOIN data.code_list_entries entry ON entry.iop_concept_id = concept.id
                        JOIN data.annotations annotation
                          ON annotation.code_list_entry_id = entry.id
                         AND annotation.type = 'EXT_RESOURCE'
                    ),
                    -- The theme URIs each resource should inherit from its catalog records.
                    inherited AS (
                        SELECT resource.resource_id, array_agg(DISTINCT theme_uri.uri) AS uris
                        FROM data.dcat_catalog_theme catalog_theme
                        JOIN data.dcat_catalog_resource resource
                          ON resource.dcat_catalog_record_id = catalog_theme.dcat_catalog_record_id
                        JOIN theme_uri
                          ON theme_uri.vocabulary_identifier = catalog_theme.theme_taxonomy
                         AND theme_uri.code = catalog_theme.code
                        WHERE resource.resource_type = '{resourceType}'
                        GROUP BY resource.resource_id
                    )
                    UPDATE data.{table} target
                    SET theme = target.theme || ARRAY(
                        SELECT uri FROM unnest(inherited.uris) AS uri
                        WHERE NOT (uri = ANY(target.theme)))
                    FROM inherited
                    WHERE target.id = inherited.resource_id;
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Swiss theme URIs go back to bare codes. Themes moved over from catalog records are left in
            // place: there is no way to tell them apart from themes the resource already carried. That
            // loses nothing, since dcat_catalog_theme still holds the originals.
            foreach (var (table, column) in _themeColumns)
            {
                migrationBuilder.Sql($"""
                    UPDATE data.{table}
                    SET {column} = ARRAY(
                        SELECT CASE
                            WHEN t.value LIKE '{SwissThemeBaseUri}/%'
                            THEN substring(t.value from length('{SwissThemeBaseUri}/') + 1)
                            ELSE t.value END
                        FROM unnest({column}) WITH ORDINALITY AS t(value, ord)
                        ORDER BY t.ord)
                    WHERE EXISTS (SELECT 1 FROM unnest({column}) AS v WHERE v LIKE '{SwissThemeBaseUri}/%');
                    """);
            }

            migrationBuilder.Sql($"""
                DELETE FROM data.vocabulary_config WHERE vocabulary_identifier = '{RegistryIdentifier}';
                """);

            // The registry entries go with the concept, by cascade.
            migrationBuilder.Sql($"""
                DELETE FROM data.iop_concepts WHERE identifiers @> ARRAY['{RegistryIdentifier}'];
                """);

            migrationBuilder.Sql($"""
                WITH swiss_theme_concept AS (
                    SELECT c.*
                    FROM data.iop_concepts c
                    JOIN data.vocabulary_config vc
                      ON vc.vocabulary_identifier = '{SwissThemeVocabulary}'
                     AND c.identifiers @> ARRAY[vc.concept_identifier]
                     AND c.version = vc.concept_version
                )
                DELETE FROM data.annotations a
                USING data.code_list_entries e, swiss_theme_concept c
                WHERE a.code_list_entry_id = e.id
                  AND e.iop_concept_id = c.id
                  AND a.type = 'EXT_RESOURCE'
                  AND a.uri LIKE '{SwissThemeBaseUri}/%';
                """);
        }
    }
}
