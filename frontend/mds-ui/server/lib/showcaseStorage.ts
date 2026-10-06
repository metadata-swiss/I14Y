export interface ShowcaseStorage {
  prepare?(): Promise<boolean>
  writeFile(path: string, contents: string | Buffer): Promise<void>
  writeImage(path: string, contents: Buffer): Promise<void>
  finalize(): Promise<boolean>
  rollback?(): Promise<void>
}
