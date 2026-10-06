import * as fs from 'node:fs/promises'

export default function (rootDir: string) {
  return {
    writeFile(path: string, contents: string | Buffer) {
      return fs.writeFile(`${rootDir}/content/${path}`, contents)
    },

    async finalize() {
      return true
    },
  }
}
