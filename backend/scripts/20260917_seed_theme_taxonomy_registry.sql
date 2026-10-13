-- Seeds the theme taxonomy vocabulary and converts stored theme codes to URIs.
--
-- Run this BEFORE the application version that reads themes as URIs starts serving traffic:
-- until it has run, stored themes are bare codes and every resource read fails.
--
-- Everything is resolved by identifier and every statement is guarded, so running it twice
-- changes nothing, and running it on a database the samples already seeded is a no-op.
--
--   psql -U <user> -d <database> -v ON_ERROR_STOP=1 -f 20260917_seed_theme_taxonomy_registry.sql

BEGIN;

-- 1. Give every Swiss theme entry its canonical URI on an EXT_RESOURCE annotation,
--    e.g. 101 -> https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/101.
--    Filling an empty annotation and creating a missing one are separate statements, and
--    1a runs first: a second annotation would leave the mapper picking either one.

-- 1a. Fill in the annotations that are there but empty.
UPDATE data.annotations annotation
SET uri = 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/' || entry.code
FROM data.code_list_entries entry
JOIN data.iop_concepts concept ON concept.id = entry.iop_concept_id
WHERE concept.identifiers @> ARRAY['DV_DCAT_DATASET_THEME']
  AND annotation.code_list_entry_id = entry.id
  AND annotation.type = 'EXT_RESOURCE'
  AND (annotation.uri IS NULL OR annotation.uri = '');

-- 1b. Create the annotations that are missing altogether.
INSERT INTO data.annotations (id, code_list_entry_id, identifier, position, type, uri, created_at)
SELECT
    gen_random_uuid()                                                                   AS id,
    entry.id                                                                            AS code_list_entry_id,
    ''                                                                                  AS identifier,
    0                                                                                   AS position,
    'EXT_RESOURCE'                                                                      AS type,
    'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/' || entry.code    AS uri,
    now()                                                                               AS created_at
FROM data.code_list_entries entry
JOIN data.iop_concepts concept ON concept.id = entry.iop_concept_id
WHERE concept.identifiers @> ARRAY['DV_DCAT_DATASET_THEME']
  AND NOT EXISTS (
    SELECT 1 FROM data.annotations annotation
    WHERE annotation.code_list_entry_id = entry.id AND annotation.type = 'EXT_RESOURCE'
      AND annotation.uri IS NOT NULL AND annotation.uri <> '');

-- 2. The registry concept (vocabulary of theme vocabularies). Publisher and responsible person
--    are inherited from the Swiss theme concept.
WITH swiss_theme_concept AS (
    SELECT concept.*
    FROM data.iop_concepts concept
    JOIN data.vocabulary_config config
      ON config.vocabulary_identifier = 'Concept_DATASET_THEME'
     AND concept.identifiers @> ARRAY[config.concept_identifier]
     AND concept.version = config.concept_version
)
INSERT INTO data.iop_concepts (
    id, concept_type, identifiers, is_locked, themes, version, created_at,
    publication_level, publisher_id, registration_status, responsible_person_id,
    code_list_entry_value_type, code_list_entry_value_max_length,
    name_de, name_en, name_fr, name_it,
    description_de, description_en, description_fr, description_it)
SELECT
    gen_random_uuid()                       AS id,
    1                                       AS concept_type,                    -- CodeList
    ARRAY['VOCAB_I14Y_THEME_TAXONOMY']      AS identifiers,
    true                                    AS is_locked,
    ARRAY[]::text[]                         AS themes,
    '1.0.0'                                 AS version,
    now()                                   AS created_at,
    2                                       AS publication_level,               -- Public
    concept.publisher_id                    AS publisher_id,
    5                                       AS registration_status,             -- Standard
    concept.responsible_person_id           AS responsible_person_id,
    1                                       AS code_list_entry_value_type,      -- String
    64                                      AS code_list_entry_value_max_length,
    'I14Y Themen-Taxonomien'                AS name_de,
    'I14Y theme taxonomies'                 AS name_en,
    'Taxonomies de thèmes I14Y'             AS name_fr,
    'Tassonomie dei temi I14Y'              AS name_it,
    'Dieses Konzept listet die auf der I14Y-Plattform registrierten Themenvokabulare auf.' AS description_de,
    'This concept lists the theme vocabularies registered on the I14Y platform.'           AS description_en,
    'Ce concept énumère les vocabulaires de thèmes enregistrés sur la plateforme I14Y.'    AS description_fr,
    'Questo concetto elenca i vocabolari dei temi registrati sulla piattaforma I14Y.'      AS description_it
