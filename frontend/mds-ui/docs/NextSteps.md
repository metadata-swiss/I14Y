# mds-ui 下一步计划

整理于 2026-10-08。分支 `main-mvp`，最新提交 `33c04f4`。每次修改计划，都记在文末的"变更记录"里。

## 总体安排：分五个版本

2026-10-06 定下了最初的方向：

> in the first version delete all the function only link to piveau and replace all things can be replace by iop core. and second version move the server feature to the iop core, but keep seo.

2026-10-08 改为五个版本，每个版本只有一个主题：

| 版本 | 主题 | 一句话 |
|---|---|---|
| **V1** | 公开展示上线 | 只保留展示功能（包括 showcase 的展示），去掉所有 piveau 代码和对 Zazuko 账号的依赖，页面正常运行没有 bug，打包成容器，通过工作流部署到 AKS。不用 CMS，编辑直接在 GitHub 上改内容 |
| **V2** | 后端接口和新界面 | 补齐 iop-core 缺少的接口（G1–G10），适配新的界面设计，加强 SEO 和 GEO |
| **V3** | 登录和管理区框架 | 用 eIAM 登录，建好管理区的框架和权限。V4 和 V5 都建立在它上面 |
| **V4** | 交互功能和 CMS | 邮件订阅、新闻邮件、评论、CMS 和 showcase 投稿 |
| **V5** | admin-ui 的功能 | 把 admin-ui 的所有功能搬进 mds-ui，最后删除 admin-ui |

**原则：每个功能只做一次，不做临时方案。** 开发成本低最重要。所以：

- V3 先只做登录和管理区框架，工作量不大；V4 和 V5 都直接用它，不需要临时的审核界面或临时工具；
- admin-ui 在 V5 结束前照常运行，所以 V5 可以放在最后，公众功能（V4）先上线；
- 邮件订阅的两条路一起做：未登录的公众用邮件里的链接，已登录的内部人员直接用账号（见 V4 第 2 步）。

其他已经定下的约束：

- 项目由 I14Y 接手，不再使用原团队 Zazuko 部署的任何服务，也没有他们的任何账号。计划里凡是依赖这些的地方，都要删除或换成 I14Y 自己的方案，见"不再使用 Zazuko 的服务和账号"一节。
- V1 不改 iop-core。iop-core 缺少的接口和 V1 删掉的功能都记在 `docs/iop-core-gaps.md`（G1–G13），方便以后加回。
- 公开页面在所有版本里都由服务端渲染，保留 SEO。

### V1：公开展示上线

- 和 public-ui 一样，只有面向公众的展示功能：没有登录，没有管理功能，不接收用户提交的任何数据。
- showcase 照样显示；新的 showcase 由编辑在 GitHub 内容仓库里添加。
- 页头的"登录"按钮保留。它只是一个跳转到 I14Y input 门户的链接，mds-ui 自己不登录。public-ui 右上角的登录图标也是这样。
- 部署在 AKS（Azure Kubernetes Service）上。服务端只负责渲染页面，不持有任何密钥。

### V2：补齐 iop-core 的接口，适配新的界面设计

- 补齐 `docs/iop-core-gaps.md` 里的 G1–G10，恢复 V1 里隐藏的筛选条件、排序和元数据下载。
- 按新的界面设计改造公开页面。设计由谁出、什么时候定稿，见"需要你决定的事"第 11 条。
- 加强 SEO 和 GEO。

### V3：登录和管理区框架

- mds-ui 重新有登录：和 admin-ui 一样用 eIAM 登录，登录后由 eIAM 给 token；用户信息和权限由 iop-core 提供。
- 建好管理区的框架：导航、布局、权限控制。具体功能在 V4 和 V5 里放进来。

### V4：交互功能和 CMS

- 邮件订阅（G12）、新闻邮件、评论（G11）、CMS 和 showcase 投稿。
- 都建立在 V3 的登录和管理区之上；发邮件的能力放在 iop-core 的 Portal 模块里。

### V5：把 admin-ui 的功能搬进 mds-ui

- admin-ui（I14Y 的 input 门户）的所有功能，在 mds-ui 的管理区里重新实现，然后删除 admin-ui。
- 原来 mds-ui 的 GOGD 管理菜单也放进这个管理区。
- 请后端团队在 V5 结束前不要删除 Admin API。

服务端功能 S1–S10 的定义见 `tmp/mds-ui-core-integration/mds-ui-core-integration-analysis.md`。

## 工期估计（2026-10-08，粗估）

单位是**人天**（一个人干一天），不包括等待外部审批的时间。这些都是粗略估计，误差可能有 ±50%。V1 做完以后，用实际花掉的时间回头校准一次。

### 各版本的工作量

**V1：约 21–39 人天**

| 步骤 | 人天 | 说明 |
|---|---|---|
| 1 删除非展示功能 | 2–3 | |
| 2 安全、性能、SEO 的问题 | 1–2 | |
| 3 Showcase 展示和内容迁移 | 4–6 | 列表页重写；内容仓库里的 piveau 地址换成 iop-core 标识符 |
| 4 网址和重定向 | 1–3 | 如果旧的 piveau 编号和 iop-core 标识符对应不上，要先建对照表，会更多 |
| 5 删除 Decap，写编辑指南 | 1–2 | |
| 6 彻底去掉 piveau，清理代码 | 3–5 | |
| 7 测试和 CI | 3–5 | |
| 8 部署到 AKS | 3–5 | 集群权限、域名、证书、iop-core 的 CORS 要等其他团队 |
| 9 测试和上线 | 3–5 | |

**V2：约 36–65 人天**

| 部分 | 人天 | 说明 |
|---|---|---|
| iop-core 接口 G1–G10（后端） | 15–25 | 要看后端新搜索服务的进度 |
| 按新界面改造公开页面 | 15–30 | 最不确定：取决于新设计的改动有多大，设计本身的时间没算 |
| 把恢复的筛选、排序、元数据下载放进界面 | 3–5 | |
| SEO 和 GEO | 3–5 | |

**V3：约 6–10 人天。** 另外要等 eIAM 登记回调地址，可能要几周。

**V4：约 45–128 人天，取决于用现成产品还是自己开发**

| 部分 | 用现成产品 | 自己开发 |
|---|---|---|
| Portal 模块和发邮件的能力 | 6–10 | 6–10 |
| 邮件订阅（两条路） | 13–20 | 13–20 |
| 新闻邮件 | 8–13 | 8–13 |
| CMS 和 showcase 投稿 | 11–16（Sveltia） | 35–50（自己的内容模块） |
| 评论 | 7–12（Isso）到 9–15（Comentario） | 23–35 |
| **合计** | **约 45–74** | **约 85–128** |

另外要等：发信服务的审批、发件域名的 DNS 设置、邮件订阅和评论的数据保护审查、许可证例外。

**V5：约 80–130 人天**

| 部分 | 人天 |
|---|---|
| 搜索总览 | 5–8 |
| 数据服务、映射表、公共服务 | 18–28 |
| 概念（代码表、导入导出、版本、锁定） | 10–15 |
| 数据集（admin-ui 里 24 个组件） | 20–30 |
| 通用组件（admin-ui 里 35 个） | 10–15 |
| GOGD 管理菜单（要先定义显示什么，后端要加角色和接口） | 8–15 |
| 切换、改写端到端测试、删除 admin-ui | 5–10 |

