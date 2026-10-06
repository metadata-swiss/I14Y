import type { RestEndpointMethodTypes } from '@octokit/rest'
import { Octokit } from '@octokit/rest'
import { createAppAuth } from '@octokit/auth-app'
import { minimatch } from 'minimatch'

const BASE_PATH = 'assets'

function getAuth() {
  if (process.env.GITHUB_APP_ID) {
    return {
      appId: parseInt(process.env.GITHUB_APP_ID),
      privateKey: process.env.GITHUB_APP_PRIVATE_KEY!,
      installationId: process.env.GITHUB_APP_INSTALLATION_ID!,
    }
  }

  return process.env.GITHUB_TOKEN!
}

export default function (slug: string) {
  const logger = console

  let baseSha: string
  const prBranch = `cms/Showcases/${slug}`

  const owner = process.env.GITHUB_OWNER!
  const repo = process.env.GITHUB_CMS_REPO!
  const baseBranch = process.env.GITHUB_BASE_BRANCH!

  logger.info(`Using GitHub repository: ${owner}/${repo}, base branch: ${baseBranch}`)

  const auth = getAuth()
  const octokit = new Octokit({
    authStrategy: typeof auth === 'string' ? undefined : createAppAuth,
    auth,
  })

  const tree: RestEndpointMethodTypes['git']['createTree']['parameters']['tree'] = []

  return {
    async prepare() {
      try {
        logger.info('Getting base branch')
        const gotBranch = await octokit.repos.getBranch({
          owner,
          repo,
          branch: baseBranch,
        })
        baseSha = gotBranch.data.commit.sha

        logger.info(`Creating branch '${prBranch}'`)
        await octokit.git.createRef({
          owner,
          repo,
          branch: baseBranch,
          ref: `refs/heads/${prBranch}`,
          sha: baseSha,
        })

        await this.deleteExistingImages()
      }
      catch (error: any) { // eslint-disable-line @typescript-eslint/no-explicit-any
        logger.error('Error preparing git storage')
        if (error.status === 422) {
          logger.error(error)
          return false
        }
        else {
          throw error
        }
      }

      return true
    },
    async writeFile(path: string, payload: string | Buffer) {
      const blobContents = Buffer.isBuffer(payload)
        ? { content: payload.toString('base64'), encoding: 'base64' }
        : { content: payload, encoding: 'utf-8' }

      const blob = await octokit.git.createBlob({
        owner,
        repo,
        ...blobContents,
      })

      tree.push({
        path: `${BASE_PATH}/${path}`,
        type: 'blob',
        mode: '100644',
        sha: blob.data.sha,
      })
    },
    async finalize(): Promise<boolean> {
      try {
        const commitTree = await octokit.git.createTree({
          owner,
          repo,
          tree,
          base_tree: baseSha,
        })

        const newCommit = await octokit.git.createCommit({
          owner,
          repo,
          message: `Add new showcase: ${slug}`,
          tree: commitTree.data.sha,
          parents: [baseSha],
        })

        await octokit.git.updateRef({
          owner,
          repo,
          ref: `heads/${prBranch}`,
          sha: newCommit.data.sha,
        })

        const pr = await octokit.pulls.create({
          owner,
          repo,
          title: `New showcase submission: ${slug}`,
          head: prBranch,
          base: baseBranch,
        })

        await octokit.issues.addLabels({
          owner,
          repo,
          issue_number: pr.data.number,
          labels: ['decap-cms/draft'],
        })

        logger.info(`Created pull request '${owner}/${repo}#${pr.data.number}'`)
      }
      catch (error) {
        logger.error('Failed to create PR')
        logger.error(error)
        return false
      }

      return true
    },
    async rollback() {
      try {
        logger.info(`Rolling back changes, deleting branch '${prBranch}'`)
        await octokit.git.deleteRef({
          owner,
          repo,
          ref: `heads/${prBranch}`,
        })
      }
      catch (e) {
        logger.warn('Failed to delete branch on rollback')
        logger.warn(e)
      }
    },
    async deleteExistingImages() {
      const uploadsPath = `${BASE_PATH}/img/uploads`

      logger.info(`Checking for existing images to delete using pattern '${uploadsPath}/${slug}-image*'`)
      const { data } = await octokit.repos.getContent({
        owner,
        repo,
        path: uploadsPath,
      })

      const imagesToDelete = (Array.isArray(data) ? data : [])
        .filter(item => minimatch(item.path, `${uploadsPath}/${slug}-image*`))
        .map(item => item.path)

      logger.info(`Found ${imagesToDelete.length} images to delete`)
      for (const path of imagesToDelete) {
        tree.push({
          path,
          mode: '100644',
          type: 'blob',
          sha: null,
        })
      }
    },
  }
}
