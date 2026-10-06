#!/usr/bin/env node

// read files from PWD and calculate signatures

import { createHmac } from 'node:crypto'
import { readFileSync } from 'node:fs'
import { join, dirname } from 'node:path'
import url from 'node:url'

const __dirname = dirname(url.fileURLToPath(new URL(import.meta.url)))
const secret = ''

const file = process.argv[2]
if (file) {
  const filePath = join(__dirname, file)
  const payload = readFileSync(filePath)
  const signature = createHmac('sha256', secret).update(payload).digest('hex')
  console.log(signature)
  process.exit(0)
}

process.exit(1)