此外还要留出两边同时运行和用户验收的时间，大约 2–4 周。

### 合计和日历时间

| 版本 | 人天 |
|---|---|
| V1 | 21–39 |
| V2 | 36–65 |
| V3 | 6–10 |
| V4 | 45–128 |
| V5 | 80–130 |
| **合计** | **约 190–370** |

每人每月按大约 15 个有效工作日算（开会、评审、修 bug 已经扣除）：

| 团队 | 纯开发时间 | 加上等待、协调、验收以后 |
|---|---|---|
| 1 人 | 13–25 个月 | 16–30 个月 |
| 2 人（1 前端 + 1 后端） | 6–12 个月 | 9–15 个月 |
| 3 人（2 前端 + 1 后端） | 4–8 个月 | 6–11 个月 |

按 2 人估算，每个版本大概的上线时间：

| 版本 | 大约需要 | 累计 |
|---|---|---|
| V1 | 1–2 个月 | 1–2 个月 |
| V2 | 2–3 个月 | 3–5 个月 |
| V3 | 约 2 周，加上 eIAM 的等待 | 3.5–5.5 个月 |
| V4 | 2–4 个月 | 6–9 个月 |
| V5 | 3–5 个月 | 9–15 个月 |

### 最影响工期的因素

1. **V4 用现成产品还是自己开发**：两者差 40–55 人天，大约 2 个月。
2. **V2 新界面的改动有多大**：设计还没有出来，这是最大的未知数。
3. **外部等待**：AKS 的权限和域名（V1）；eIAM 登记（V3）；发信服务、发件域名和数据保护审查（V4）；后端新搜索服务的进度（V2）。
4. **旧网址能不能对应到新标识符**：对应不上的话，V1 第 4 步会明显变大。

### 现在就能做、可以缩短工期的事

- [ ] 现在就申请：eIAM 回调地址的登记、AKS 的权限、发信服务和发件域名。这些可能要等几周。
- [ ] V2 的后端接口可以和 V1 同时开始：V1 的"不改 iop-core"指的是 mds-ui 不依赖新接口，后端团队可以先做。
- [ ] 尽早拿到新的界面设计，这样 V1 就不用在马上会被替换的界面上花时间。
- [ ] V3 和 V4 的后端部分（Portal 模块、发信能力）可以和前端同时进行。

## V1 的范围

| 功能 | V1 | 之后的版本 |
|---|---|---|
| S1 Keycloak 登录 | 删除 | V3，改用 eIAM 登录（G13） |
| GOGD 管理区（`/gogd/*`） | 删除 | V5，放进 mds-ui 的管理区（G13） |
| S2 订阅、S3 偏好设置、S4 摘要邮件 | 删除 | V4，由 iop-core 提供，公众和登录用户两条路一起做（G12） |
| S5 Showcase 提交 | 删除。showcase 照样显示，新的 showcase 由编辑在 GitHub 内容仓库里添加 | V4，和 CMS 一起 |
| S6 评论通知、S7 评分 | 已随 Hyvor 删除 | V4，评论的方案见 G11 |
| S8 Showcase JSON-LD 接口、S9 收录触发插件 | 删除，只为 piveau 存在 | 不再需要 |
| S10 服务端的 piveau 客户端 | 删除 | 不再需要 |
| Decap CMS 编辑器（`/admin`） | 删除。V1 不用 CMS，编辑直接在 GitHub 上改内容 | V4，选定的内容系统 |

V1 完成后，mds-ui 不再需要 Listmonk、GitHub App 和 Keycloak，也没有任何密钥。

## 不再使用 Zazuko 的服务和账号

I14Y 没有原团队的任何账号，他们部署的服务也不再使用。下表列出 mds-ui 里所有依赖这些的地方：

| 原团队的服务或账号 | 原来用在哪里 | 怎么处理 |
|---|---|---|
| piveau（`piveau-hub-search/repo.ref.ods.zazukoians.org`，`nuxt.config.ts`） | showcase 页面、偏好设置页现在还在调用 | V1 第 1、3、6 步删除 |
| piveau 数据预览服务（`piveau-hub-data-preview.abn.ods.zazukoians.org`，`OdsPreview.vue`） | 分发页的数据预览 | 不能再用。建议 V1 删除预览，见"需要你决定的事"第 3 条 |
| Keycloak（`keycloak.zazukoians.org`） | 登录 | V1 第 1 步删除。V3 改用 I14Y 的 Keycloak |
| Listmonk（`listmonk.int.ods.zazukoians.org`），以及里面的订阅者、邮件模板和链接签名密钥 | 邮件订阅、showcase 提交通知 | V1 第 1 步删除。这些数据拿不到，V4 的订阅从零开始，而且不用 Listmonk（AGPL-3.0，在 I14Y 的许可证黑名单上） |
| Hyvor Talk 账号，以及里面的旧评论 | 评论 | 已经删除。旧评论拿不到，V4 的评论从零开始 |
| Netlify 项目和 passbolt 里的密码 | Decap 编辑器的 GitHub 登录 | 不能再用。V1 删除 Decap；V4 选内容系统时，如果选了基于 GitHub 的，再由 I14Y 自己做登录 |
| GitHub App `piveau-ui-github-app` | showcase 提交时创建 PR | V1 第 1 步删除。V4 如果投稿要开 PR，用 I14Y 自己的 GitHub App |
| 原团队的部署：域名 `piveau.ods.zazukoians.org`、通配符证书、镜像 `ghcr.io/metadata-swiss/ods-ui`、PR 预览部署 | 运行网站 | V1 第 8 步改写 `k8s/`；README 里预览部署一节删除 |
| 原团队仓库里的工作流（`opendata-swiss/metadata.swiss` 的 `subscriptions.yaml`） | 定时发摘要邮件 | 不再需要 |
| 通知邮件收件人 `noreply@zazuko.com` | showcase 提交通知 | 随 V1 第 1 步删除 |
| 文档：README、`docs/authentication.md`、`docs/cms.md`、`docs/cms/showcases.md`、`docs/subscriptions/index.md` | 写的都是原团队的地址和账号 | V1 第 1 步改写或删除 |

**原则（2026-10-08）：内容一直放在单独的内容仓库里，不放进 I14Y 的主仓库。** 以后选 CMS 时，要选支持这种结构的。

另外，CMS 内容仓库（GitHub 上 `opendata-swiss` 组织下的 `opendata-swiss-cms-content`、`-int`、`-test`）不是 Zazuko 部署的服务，但构建镜像时要从那里克隆内容，编辑也要往里写。I14Y 有没有这个组织的权限，要先确认，见"需要你决定的事"第 9 条。

## V1 的进度

| 阶段 | 内容 | 状态 |
|---|---|---|
| 0 | mds-ui 在本地运行 | ✅ `e221ba0` |
| 1 | 生成 iop-core 客户端（最后用的是 orval，不是原计划的 hey-api） | ✅ `e938f84` |
| 2 | 补齐 iop-core 缺少的接口 | ⏸ V1 不改 iop-core，挪到 V2 |
| 3 | 页面改用 iop-core | ✅ `b13cde2`，只剩网址和重定向 |
| 4 | Showcase | 未开始。现在只改展示部分，提交表单删除 |
| 5 | 服务端功能 | 未开始。原计划是改写，现在改为全部删除 |
| 6 | CMS 编辑器（Decap） | 未开始。V1 不用 CMS，改为删除 Decap（第 5 步） |
| 7 | 彻底去掉 piveau | 未开始 |
| 8 | 部署 | 未开始 |
| 9 | 测试和上线 | 未开始 |

