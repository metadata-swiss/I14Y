import type { PagesCollectionItem } from '@nuxt/content'
import type { OdsNavTabItem } from '~/components/headers/model/ods-nav-tab-item'
import { sortContent } from '~/lib/sortContent'

const stemPattern = /pages\/(?<name>.+)\.\w\w/i
const getSlug = (page: PagesCollectionItem) => page.stem.match(stemPattern)?.groups?.name

export async function useNavigationItems(): Promise<OdsNavTabItem[]> {
  const { t, locale } = useI18n()

  const { data: pages } = await useAsyncData('navigation-pages', () => {
    return queryPublishedContent('pages')
      .where('path', 'LIKE', `%.${locale.value}`)
      .where('mainMenu', '=', true)
      .all()
  })

  const { pagesSubMenu } = sortContent(pages.value, getSlug).reduce(nestSubmenus, {
    pagesSubMenu: [],
    tempSubSubMenus: {},
  })

  return [
    {
      label: t('message.header.navigation.home'),
      to: '/',
    },
    {
      label: t('message.header.navigation.datasets'),
      to: '/datasets',
    },
    {
      label: t('message.header.navigation.organizations'),
      to: '/organizations',
    },
    {
      label: t('message.header.navigation.showcases'),
      to: '/showcases',
    },
    {
      label: t('message.header.navigation.blog'),
      to: '/blog',
    },
    {
      label: t('message.header.navigation.handbook'),
      to: '/handbook',
    },
    {
      label: t('message.header.navigation.admin.title'),
      adminOnly: true,
      subMenu: [
        { label: t('message.header.navigation.admin.dashboard'), to: '/gogd/dashboard' },
        { label: t('message.header.navigation.admin.metrics'), to: '/gogd/metrics' },
        { label: t('message.header.navigation.admin.categories'), to: '/gogd/categories' },
      ],
    },
    {
      label: t('message.header.navigation.more'),
      subMenu: pagesSubMenu,
    },
  ]
}

interface PagesNavWithSubmenus {
  pagesSubMenu: OdsNavTabItem[]
  tempSubSubMenus: Record<string, OdsNavTabItem[] | undefined>
}

function nestSubmenus({ pagesSubMenu, tempSubSubMenus }: PagesNavWithSubmenus, page: PagesCollectionItem): PagesNavWithSubmenus {
  const path = page.permalink ? page.permalink : getSlug(page)
  const menuItem: OdsNavTabItem = {
    label: page.title,
    to: `/${path}`,
  }

  if (page.parent) {
    const parentLink = `/${page.parent}`
    const parentMenuItem = pagesSubMenu.find(item => item.to === parentLink)

    // check if we already have a menu item for the parent
    if (parentMenuItem) {
      parentMenuItem.subMenu = [...parentMenuItem.subMenu || [], menuItem]

      return {
        pagesSubMenu,
        tempSubSubMenus,
      }
    }

    // if not, put it in a temp object
    return {
      pagesSubMenu,
      tempSubSubMenus: {
        ...tempSubSubMenus,
        [parentLink]: [
          ...tempSubSubMenus[parentLink] || [],
          menuItem,
        ],
      },
    }
  }

  // if we already have some sub-sub menu in the temp object, connect to parent
  if (tempSubSubMenus[`/${path}`]) {
    menuItem.subMenu = [
      { ...menuItem },
      ...tempSubSubMenus[`/${path}`] || [],
    ]
    delete menuItem.to
  }

  return {
    pagesSubMenu: [...pagesSubMenu, menuItem],
    tempSubSubMenus: {
      ...tempSubSubMenus,
      [`/${path}`]: undefined,
    },
  }
}
