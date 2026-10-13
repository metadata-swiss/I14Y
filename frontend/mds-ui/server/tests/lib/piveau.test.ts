import { expect } from 'chai'
import { HubSearch } from '../../lib/piveau'
import sinon from 'sinon'

describe('HubSearch', () => {
  describe('datasets.search', () => {
    describe('all', () => {
      it('should fetch datasets and return results', async () => {
        // given
        const fetchStub = sinon.stub().resolves({
          ok: true,
          json: async () => ({
            result: {
              results: [{ id: 'ds-1', title: 'Dataset 1' }, { id: 'ds-2', title: 'Dataset 2' }],
            },
          }),
        })
        const hubSearch = new HubSearch('https://example.com', fetchStub)

        // when
        const result = await hubSearch.datasets.search().all()

        // then
        expect(result).to.deep.equal([{ id: 'ds-1', title: 'Dataset 1' }, { id: 'ds-2', title: 'Dataset 2' }])
        expect(fetchStub).to.have.been.calledWith(sinon.match((url: URL) => {
          return url.pathname === '/search'
            && url.searchParams.get('filters') === 'dataset'
        }))
      })
    })

    describe('scroll', () => {
      it('should fetch datasets in pages and return results', async () => {
        // given
        const fetchStub = sinon.stub()
        fetchStub.onFirstCall().resolves({
          ok: true,
          json: async () => ({
            result: {
              scrollId: 'scroll-123',
              results: [{ id: 'ds-1', title: 'Dataset 1' }, { id: 'ds-2', title: 'Dataset 2' }],
            },
          }),
        })
        fetchStub.onSecondCall().resolves({
          ok: true,
          json: async () => ({
            result: {
              results: [{ id: 'ds-3', title: 'Dataset 3' }],
            },
          }),
        })
        fetchStub.onThirdCall().resolves({
          ok: false,
        })
        const hubSearch = new HubSearch('https://example.com', fetchStub)

        // when
        const results = []
        for await (const page of hubSearch.datasets.search().scroll()) {
          results.push(...page)
        }

        // then
        expect(results).to.deep.equal([
          { id: 'ds-1', title: 'Dataset 1' },
          { id: 'ds-2', title: 'Dataset 2' },
          { id: 'ds-3', title: 'Dataset 3' },
        ])
        expect(fetchStub.firstCall).to.have.been.calledWith(sinon.match((url: URL) => {
          return url.pathname === '/search'
            && url.searchParams.get('filters') === 'dataset'
            && url.searchParams.get('scroll') === 'true'
        }))
        expect(fetchStub.secondCall).to.have.been.calledWith(sinon.match((url: URL) => {
          return url.pathname === '/scroll'
            && url.searchParams.get('scrollId') === 'scroll-123'
        }))
        expect(fetchStub.thirdCall).to.have.been.calledWith(sinon.match((url: URL) => {
          return url.pathname === '/scroll'
            && url.searchParams.get('scrollId') === 'scroll-123'
        }))
      })

      it('should fetch datasets in pages and return results (last response empty array)', async () => {
        // given
        const fetchStub = sinon.stub()
        fetchStub.onFirstCall().resolves({
          ok: true,
          json: async () => ({
            result: {
              scrollId: 'scroll-123',
              results: [{ id: 'ds-1', title: 'Dataset 1' }],
            },
          }),
        })
        fetchStub.onSecondCall().resolves({
          ok: true,
          json: async () => ({
            result: {
              scrollId: 'scroll-123',
              results: [{ id: 'ds-2', title: 'Dataset 2' }],
            },
          }),
        })
        fetchStub.onThirdCall().resolves({
          ok: true,
          json: async () => ({
            result: {
              results: [],
            },
          }),
        })
        const hubSearch = new HubSearch('https://example.com', fetchStub)

        // when
        const results = []
        for await (const page of hubSearch.datasets.search().scroll()) {
          results.push(...page)
        }

        // then
        expect(results).to.deep.equal([
          { id: 'ds-1', title: 'Dataset 1' },
          { id: 'ds-2', title: 'Dataset 2' },
        ])
        expect(fetchStub.firstCall).to.have.been.calledWith(sinon.match((url: URL) => {
          return url.pathname === '/search'
            && url.searchParams.get('filters') === 'dataset'
            && url.searchParams.get('scroll') === 'true'
        }))
        expect(fetchStub.secondCall).to.have.been.calledWith(sinon.match((url: URL) => {
          return url.pathname === '/scroll'
            && url.searchParams.get('scrollId') === 'scroll-123'
        }))
        expect(fetchStub.thirdCall).to.have.been.calledWith(sinon.match((url: URL) => {
          return url.pathname === '/scroll'
            && url.searchParams.get('scrollId') === 'scroll-123'
        }))
      })
    })
  })
})
