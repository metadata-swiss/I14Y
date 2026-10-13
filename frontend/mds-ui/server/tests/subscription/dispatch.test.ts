import { expect } from 'chai'
import sinon from 'sinon'
import { dispatchDigest, dispatchDatasetDigest, matchPreferences } from '../../lib/subscription/dispatch'
import type { Dataset, HubSearch } from '../../lib/piveau'
import type Listmonk from '../../lib/listmonk'
import type { Subscriber } from '../../lib/listmonk'

describe('subscription/dispatch', () => {
  describe('dispatchDatasetDigest', () => {
    it('filters datasets by subscriber preferences and sends digests', async () => {
      // given
      const datasets = [
        { id: 'a', title: { de: 'Datensatz A', fr: '', it: '', en: '' }, categories: [{ id: 'cat-1' }] },
        { id: 'b', title: { de: 'Datensatz B', fr: '', it: '', en: '' }, categories: [{ id: 'cat-2' }] },
        { id: 'c', title: { de: 'Datensatz C', fr: '', it: '', en: '' }, categories: [{ id: 'cat-1' }] },
      ]
      const subscribers: Pick<Subscriber, 'id' | 'attribs'>[] = [
        { id: 1, attribs: { language: 'de', categories: ['cat-1'] } },
        { id: 2, attribs: { language: 'de', datasets: ['b'] } },
        { id: 3, attribs: { language: 'de' } }, // no prefs
      ]

      const sendDigest = sinon.stub().resolves({ ok: true })

      // when
      const { emailsSent, emailsFailed } = await dispatchDatasetDigest(datasets, subscribers, sendDigest)

      // then
      expect(emailsFailed).to.equal(0)
      expect(emailsSent).to.equal(2)
      // subscriber 1 should receive 2 datasets (cat-1)
      expect(sendDigest.firstCall.args[0]).to.deep.include({ subscriber: 1, language: 'de' })
      expect(sendDigest.firstCall.args[0].datasets).to.have.length(2)
      // subscriber 2 should receive 1 dataset (id b)
      expect(sendDigest.secondCall.args[0]).to.deep.include({ subscriber: 2, language: 'de' })
      expect(sendDigest.secondCall.args[0].datasets).to.have.length(1)
    })

    it('limits datasets per email using maxDatasetsPerEmail', async () => {
      // given
      const datasets = [
        { id: 'a', title: { de: 'A', fr: '', it: '', en: '' }, categories: [{ id: 'cat-1' }] },
        { id: 'b', title: { de: 'B', fr: '', it: '', en: '' }, categories: [{ id: 'cat-1' }] },
        { id: 'c', title: { de: 'C', fr: '', it: '', en: '' }, categories: [{ id: 'cat-1' }] },
      ]
      const subscribers: Pick<Subscriber, 'id' | 'attribs'>[] = [
        { id: 1, attribs: { language: 'de', categories: ['cat-1'] } },
      ]

      const sendDigest = sinon.stub().resolves({ ok: true })
      const maxDatasetsPerEmail = 2

      // when
      await dispatchDatasetDigest(datasets, subscribers, sendDigest, maxDatasetsPerEmail)

      // then
      expect(sendDigest).to.have.been.calledOnce
      const payload = sendDigest.firstCall.args[0]
      expect(payload).to.deep.include({ subscriber: 1, language: 'de' })
      expect(payload.datasets).to.have.length(2)
      // ensure the two first datasets are taken
      expect(payload.datasets.map((d: { id: string }) => d.id)).to.deep.equal(['a', 'b'])
    })

    it('counts failures when transactional send fails', async () => {
      // given
      const datasets = [{ id: 'x', title: { de: 'X', fr: '', it: '', en: '' } }]
      const subscribers: Pick<Subscriber, 'id' | 'attribs'>[] = [
        { id: 1, attribs: { datasets: ['x'], language: 'de' } },
      ]

      const sendDigest = sinon.stub().resolves({ ok: false, text: async () => 'fail' })

      // when
      const res = await dispatchDatasetDigest(datasets, subscribers, sendDigest)

      // then
      expect(res).to.deep.equal({ emailsSent: 0, emailsFailed: 1 })
    })
  })

  describe('dispatchDigest', () => {
    it('throws when dataset search fails', async () => {
      // given
      const piveau = {
        datasets: { search: sinon.stub().resolves(Object.assign(new Error('boom'), { cause: new Error('x') })) },
      } as unknown as HubSearch
      const listmonk = {
        subscribers: { list: sinon.stub() },
      } as unknown as Listmonk

      // when / then
      await expect(dispatchDigest('daily', { piveau, listmonk, appUrl: 'https://ex', key: '' }))
        .to.have.been.rejected
    })
  })

  describe('matchPreferences', () => {
    it('matches by category', () => {
      const subscriber = {
        id: 1,
        attribs: { categories: ['a'], datasets: ['ds-2'] },
      } as Subscriber

      const match = matchPreferences(subscriber)
      expect(match({ id: 'ds-1', categories: [{ id: 'a' }] } as Dataset)).to.equal(true)
    })

    it('prioritizes dataset ids over categories', () => {
      const subscriber = {
        id: 1,
        attribs: { categories: ['a'], datasets: ['ds-2'] },
      } as Subscriber

      const match = matchPreferences(subscriber)
      expect(match({ id: 'ds-2', categories: [{ id: 'b' }] } as Dataset)).to.equal(true)
    })

    it('returns false when categories do not match and no dataset ids are set', () => {
      const subscriber = {
        id: 3,
        attribs: { categories: ['x'] },
      } as Subscriber

      const match = matchPreferences(subscriber)
      expect(match({ id: 'ds-9', categories: [{ id: 'y' }] } as Dataset)).to.equal(false)
    })

    it('returns false when subscriber has no preferences', () => {
      const subscriber = {
        id: 4,
        attribs: {},
      } as Subscriber

      const match = matchPreferences(subscriber)
      expect(match({ id: 'ds-9', categories: [{ id: 'y' }] } as Dataset)).to.equal(false)
    })
  })
})
