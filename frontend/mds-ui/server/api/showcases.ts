import { remark } from 'remark'
import strip from 'strip-markdown'
import remarkFrontmatter from 'remark-frontmatter'
import { dcat, dcterms, rdfs, schema, xsd, prov, foaf } from '@tpluscode/rdf-ns-builders'
import type { ShowcasesCollectionItem } from '@nuxt/content'
import { execSync } from 'node:child_process'
import { join } from 'node:path'
import { queryPublishedContent } from '~~/app/composables/content'
import $rdf from '@zazuko/env-node'

const stemPattern = /showcases\/(?<stem>.*)\.(?<lang>\w\w)$/

interface AggregateShowcase {
  'id': string
  'identifier': string
  '@type': string[]
  'type': string
  'title': Record<string, string | undefined>
  'images': string[]
  'abstract': Record<string, string | undefined>
  'themes': string[]
  'datasets': Array<{ identifier: string, label: string }>
  'text': Record<string, string | undefined>
  'keywords': string[]
  'modified': string | undefined
  'issued': string | undefined
  'pinned': boolean
  'qualifiedAttribution': Array<{
    '@type': 'prov:Attribution'
    'prov:agent': string | {
      '@type': 'foaf:Person' | 'foaf:Organization'
      'foaf:name': string
      'foaf:homepage'?: string[]
    }
    'dcat:hadRole': string
  }>
}

type ImplicitTypeCoercion = { '@type': '@id' }
type TypeCoercion = { '@id': string, '@type': string }
type Container = { '@id': string, '@container': string }
type Context = Record<string, string | ImplicitTypeCoercion | TypeCoercion | Container>

const ldContext: Context = {
  'id': '@id',
  'dcat': dcat().value,
  'prov': prov().value,
  'label': rdfs.label.value,
  'foaf': foaf().value,
  'foaf:homepage': {
    '@type': '@id',
  },
  'type': {
    '@id': dcterms.type.value,
    '@type': '@id',
  },
  'themes': {
    '@id': dcat.theme.value,
    '@type': '@id',
  },
  'datasets': {
    '@id': dcterms.references.value,
    '@type': '@id',
  },
  'title': {
    '@id': dcterms.title.value,
    '@container': '@language',
  },
  'abstract': {
    '@id': dcterms.abstract.value,
    '@container': '@language',
  },
  'text': {
    '@id': schema.text.value,
    '@container': '@language',
  },
  'pinned': {
    '@id': 'piveau:pinned',
    '@type': xsd.boolean.value,
  },
  'dcat:hadRole': {
    '@id': 'dcat:hadRole',
    '@type': '@id',
  },
  'prov:agent': {
    '@id': 'prov:agent',
    '@type': '@id',
  },
  'identifier': dcterms.identifier.value,
  'images': {
    '@id': schema.image.value,
    '@type': '@id',
  },
  'keywords': dcat.keyword.value,
  'modified': dcterms.modified.value,
  'issued': dcterms.issued.value,
  'Dataset': dcat.Dataset.value,
  'piveau': 'https://piveau.eu/ns/voc#',
  'qualifiedAttribution': prov.qualifiedAttribution.value,
}