计划之外已经完成的：

- 文件按 Nuxt 4 的标准结构重新整理，`lib/` 改名为 `utils/`（`b13cde2`）。
- 登录按钮的地址可以用 `NUXT_PUBLIC_LOGIN_URL` 修改（`b13cde2`）。
- 修复了除 showcases 和 subscription 之外所有页面的类型错误（`b13cde2`）。
- 关闭评论功能，删除 Hyvor Talk（`b2f2b8f`、`33c04f4`）。

检查结果（2026-10-08）：

- lint 通过。
- typecheck 还有 27 个错误：`showcases/index.vue` 7 个、`showcases/submit.vue` 6 个、`subscription/preferences.vue` 14 个。后两个页面会在第 1 步删除；`showcases/index.vue` 会在第 3 步重写。
- 服务端的 mocha 测试 11 个全部通过。这些测试测的都是第 1 步要删除的功能。

## V1 剩下的步骤，按顺序

第 1 步和第 2 步可以马上开始；第 2 步里数据预览那一条，按"需要你决定的事"第 3 条的建议做。第 3 步要先确认内容仓库的权限（第 9 条），第 4 步要等网址的决定（第 2 条）。

### 第 1 步：删除所有不属于"公开展示"的功能（S1–S5、S8–S10、GOGD）

这一步合并了原计划阶段 5（服务端功能）和阶段 4 里删除 S8、S9 的部分。放在最前面，原因有两个：

- 深度分析里发现的大部分安全问题都在这些代码里，删掉以后就不用再修：
  - 摘要邮件接口没有保护，任何人都能让系统给所有订阅者群发邮件；
  - Listmonk 管理员 token 会写进日志；
  - 邮箱直接拼进 Listmonk 的 SQL 查询；
  - 登录和订阅之后的开放重定向；
  - 偏好设置链接的 HMAC 校验太弱；
  - showcase 提交接口没有验证码和限流；
  - `require-auth` 的两个 bug。
- 删掉以后，后面几步要改的代码少了很多。

删除邮件订阅（S2–S4）：

- [ ] 服务端：`server/api/subscribe/`、`server/api/subscription/`、`server/lib/subscription/`。
- [ ] 页面：
  - `app/pages/subscription/`（偏好设置页）；
  - 数据集页上订阅数据集和分类的表单（`app/pages/datasets/[datasetId]/index.vue`）；
  - 只有偏好设置页用到的组件 `OdsCheckbox`、`OdsRadio`。
- [ ] 翻译：4 个语言文件里的 `message.subscribe`，以及偏好设置页的文案。

删除 showcase 提交（S5）：

- [ ] 页面 `app/pages/showcases/submit.vue`，以及 showcases 列表页上指向它的链接。
- [ ] 服务端：
  - `server/api/showcases.post.ts`；
  - `server/lib/git.ts`、`server/lib/fs.ts`、`server/lib/images.ts`、`server/lib/showcaseStorage.ts`。
- [ ] `src/schema/showcase` 里的 `submissionSchema`。同一个文件里给 `content.config.ts` 用的 showcase 内容 schema 要保留。

删除只为 piveau 存在的服务端功能（S8–S10）：

- [ ] `server/api/showcases.ts`（S8）、`server/plugins/showcase-harvesting-trigger.ts`（S9）、`server/lib/piveau.ts`（S10）。

删除登录（S1）和 GOGD 管理区：

- [ ] 文件：`server/api/auth/`、`server/lib/login.ts`、`server/middleware/basic-auth.ts`、`app/middleware/require-auth.ts`。
- [ ] `useUserSession` 的调用：`app/app.vue`、`app/components/headers/OdsHeader.vue`、`app/components/headers/OdsTopHeader.vue`。页头的"登录"按钮保留，它只是链接。
- [ ] GOGD 管理区：
  - `app/pages/gogd/`；
  - `app/composables/navigation-items.ts` 里 `adminOnly` 的菜单项；
  - 4 个语言文件里的 `message.header.navigation.admin`。

删除以后跟着清理：

- [ ] `server/lib/listmonk/`。Listmonk 不再有任何用途。
- [ ] `server/lib/` 和 `server/plugins/` 里不再被引用的文件，例如 `auth.ts`、`locale.ts`、`zod-locale.ts`、`log-config.ts`。删之前逐个确认。
- [ ] 依赖：`nuxt-auth-utils`、`@octokit/auth-app`、`@octokit/rest`、`sharp`（连同 `nuxt.config.ts` 里 `dev:reload` 的 hook）。
- [ ] `nuxt.config.ts`：
  - `listmonk`、`subscription`、`oauth`、`apiTunerTests`；
  - 整个 `showcases` 配置项：`maxImageWidth`、`submissionNotification` 只给提交用；`catalogId`、`resourceType` 现在没有代码读取；
  - 全部 `routeRules`，以及文件开头对 `NitroRouteConfig` 的类型扩展。
- [ ] 测试：`server/tests/` 里的测试和测试数据都是测这些功能的，全部删除。`package.json` 里的 mocha、api-tuner 脚本和配置也一起删除。前端测试在第 7 步用 vitest 重新建。
- [ ] 文档，去掉原团队的地址和账号：
  - README：环境变量 `NUXT_LISTMONK_*`、`NUXT_OAUTH_KEYCLOAK_*`、`NUXT_SESSION_PASSWORD`、`GITHUB_*`、`NUXT_SHOWCASES_*`；订阅一节；Decap 登录一节（Netlify 和 passbolt）；PR 预览部署一节和两张 UML 图；piveau 的默认地址；
  - `docs/authentication.md`（Netlify 登录）、`docs/cms.md` 和 `docs/cms/showcases.md`（原团队的网址）：按"需要你决定的事"第 1 条的结论改写或删除；
  - `docs/subscriptions/` 保留，G12 引用了它，但在开头注明：里面的 Listmonk 实例和工作流属于原团队，已经不能用。

**完成标准：**

- lint 通过；
- typecheck 只剩 `showcases/index.vue` 的 7 个错误；
- `server/` 下只剩 `server/api/healthz.ts` 和服务端渲染需要的部分；
- 运行 mds-ui 不需要任何密钥。

### 第 2 步：剩下的安全问题，以及性能和 SEO（约 1 天）

- [ ] 数据集描述来自外部，改为按纯 Markdown 渲染，不再用 `<MDC>`（数据集页、分发页）。
- [ ] **修我造成的问题**：现在每个页面都会加载数据预览用的图表库（piveau-preview-plugin、vega）。
  - 原因：整理文件时 `charts.client.ts` 被移进 `app/plugins/`，变成了全局插件。
  - 如果按建议在 V1 删除数据预览（"需要你决定的事"第 3 条），就直接删除 `piveau-preview-plugin`、`app/plugins/charts.client.ts`、`OdsPreview.vue`，以及 `nuxt.config.ts` 全局 `css` 里 ag-grid 和预览插件的样式，这个问题跟着解决。
  - 如果保留预览：把 `registerCharts()` 移进 `OdsPreview`，对这个组件做懒加载，样式也只在预览组件里引入。
