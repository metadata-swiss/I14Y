-- Adds the EU theme matching each stored i14y theme (mapping_themes_i14y_eu, exact and close
-- matches). Existing themes are kept; an EU theme already present is not added twice.
--
-- Run this AFTER 20260917_seed_theme_taxonomy_registry.sql, which stores every i14y theme as its
-- canonical URI and moves catalog record themes onto the resources. Then run a full reindex.
--
-- Running it twice changes nothing. It has no rollback: an added EU theme cannot be told apart
-- from one entered by hand, so take a backup first.
--
--   psql -U <user> -d <database> -v ON_ERROR_STOP=1 -f 20261029_add_eu_themes_from_i14y_mapping.sql

BEGIN;

CREATE TEMP TABLE theme_mapping ON COMMIT DROP AS
SELECT
    'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/' || m.i14y_code  AS i14y_uri,
    'http://publications.europa.eu/resource/authority/data-theme/' || m.eu_code        AS eu_uri
FROM (VALUES
    ('101', 'ECON'),        -- Arbeit
    ('102', 'REGI'),        -- Bauen
    ('103', 'EDUC'),        -- Bildung
    ('104', 'INTR'),        -- Aussenbeziehungen
    ('105', 'JUST'),        -- Gerichtsbarkeit
    ('106', 'SOCI'),        -- Gesellschaft
    ('107', 'GOVE'),        -- Politische Aktivitäten
    ('108', 'EDUC'),        -- Kultur
    ('109', 'AGRI'),        -- Landwirtschaft
    ('110', 'REGI'),        -- Infrastruktur
    ('111', 'JUST'),        -- Sicherheit
    ('112', 'ECON'),        -- Steuern
    ('113', 'ENVI'),        -- Umwelt
    ('114', 'HEAL'),        -- Gesundheit
    ('115', 'ECON'),        -- Wirtschaft
    ('116', 'TRAN'),        -- Mobilität
    ('117', 'SOCI'),        -- Einwohner
    ('118', 'ECON'),        -- Unternehmen
    ('119', 'GOVE'),        -- Behörden
    ('120', 'REGI'),        -- Gebäude und Grundstücke
    ('121', 'AGRI'),        -- Tiere
    ('122', 'REGI'),        -- Geoinformationen
    ('123', 'JUST'),        -- Rechtssammlung
    ('124', 'ENER'),        -- Energie
    ('125', 'GOVE'),        -- Öffentliche Statistik
    ('126', 'SOCI')         -- Soziale Sicherheit
) AS m(i14y_code, eu_code);

-- Each statement appends the missing EU themes, in the order of the i14y themes they come from.
-- Sectors and thematic areas draw on the same vocabulary, so they get the same treatment.

UPDATE data.dataset
SET theme = theme || ARRAY(
    SELECT m.eu_uri
    FROM unnest(theme) WITH ORDINALITY AS t(value, ord)
    JOIN theme_mapping m ON m.i14y_uri = t.value
    WHERE NOT (m.eu_uri = ANY(theme))
    GROUP BY m.eu_uri
    ORDER BY min(t.ord))
WHERE EXISTS (
    SELECT 1 FROM unnest(theme) AS v JOIN theme_mapping m ON m.i14y_uri = v
    WHERE NOT (m.eu_uri = ANY(theme)));

UPDATE data.data_service
SET theme = theme || ARRAY(
    SELECT m.eu_uri
    FROM unnest(theme) WITH ORDINALITY AS t(value, ord)
    JOIN theme_mapping m ON m.i14y_uri = t.value
    WHERE NOT (m.eu_uri = ANY(theme))
    GROUP BY m.eu_uri
    ORDER BY min(t.ord))
WHERE EXISTS (
    SELECT 1 FROM unnest(theme) AS v JOIN theme_mapping m ON m.i14y_uri = v
    WHERE NOT (m.eu_uri = ANY(theme)));

UPDATE data.mapping_tables
SET themes = themes || ARRAY(
    SELECT m.eu_uri
    FROM unnest(themes) WITH ORDINALITY AS t(value, ord)
    JOIN theme_mapping m ON m.i14y_uri = t.value
    WHERE NOT (m.eu_uri = ANY(themes))
    GROUP BY m.eu_uri
    ORDER BY min(t.ord))
WHERE EXISTS (
    SELECT 1 FROM unnest(themes) AS v JOIN theme_mapping m ON m.i14y_uri = v
    WHERE NOT (m.eu_uri = ANY(themes)));

UPDATE data.iop_concepts
SET themes = themes || ARRAY(
    SELECT m.eu_uri
    FROM unnest(themes) WITH ORDINALITY AS t(value, ord)
    JOIN theme_mapping m ON m.i14y_uri = t.value
    WHERE NOT (m.eu_uri = ANY(themes))
    GROUP BY m.eu_uri
    ORDER BY min(t.ord))
WHERE EXISTS (
    SELECT 1 FROM unnest(themes) AS v JOIN theme_mapping m ON m.i14y_uri = v
    WHERE NOT (m.eu_uri = ANY(themes)));

UPDATE data.public_services
SET sector = sector || ARRAY(
    SELECT m.eu_uri
    FROM unnest(sector) WITH ORDINALITY AS t(value, ord)
    JOIN theme_mapping m ON m.i14y_uri = t.value
    WHERE NOT (m.eu_uri = ANY(sector))
    GROUP BY m.eu_uri
    ORDER BY min(t.ord))
WHERE EXISTS (
    SELECT 1 FROM unnest(sector) AS v JOIN theme_mapping m ON m.i14y_uri = v
    WHERE NOT (m.eu_uri = ANY(sector)));

UPDATE data.public_services
SET thematic_area = thematic_area || ARRAY(
    SELECT m.eu_uri
    FROM unnest(thematic_area) WITH ORDINALITY AS t(value, ord)
    JOIN theme_mapping m ON m.i14y_uri = t.value
    WHERE NOT (m.eu_uri = ANY(thematic_area))
    GROUP BY m.eu_uri
    ORDER BY min(t.ord))
WHERE EXISTS (
    SELECT 1 FROM unnest(thematic_area) AS v JOIN theme_mapping m ON m.i14y_uri = v
    WHERE NOT (m.eu_uri = ANY(thematic_area)));

-- i14y themes the mapping does not cover; expected to be empty.
SELECT value AS unmapped_i14y_theme, count(*) AS occurrences
FROM (
    SELECT unnest(theme) FROM data.dataset
    UNION ALL SELECT unnest(theme) FROM data.data_service
    UNION ALL SELECT unnest(themes) FROM data.mapping_tables
    UNION ALL SELECT unnest(themes) FROM data.iop_concepts
    UNION ALL SELECT unnest(sector) FROM data.public_services
    UNION ALL SELECT unnest(thematic_area) FROM data.public_services
) AS stored(value)
WHERE value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
  AND value NOT IN (SELECT i14y_uri FROM theme_mapping)
GROUP BY value;

COMMIT;
