import React from 'react'
import PropTypes from 'prop-types'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'react-bootstrap-typeahead/css/Typeahead.css'
import 'react-bootstrap-typeahead/css/Typeahead.bs5.css'

export default class PiveauSearchComponent extends React.Component {
  static propTypes = {
    field: PropTypes.object.isRequired,
    onChange: PropTypes.func.isRequired,
  }

  state = {
    isLoading: false,
  }

  prepareSearchUrl(searchParams = {}) {
    const url = new URL('search', this.props.field.get('piveau').get('search'))
    url.searchParams.set('filter', this.filter)
    for (const [key, value] of Object.entries(searchParams)) {
      url.searchParams.set(key, value)
    }
    return url
  }

  changed = (items) => {
    this.props.onChange(items)
  }
}
