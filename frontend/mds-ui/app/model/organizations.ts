import type { Organization } from '~/piveau/organizations.ts'

export interface OrganizationTreeNode {
  id: string
  organization: Organization
  children: OrganizationTreeNode[]
}
