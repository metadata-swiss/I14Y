import React from 'react'
import { toArray } from './util.js'
import PropTypes from 'prop-types'

export default class DatasetPreviewComponent extends React.Component {
  static propTypes = {
    field: PropTypes.object.isRequired,
    value: PropTypes.array,
    locale: PropTypes.string,
  }

  state = {
    hub: '',
    datasets: [],
  }

  async componentDidMount() {
    const hubUrl = this.props.field.get('piveau').get('hub')
    this.setState({
      hub: hubUrl,
    })

    this.setState({
      datasets: toArray(this.props.value),
    })

    // ToDo https://github.com/decaporg/decap-cms/issues/7523
    const searchUrl = new URL('datasets/', this.props.field.get('piveau').get('search'))
    const datasets = toArray(this.props.value).map(async ({ id }) => {
      const response = await fetch(new URL(id, searchUrl))
      const { result: dataset } = await response.json()

      return {
        id,
        label: dataset.title[this.props.locale] || id,
      }
    })

    this.setState({
      datasets: await Promise.all(datasets),
    })
  }

  render() {
    return (
      <section>
        Datasets:
        <ul>
          {this.state.datasets.map(dataset => (
            <li key={dataset.id}>
              <a href={`${this.state.hub}datasets/${dataset.id}`}>{dataset.label}</a>
            </li>
          ))}
        </ul>
      </section>
    )
  }
}
