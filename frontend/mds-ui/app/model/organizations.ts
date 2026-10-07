import type { AgentModel } from '~~/api-client/generated/iop-core'
import { localize } from '~/utils/getCurrentTranslation'

/**
 * An organization is an iop-core agent. Agents returned by iop-core always have an id.
 * Pages link to organizations by their identifier, which is also what GET /api/Search filters publishers by.
 */
export type Organization = AgentModel & { id: string }

export interface OrganizationTreeNode {
  id: string
  organization: Organization
  children: OrganizationTreeNode[]
}

/**
 * Get the name of an organization in the given language, falling back to its preferred label and its identifier.
 */
export function organizationLabel(organization: AgentModel, lang: string) {
  return localize(organization.name, lang) || localize(organization.prefLabel, lang) || organization.identifier || ''
}

/**
 * Get the id of the parent organization, if any.
 */
export function getParentId(organization: AgentModel) {
  return organization.subAgentOf?.[0]?.id
}
