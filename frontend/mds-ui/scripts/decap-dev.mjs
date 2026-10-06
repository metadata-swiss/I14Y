// Starts the Decap CMS local backend and the admin UI dev server side by side.
// decap-server reads and writes the CMS content checked out in ./content (see `npm run predev`).
import { spawn } from 'node:child_process'
import { existsSync, rmSync } from 'node:fs'
import { resolve } from 'node:path'

const root = resolve(import.meta.dirname, '..')
const contentDir = resolve(root, 'content')

if (!existsSync(contentDir)) {
  console.error(`CMS content not found in ${contentDir}. Run "npm run predev" first.`)
  process.exit(1)
}

rmSync(resolve(root, 'public/admin'), { recursive: true, force: true })

const processes = [
  spawn('decap-server', {
    cwd: root,
    stdio: 'inherit',
    shell: true,
    env: { ...process.env, PORT: '8088', BIND_HOST: 'localhost', GIT_REPO_DIRECTORY: contentDir },
  }),
  spawn('dotenv -- vite src/admin', { cwd: root, stdio: 'inherit', shell: true }),
]

for (const child of processes) {
  child.on('exit', (code) => {
    processes.filter(other => other !== child).forEach(other => other.kill())
    process.exit(code ?? 0)
  })
}