- [ ] 现在每个页面都会下载整个 CMS 页面数据库（`sql_dump.txt`），因为面包屑和菜单的内容查询没有包进 `useAsyncData`。
  - 包进去以后，查询结果会随服务端渲染的 HTML 一起传给浏览器，浏览器不用再查一次。
  - 文件：`app/composables/breadcrumbs.ts`、`app/composables/navigation-items.ts`。
- [ ] 不存在的数据集、组织和博客文章现在返回 200 和空白页。改为用 `createError` 返回 404。
- [ ] 页面里嵌套了两个 `<main>`，而且 id 相同。去掉 `app/app.vue` 外层的那个。
- [ ] 5 处"阅读更多"链接写的是 `aria-label="false"`，读屏软件会把它们读成"false"。

### 第 3 步：阶段 4，Showcase 的展示部分

- [ ] 列表页改为读取 Nuxt Content（按分类、类型、关键词筛选，按标题和日期排序）。
- [ ] 详情页中数据集和分类的名称，改为从 iop-core 获取。
- [ ] 组织页上的 showcase 数量，改为从 Nuxt Content 统计（现在固定显示 0）。
- [ ] 首页的"推荐 showcase"区块不再使用 piveau。
  - 文件：`app/components/content/OdsSectionPromotedShowcases.vue`
- [ ] showcase 内容的 schema 加上 `issued` 和 `modified` 字段。
- [ ] 在 CMS 内容仓库做一次性数据迁移：
  - 给现有的 showcase 补上 `issued` 和 `modified`；
  - piveau 数据集 URI 改为 iop-core 标识符。
- [ ] 删除 `app/piveau/showcases.ts`。
- [ ] 完成后，typecheck 应该没有错误。

### 第 4 步：阶段 3 收尾，网址和重定向

要等"需要你决定的事"第 2 条有了结论。在第 8 步部署之前完成即可。

- [ ] 确定数据集网址用 DCAT 标识符（现在的做法）还是 GUID。
- [ ] 给旧的 piveau 网址加重定向，保住搜索引擎里已有的收录和分享出去的链接。
- [ ] 被删除的页面（`/subscription/preferences`、`/showcases/submit`）怎么处理，见"需要你决定的事"第 5 条。

### 第 5 步：阶段 6，删除 Decap，编辑改在 GitHub 上直接编辑

V1 不用 CMS（2026-10-08 决定）。CMS 在 V4 加回来，到时候按 `docs/CmsComparison.md` 选定。

- [ ] 删除 `src/admin/`、`package.json` 里构建 Decap 的 `prebuild` 和 `decap` 脚本，以及只给 Decap 用的依赖：`decap-cms-app`、`decap-server`、`bootstrap`、`react-bootstrap-typeahead`、`vite-plugin-node-polyfills`、`vite-plugin-static-copy`、`eslint-plugin-react`。删之前逐个确认没有别处在用。
- [ ] 删除 `nuxt.config.ts` 里 `/admin/` 的 devProxy。
- [ ] 删除或改写 `docs/authentication.md` 里 Decap 和 Netlify 的部分。
- [ ] 把 `docs/cms.md` 和 `docs/cms/` 改写成**给编辑的 GitHub 编辑指南**：在哪个仓库、哪个目录改哪类内容；每种语言一个文件；文件头字段的含义；9 种自定义块（`::OdsCard{…}` 等）怎么写，附可以直接复制的例子；怎样上传图片；怎样开 PR、谁来审核；合并以后多久上线。
- [ ] 内容仓库要有写权限，见"需要你决定的事"第 9 条。

删掉 Decap 以后，它带进的 3 个许可证黑名单上的包（`@vercel/stega`、两个版本的 `dompurify`）和 `decap-server` 的 `simple-git` 严重漏洞也跟着消失。

### 第 6 步：阶段 7，彻底去掉 piveau，顺便清理

- [ ] 删除：
  - 依赖 `@piveau/sdk-core`、`@piveau/sdk-vue`；
  - `app/piveau/`、`i18n/locales/piveau/`、`app/model/dataset.ts`；
  - `app/plugins/piveau-vue-query.ts` 里注册 `piveauKitPlugin` 的部分（Vue Query 的部分保留，文件可以改名为 `vue-query.ts`）；
  - `app/app.vue` 里同步 piveau 语言设置的代码；
  - `piveauHubSearchUrl`、`piveauHubRepoUrl` 配置。
- [ ] `app/model/dataset/table-entry.ts` 从 `@piveau/sdk-vue` 引入的类型，改成项目自己定义的类型。
- [ ] 数据预览：如果第 2 步还没删，现在删除。它调用的是原团队部署的预览服务，已经不能用。
- [ ] 在代码里搜索剩下的 `piveau` 引用。
- [ ] 顺便清理：
  - 没有被引用的组件：`OdsMetaInfo`、`OdsTabs`、`ToastMarkdownEditor`、`OdsDetailTermsOfUse`、`OdsMetadataDownloadList`；
  - 没有用到的依赖：`nuxt-basic-auth-module`、`pinia`、`@pinia/nuxt`、`rimraf`、`@toast-ui/editor`；
  - eslint、typescript 等开发工具从 `dependencies` 移到 `devDependencies`。依赖扫描现在有 60 个漏洞，大部分来自这一点和第 1 步删除的 sharp；
  - 重复逻辑：许可证映射有 3 份，预览格式映射有 2 份；
  - 硬编码文案（约 30 处）和缺少的翻译 key `message.handbook.read_more`；
  - 占位内容：picsum 随机头像、页脚的 "foobar"、只打印 `console.log` 的关键词点击。

**完成标准：** 不配置任何 piveau 地址，mds-ui 也能正常运行。

### 第 7 步：测试和 CI（可以和第 3–6 步同时进行）

- [ ] 用 vitest 和 `@nuxt/test-utils` 给 `app/model/dataset/` 的两个适配器写单元测试，再给 `app/composables/` 写测试。
- [ ] 给 mds-ui 建一个自己的 CI 工作流：lint、typecheck、测试、构建。不加进 `quality-gates.yml`。
- [ ] 修正 README：
  - 写着运行 `npm test`，但并没有这个脚本；
  - 装了 husky，但没有 `.husky/` 目录。

### 第 8 步：阶段 8，部署到 AKS

本仓库里还没有部署到 AKS 的先例：后端部署在 Container Apps 上（`i14y-backend-deploy.yml`），public-ui 和 admin-ui 部署在 Static Web Apps 上。所以没有现成的工作流可以照抄，开始前需要集群的信息，见"需要你决定的事"第 7 条。

好在原团队本来就把 mds-ui 部署在 Kubernetes 上，`k8s/` 里有一套 kustomize 配置（deployment、service、ingress）。原计划是删除它，现在改为保留，在它的基础上改写：

- [ ] 名字和标签：`piveau-ui`、`piveau-previews` 改成 `mds-ui`。
- [ ] 镜像：改成本仓库 `docker/Dockerfile.mds-ui` 构建的镜像。容器端口从 80 改成 3000，因为镜像里设置了 `PORT=3000`，并且以 `node` 用户运行。
- [ ] 环境变量：
  - 只剩 `NUXT_APP_URL`、`NUXT_PUBLIC_IOP_CORE_URL`、`NUXT_PUBLIC_LOGIN_URL`，放进 ConfigMap；
  - piveau、Keycloak、GitHub、Listmonk 的变量全部删除；
  - V1 没有密钥，不需要 Secret，也不需要 Key Vault。
