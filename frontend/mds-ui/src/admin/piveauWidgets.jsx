import CMS from 'decap-cms-app'
import DatasetSearchComponent from './DatasetSearchComponent'
import DatasetPreviewComponent from './DatasetPreviewComponent'
import VocabularySelectComponent from './VocabularySelectComponent.jsx'
import ShowcaseSearchComponent from './ShowcaseSearchComponent.jsx'
import OrganizationSearchComponent from './OrganizationSearchComponent.jsx'

CMS.registerWidget('piveau-dataset', DatasetSearchComponent, DatasetPreviewComponent, {
  properties: {
    piveau: {
      type: 'object',
      properties: {
        search: {
          type: 'string',
        },
      },
    },
  },
})

CMS.registerWidget('piveau-showcase', ShowcaseSearchComponent, {
  properties: {
    piveau: {
      type: 'object',
      properties: {
        search: {
          type: 'string',
        },
      },
    },
  },
})

CMS.registerWidget('piveau-organization', OrganizationSearchComponent, {
  properties: {
    piveau: {
      type: 'object',
      properties: {
        search: {
          type: 'string',
        },
      },
    },
  },
})

CMS.registerWidget('piveau-vocabulary', VocabularySelectComponent, DatasetPreviewComponent, {
  properties: {
    piveau: {
      type: 'object',
      properties: {
        search: {
          type: 'string',
        },
        vocabulary: {
          type: 'string',
        },
      },
    },
  },
})
