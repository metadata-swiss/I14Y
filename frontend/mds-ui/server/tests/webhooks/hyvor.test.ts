import type { Comment, Page, Rating } from '#server/lib/webhooks/hyvor'
import Hyvor from '../../lib/webhooks/hyvor'
import type { HubRepo, HubSearch } from '../../lib/piveau'
import { ns } from '../../lib/piveau'
import type { ListmonkConfig } from '../../lib/listmonk'
import Listmonk from '../../lib/listmonk'
import sinon from 'sinon'
import { expect } from 'chai'
import waitUntil from 'async-wait-until'
import type { NitroRuntimeConfig } from 'nitropack/types'
import $rdf from '@zazuko/env-node'
import { rdf, schema } from '@tpluscode/rdf-ns-builders'
import type { GraphPointer } from 'clownface'

describe('Hyvor Webhooks', () => {
  describe('handleComment', () => {
    it('sends a notification when a top-level comment is created', async () => {
      // given
      const listmonk = {
        transactional: {
          send: sinon.stub().resolves({ ok: true }),
        },
      } as unknown as Listmonk
      const config = {
        hyvor: {
          publisherNotificationTemplateId: 4,
        },
      } as unknown as NitroRuntimeConfig
      const hubSearch = {
        datasets: {
          get: sinon.stub().resolves({
            id: '123',
            title: {
              de: 'Test Dataset',
            },
            contact_point: [{
              name: 'Publisher',
              email: 'publisher@example.com',
            }],
          }),
        },
      } as unknown as HubSearch
      const hyvor = new Hyvor(config, listmonk, hubSearch, {} as HubRepo)
      const comment: Comment = {
        parent: null,
        user: {
          email: 'commenter@example.com',
        },
        page: {
          identifier: 'dataset-123',
          url: 'https://example.com/dataset/123',
          title: 'Dataset',
          ratings: {
            average: 0,
          },
        },
        body_html: '',
        status: 'published',
        history: [],
      }

      // when
      await hyvor.handleComment(comment)

      // then
      expect(listmonk.transactional.send).to.have.been.calledWith(sinon.match({
        template_id: 4,
        subscriber_email: 'publisher@example.com',
        subscriber_mode: 'external',
        data: {
          page: {
            url: 'https://example.com/dataset/123',
            title: 'Dataset',
          },
          publisher: {
            name: 'Publisher',
          },
          author: {
            email: 'commenter@example.com',
          },
        },
      }))
    })

    describe('using real Listmonk', function () {
      this.timeout(10000)

      before(function () {
        if (!process.env.LISTMONK_ADMIN_API_TOKEN) {
          this.skip()
        }
      })

      beforeEach(async () => {
        await fetch('http://localhost:8025/api/v1/messages', {
          method: 'DELETE',
        })
      })

      it('delivers email to mailpit', async () => {
        // given
        const listmonk = new Listmonk({
          api: {
            token: process.env.LISTMONK_ADMIN_API_TOKEN!,
            url: 'http://localhost:9000/',
            user: 'admin-api',
          },
        } as ListmonkConfig)
        const config = {
          hyvor: {
            publisherNotificationTemplateId: 5,
          },
        } as unknown as NitroRuntimeConfig
        const hubSearch = {
          datasets: {
            get: sinon.stub().resolves({
              id: '123',
              title: {
                de: 'Test Dataset',
              },
              contact_point: [{
                name: 'Publisher',
                email: 'publisher@example.com',
              }],
            }),
          },
        } as unknown as HubSearch
        const hyvor = new Hyvor(config, listmonk, hubSearch, {} as HubRepo)
        const comment: Comment = {
          parent: null,
          user: {
            email: 'commenter@example.com',
          },
          page: {
            identifier: 'dataset-123',
            url: 'https://example.com/dataset/123',
            title: 'Test Dataset',
            ratings: {
              average: 0,
            },
          },
          body_html: '<p>Comment</p>',
          history: [],
          status: 'published',
        }

        // when
        const result = await hyvor.handleComment(comment)

        // then
        expect(result).to.be.undefined
        let data: { total: number, messages: unknown[] } | undefined
        await waitUntil(async () => {
          const mailpitRes = await fetch('http://localhost:8025/api/v1/messages?query=to:publisher@example.com')
          data = await mailpitRes.json()
          return data!.total === 1
        })

        await new Promise(resolve => setTimeout(resolve, 2000))
        expect(data).to.deep.contain({
          total: 1,
        })
        expect(data!.messages[0]).to.have.property('Subject', 'commentNotification')
      })
    })

    it('does not send a notification when comment is a reply', async () => {
      // given
      const listmonk = {
        transactional: {
          send: sinon.spy(),
        },
      } as unknown as Listmonk
      const config = {
        hyvor: {
          publisherNotificationTemplateId: 4,
        },
      } as unknown as NitroRuntimeConfig
      const hyvor = new Hyvor(config, listmonk, {} as HubSearch, {} as HubRepo)
      const comment: Comment = {
        parent: <Comment>{},
        page: <Page>{},
        user: {
          email: 'test@example.com',
        },
        body_html: '',
        status: 'published',
        history: [],
      }

      // when
      await hyvor.handleComment(comment)

      // then
      expect(listmonk.transactional.send).not.to.have.been.called
    })

    it('does not send a notification when created comment is pending', async () => {
      // given
      const listmonk = {
        transactional: {
          send: sinon.stub().resolves({ ok: true }),
        },
      } as unknown as Listmonk
      const config = {
        hyvor: {
          publisherNotificationTemplateId: 4,
        },
      } as unknown as NitroRuntimeConfig
      const hubSearch = {
        datasets: {
          get: sinon.stub().resolves({
            id: '123',
            title: {
              de: 'Test Dataset',
            },
            contact_point: [{
              name: 'Publisher',
              email: 'publisher@example.com',
            }],
          }),
        },
      } as unknown as HubSearch
      const hyvor = new Hyvor(config, listmonk, hubSearch, {} as HubRepo)
      const comment: Comment = {
        parent: null,
        page: {
          identifier: 'dataset-123',
          url: 'https://example.com/dataset/123',
          title: 'Dataset',
          ratings: {
            average: 0,
          },
        },
        user: {
          email: 'test@example.com',
        },
        body_html: '',
        status: 'pending',
        history: [],
      }

      // when
      await hyvor.handleComment(comment)

      // then
      expect(listmonk.transactional.send).not.to.have.been.called
    })

    it('sends a notification when a pending was approved', async () => {
      // given
      const listmonk = {
        transactional: {
          send: sinon.stub().resolves({ ok: true }),
        },
      } as unknown as Listmonk
      const config = {
        hyvor: {
          publisherNotificationTemplateId: 4,
        },
      } as unknown as NitroRuntimeConfig
      const hubSearch = {
        datasets: {
          get: sinon.stub().resolves({
            id: '123',
            title: {
              de: 'Test Dataset',
            },
            contact_point: [{
              name: 'Publisher',
              email: 'publisher@example.com',
            }],
          }),
        },
      } as unknown as HubSearch
      const hyvor = new Hyvor(config, listmonk, hubSearch, {} as HubRepo)
      const comment: Comment = {
        parent: null,
        page: {
          identifier: 'dataset-123',
          url: 'https://example.com/dataset/123',
          title: 'Dataset',
          ratings: {
            average: 0,
          },
        },
        user: {
          email: 'test@example.com',
        },
        body_html: '',
        status: 'published',
        history: [{
          type: 'moderation',
          new_status: 'published',
        }],
      }

      // when
      await hyvor.handleComment(comment)

      // then
      expect(listmonk.transactional.send).not.to.have.been.calledWith(sinon.match({
        template_id: 4,
        subscriber_email: 'publisher@example.com',
        subscriber_mode: 'external',
        data: {
          page: {
            url: 'https://example.com/dataset/123',
            title: 'Dataset',
          },
          publisher: {
            name: 'Publisher',
          },
          author: {
            email: 'commenter@example.com',
          },
        },
      }))
    })

    it('does not send a notification when template id is not configured', async () => {
      // given
      const listmonk = {
        transactional: {
          send: sinon.spy(),
        },
      } as unknown as Listmonk
      const config = {
        hyvor: {
          publisherNotificationTemplateId: 0,
        },
      } as unknown as NitroRuntimeConfig
      const hubSearch = { } as unknown as HubSearch
      const hyvor = new Hyvor(config, listmonk, hubSearch, {} as HubRepo)
      const comment: Comment = {
        parent: null,
        page: <Page>{},
        user: {
          email: 'test@example.com',
        },
        body_html: '',
        status: 'published',
        history: [],
      }

      // when
      await hyvor.handleComment(comment)

      // then
      expect(listmonk.transactional.send).not.to.have.been.called
    })
  })

  describe('handleRating', () => {
    it('returns error when payload does not contain page data', async () => {
      // given
      const hyvor = new Hyvor({} as NitroRuntimeConfig, {} as Listmonk, {} as HubSearch, {} as HubRepo)

      // when
      const err = await hyvor.handleRating({} as Rating)

      // then
      expect(err).to.be.an('error')
    })

    it('returns error when payload page identifier is not in expected format', async () => {
      // given
      const hyvor = new Hyvor({} as NitroRuntimeConfig, {} as Listmonk, {} as HubSearch, {} as HubRepo)

      // when
      const err = await hyvor.handleRating({
        page: {
          url: 'https://example.com/dataset/123',
          title: 'Dataset',
          identifier: 'dataset-sugus',
          ratings: {
            average: 4.5,
          },
        },
      } as unknown as Rating)

      // then
      expect(err).to.be.an('error')
    })

    it('retrieves and updates showcase resource', async () => {
      // given
      const identifier = 'showcase/sugus'
      const catalogId = 'showcases-ods'
      const resourceType = 'showcase'

      const graph = $rdf.clownface()
        .blankNode()
        .addOut(rdf.type, ns('CustomResource'))
        .addOut(schema.ratingValue, 3)

      const hubRepo = {
        getResource: sinon.stub().resolves(graph),
        putResource: sinon.stub().resolves(),
      } as unknown as HubRepo
      const config = {
        showcases: {
          catalogId,
          resourceType,
        },
      } as NitroRuntimeConfig

      const hyvor = new Hyvor(config, {} as Listmonk, {} as HubSearch, hubRepo)

      // when
      const res = await hyvor.handleRating({
        page: {
          url: 'https://example.com/showcase/123',
          title: 'Dataset',
          identifier,
          ratings: {
            average: 4.5,
          },
        },
      } as unknown as Rating)

      // then
      expect(res).to.be.undefined
      expect(hubRepo.getResource).to.have.been.calledWith(sinon.match({
        id: 'sugus',
        catalogId,
        resourceType,
      }))
      expect(hubRepo.putResource).to.have.been.calledWith(sinon.match({
        id: 'sugus',
        catalogId,
        resourceType,
      }), sinon.match((data: GraphPointer) => {
        return data.out(schema.ratingValue).value === '4.5'
      }))
    })
  })
})
