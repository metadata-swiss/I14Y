import type { ShowcaseStorage } from '~~/server/lib/showcaseStorage'
import sharp from 'sharp'

interface ImageOptions {
  maxImageWidth: number
}

export function storage(store: Omit<ShowcaseStorage, 'writeImage'>, options: ImageOptions): ShowcaseStorage {
  return {
    ...store,
    async writeImage(path: string, contents: Buffer) {
      const resized = sharp(contents)
        .rotate()
        .resize(options.maxImageWidth)
        .jpeg({ quality: 90 })

      return this.writeFile(path, await resized.toBuffer())
    },
  }
}