FROM swiss_theme_concept concept
WHERE NOT EXISTS (
    SELECT 1 FROM data.iop_concepts existing WHERE existing.identifiers @> ARRAY['VOCAB_I14Y_THEME_TAXONOMY'])
LIMIT 1;

-- 3. Its entries: one per vocabulary usable as a theme source, keyed by vocabulary identifier.
WITH registry_concept AS (
    SELECT concept.*
    FROM data.iop_concepts concept
    WHERE concept.identifiers @> ARRAY['VOCAB_I14Y_THEME_TAXONOMY']
)
INSERT INTO data.code_list_entries (id, code, iop_concept_id, position, created_at, name_de, name_en, name_fr, name_it)
SELECT
    gen_random_uuid()                       AS id,
    v.code                                  AS code,
    concept.id                              AS iop_concept_id,
    v.position                              AS position,
    now()                                   AS created_at,
    v.name_de                               AS name_de,
    v.name_en                               AS name_en,
    v.name_fr                               AS name_fr,
    v.name_it                               AS name_it
FROM registry_concept concept
CROSS JOIN (VALUES
    ('Concept_DATASET_THEME', 0,
     'I14Y Themenvokabular', 'I14Y themes vocabulary', 'Vocabulaire des thèmes I14Y', 'Vocabolario dei temi I14Y'),
    ('VOCAB_EU_DATA_THEME', 1,
     'EU Themenvokabular', 'EU thematic vocabulary', 'Vocabulaire thématique de l''UE', 'Vocabolario tematico dell''UE')
) AS v(code, position, name_de, name_en, name_fr, name_it)
WHERE NOT EXISTS (
    SELECT 1 FROM data.code_list_entries entry
    WHERE entry.iop_concept_id = concept.id AND entry.code = v.code);

-- 4. Registering it as a vocabulary is what turns the concept into the registry.
INSERT INTO data.vocabulary_config (id, vocabulary_identifier, concept_identifier, concept_version, created_at)
SELECT
    gen_random_uuid()                       AS id,
    'VOCAB_I14Y_THEME_TAXONOMY'             AS vocabulary_identifier,
    'VOCAB_I14Y_THEME_TAXONOMY'             AS concept_identifier,
    '1.0.0'                                 AS concept_version,
    now()                                   AS created_at
WHERE EXISTS (
    SELECT 1 FROM data.iop_concepts concept WHERE concept.identifiers @> ARRAY['VOCAB_I14Y_THEME_TAXONOMY'])
  AND NOT EXISTS (
    SELECT 1 FROM data.vocabulary_config config WHERE config.vocabulary_identifier = 'VOCAB_I14Y_THEME_TAXONOMY');

-- 5. Existing rows hold bare theme codes; themes are now stored as URIs. Values that already
--    are URIs are left alone, so re-running this changes nothing. Sectors and thematic areas
--    are in here because they have always drawn on the Swiss theme vocabulary too.
UPDATE data.dataset
SET theme = ARRAY(
    SELECT CASE WHEN t.value LIKE 'http%' THEN t.value
           ELSE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/' || t.value END
    FROM unnest(theme) WITH ORDINALITY AS t(value, ord)
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(theme) AS v WHERE v NOT LIKE 'http%');

UPDATE data.data_service
SET theme = ARRAY(
    SELECT CASE WHEN t.value LIKE 'http%' THEN t.value
           ELSE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/' || t.value END
    FROM unnest(theme) WITH ORDINALITY AS t(value, ord)
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(theme) AS v WHERE v NOT LIKE 'http%');

UPDATE data.mapping_tables
SET themes = ARRAY(
    SELECT CASE WHEN t.value LIKE 'http%' THEN t.value
           ELSE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/' || t.value END
    FROM unnest(themes) WITH ORDINALITY AS t(value, ord)
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(themes) AS v WHERE v NOT LIKE 'http%');