- [ ] Ingress：
  - 域名和 TLS 证书换成 I14Y 的（现在是 `piveau.ods.zazukoians.org` 和 Zazuko 的通配符证书）；
  - 去掉完全放开的 CORS；
  - 去掉 `proxy-body-size: 20g`（原来给 showcase 图片上传用）和 24 小时的超时。
- [ ] 保留 `/api/healthz` 的三个探针。内存现在是 requests 2Gi、limits 4Gi，上线后按实际用量调整。
- [ ] 每个环境一个 kustomize overlay（例如 dev、int、prod），放各自的域名和 iop-core 地址。
- [ ] 镜像推到 AKS 能拉取的镜像仓库（ACR）。
- [ ] 部署工作流：构建镜像 → 推送 → `kubectl apply -k k8s/overlays/<环境>`。
- [ ] CMS 内容仓库合并到 main 时自动部署；再加一个每天定时构建，保证定时发布的内容能按时上线。内容是在构建镜像时克隆进去的，所以内容一变就要重新构建。
- [ ] iop-core 的 CORS：生产环境里，浏览器翻页、筛选时会直接调用 iop-core（`app/plugins/iop-core-client.ts`，只有开发时才走 `/iop-core` 代理）。请运维在 iop-core 的 ingress 上允许 mds-ui 的域名使用 `GET`。
- [ ] 网络：AKS 的出站流量要能访问 iop-core，因为服务端渲染时也要调用它。
- [ ] 镜像里不能包含 BIT Proxy Root CA。

### 第 9 步：阶段 9，测试和上线

**"页面正常运行、没有 bug"的验收标准：**

- 4 种语言的所有页面都能打开，没有 500 错误，浏览器控制台没有报错；
- 不存在的数据集、组织、博客和手册页面返回 404；
- lint、typecheck、单元测试和构建在 CI 里全部通过；
- 代码里搜不到 `piveau`、`zazuko`、`keycloak`、`listmonk`、`hyvor`；
- 运行 mds-ui 不需要任何密钥；
- 基本的无障碍检查通过（没有 `aria-label="false"`，只有一个 `<main>`）。

- [ ] 功能测试：4 种语言的所有页面。
- [ ] SEO 检查：
  - 网页源代码里有完整内容和标题；
  - 不存在的页面返回 404；
  - 旧网址能正确重定向。
- [ ] 按"需要你决定的事"第 5 条，处理旧邮件里的链接。
- [ ] 切换域名，上线后头几天关注日志。

## V2：补齐 iop-core 的接口，适配新的界面设计

V1 上线以后开始。

### V2 第 1 步：补齐 iop-core 缺少的接口

V1 里绕过或隐藏的功能，在这一步恢复。详细说明见 `docs/iop-core-gaps.md`。后端团队正在做新的搜索服务（Elasticsearch，分支 `feature/#730-set-up-new-indexSearch-refactoring-core` 等），G1–G5、G7、G9、G10 最好直接做进它的接口里，所以要尽早把 mds-ui 的需求交给他们。

- [ ] G1：按 DCAT 目录过滤，只显示 opendata.swiss 的数据集。
- [ ] G2–G5：按 EU 数据主题分类、许可证、关键词过滤，以及排序。恢复数据集搜索的这几个筛选条件和排序。
- [ ] G6：单个数据集和分发的 DCAT 导出（JSON-LD、Turtle、RDF/XML）。恢复元数据下载，第 3 步的 GEO 也要用。
- [ ] G7–G9：组织搜索、组织的 URI、搜索结果里的关键词和日期。
- [ ] G10：按修改时间过滤（`modifiedSince`）。V4 的摘要邮件要用，在这里一起做。

### V2 第 2 步：适配新的界面设计

- [ ] 拿到新的界面设计（见"需要你决定的事"第 11 条），确认用哪个版本的联邦设计系统。
- [ ] 按新设计改造公开页面：首页、数据集搜索和详情、组织、showcase、博客、手册。
- [ ] 把第 1 步恢复的筛选条件、排序和元数据下载放进新界面。
- [ ] 如果决定要数据预览（"需要你决定的事"第 3 条），在这一步按新界面重新做。

### V2 第 3 步：加强 SEO 和 GEO（可以和第 2 步同时进行）

- [ ] 数据集和组织页面加上 `description` 和 Open Graph 标签，内容来自 iop-core 的描述。
- [ ] 所有页面通过 `useLocaleHead()` 加上 `hreflang` 和 `canonical`。
- [ ] 生成 sitemap，数据集列表从 iop-core 获取，带 `lastmod`。
- [ ] 数据集页面加上 schema.org `Dataset` 的 JSON-LD，并用 `<link rel="alternate">` 指向 DCAT 导出（G6）。
- [ ] 为爬虫流量调大 iop-core 的输出缓存，或者加 CDN。

## V3：登录和管理区框架

V2 完成以后开始（也就是之前讨论里的"V3a"）。这是 V4 和 V5 共同的基础：V4 的订阅管理、新闻邮件、审核和 CMS，V5 的 admin-ui 功能，都放进这里建好的管理区。

**要点：**

- **登录**：照 admin-ui 的做法，在浏览器里用 eIAM 登录拿到 token，iop-core 校验它，登录本身不需要改后端（见第 1 步）。这和 V1 第 1 步删除的登录不是一回事：删除的那个是 mds-ui 自己的服务端去登录 Zazuko 的 Keycloak。mds-ui 仍然不持有任何密钥。
- **管理区框架**：导航、布局、"没有权限"页面、按 iop-core 返回的角色和权限显示或隐藏功能。
- **不包括**：admin-ui 的具体功能，放在 V5。admin-ui 在这期间照常运行，页头的"登录"按钮也先继续指向 input 门户，等 V5 切换时再改。

### V3 第 1 步：用 eIAM 登录（照 admin-ui 的做法）和管理区的框架

用户用 eIAM 登录，登录后由 eIAM 给 token，和 admin-ui 一样。admin-ui 配置里的 `KEYCLOAK_AUTHORITY_URL` 指向的就是 eIAM（`https://identity-eiam-r.eiam.admin.ch/realms/edi_bfs-i14y`，client `BFS-i14y`）。mds-ui 只连 eIAM，不连 iop-core 另外接受的那个 Keycloak。详见 `docs/iop-core-gaps.md` 的 "How mds-ui logs in: with eIAM, like admin-ui"。

- [ ] 在 eIAM 登记 mds-ui 的回调地址和登出地址：用 `BFS-i14y` 这个 client，或者新申请一个。要走 eIAM 的申请流程，可能要等一段时间，早点提。
- [ ] 在浏览器里用 OIDC 登录（授权码 + PKCE），和 admin-ui 一样用 `oidc-client-ts`。它不依赖 Angular，Vue 里也能用。参考 admin-ui 的 `src/app/auth/`：
  - 登录、登出，以及两个回调页；
  - 没有权限时显示的页面；
  - 未登录时访问管理区，自动跳到登录。