export default defineEventHandler(async (event) => {
  const { public: { rootDir } } = useRuntimeConfig(event)
  const showcases = await queryPublishedContent(event, 'showcases')
    .select('title', 'themes', 'datasets', 'description', 'rawbody', 'stem', 'images', 'keywords', 'type', 'pinned', 'relationships')
    .where('active', '=', true)
    .all()

  const aggregatedShowcases = showcases.reduce(async (promise, showcase) => {
    const arr = await promise

    const { stem, lang } = showcase.stem.match(stemPattern)?.groups || {}
    if (!stem || !lang) {
      console.warn(`${showcase.stem} did not match stem pattern`)
      return arr
    }

    const id = `showcase/${stem}`
    let aggregate = arr.find(agg => agg.id === id)
    if (!aggregate) {
      const { modified, issued } = getShowcaseDates(rootDir, stem)
      aggregate = {
        id,
        'identifier': stem,
        '@type': ['Showcase', 'Dataset', 'piveau:CustomResource'],
        'type': showcase.type,
        'title': {},
        'images': showcase.images.map(image => image.image) || [],
        'abstract': {},
        'themes': showcase.themes || [],
        'datasets': mapDatasets(showcase.datasets) || [],
        'text': {},
        'keywords': showcase.keywords || [],
        'pinned': showcase.pinned || false,
        modified,
        issued,
        'qualifiedAttribution': [],
      }
      arr.push(aggregate)
    }

    aggregate.title[lang] = showcase.title || ''
    aggregate.abstract[lang] = showcase.description || ''
    aggregate.text[lang] = await stripMarkdown(showcase.rawbody) || ''
    if (lang === 'de') {
      aggregate.qualifiedAttribution = showcase.relationships
        ?.map(toAttribution)
        .filter(qa => !!qa) || []
    }

    return arr
  }, Promise.resolve<AggregateShowcase[]>([]))

  return event.respondWith(new Response(JSON.stringify({
    '@context': [
      { '@base': 'http://localhost:3000/' },
      ldContext,
    ],
    '@graph': await aggregatedShowcases,
  }), {
    headers: { 'Content-Type': 'application/ld+json' },
  }))
})

function mapDatasets(datasets: ShowcasesCollectionItem['datasets'] | undefined) {
  return datasets?.map(ds => ({
    identifier: ds.id,
    label: ds.label,
  })) || []
}

async function stripMarkdown(md: string | undefined) {
  if (!md) {
    return ''
  }

  const stripped = await remark()
    .use(strip)
    .use(remarkFrontmatter)
    .process(md)
  return stripped.value.toString()
}

function getShowcaseDates(rootDir: string, stem: string) {
  try {
    const showcasesDir = join(rootDir, 'content/showcases')
    const files = [
      `${stem}.de.md`,
      `${stem}.en.md`,
      `${stem}.fr.md`,
      `${stem}.it.md`,
    ].join(' ')

    // Get all commit dates for the showcase files, sorted from newest to oldest
    // Note: Accurate dates require a repository clone with sufficient history (avoid --depth 1)
    const command = `git log --format=%aI -- ${files}`
    const result = execSync(command, { cwd: showcasesDir, encoding: 'utf-8' })

    const dates = result.trim().split('\n').filter(Boolean)

    return {
      modified: dates[0],
      issued: dates.at(-1),
    }
  }
  catch (e) {
    console.error(`Failed to get git dates for ${stem}`, e)
    return {
      modified: undefined,
      issued: undefined,
    }
  }
}

const ResponsiblePartyRole = $rdf.namespace('http://inspire.ec.europa.eu/metadata-codelist/ResponsiblePartyRole/')
const Organization = $rdf.namespace('https://opendata.swiss/id/organization/')

function toAttribution(relationship: Required<ShowcasesCollectionItem>['relationships'][number]): AggregateShowcase['qualifiedAttribution'][number] | undefined {
  const attribution: Omit<AggregateShowcase['qualifiedAttribution'][number], 'prov:agent'> = {
    '@type': 'prov:Attribution',
    'dcat:hadRole': ResponsiblePartyRole(relationship.role).value,
  }

  switch (relationship.type) {
    case 'organization':
      return {
        ...attribution,
        'prov:agent': Organization(relationship.organization[0]!.id).value,
      }
    case 'person':
      return {
        ...attribution,
        'prov:agent': {
          '@type': 'foaf:Person',
          'foaf:name': relationship.name,
        },
      }
    case 'organization-external':
      return {
        ...attribution,
        'prov:agent': {
          '@type': 'foaf:Organization',
          'foaf:name': relationship.name,
          'foaf:homepage': relationship.url || [],
        },
      }
    default:
      return undefined
  }
}