UPDATE data.iop_concepts
SET themes = ARRAY(
    SELECT CASE WHEN t.value LIKE 'http%' THEN t.value
           ELSE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/' || t.value END
    FROM unnest(themes) WITH ORDINALITY AS t(value, ord)
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(themes) AS v WHERE v NOT LIKE 'http%');

UPDATE data.public_services
SET sector = ARRAY(
    SELECT CASE WHEN t.value LIKE 'http%' THEN t.value
           ELSE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/' || t.value END
    FROM unnest(sector) WITH ORDINALITY AS t(value, ord)
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(sector) AS v WHERE v NOT LIKE 'http%');

UPDATE data.public_services
SET thematic_area = ARRAY(
    SELECT CASE WHEN t.value LIKE 'http%' THEN t.value
           ELSE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/' || t.value END
    FROM unnest(thematic_area) WITH ORDINALITY AS t(value, ord)
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(thematic_area) AS v WHERE v NOT LIKE 'http%');

-- 6. The export reads themes from the resource now, so catalog record themes move there, resolved
--    through the vocabulary their row names, whichever it is: a catalog record could already name any
--    configured vocabulary as its taxonomy, so restricting this to the standard ones would lose data.
--    An entry carrying no EXT_RESOURCE URI cannot move and is left alone; only the i14y and EU
--    vocabularies are ours to annotate. dcat_catalog_theme is untouched, so nothing is lost either way.


WITH theme_uri AS (
    SELECT config.vocabulary_identifier, entry.code, annotation.uri
    FROM data.vocabulary_config config
    JOIN data.iop_concepts concept
      ON concept.identifiers @> ARRAY[config.concept_identifier]
     AND concept.version = config.concept_version
    JOIN data.code_list_entries entry ON entry.iop_concept_id = concept.id
    JOIN data.annotations annotation
      ON annotation.code_list_entry_id = entry.id
     AND annotation.type = 'EXT_RESOURCE'
     AND annotation.uri IS NOT NULL AND annotation.uri <> ''
),
inherited AS (
    SELECT resource.resource_id, array_agg(DISTINCT theme_uri.uri) AS uris
    FROM data.dcat_catalog_theme catalog_theme
    JOIN data.dcat_catalog_resource resource
      ON resource.dcat_catalog_record_id = catalog_theme.dcat_catalog_record_id
    JOIN theme_uri
      ON theme_uri.vocabulary_identifier = catalog_theme.theme_taxonomy
     AND theme_uri.code = catalog_theme.code
    WHERE resource.resource_type = 'Dataset'
    GROUP BY resource.resource_id
)
UPDATE data.dataset target
SET theme = target.theme || ARRAY(
    SELECT uri FROM unnest(inherited.uris) AS uri
    WHERE NOT (uri = ANY(target.theme)))
FROM inherited
WHERE target.id = inherited.resource_id;

WITH theme_uri AS (
    SELECT config.vocabulary_identifier, entry.code, annotation.uri
    FROM data.vocabulary_config config
    JOIN data.iop_concepts concept
      ON concept.identifiers @> ARRAY[config.concept_identifier]
     AND concept.version = config.concept_version
    JOIN data.code_list_entries entry ON entry.iop_concept_id = concept.id
    JOIN data.annotations annotation
      ON annotation.code_list_entry_id = entry.id
     AND annotation.type = 'EXT_RESOURCE'
     AND annotation.uri IS NOT NULL AND annotation.uri <> ''
),
inherited AS (
    SELECT resource.resource_id, array_agg(DISTINCT theme_uri.uri) AS uris
    FROM data.dcat_catalog_theme catalog_theme
    JOIN data.dcat_catalog_resource resource
      ON resource.dcat_catalog_record_id = catalog_theme.dcat_catalog_record_id
    JOIN theme_uri
      ON theme_uri.vocabulary_identifier = catalog_theme.theme_taxonomy
     AND theme_uri.code = catalog_theme.code
    WHERE resource.resource_type = 'DataService'
    GROUP BY resource.resource_id
)
UPDATE data.data_service target
SET theme = target.theme || ARRAY(
    SELECT uri FROM unnest(inherited.uris) AS uri
    WHERE NOT (uri = ANY(target.theme)))
FROM inherited
WHERE target.id = inherited.resource_id;

COMMIT;