- [ ] eIAM 的地址和 client id 做成 mds-ui 的配置，例如 `NUXT_PUBLIC_EIAM_AUTHORITY_URL`、`NUXT_PUBLIC_EIAM_CLIENT_ID`。它们不是密钥，mds-ui 仍然没有密钥。
- [ ] 调用 iop-core 时带上 token：在 `api-client/iop-core-fetch.ts` 里加 `Authorization: Bearer` 请求头。iop-core 已经能校验 eIAM 的 token，登录本身不需要改后端。
- [ ] 用户信息和权限从 iop-core 取，这一点比 admin-ui 做得更好：
  - `GET /api/Users/current`：姓名、邮箱、业务角色、所属组织；
  - `GET /api/Users/current-agents`：用户所属的组织；
  - `AllowActions`：能做哪些操作；
  - 不像 admin-ui 那样自己解析 token 里的角色，也不向 eIAM 取用户信息（admin-ui 设了 `loadUserInfo: true`）。
- [ ] token 只放在内存或 sessionStorage 里，不像 admin-ui 那样复制到 `localStorage`：放在那里，页面里任何被注入的脚本都能读到它。
- [ ] token 快过期时的处理，先照 admin-ui：弹窗提示用户重新登录。
- [ ] 管理区的页面只在浏览器里渲染（`routeRules` 里设 `ssr: false`），因为 token 只在浏览器里。这些页面也不需要 SEO。
- [ ] 管理区有一个入口（例如 `/admin` 下的登录页），给内部人员先用起来。页头的"登录"按钮暂时继续指向 input 门户，V5 切换时再改。

## V4：交互功能和 CMS

V3 完成以后开始。

**原则：每个功能只做一次。** 这些功能都直接用 V3 的 eIAM 登录和管理区，不做临时的审核界面或临时工具。网站在整个过程中一直可用。

### V4 第 1 步：在 iop-core 里搭建 Portal 模块和发邮件的能力

- [ ] 新建项目 `Bfs.Iop.Core.Portal`，在 `Bfs.Iop.Core.Api` 里注册，路由放在 `/api/portal/...` 下，和现有代码隔离。
- [ ] 给它单独生成一份 swagger 文档（`portal`），mds-ui 再用 orval 从这份文档生成一个客户端。
- [ ] 只读模式例外：新增 `[AllowInReadOnlyMode]` 属性，让 `ReadOnlyModeFilter` 放行 portal 接口。
- [ ] 限流：给匿名的 POST 接口加 `AddRateLimiter`。
- [ ] **发邮件的能力**：iop-core 现在完全不能发邮件（代码里的 notifier 只写审计日志）。订阅、新闻邮件、评论通知、showcase 投稿通知都要用：
  - 用 MailKit（MIT）通过 SMTP 中继发送，或者用 Azure Communication Services Email。要确认 BFS 能用哪一种，以及数据存在哪里；
  - 邮件模板每种语言一份，放在仓库里；
  - 发件域名配好 SPF、DKIM 和 DMARC；
  - 不用 Listmonk：它是 AGPL-3.0，在 I14Y 的许可证黑名单上，原团队的实例也拿不到。详见 `docs/iop-core-gaps.md` 的 "Building it again without Listmonk"。
- [ ] 密钥：iop-core 的 Container App 通过托管身份从 Key Vault 读取发邮件的凭据。
- [ ] CORS：请运维在 iop-core 的 ingress 上允许 mds-ui 的域名使用 `POST`、`PUT` 和 `Content-Type` 请求头。

### V4 第 2 步：邮件订阅（G12），两条路一起做

用邮箱订阅数据集、分类或组织，每天或每周收到摘要邮件。

| 用户 | 怎样订阅 | 怎样管理 |
|---|---|---|
| 未登录的公众（绝大多数） | 填邮箱，点确认邮件里的链接后生效 | 每封邮件里的签名链接 |
| 已登录的内部人员（eIAM） | 直接用账号里的邮箱，不需要确认 | 管理区里的"我的订阅"，邮件里的链接也有效 |

- [ ] **一张订阅者表**，加一个可以为空的"eIAM 用户"字段。内部人员以前用同一个邮箱匿名订阅过的话，登录后订阅时合并成一条，避免收到两份摘要邮件。
- [ ] 登录用户的邮箱**只能取自** `/api/Users/current`，不能自己填，否则就能替别人的邮箱订阅、绕过确认。eIAM 里的邮箱改了，下次登录时同步更新。
- [ ] mds-ui：订阅表单（登录和未登录两种状态），确认页 `/subscription/confirm`，管理页 `/subscription/manage`，以及管理区里的"我的订阅"。
- [ ] 摘要邮件：iop-core 的定时任务用 G10（V2 已经做好）找出更新的数据集，按订阅者的语言发送。
- [ ] 原来的订阅者在原团队的 Listmonk 里，拿不到，从零开始。
- [ ] 完整流程和要避免的问题，见 `docs/iop-core-gaps.md` 的 G12 一节。

### V4 第 3 步：新闻邮件

OGD 办公室给订阅了新闻邮件的人群发通讯。原来的 mds-ui 代码里没有这个功能，是新增的。

- [ ] 和邮件订阅共用订阅者表：在订阅表单里多一个"接收新闻邮件"的选项，同样要确认邮箱。
- [ ] 管理区里的撰写和发送界面：四种语言、预览、先发给自己测试、定时发送。只有"新闻邮件编辑"角色能用。
- [ ] 用第 1 步的发信能力分批发送；每封邮件都有退订链接和一键退订的邮件头。

### V4 第 4 步：CMS 和 showcase 投稿

- [ ] 按 `docs/CmsComparison.md` 的比较，选定内容系统（"需要你决定的事"第 1 条）：Decap、Sveltia、Orchard Core，或者自己开发并嵌入管理区的内容模块。
- [ ] 公众投稿 showcase：投稿进入选定系统的待审流程。基于 Git 的系统由 I14Y 自己的 GitHub App 开 PR；基于数据库的系统直接进待审队列。提交表单原来的实现在提交 `33c04f4` 里：`app/pages/showcases/submit.vue`、`server/api/showcases.post.ts`。
- [ ] 编辑从直接在 GitHub 上改内容，切换到新的内容系统。如果内容迁进了数据库，还要把公开页面从 Nuxt Content 改为读取新的接口。

### V4 第 5 步：评论（G11）

- [ ] 按 `docs/HyvorCommentsNote.md` 里 "Replacing Hyvor Talk" 的比较，选定方案：Comentario、Remark42、Isso，或者自己开发的评论模块（发布者可以用 eIAM 以官方身份回复）。
- [ ] 按同一份文档的 "Adding comments back" 加回评论区和评论数；有新评论时，通过 Portal 模块给数据集联系人发邮件。
- [ ] 旧评论在原团队的 Hyvor 账号里，拿不到，从零开始。

## V5：把 admin-ui 的功能搬进 mds-ui，删除 admin-ui

V4 完成以后开始（也就是之前讨论里的"V3b"）。用 V3 做好的 eIAM 登录和管理区。

2026-10-08 决定：admin-ui（I14Y 的 input 门户 `input.i14y.d.c.bfs.admin.ch`）以后会删除，它的所有功能都在 mds-ui 里重新实现。功能清单、admin-ui 的登录方式、iop-core 已有和缺少的接口，见 `docs/iop-core-gaps.md` 的 "G13: admin functions and login"。

