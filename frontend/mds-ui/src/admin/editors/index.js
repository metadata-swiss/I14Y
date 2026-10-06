import CMS from 'decap-cms-app'

export function registerEditorComponent({ id, label, fields = [], content }) {
  const propsPattern = /:?(?<name>\w+)=(?:"(?<value>[^"]*)"|'(?<jsonValue>[^']*)')/g
  // Match outer block `::id ... ::` while allowing nested `::Name ... ::` blocks inside `content`.
  // Strategy: In `content`, consume either a full nested block starting with `::Word` up to its own closing `::`,
  // or any other character that is not the section-closing line `::` by itself.
  // Match a `{...}` fields block while allowing `}` to appear inside quoted values (e.g. JSON props).
  const fieldsPattern = String.raw`\{(?:[^{}'"]|'[^']*'|"[^"]*")+\}`
  const nestedBlockPattern = String.raw`::\w+(?:${fieldsPattern})?(?:\n[\s\S]*?\n::)`
  // Use greedy repetition so the outer closer matches the last `::` line, not the first nested one.
  const contentPattern = String.raw`(?:(?:${nestedBlockPattern})|(?:(?!^::\s*$)[\s\S]))*`
  const pattern = new RegExp(`^::${id}(?<fields>${fieldsPattern})?(?:\\n(?<content>${contentPattern}))?\\n::$`, 'ms')

  if (content) {
    fields.push({
      name: 'content',
      label: 'Content',
      widget: 'markdown',
    })
  }

  CMS.registerEditorComponent({
    id,
    label,
    fields,
    collapsed: true,
    pattern,
    fromBlock: (match) => {
      const props = {}
      let matchResult
      while ((matchResult = propsPattern.exec(match?.groups?.fields)) !== null) {
        const { name, value, jsonValue } = matchResult.groups
        if (jsonValue !== undefined) {
          try {
            props[name] = JSON.parse(jsonValue)
          }
          catch {
            props[name] = jsonValue
          }
        }
        else {
          props[name] = value
        }
      }

      if (content) {
        props.content = match?.groups?.content || ''
      }

      return props
    },
    toBlock: (props) => {
      let propsString = Object.entries(props)
        .filter(([name]) => name !== 'content')
        .map(([name, value]) => {
          const isJson = fields
            .find(field => field.name === name)
            ?.json === true

          if (isJson) {
            return `:${name}='${JSON.stringify(value)}'`
          }

          return `${name}="${value}"`
        })
        .join(' ')
      if (propsString.length) {
        propsString = `{${propsString}}`
      }
      return `::${id}${propsString}${props.content ? `\n${props.content}` : ''}\n::`
    },
  })
}
