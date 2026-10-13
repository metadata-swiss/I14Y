-- Reverts 20260917_seed_theme_taxonomy_registry.sql: Swiss theme URIs go back to bare codes.
--
-- The registry, its vocabulary config and the EXT_RESOURCE annotations stay. The forward script
-- skips whatever already existed, so it does not record what it created, and deleting all of them
-- would destroy rows it never touched. The previous application version reads neither.
--
-- Themes moved over from catalog records also stay: nothing tells them apart from themes the
-- resource already carried, and dcat_catalog_theme still holds the originals.
--
--   psql -U <user> -d <database> -v ON_ERROR_STOP=1 -f 20260917_seed_theme_taxonomy_registry_rollback.sql

BEGIN;

UPDATE data.dataset
SET theme = ARRAY(
    SELECT CASE
        WHEN t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
        THEN substring(t.value from length('https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/') + 1)
        ELSE t.value END
    FROM unnest(theme) WITH ORDINALITY AS t(value, ord)
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(theme) AS v
              WHERE v LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%');

UPDATE data.data_service
SET theme = ARRAY(
    SELECT CASE
        WHEN t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
        THEN substring(t.value from length('https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/') + 1)
        ELSE t.value END
    FROM unnest(theme) WITH ORDINALITY AS t(value, ord)
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(theme) AS v
              WHERE v LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%');

UPDATE data.mapping_tables
SET themes = ARRAY(
    SELECT CASE
        WHEN t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
        THEN substring(t.value from length('https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/') + 1)
        ELSE t.value END
    FROM unnest(themes) WITH ORDINALITY AS t(value, ord)
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(themes) AS v
              WHERE v LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%');

UPDATE data.iop_concepts
SET themes = ARRAY(
    SELECT CASE
        WHEN t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
        THEN substring(t.value from length('https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/') + 1)
        ELSE t.value END
    FROM unnest(themes) WITH ORDINALITY AS t(value, ord)
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(themes) AS v
              WHERE v LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%');

UPDATE data.public_services
SET sector = ARRAY(
    SELECT CASE
        WHEN t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
        THEN substring(t.value from length('https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/') + 1)
        ELSE t.value END
    FROM unnest(sector) WITH ORDINALITY AS t(value, ord)
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(sector) AS v
              WHERE v LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%');

UPDATE data.public_services
SET thematic_area = ARRAY(
    SELECT CASE
        WHEN t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
        THEN substring(t.value from length('https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/') + 1)
        ELSE t.value END
    FROM unnest(thematic_area) WITH ORDINALITY AS t(value, ord)
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(thematic_area) AS v
              WHERE v LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%');

COMMIT;
