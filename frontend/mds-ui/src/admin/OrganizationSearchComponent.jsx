import PiveauSearchComponent from './PiveauSearchComponent.jsx'
import { AsyncTypeahead } from 'react-bootstrap-typeahead'
import { toArray } from './util.js'
import React from 'react'

// Bypass client-side filtering by returning `true`. Results are already
// filtered by the search endpoint, so no need to do it again.
const filterBy = () => true

export default class OrganizationSearchComponent extends PiveauSearchComponent {
  state = {
    options: toArray(this.props.value),
  }

  get filter() {
    return 'organization'
  }

  get labelProp() {
    return 'name'
  }

  componentDidUpdate(prevProps) {
    if (prevProps.value !== this.props.value) {
      this.setState({ options: toArray(this.props.value) })
    }
  }

  handleSearch = async (q) => {
    this.setState({
      isLoading: true,
    })

    try {
      const response = await fetch(this.prepareSearchUrl({ q }))

      const { result } = await response.json()

      const options = result.results.map((item) => {
        const { id } = item
        return {
          id,
          label: item[this.labelProp][this.props.locale || 'en'] || id,
        }
      })

      this.setState({ options })
    }
    finally {
      this.setState({
        isLoading: false,
      })
    }
  }

  render() {
    return (
      <AsyncTypeahead
        id={this.props.field.get('name')}
        className={this.props.classNameWrapper}
        filterBy={filterBy}
        minLength={this.props.minLength || 3}
        isLoading={this.state.isLoading}
        labelKey="label"
        multiple
        onSearch={this.handleSearch}
        placeholder="Type to search..."
        selected={toArray(this.props.value)}
        options={this.state.options}
        onChange={this.changed}
      />
    )
  }
}
