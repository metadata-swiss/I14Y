import React from 'react'
import Select from 'react-select'
import { reactSelectStyles } from 'decap-cms-ui-default'
import PiveauSearchComponent from './PiveauSearchComponent.jsx'
import { Map, List, fromJS } from 'immutable'
import find from 'lodash/find'
import onetime from 'onetime'

function optionToString(option) {
  return option && (typeof option.value === 'number' || typeof option.value === 'string')
    ? option.value
    : null
}

function convertToOption(raw) {
  if (typeof raw === 'string') {
    return { label: raw, value: raw }
  }
  return Map.isMap(raw) ? raw.toJS() : raw
}

function getSelectedValue({ value, options, isMultiple }) {
  if (isMultiple) {
    const selectedOptions = List.isList(value) ? value.toJS() : value

    if (!selectedOptions || !Array.isArray(selectedOptions)) {
      return null
    }

    return selectedOptions
      .map(i => options.find(o => o.value === (i.value || i)))
      .filter(Boolean)
      .map(convertToOption)
  }
  else {
    return find(options, ['value', value]) || null
  }
}

export default class VocabularySelectComponent extends PiveauSearchComponent {
  fetchOptions = onetime(async (url) => {
    const res = await fetch(url)
    const { result: { results } } = await res.json()

    return results.map(({ pref_label, resource }) => ({ label: pref_label.de, value: resource }))
  })

  state = {
    options: [],
  }

  get filter() {
    return 'vocabulary'
  }

  prepareSearchUrl(q) {
    const url = super.prepareSearchUrl(q)
    url.searchParams.set('vocabulary', this.props.field.get('piveau').get('vocabulary'))
    return url
  }

  componentDidMount() {
    const vocabulary = this.props.field.get('piveau').get('vocabulary')

    this.setState({ loading: true })
    this.fetchOptions(this.prepareSearchUrl({ vocabulary })).then((options) => {
      this.setState({ options, loading: false })
    })
  }

  handleChange = (selectedOption) => {
    const { onChange, field } = this.props
    const isMultiple = field.get('multiple', false)
    const isEmpty = isMultiple ? !selectedOption?.length : !selectedOption

    if (field.get('required') && isEmpty && isMultiple) {
      onChange(List())
    }
    else if (isEmpty) {
      onChange(null)
    }
    else if (isMultiple) {
      const options = selectedOption.map(optionToString)
      onChange(fromJS(options))
    }
    else {
      onChange(optionToString(selectedOption))
    }
  }

  render() {
    const { field, value, forID, classNameWrapper, setActiveStyle, setInactiveStyle } = this.props
    const isMultiple = field.get('multiple', false)
    const isClearable = !field.get('required', true) || isMultiple

    const { options } = this.state
    const selectedValue = getSelectedValue({
      options,
      value,
      isMultiple,
    })

    return (
      <Select
        inputId={forID}
        value={selectedValue}
        onChange={this.handleChange}
        className={classNameWrapper}
        onFocus={setActiveStyle}
        onBlur={setInactiveStyle}
        options={options}
        styles={reactSelectStyles}
        isMulti={isMultiple}
        isClearable={isClearable}
        isLoading={this.state.loading}
        placeholder=""
      />
    )
  }
}
