-- Reverts 20260917_seed_theme_taxonomy_registry.sql: Swiss theme URIs go back to bare codes.
--
-- The registry, its vocabulary config and the EXT_RESOURCE annotations stay. The forward script
-- skips whatever already existed, so it does not record what it created, and deleting all of them
-- would destroy rows it never touched. The previous application version reads neither.
--
-- Every other URI is removed: the previous version only knows Swiss codes, and those URIs came from
-- catalog records (dcat_catalog_theme still holds the originals) or from the EU theme mapping.
-- Swiss themes moved over from catalog records stay, as nothing tells them apart.
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
    WHERE t.value NOT LIKE 'http%'
       OR t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(theme) AS v WHERE v LIKE 'http%');

UPDATE data.data_service
SET theme = ARRAY(
    SELECT CASE
        WHEN t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
        THEN substring(t.value from length('https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/') + 1)
        ELSE t.value END
    FROM unnest(theme) WITH ORDINALITY AS t(value, ord)
    WHERE t.value NOT LIKE 'http%'
       OR t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(theme) AS v WHERE v LIKE 'http%');

UPDATE data.mapping_tables
SET themes = ARRAY(
    SELECT CASE
        WHEN t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
        THEN substring(t.value from length('https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/') + 1)
        ELSE t.value END
    FROM unnest(themes) WITH ORDINALITY AS t(value, ord)
    WHERE t.value NOT LIKE 'http%'
       OR t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(themes) AS v WHERE v LIKE 'http%');

UPDATE data.iop_concepts
SET themes = ARRAY(
    SELECT CASE
        WHEN t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
        THEN substring(t.value from length('https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/') + 1)
        ELSE t.value END
    FROM unnest(themes) WITH ORDINALITY AS t(value, ord)
    WHERE t.value NOT LIKE 'http%'
       OR t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(themes) AS v WHERE v LIKE 'http%');

UPDATE data.public_services
SET sector = ARRAY(
    SELECT CASE
        WHEN t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
        THEN substring(t.value from length('https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/') + 1)
        ELSE t.value END
    FROM unnest(sector) WITH ORDINALITY AS t(value, ord)
    WHERE t.value NOT LIKE 'http%'
       OR t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(sector) AS v WHERE v LIKE 'http%');

UPDATE data.public_services
SET thematic_area = ARRAY(
    SELECT CASE
        WHEN t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
        THEN substring(t.value from length('https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/') + 1)
        ELSE t.value END
    FROM unnest(thematic_area) WITH ORDINALITY AS t(value, ord)
    WHERE t.value NOT LIKE 'http%'
       OR t.value LIKE 'https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/%'
    ORDER BY t.ord)
WHERE EXISTS (SELECT 1 FROM unnest(thematic_area) AS v WHERE v LIKE 'http%');

COMMIT;