**要点：**

- **范围**：编辑所有 I14Y 资源，包括数据集、数据服务、公共服务、概念、映射表，以及搜索总览、注册状态和发布级别、版本、导入导出。admin-ui 大约有 90 个组件。另外加上原来 mds-ui 的 GOGD 管理菜单。
- **后端**：admin-ui 现在调用的 Admin API 会删除，但 Core API 已经有所有这些资源的读写接口。所以主要是前端的工作，用 mds-ui 已经从 Core swagger 生成的 orval 客户端即可。每个页面重做时，还要逐个核对接口。
- **admin-ui 和 Admin API 要一直运行到 V5 结束**：请后端团队在那之前不要删除 Admin API。
- **原则**：一个资源类型一个资源类型地做。admin-ui 一直保持运行，直到 mds-ui 能完全替代它。

### V5 第 1 步：搜索总览

- [ ] 对应 admin-ui 的 `/catalog/*`：搜索所有资源，可以按类型、发布者、注册状态、发布级别、主题、格式等筛选。用 iop-core 的 `Search` 接口。

### V5 第 2 步：逐个资源类型重做（从简单到复杂）

每类资源都包括：新建、编辑、删除；版本；注册状态和发布级别，以及修改提议；导入和导出。建议先做组件少的，例如数据服务（admin-ui 里 5 个组件），最后做数据集（24 个组件）。

- [ ] 数据服务：它服务哪些数据集。
- [ ] 映射表：关系，以及关系的导入导出。
- [ ] 概念：代码表的条目，以及条目的导入导出；锁定；结构引用。
- [ ] 公共服务：渠道、前提条件和关系。
- [ ] 数据集：分发和访问服务、数据结构（类和属性，导入导出）、质量信息、在 DCAT 目录里的条目。

### V5 第 3 步：GOGD 管理菜单

- [ ] Dashboard、质量和指标、DCAT 分类。
  - 后端要提供 OGD 办公室的管理员角色，以及这些页面需要的接口。iop-core 已经有一部分：组织统计 `GET /api/Agents/statistics`、数据集质量信息 `/api/DatasetQualityInformation`、词表配置 `/api/Vocabularies/configurations`。
  - Dashboard 和质量指标页原团队从来没做出来，要先定义显示什么。

### V5 第 4 步：切换到 mds-ui，删除 admin-ui

- [ ] 两边同时运行一段时间，让录入数据的用户验收。
- [ ] input 门户的域名改为指向 mds-ui 的管理区，或者重定向过去，保住用户的书签。
- [ ] 页头的"登录"按钮改为在 mds-ui 里登录、进入管理区。`NUXT_PUBLIC_LOGIN_URL` 届时不再需要。
- [ ] public-ui 的 `ADMIN_APP_ROUTE`（`src/assets/config/appconfig.json`）改为指向 mds-ui 的管理区。
- [ ] 改写 admin-ui 的端到端测试（`tests/e2e-tests/src/Bfs.Iop.Admin.Testautomation`），让它测 mds-ui。
- [ ] 删除 admin-ui：`frontend/admin-ui`、`docker/Dockerfile.admin-ui`，以及部署工作流 `i14y-admin-ui-dev-deploy.yml` 和 `i14y-frontend-release-deploy.yml` 里 admin-ui 的部分。
- [ ] Admin API 的删除由后端团队安排，要在 admin-ui 停用之后。

## 需要你决定的事

| # | 问题 | 最晚什么时候要决定 |
|---|---|---|
| 1 | V4 用哪个内容系统：Decap、Sveltia、Orchard Core，还是自己开发并嵌入管理区的内容模块（比较见 `docs/CmsComparison.md`）。V1 不用 CMS 已经定了（2026-10-08），编辑直接在 GitHub 上改内容。 | V4 开始之前 |
| 2 | 数据集网址用 DCAT 标识符（现在的做法）还是 GUID；旧的 piveau 网址要不要加重定向。要先向 iop-core 团队确认标识符是否唯一、是否会变。 | V1 第 8 步（部署）之前 |
| 3 | 数据预览：现在的预览调用原团队部署的 piveau 预览服务，已经不能用。<br>- V1 删除（建议），以后需要时再自己做一个预览；<br>- V1 就做一个替代方案。 | V1 第 2 步之前最好，最晚第 6 步 |
| 4 | G1：V1 没有按目录过滤，网站会显示 iop-core 里所有公开的数据集，不只是 opendata.swiss 的。V1 能不能接受？ | V1 上线之前 |
| 5 | 旧邮件里的链接：偏好设置页和 showcase 提交页删除后会返回 404，要不要重定向到一个说明页？<br>现有的订阅者和链接签名密钥都在原团队的 Listmonk 里，I14Y 拿不到，所以没法通知他们，也没法迁移；G12 做好后，订阅从零开始。 | V1 上线之前 |
| 6 | 后端的改动：`main-mvp` 上提交了 `Bfs.Iop.Core.Api.ClientGenerator/Program.cs` 里的 `StoreSwaggerJson = true`，以及生成的 `IopCoreApiClient.swagger.json`（约 2.2 万行）。要么保留，要么把 swagger 文件复制到 mds-ui，后端恢复原样。 | 合并进 `main` 之前 |
| 7 | 环境信息：<br>- 各环境的 iop-core 地址、登录地址和 mds-ui 的域名；<br>- AKS：用哪个集群和 namespace；ingress controller 是什么（原来的配置用的是 nginx）；TLS 证书怎么签发（例如 cert-manager）；镜像仓库；GitHub Actions 怎样获得部署权限；集群由谁运维。 | V1 第 8 步（部署） |
| 8 | 评论功能的方案（G11），比较见 `docs/HyvorCommentsNote.md` 的 "Replacing Hyvor Talk"；以及是不是真的需要公开评论。 | V4 开始之前 |
| 9 | CMS 内容仓库：GitHub 上 `opendata-swiss` 组织由谁管理？I14Y 有没有 `opendata-swiss-cms-content`（以及 `-int`、`-test`）的写权限？<br>- 有：照常使用；<br>- 没有：把内容复制到 I14Y 自己的 GitHub 组织，并修改 `docker/Dockerfile.mds-ui` 的 `CMS_CONTENT_REPO`、`package.json` 的 `predev` 和 `src/admin/config.yml`。 | V1 第 3 步之前（要在内容仓库里做数据迁移） |
| 10 | 邮件订阅（G12）：<br>- 身份已经定了：公众用邮件里的链接，内部人员用 eIAM 账号，两条路在 V4 一起做；<br>- 还要决定：订阅存在哪里、谁发邮件，iop-core 加 SMTP，还是自己部署 Listmonk（需要许可证例外）。比较见 `docs/iop-core-gaps.md` 的 "Open for discussion"。 | V4 开始之前 |
| 11 | 新的界面设计：由谁设计、什么时候定稿、用哪个版本的联邦设计系统？V2 要按它改造公开页面，所以 V1 不要在界面细节上花太多工夫。 | V2 开始之前 |
| 12 | 从 V1 上线到 V4，编辑都要直接在 GitHub 上改 Markdown，这可能持续很久。OGD 办公室能不能接受？不能接受的话，要么把 CMS 提前，要么在 V1 之后加一个临时编辑器（例如 Sveltia，约 6–8 人天），但这违背"每个功能只做一次"的原则。 | V1 上线之前 |

## 顺手处理的小事

- [ ] 删除本地那个名叫 `HEAD` 的分支。它让每个 git 命令都提示 `refname 'HEAD' is ambiguous`。它指向的提交 `85fb242` 在远端分支上已经有了，删除不会丢东西：

  ```bash
  git branch -D HEAD
  ```

- [ ] 卸载 VS Code 的 Vetur 插件。它是给 Vue 2 用的，会在 Problems 面板里报很多假错误：

  ```bash
  code --uninstall-extension octref.vetur
  ```

- [ ] 把全局的 `NODE_TLS_REJECT_UNAUTHORIZED=0` 换成 `NODE_EXTRA_CA_CERTS`，指向 BIT Proxy Root CA 的文件。前者会关掉所有 TLS 证书检查。
- [ ] 在 `agent-toolbox` 里更新代码知识图谱。现在的图谱里完全没有 mds-ui。

## 变更记录

- **2026-10-06**：定下 V1 和 V2，以及 V1 的阶段 0–9。
- **2026-10-07**：
  - 客户端改用 orval，不用原计划的 hey-api。
  - 阶段 3 完成。
  - V1 不改 iop-core，缺少的接口记在 `docs/iop-core-gaps.md`，原来 V1 的阶段 2 挪到 V2。
  - 关闭评论，删除 Hyvor（S6、S7），评论服务记为 G11。
- **2026-10-08**：
  - 深度分析之后，加入安全、性能、SEO、测试和 CI 的步骤。
  - mds-ui 不保留邮件订阅，以后由后端提供，记为 G12。
  - V1 和 public-ui 一样只做公开展示：没有登录，没有管理功能，showcase 提交也去掉。删除这些功能合并成第 1 步。V2 从"把服务端功能迁到 iop-core"改为"由 iop-core 提供交互功能，mds-ui 加回界面"。
  - 改正一处错误：摘要邮件的 `subscriptions.yaml` 工作流在原团队的仓库里，不在本仓库。
  - mds-ui 部署在 AKS 上，不用 Container App。原团队的 `k8s/` 因此改为保留并改写，不再删除。
  - V1 删掉的管理功能和登录记为 G13，以后加回。
  - admin-ui 以后会删除，它的所有功能都在 mds-ui 里重新实现。G13 因此扩大到 admin-ui 的全部功能；管理页放哪里的问题有了答案：放在 mds-ui 里。
  - V2 之后加一个版本 V3，专门做管理功能：登录、搜索总览、各类资源的编辑、GOGD 管理菜单，最后删除 admin-ui。总体安排从两个版本改为三个版本。
  - 项目由 I14Y 接手，不再使用 Zazuko 部署的服务，也没有他们的账号。新增"不再使用 Zazuko 的服务和账号"一节，并做了这些修改：
    - 数据预览、Decap 登录不能再用，建议 V1 删除；
    - 原来的订阅者和旧评论拿不到，V2 从零开始；
    - V2 不用 Listmonk（AGPL-3.0），邮件改由 iop-core 的 Portal 模块发送；
    - 删除了"请 Zazuko 把新域名加进 Netlify"的选项；
    - 新增"需要你决定的事"第 9 条：CMS 内容仓库的权限。
  - V3 的用户信息改为从 iop-core 取（`/api/Users/current`、`AllowActions`），不再像 admin-ui 那样自己解析 token：iop-core 负责连接 Keycloak 并提供用户信息。
  - V3 的登录：先按"mds-ui 不能连接 Keycloak"改成由 iop-core 代为登录（BFF）；随后澄清，用户是用 eIAM 登录、由 eIAM 给 token，照 admin-ui 的做法。最终方案：浏览器里用 eIAM 登录（`oidc-client-ts`，授权码 + PKCE），token 交给 iop-core；mds-ui 不连 iop-core 另外接受的那个 Keycloak。BFF 方案撤回，后端不需要新增登录接口，只需要在 eIAM 登记 mds-ui 的回调地址。
  - 公众的邮件订阅不需要登录：通过确认邮件和邮件里的链接完成，流程写进了 G12。
  - `docs/HyvorCommentsNote.md` 新增 "Replacing Hyvor Talk"（英文，供团队讨论，不给建议）：比较 Comentario、Remark42、Isso 和自己开发的评论模块，包括架构图、评论流程、对比表、工作量、优缺点和决定性问题。
  - 新增 `docs/CmsComparison.md`（英文，供团队讨论）。先比较了保留 Decap 和换成 Sveltia；后来改为比较四个方案，不给建议：Decap、Sveltia、Orchard Core、自己开发并嵌入 mds-ui 管理区的内容模块。每个方案都有基于 I14Y 现有架构的架构图和编辑流程图，以及易用性、难度、风险、优缺点的对比。Nuxt Studio 不再列入。
  - 确定内容一直放在单独的内容仓库里，不放进主仓库。这会影响 CMS 的选择：Nuxt Studio 默认把内容提交到网站代码所在的仓库，用于单独内容仓库的做法没有文档说明，要先验证。
  - G12 新增 "Open for discussion"（英文，供团队讨论）：公众身份的三种做法（邮件链接、自建 public 账号、AGOV），以及订阅的存储和发信（iop-core 加 SMTP，或自己部署 Listmonk 及其架构）。新增"需要你决定的事"第 10 条。
  - **改为四个版本**：V1 公开展示上线（不用 CMS，删除 Decap，编辑直接在 GitHub 上改内容）；V2 补齐 iop-core 接口（G1–G10）、适配新的界面设计、SEO 和 GEO；V3 管理功能和 eIAM 登录；V4 邮件订阅、新闻邮件、评论、CMS 和 showcase 投稿。原则是每个功能只做一次、不做临时方案，开发成本低最重要。
    - 原来 V2 的 Portal 模块、发邮件的能力、showcase 投稿、邮件订阅、评论都挪到 V4；G10 留在 V2，和其他接口一起做。
    - 邮件订阅的两条路（公众用邮件链接、内部人员用 eIAM 账号）在 V4 一起做，用同一张订阅者表。
    - 新增新闻邮件（V4 第 3 步），这是原来 mds-ui 没有的功能。
    - 删除原来 V2 的"决定 mds-ui 的部署方式"：mds-ui 已经定在 AKS 上。
    - V1 第 5 步改为删除 Decap，并把 `docs/cms.md` 改写成给编辑的 GitHub 编辑指南；第 9 步加上"没有 bug"的验收标准。
    - "需要你决定的事"：第 1、8、10 条改到 V4；新增第 11 条（新的界面设计）和第 12 条（编辑在 GitHub 上编辑要持续到 V4）。
  - 新增 `docs/WorkPlan.md`：这份计划的英文简要版，只列每个版本的步骤和要先做的决定。
  - **改为五个版本**：V3 只保留登录和管理区框架（原来讨论里的"V3a"）；admin-ui 的功能和 GOGD 管理菜单挪到新的 V5（原来的"V3b"），放在 V4 之后。admin-ui 和 Admin API 要一直运行到 V5 结束。页头"登录"按钮的切换也挪到 V5。
  - 新增"工期估计"一节：各版本按步骤粗估的人天，合计约 190–370 人天；按 1 前端 + 1 后端估算约 9–15 个月；列出最影响工期的因素和现在就能做的事。
