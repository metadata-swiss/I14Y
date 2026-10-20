# mds-ui 下一步计划

整理于 2026-10-08，2026-10-13 更新（CMS 和 showcase 的决定），2026-10-14 加上 V6，2026-10-20 更新 V1 的进度和部署（第 8 步）。分支 `main-mvp`。每次修改计划，都记在文末的"变更记录"里。

## 总体安排：分六个版本

2026-10-06 定下了最初的方向：

> in the first version delete all the function only link to piveau and replace all things can be replace by iop core. and second version move the server feature to the iop core, but keep seo.

2026-10-08 改为五个版本，2026-10-14 加上 V6，2026-10-16 调整 V2 和 V3。每个版本只有一个主题：

| 版本 | 主题 | 一句话 |
|---|---|---|
| **V1** | 公开展示上线 | 只保留展示功能（包括 showcase 的展示），去掉所有 piveau 代码和对 Zazuko 账号的依赖，页面正常运行没有 bug，打包成容器，通过工作流部署到 AKS。不用 CMS，编辑直接在 GitHub 上改内容 |
| **V2** | 后端接口和新界面 | 补齐 iop-core 缺少的接口（G1–G10，以及数据预览的 G15）；在后端建好 showcase，和其他对象一样创建和管理（G14 的后端部分）；适配新的界面设计，加强 SEO（包括 GEO 也要用的结构化数据） |
| **V3** | 登录、管理区框架和 public-ui 的功能 | 用 eIAM 登录，建好管理区的框架和权限，V4 和 V5 都建立在它上面；把 public-ui 的功能加进 mds-ui（先列清单，搬哪些待定） |
| **V4** | 交互功能和 CMS | 邮件订阅、新闻邮件、评论；showcase 的前端改用 iop-core（管理区编辑、公众投稿、公开页面、迁移）；页面、手册、博客用 Nuxt Studio 编辑。所有编辑都用 eIAM 登录 |
| **V5** | admin-ui 的功能 | 把 admin-ui 的所有功能搬进 mds-ui，最后删除 admin-ui |
| **V6**（加分项） | GEO、AI 助手和推荐 | **只在 V1–V5 完成、还有余量时做。**GEO（SEO 的普通扩展、把数据和 showcase 推给 AI）；AI 助手（元数据助手、自然语言搜索）；推荐（newsletter 推荐、个性排序）。另列了可能增加的功能 |

**原则：每个功能只做一次，不做临时方案。** 开发成本低最重要。所以：

- V3 先只做登录和管理区框架，工作量不大；V4 和 V5 都直接用它，不需要临时的审核界面或临时工具；
- admin-ui 在 V5 结束前照常运行，所以 V5 可以放在最后，公众功能（V4）先上线；
- 邮件订阅的两条路一起做：未登录的公众用邮件里的链接，已登录的内部人员直接用账号（见 V4 第 2 步）；
- **V6 是加分项，不是承诺**：工期和人手首先保证 V1–V5 完成；V6 只用剩下的余量做，余量不够就不做（2026-10-14 决定）。

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

- 补齐 `docs/iop-core-gaps.md` 里的 G1–G10，恢复 V1 里隐藏的筛选条件、排序和元数据下载；G15 让分发页的数据预览重新可用。
- 在 iop-core 里建好 `Showcase`（G14 的后端部分）：实体、增删改查接口、两种权限、草稿/审核/发布，和 Dataset 等对象一样。前端、公众投稿接口和迁移仍在 V4（2026-10-16 决定）。
- 按新的界面设计改造公开页面。设计由谁出、什么时候定稿，见"需要你决定的事"第 11 条。
- 加强 SEO。其中的结构化数据（schema.org、sitemap、DCAT 导出）也是 V6 做 GEO 的基础。

### V3：登录和管理区框架

- mds-ui 重新有登录：和 admin-ui 一样用 eIAM 登录，登录后由 eIAM 给 token；用户信息和权限由 iop-core 提供。
- 建好管理区的框架：导航、布局、权限控制。具体功能在 V4 和 V5 里放进来。
- 把 public-ui 的功能加进 mds-ui（2026-10-16 加入）。先列功能清单，搬哪些、是否取代 public-ui 待定，见 V3 第 2 步。

### V4：交互功能和 CMS

- 邮件订阅（G12）、新闻邮件、评论（G11）。
- 内容（2026-10-13 决定）：
  - **showcase 搬进 iop-core**（G14），在管理区里编辑，公众投稿直接进 iop-core。后端的实体和接口在 V2 已经做好；
  - **页面、手册、博客留在 GitHub 内容仓库**，用 Nuxt Studio 可视化编辑；
  - **所有编辑都用 eIAM 登录**，不需要 GitHub 账号。
- 都建立在 V3 的登录和管理区之上；发邮件的能力放在 iop-core 的 Portal 模块里。

### V5：把 admin-ui 的功能搬进 mds-ui

- admin-ui（I14Y 的 input 门户）的所有功能，在 mds-ui 的管理区里重新实现，然后删除 admin-ui。
- 原来 mds-ui 的 GOGD 管理菜单也放进这个管理区。
- 请后端团队在 V5 结束前不要删除 Admin API。

### V6（加分项）：GEO、AI 助手和推荐

- **只在 V1–V5 完成、还有余量时做**；V1–V5 的工期估计不包括它。按顺序做，余量用完就停在那一步，已经做完的每一步都能单独上线。
- **GEO**：
  - 第一步，SEO 的普通扩展：在 V2 的 SEO 基础上补更多结构化数据，让内容更容易被引用。成本低，对搜索引擎和 AI 都有用；
  - 第二步，把我们的数据和 showcase 推给 AI：主动把目录提交给 AI 助手所依赖的搜索引擎、数据门户和知识库，让 AI 回答问题时引用 opendata.swiss 的数据。
- **AI 助手**（都通过 iop-core 里新的 AI 网关调用大模型）：
  - 元数据助手：在管理区编辑数据集时，建议关键词、主题、摘要和翻译，指出缺少的信息，由人逐条确认；
  - 自然语言搜索：把一句话的搜索转换成筛选条件，再用普通搜索查真实数据，模型不编写答案。
- **推荐**：I14Y 的数据本来就是一张关系图（数据集、机构、主题、概念和代码表、showcase），按两个对象在图上的远近来推荐：
  - 网站上的"相关数据集"，以及新闻邮件和摘要邮件里的个性化推荐；
  - 个性排序：按兴趣调整搜索结果、列表和邮件条目的顺序，只调顺序，不隐藏任何内容。
- 另外列出了**可能增加的功能**，没有排期，需要时再评估。

服务端功能 S1–S10 的定义见 `tmp/mds-ui-core-integration/mds-ui-core-integration-analysis.md`。

## 工期估计（2026-10-08，粗估）

单位是**人天**（一个人干一天），不包括等待外部审批的时间。这些都是粗略估计，误差可能有 ±50%。V1 做完以后，用实际花掉的时间回头校准一次。

### 各版本的工作量

**V1：约 20–36 人天**

| 步骤 | 人天 | 说明 |
|---|---|---|
| 1 删除非展示功能 | 2–3 | |
| 2 安全、性能、SEO 的问题 | 1–2 | |
| 3 Showcase 展示和内容迁移 | 4–6 | 列表页重写；内容仓库的迁移暂停到 V4 以后 |
| 4 网址和重定向 | 0 | 2026-10-16 决定：网址用标识符（现在已是），旧网址不做重定向 |
| 5 Decap | 1–2 | 2026-10-16：Decap 里的 piveau 已删；删除 Decap 推迟到 V2 的 showcase 后端完成以后 |
| 6 彻底去掉 piveau，清理代码 | 3–5 | |
| 7 测试和 CI | 3–5 | |
| 8 部署到 AKS | 3–5 | 2026-10-20：dev 已上线，剩下部署工作流和 int、prod。浏览器经 mds-ui 的代理访问 iop-core，不需要 iop-core 的 CORS |
| 9 测试和上线 | 3–5 | |

**V2：约 52–91 人天**

| 部分 | 人天 | 说明 |
|---|---|---|
| iop-core 接口 G1–G10（后端） | 15–25 | 要看后端新搜索服务的进度 |
| 数据预览接口 G15（后端） | 6–12 | 不含 ODS；前端不用改，只要设置 `NUXT_PUBLIC_DATA_PREVIEW_URL` |
| showcase 的后端 G14（实体、接口、权限、AuditTrail、测试） | 10–14 | 2026-10-16 从 V4 提前到 V2 |
| 按新界面改造公开页面 | 15–30 | 最不确定：取决于新设计的改动有多大，设计本身的时间没算 |
| 把恢复的筛选、排序、元数据下载放进界面 | 3–5 | |
| SEO（包括 GEO 也要用的结构化数据） | 3–5 | |

**V3：约 6–10 人天，加上 public-ui 的功能。** 登录和管理区框架 6–10 人天，另外要等 eIAM 登记回调地址，可能要几周。public-ui 的功能全部搬过来约 41–66 人天（见 V3 第 2 步），搬哪些还没定，所以下面的合计不含这部分。

**V4：约 59–116 人天，取决于评论用现成产品还是自己开发**

CMS 已经定了（2026-10-13），所以只有评论还要在"现成产品"和"自己开发"之间选。

| 部分 | 评论用现成产品 | 评论自己开发 |
|---|---|---|
| Portal 模块和发邮件的能力 | 6–10 | 6–10 |
| 邮件订阅（两条路） | 13–20 | 13–20 |
| 新闻邮件 | 8–13 | 8–13 |
| showcase 前端改用 iop-core 和 Nuxt Studio（包括投稿和两个原型，见 `docs/CmsComparison.md`；后端在 V2） | 25–38 | 25–38 |
| 评论 | 7–12（Isso）到 9–15（Comentario） | 23–35 |
| **合计** | **约 59–96** | **约 75–116** |

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

**V6（加分项）：约 49–79 人天，不算进下面的合计**

| 部分 | 人天 |
|---|---|
| 第 1 步：GEO 一，SEO 的普通扩展 | 3–5 |
| 第 2 步：GEO 二，把数据和 showcase 推给 AI | 5–10 |
| 第 3 步：AI 网关和元数据助手 | 10–15 |
| 第 4 步：自然语言搜索 | 6–10 |
| 第 5 步：推荐，包括 newsletter 推荐（关系图和每晚计算、接口、网站、邮件、评估） | 22–34 |
| 第 6 步：个性排序 | 3–5 |

"可能增加的功能"没有估计。另外要等：GEO 第二步要 AI 爬虫政策的法律意见；AI 助手要选定模型服务并通过数据保护审查；推荐和个性排序要数据保护审查。另外还有模型服务的使用费用。

### 合计和日历时间

承诺的范围是 V1–V5；V6 只用余量做，单独列出。

| 版本 | 人天 |
|---|---|
| V1 | 20–36 |
| V2 | 52–91 |
| V3 | 6–10，另加 public-ui 的功能（全部约 41–66） |
| V4 | 59–116 |
| V5 | 80–130 |
| **V1–V5 合计** | **约 217–383**（不含 public-ui 的功能） |
| V6（加分项） | 49–79 |

每人每月按大约 15 个有效工作日算（开会、评审、修 bug 已经扣除）。下表只算 V1–V5：

| 团队 | 纯开发时间 | 加上等待、协调、验收以后 |
|---|---|---|
| 1 人 | 15–26 个月 | 18–31 个月 |
| 2 人（1 前端 + 1 后端） | 7.5–13 个月 | 9.5–15.5 个月 |
| 3 人（2 前端 + 1 后端） | 5–8.5 个月 | 6.5–11.5 个月 |

V6 全部做完，按 2 人大约还要 2–3 个月。public-ui 的功能如果全部搬过来，按 2 人大约多 1.5–2 个月（2026-10-16 加入，上面的表还没算进去）。

按 2 人估算，每个版本大概的上线时间：

| 版本 | 大约需要 | 累计 |
|---|---|---|
| V1 | 1–2 个月 | 1–2 个月 |
| V2 | 2–3.5 个月 | 3–5.5 个月 |
| V3 | 约 2 周，加上 eIAM 的等待 | 3.5–6 个月 |
| V4 | 2.5–4.5 个月 | 6–10.5 个月 |
| V5 | 3–5 个月 | 9–15.5 个月 |
| V6（加分项，有余量时） | 2–3 个月 | — |

### 最影响工期的因素

1. **V4 的评论用现成产品还是自己开发**：两者差 16–20 人天，大约 1 个月。CMS 已经定了，不再是变数；但如果 V4 的两个原型（Nuxt UI 能否放进 mds-ui、Nuxt Studio 能否接 eIAM 和我们的内容仓库）不通过，备用方案还要多 4–15 人天。
2. **V2 新界面的改动有多大**：设计还没有出来，这是最大的未知数。
3. **外部等待**：AKS 的权限和域名（V1）；eIAM 登记（V3）；发信服务、发件域名和数据保护审查（V4）；后端新搜索服务的进度（V2）。
4. **旧网址能不能对应到新标识符**：对应不上的话，V1 第 4 步会明显变大。

### 现在就能做、可以缩短工期的事

- [ ] 现在就申请：eIAM 回调地址的登记、AKS 的权限、发信服务和发件域名。这些可能要等几周。
- [ ] V2 的后端接口可以和 V1 同时开始：V1 的"不改 iop-core"指的是 mds-ui 不依赖新接口，后端团队可以先做。
- [ ] 尽早拿到新的界面设计，这样 V1 就不用在马上会被替换的界面上花时间。
- [ ] V3 和 V4 的后端部分（Portal 模块、发信能力）可以和前端同时进行。
- [ ] V4 的 `Showcase` 实体和接口（G14）不依赖登录，后端可以在 V2 期间先做。

## V1 的范围

| 功能 | V1 | 之后的版本 |
|---|---|---|
| S1 Keycloak 登录 | 删除 | V3，改用 eIAM 登录（G13） |
| GOGD 管理区（`/gogd/*`） | 删除 | V5，放进 mds-ui 的管理区（G13） |
| S2 订阅、S3 偏好设置、S4 摘要邮件 | 删除 | V4，由 iop-core 提供，公众和登录用户两条路一起做（G12） |
| S5 Showcase 提交 | 删除。showcase 照样显示，新的 showcase 由编辑在 GitHub 内容仓库里添加 | V4，投稿直接写进 iop-core（G14） |
| S6 评论通知、S7 评分 | 已随 Hyvor 删除 | V4，评论的方案见 G11 |
| S8 Showcase JSON-LD 接口、S9 收录触发插件 | 删除，只为 piveau 存在 | 不再需要 |
| S10 服务端的 piveau 客户端 | 删除 | 不再需要 |
| Decap CMS 编辑器（`/admin`） | 删除。V1 不用 CMS，编辑直接在 GitHub 上改内容 | V4：showcase 在管理区编辑（G14），页面、手册、博客用 Nuxt Studio，都用 eIAM 登录 |

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
| Netlify 项目和 passbolt 里的密码 | Decap 编辑器的 GitHub 登录 | 不能再用。V1 删除 Decap；V4 的 Nuxt Studio 用 eIAM 登录，不需要 GitHub 登录 |
| GitHub App `piveau-ui-github-app` | showcase 提交时创建 PR | V1 第 1 步删除。V4 的投稿直接写进 iop-core，不再需要 GitHub App |
| 原团队的部署：域名 `piveau.ods.zazukoians.org`、通配符证书、镜像 `ghcr.io/metadata-swiss/ods-ui`、PR 预览部署 | 运行网站 | V1 第 8 步改写 `k8s/`；README 里预览部署一节删除 |
| 原团队仓库里的工作流（`opendata-swiss/metadata.swiss` 的 `subscriptions.yaml`） | 定时发摘要邮件 | 不再需要 |
| 通知邮件收件人 `noreply@zazuko.com` | showcase 提交通知 | 随 V1 第 1 步删除 |
| 文档：README、`docs/authentication.md`、`docs/cms.md`、`docs/cms/showcases.md`、`docs/subscriptions/index.md` | 写的都是原团队的地址和账号 | V1 第 1 步改写或删除 |

**原则（2026-10-08）：内容一直放在单独的内容仓库里，不放进 I14Y 的主仓库。** 2026-10-13 起，这只适用于页面、手册和博客；showcase 在 V4 搬进 iop-core。Nuxt Studio 能不能用单独的内容仓库，要在 V4 的原型里验证。

另外，CMS 内容仓库（GitHub 上 `opendata-swiss` 组织下的 `opendata-swiss-cms-content`、`-int`、`-test`）不是 Zazuko 部署的服务，但构建镜像时要从那里克隆内容，编辑也要往里写。I14Y 有没有这个组织的权限，要先确认，见"需要你决定的事"第 9 条。

## V1 的进度

| 阶段 | 内容 | 状态 |
|---|---|---|
| 0 | mds-ui 在本地运行 | ✅ `e221ba0` |
| 1 | 生成 iop-core 客户端（最后用的是 orval，不是原计划的 hey-api） | ✅ `e938f84` |
| 2 | 补齐 iop-core 缺少的接口 | ⏸ V1 不改 iop-core，挪到 V2 |
| 3 | 页面改用 iop-core | ✅ `b13cde2`，只剩网址和重定向 |
| 4 | Showcase | 展示部分完成（2026-10-16，第 3 步），提交表单已删除；其余暂停到 V4 |
| 5 | 服务端功能 | ✅ 全部删除 `8c1e23d`（第 1 步） |
| 6 | CMS 编辑器（Decap） | Decap 里的 piveau 已删（2026-10-16）；Decap 等 V2 的 showcase 后端完成以后再删（第 5 步） |
| 7 | 彻底去掉 piveau | 基本完成（2026-10-16，第 6 步），剩下残留引用的搜索和清理 |
| 8 | 部署 | dev 已上线（2026-10-20，镜像 `0.1.1`，`https://mds.i14y.d.c.bfs.admin.ch`）；剩下部署工作流和 int、prod（第 8 步） |
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

**2026-10-14 完成（`8c1e23d`）**，文档在 2026-10-20 补完，只剩没打勾的截图一项。2026-10-20 按代码逐项核对过。

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

- [x] 服务端：`server/api/subscribe/`、`server/api/subscription/`、`server/lib/subscription/`。
- [x] 页面：
  - `app/pages/subscription/`（偏好设置页）；
  - 数据集页上订阅数据集和分类的表单（`app/pages/datasets/[datasetId]/index.vue`）；
  - 只有偏好设置页用到的组件 `OdsCheckbox`、`OdsRadio`。
- [x] 翻译：4 个语言文件里的 `message.subscribe`，以及偏好设置页的文案。

删除 showcase 提交（S5）。V4 要重做，所以先把设计记下来，见 `docs/ShowcaseSubmission.md`：

- [ ] 删除之前：在本地打开 `/de/showcases/submit`，截图整个页面、展开的数据集搜索和错误提示条，放进 `docs/img/showcase-submission/`。
  - 没有做（2026-10-20 发现）：页面删除前没有截图，`docs/images/showcase-form.png` 是原团队的旧图。需要时检出 `8c1e23d` 的上一个提交，在本地打开页面截图。
- [x] 页面 `app/pages/showcases/submit.vue`，以及 showcases 列表页上指向它的链接。
- [x] **保留**翻译，V4 直接用：`message.showcase.submission_form.*`、`message.header.navigation.showcase_submit`、`message.server.api.showcases.post.*`。
- [x] 服务端：
  - `server/api/showcases.post.ts`；
  - `server/lib/git.ts`、`server/lib/fs.ts`、`server/lib/images.ts`、`server/lib/showcaseStorage.ts`。
- [x] `src/schema/showcase` 里的 `submissionSchema`。同一个文件里给 `content.config.ts` 用的 showcase 内容 schema 要保留。

删除只为 piveau 存在的服务端功能（S8–S10）：

- [x] `server/api/showcases.ts`（S8）、`server/plugins/showcase-harvesting-trigger.ts`（S9）、`server/lib/piveau.ts`（S10）。

删除登录（S1）和 GOGD 管理区：

- [x] 文件：`server/api/auth/`、`server/lib/login.ts`、`server/middleware/basic-auth.ts`、`app/middleware/require-auth.ts`。
- [x] `useUserSession` 的调用：`app/app.vue`、`app/components/headers/OdsHeader.vue`、`app/components/headers/OdsTopHeader.vue`。页头的"登录"按钮保留，它只是链接。
- [x] GOGD 管理区：
  - `app/pages/gogd/`；
  - `app/composables/navigation-items.ts` 里 `adminOnly` 的菜单项；
  - 4 个语言文件里的 `message.header.navigation.admin`。

删除以后跟着清理：

- [x] `server/lib/listmonk/`。Listmonk 不再有任何用途。
- [x] `server/lib/` 和 `server/plugins/` 里不再被引用的文件，例如 `auth.ts`、`locale.ts`、`zod-locale.ts`、`log-config.ts`。删之前逐个确认。
- [x] 依赖：`nuxt-auth-utils`、`@octokit/auth-app`、`@octokit/rest`、`sharp`（连同 `nuxt.config.ts` 里 `dev:reload` 的 hook）。
- [x] `nuxt.config.ts`：
  - `listmonk`、`subscription`、`oauth`、`apiTunerTests`；
  - 整个 `showcases` 配置项：`maxImageWidth`、`submissionNotification` 只给提交用；`catalogId`、`resourceType` 现在没有代码读取；
  - 全部 `routeRules`，以及文件开头对 `NitroRouteConfig` 的类型扩展。
- [x] 测试：`server/tests/` 里的测试和测试数据都是测这些功能的，全部删除。`package.json` 里的 mocha、api-tuner 脚本和配置也一起删除。前端测试在第 7 步用 vitest 重新建。
- [x] 文档，去掉原团队的地址和账号（2026-10-20 完成）：README 的 piveau 变量和原团队地址删除，环境变量按现在的配置改正（Matomo 的变量名少了 `PUBLIC_`，补上 `NUXT_PUBLIC_DATA_PREVIEW_URL`）；两张没有引用的 `docs/*-pr-preview.uml.svg` 删除；`docs/subscriptions/` 开头加上说明。`docs/authentication.md` 挪到第 5 步；`docs/cms.md` 暂时不改（2026-10-16 决定）。
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
- [x] **修我造成的问题**：每个页面都加载数据预览用的图表库（piveau-preview-plugin、vega）。原因是整理文件时 `charts.client.ts` 被移进 `app/plugins/`，变成了全局插件。
  - 已修（2026-10-15，按第 3 条决定**保留预览**）：删除全局插件和全局样式；`registerCharts()` 和样式移进 `OdsPreview`，分发页懒加载这个组件；写死的原团队服务地址换成配置项 `NUXT_PUBLIC_DATA_PREVIEW_URL`，没有配置时不显示预览。预览服务由 iop-core 在 V2 提供（G15）。
- [x] 现在每个页面都会下载整个 CMS 页面数据库（`sql_dump.txt`），因为面包屑和菜单的内容查询没有包进 `useAsyncData`。
  - 包进去以后，查询结果会随服务端渲染的 HTML 一起传给浏览器，浏览器不用再查一次。
  - 文件：`app/composables/breadcrumbs.ts`、`app/composables/navigation-items.ts`。
  - 已修（2026-10-15）：菜单的查询本来就在 `useAsyncData` 里。面包屑的首页一项改用翻译文字，不再查询内容；内容页的面包屑查询包进 `useAsyncData`。数据集页和分发页的面包屑改由 `useDatasetBreadcrumbs` 生成，在服务端渲染（以前只在浏览器里），"搜索结果"一项也能在服务端显示；sessionStorage 只存搜索条件，不在 `computed` 里写。组织页和分发页的最后一项补上了链接。
- [x] 不存在的数据集、组织和博客文章现在返回 200 和空白页。改为用 `createError` 返回 404。
- [x] 页面里嵌套了两个 `<main>`，而且 id 相同。已改为页头和页脚在 `<main>` 外面，`<main>` 只包正文（2026-10-15）。
- [x] 5 处"阅读更多"链接写的是 `aria-label="false"`，已删除。网站不另做无障碍方面的工作。

### 第 3 步：阶段 4，Showcase 的展示部分

V4 会把 showcase 搬进 iop-core（G14），到时只换数据来源。所以这一步把"读数据"和"显示"分开：所有 showcase 数据都通过一个 composable（例如 `useShowcases()`）读取，列表、筛选、详情和卡片组件只依赖它返回的数据。V4 只改这个 composable，页面不用重写。

- [x] 列表页改为读取 Nuxt Content（按分类、类型、关键词筛选，按标题和日期排序）。
  - 已改（2026-10-16）：读取、搜索、筛选、排序和分页都在 `app/composables/useShowcases.ts`，V4 改用 iop-core（G14）时只换读取的部分。只按标题排序；按日期排序在 V4 由 iop-core 提供（2026-10-16 决定：内容里不加日期字段）。
- [x] 详情页中数据集和分类的名称，改为从 iop-core 获取。
  - 已改（2026-10-16）：分类名称来自 iop-core 的词表。2026-10-16 起 showcase 的主题改用 I14Y 主题（`Concept_DATASET_THEME`，存代码，例如 `117`），和 iop-core 其他对象的主题一样；之前写的 showcase 还是 EU 数据主题的 URI，这些在网站上不显示，以后迁移时再改。数据集按 piveau URI 的最后一段在 iop-core 里查找标识符；找不到时显示内容里的 `label`，不带链接。现有 showcase 引用的数据集在 iop-core 里都找不到，要等第 4 步决定旧标识符怎么对应。showcase 类型在 iop-core 里没有词表，改为翻译（`message.showcase.type.*`，目前有 application 和 data_visualization）。
- [ ] ~~组织页上的 showcase 数量，改为从 Nuxt Content 统计~~ 暂停，V4 以后再做：V4 直接用 iop-core 的计数接口（2026-10-16 决定，现在固定显示 0）。
- [x] 首页的"推荐 showcase"区块不再使用 piveau。先显示编辑在首页内容里选的（`:showcases`），再显示置顶的，然后补足到 `max`。和列表页共用 `OdsShowcaseCard`。
  - 文件：`app/components/content/OdsSectionPromotedShowcases.vue`
- ~~showcase 内容的 schema 加上 `issued` 和 `modified` 字段。~~ 不做（2026-10-16 决定）。
- [ ] ~~在 CMS 内容仓库做一次性数据迁移~~ 暂停，V4 以后再做（2026-10-16 决定）：
  - piveau 数据集 URI 改为 iop-core 标识符，在 V4 的迁移脚本里一起做。
- [x] 删除 `app/piveau/showcases.ts`，以及只给它用的 `app/composables/useShowcaseTypes.ts`（2026-10-16）。
- [x] 完成后，typecheck 应该没有错误。（2026-10-16：0 个错误）

### 第 4 步：阶段 3 收尾，网址和重定向

第 2 条已决定（2026-10-16），这一步没有要做的代码改动。

- [x] 数据集网址用 DCAT 标识符，和 public-ui 一样（2026-10-16 决定）。public-ui 的网址是 `/{语言}/catalog/datasets/{identifier}`，IRI 是 `https://register.ld.admin.ch/i14y/dataset/{identifier}`，都用标识符，不用 GUID。mds-ui 现在就是这样（`/{语言}/datasets/{identifier}`），不用改。
- [x] ~~给旧的 piveau 网址加重定向~~ 不处理（2026-10-16 决定）：旧网站的数据集网址用的是 piveau/CKAN 的名称，切换域名以后这些旧链接返回 404，搜索引擎的收录会在重新抓取后换成新网址。
- [ ] 被删除的页面（`/subscription/preferences`、`/showcases/submit`）怎么处理，见"需要你决定的事"第 5 条。

### 第 5 步：阶段 6，去掉 Decap 里的 piveau；Decap 等 showcase 后端完成以后再删

V1 不用 CMS（2026-10-08 决定）。2026-10-16 决定：Decap 先保留，等 V2 的 showcase 后端（G14）完成以后再删；其中依赖 piveau 的部分现在就删掉。V4 的方案已经定了（2026-10-13，见 `docs/CmsComparison.md` 的 "Decision"）：showcase 在管理区编辑，页面、手册、博客用 Nuxt Studio，都用 eIAM 登录。

- [x] 去掉 Decap 里的 piveau（2026-10-16）：
  - showcase 表单的 4 个 piveau 选择器换成 Decap 自带的字段，保存的格式不变：主题是多选（26 个 I14Y 主题，存代码，和 iop-core 的 `Concept_DATASET_THEME` 一样），类型是下拉框（application、data_visualization），数据集和组织是手动填写 `id` 和 `label` 的列表；
  - 删除 `piveauWidgets.jsx` 和 6 个 piveau 组件、只给它们用的 `util.js`，以及 `vite/configTransform.ts` 里的 `NUXT_PUBLIC_PIVEAU_HUB_SEARCH_URL`；
  - 注意：Decap 在线上本来就不能登录（GitHub 后端的登录要经过原团队的 Netlify），现在只能在本地配合 `decap-server` 使用。
- [ ] 等 V2 的 showcase 后端完成以后：删除 `src/admin/`、`package.json` 里构建 Decap 的 `prebuild` 和 `decap` 脚本，以及只给 Decap 用的依赖：`decap-cms-app`、`decap-server`、`bootstrap`、`react-bootstrap-typeahead`、`vite-plugin-node-polyfills`、`vite-plugin-static-copy`、`eslint-plugin-react`。删之前逐个确认没有别处在用。
- [ ] 删除 `nuxt.config.ts` 里 `/admin/` 的 devProxy。
- [ ] 删除或改写 `docs/authentication.md` 里 Decap 和 Netlify 的部分。
- [ ] ~~把 `docs/cms.md` 和 `docs/cms/` 改写~~ 暂时不改（2026-10-16 决定）。原计划是改写成**给编辑的 GitHub 编辑指南**：在哪个仓库、哪个目录改哪类内容；每种语言一个文件；文件头字段的含义；9 种自定义块（`::OdsCard{…}` 等）怎么写，附可以直接复制的例子；怎样上传图片；怎样开 PR、谁来审核；合并以后多久上线。这份指南只用到 V4：之后 showcase 在管理区编辑，其余内容用 Nuxt Studio。所以写得简短实用即可。
- [ ] 内容仓库要有写权限，见"需要你决定的事"第 9 条。

删掉 Decap 以后，它带进的 3 个许可证黑名单上的包（`@vercel/stega`、两个版本的 `dompurify`）和 `decap-server` 的 `simple-git` 严重漏洞也跟着消失。

### 第 6 步：阶段 7，彻底去掉 piveau，顺便清理

- [x] 删除（2026-10-16 已完成，提前到第 3 步之前做）：
  - 依赖 `@piveau/sdk-core`、`@piveau/sdk-vue`；
  - `app/piveau/`、`i18n/locales/piveau/`、`app/model/dataset.ts`；
  - `app/plugins/piveau-vue-query.ts` 里注册 `piveauKitPlugin` 的部分（Vue Query 的部分保留，文件可以改名为 `vue-query.ts`）；
  - `app/app.vue` 里同步 piveau 语言设置的代码；
  - `piveauHubSearchUrl`、`piveauHubRepoUrl` 配置。
- [x] `app/model/dataset/table-entry.ts` 从 `@piveau/sdk-vue` 引入的类型：用到它们的 `addPiveauPropertyTableEntry` 已经没有调用者，整个删掉了。
- [x] 元数据下载（`OdsMetadataDownload`）不再拼 piveau 的地址；在 iop-core 提供导出（G6）之前不显示。
- [ ] 数据预览：保留（第 3 条决定），第 2 步已经去掉对原团队服务的依赖，不用再处理。
- [ ] 在代码里搜索剩下的 `piveau` 引用。
- [ ] 顺便清理：
  - [x] 没有被引用的组件：`OdsMetaInfo`、`OdsTabs`、`ToastMarkdownEditor`、`OdsDetailTermsOfUse`（2026-10-16 删除；`OdsMetadataDownloadList` 已在去掉 piveau 时删除）；
  - [x] 没有用到的依赖：`nuxt-basic-auth-module`、`pinia`、`@pinia/nuxt`、`rimraf`、`@toast-ui/editor`（2026-10-16 删除，共移除 30 个包）；
  - [x] `eslint`、`@nuxt/eslint`、`typescript`、`vue-tsc` 从 `dependencies` 移到 `devDependencies`（2026-10-16）。Docker 的构建阶段用 `npm ci` 装全部依赖，镜像里只有 `.output`，所以不影响构建。
    - 漏洞数：改之前共 64 个（生产依赖 58 个），改之后 62 个（56 个）。之前写的"大部分来自开发工具"不对：生产依赖里约 40 个来自 Decap（`decap-cms-app` 和它的组件、后端，以及只给 Decap 用的 `vite-plugin-node-polyfills`、`vite-plugin-static-copy`），删除 Decap 时会一起消失；其余大多来自 Nuxt 自己的构建工具（`nuxt`、`@nuxt/devtools`、`@intlify/unplugin-vue-i18n`），要靠升级 Nuxt 解决；
  - [x] 重复逻辑：许可证映射原来有 3 份，合并成 `app/utils/terms-of-use.ts` 一份（2026-10-16）。两种 URI（DCAT-AP CH 词表和 opendata.swiss 使用条款页的锚点）都能识别，`OdsTerms` 原来只认第一种；
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

**2026-10-20：dev 已上线**：`https://mds.i14y.d.c.bfs.admin.ch`，镜像 `0.1.1`。集群 `aks-i14y-search-dev`（资源组 `rg-i14y-aks-dev`），namespace `bfs-np-mds-ui-dev`。

访问路径。和集群里的其他服务一样，不用 ingress controller，域名和 TLS 都在 Application Gateway 上：

```
浏览器
  → Application Gateway agw-i14y-dev（公网 IP 74.161.96.4，HTTPS:443，通配符证书 i14y-dev-wildcard-letsencrypt）
  → 内部 LoadBalancer 10.71.0.10:80（Service mds-ui）
  → pod :3000
```

- 网关上 mds-ui 专用的配置：监听器 `mds-dev-frontend-listener`（主机名 `mds.i14y.d.c.bfs.admin.ch`）、规则 `bfs-mds-dev-frontend-rule`、后端池 `bfs-mds-frontend-dev`（只有 `10.71.0.10`）、HTTP 设置 `bfs-mds-frontend-settings`（Http:80）。其他服务共用的后端池 `bfs-bep-i14y-dev` 里不能有 `10.71.0.10`，否则 core 和 keycloak 的探测会去探它，一直失败（2026-10-20 已删除）。
- Service 的 `loadBalancerSourceRanges` 只放行网关的子网（`10.1.10.0/24`）和两台跳板机所在的子网，见 `k8s/overlays/dev/kustomization.yaml`。
- 跳板机可以直接打开 `http://10.71.0.10`：`vm-i14y-jumphost`（management VNet）和 `bfs-vm-i14y-db-jumphost-dev`（preprod-shared VNet），这两个 VNet 都已和 AKS 的 VNet 对等连接。
- DNS：`mds.i14y.d.c.bfs.admin.ch` 指向网关的公网 IP（2026-10-20 生效）。
- 浏览器怎样访问 iop-core（2026-10-19）：iop-core 没有允许 mds-ui 域名的 CORS，所以浏览器不直接调用它，而是调用 mds-ui 服务端的 `/input-backend/`，由服务端转发。这和 public-ui 的做法一样：public-ui 也不直接调用 iop-core，而是经过 Admin API。
  - 只转发 `api-client/input-backend.paths.json` 里列出的 GET 接口（现在是 5 个），不转发 `Authorization` 和 Cookie；其他请求返回 404 或 405；
  - 开放的接口有文档：`/input-backend/docs`（Swagger UI）和 `/input-backend/openapi.json`；
  - 服务端渲染时直接调用 `NUXT_PUBLIC_IOP_CORE_URL`；
  - 详见 `api-client/README.md`。

从公司网络手动部署时要注意 BIT Proxy：
- `kubectl` 要绕过代理：`NO_PROXY` 加上 `.azmk8s.io`；
- `az acr login` 不能用（代理重新签发的证书缺少 Authority Key Identifier，Azure CLI 的 Python 会拒绝）。改为用 curl（加 `--ssl-no-revoke`）把 Entra ID 的 token 换成 ACR 的 token，再交给 `docker login`，用户名 `00000000-0000-0000-0000-000000000000`；
- 本地构建要把 BIT Proxy Root CA 临时导出到 `build/certificates/`，用 `CORPORATE_CA_FILE` 传入，构建完删掉；
- 推送大的镜像层时代理可能会断开连接，重试即可（已上传的层会跳过）。以后由 GitHub Actions 构建和推送，就没有这些问题。

好在原团队本来就把 mds-ui 部署在 Kubernetes 上，`k8s/` 里有一套 kustomize 配置（deployment、service、ingress）。原计划是删除它，现在改为保留，在它的基础上改写。其中的 `ingress.yaml` 已经删除，连同里面 Zazuko 的域名和证书、完全放开的 CORS、`proxy-body-size: 20g` 和 24 小时的超时：

- [x] 名字和标签：`piveau-ui`、`piveau-previews` 改成 `mds-ui`（2026-10-19）。原团队的 4 个文件删除，改为 `k8s/base/`（deployment、service）加 `k8s/overlays/dev/`。
- [x] 镜像：改成本仓库 `docker/Dockerfile.mds-ui` 构建的镜像（2026-10-19）。容器端口从 80 改成 3000，因为镜像里设置了 `PORT=3000`，并且以 `node` 用户运行。
- [x] 环境变量（2026-10-19）：只剩 `NUXT_PUBLIC_IOP_CORE_URL`、`NUXT_PUBLIC_LOGIN_URL` 和 Matomo 的两个，直接写在 overlay 里；
  - Nuxt Content 的数据库位置 `NUXT_CONTENT_DATABASE_FILENAME=/app/.data/contents.sqlite` 写在 Dockerfile 里。默认会写进 `.output/server/`，那样根文件系统就不能只读；
  - piveau、Keycloak、GitHub、Listmonk 的变量全部删除；
  - V1 没有密钥，不需要 Secret，也不需要 Key Vault。
- [ ] 网关给 mds-ui 加自定义的健康探测（`/api/healthz`）。现在用的是默认探测：它请求 `/`，得到 302 也算健康。要改共用的网关，先和运维确认。
- [x] 保留 `/api/healthz` 的三个探针。内存改为 requests 256Mi、limits 768Mi（本地测试约 130 MB），上线后按实际用量调整。另外：非 root（uid 1000）、只读根文件系统（`.data` 和 `/tmp` 用 emptyDir）、去掉所有 capabilities、不挂载 service account token，符合集群里的 Azure Policy（现在是 dryrun）。
- [ ] 每个环境一个 kustomize overlay（例如 dev、int、prod），放各自的域名和 iop-core 地址。dev 已经有了（2026-10-19）；int 和 prod 要先知道各自的集群、域名和 iop-core 地址（第 7 条），并照 dev 在网关上配好监听器、后端池和 DNS。
- [x] 镜像推到 AKS 能拉取的镜像仓库（ACR）：`i14ycontainers`（登录地址 `i14ycontainers-hkbqadazetaecugr.azurecr.io`），仓库 `mds-ui`，第一个版本 `0.1.0`（2026-10-19，从本地手动推送），`0.1.1` 加上了 `/input-backend/`（2026-10-19）。
- [ ] 部署工作流：构建镜像 → 推送 → `kubectl apply -k k8s/overlays/<环境>`。用 GitHub Actions 的 OIDC 登录 Azure；在 GitHub 的机器上构建，就没有上面 BIT Proxy 的那些问题。
- [ ] CMS 内容仓库合并到 main 时自动部署；再加一个每天定时构建，保证定时发布的内容能按时上线。内容是在构建镜像时克隆进去的，所以内容一变就要重新构建。
- [x] ~~iop-core 的 CORS~~ 不需要了（2026-10-19）：浏览器经 mds-ui 服务端的 `/input-backend/` 访问 iop-core（见上）。`app/plugins/iop-core-client.ts` 在浏览器里一律用 `/input-backend`，本地开发也一样；原来只在开发时用的 `/iop-core` 代理已删除。
- [x] 网络：AKS 的出站流量能访问 iop-core（2026-10-19 在 dev 上确认，数据集页正常）。
- [x] 镜像里不能包含 BIT Proxy Root CA：本地构建时证书只装在构建阶段，运行阶段的镜像里没有（2026-10-19 检查过）。

### 第 9 步：阶段 9，测试和上线

**"页面正常运行、没有 bug"的验收标准：**

- 4 种语言的所有页面都能打开，没有 500 错误，浏览器控制台没有报错；
- 不存在的数据集、组织、博客和手册页面返回 404；
- lint、typecheck、单元测试和构建在 CI 里全部通过；
- 不依赖 piveau 或原团队（Zazuko）的任何后端服务和账号；代码里搜不到 `zazuko`、`keycloak`、`listmonk`、`hyvor`，`piveau` 只出现在前端预览组件 `piveau-preview-plugin` 的包名里（第 3 条决定）；
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
- [ ] G6：单个数据集和分发的 DCAT 导出（JSON-LD、Turtle、RDF/XML）。恢复元数据下载，第 3 步的 SEO 和 V6 的 GEO 也要用。
- [ ] G7–G9：组织搜索、组织的 URI、搜索结果里的关键词和日期。
- [ ] G10：按修改时间过滤（`modifiedSince`）。V4 的摘要邮件要用，在这里一起做。
- [ ] G14 的后端部分：`Showcase` 实体（基于 `PublishableEntityBase`）、增删改查接口、两种权限（编辑、发布）、草稿/审核/发布、AuditTrail、测试，和 Dataset 等对象一样。详见 `docs/iop-core-gaps.md` 的 G14。不包括公众投稿接口、迁移和 mds-ui 的界面，它们在 V4（2026-10-16 决定）。
- [ ] G15：数据预览接口，回答格式和 mds-ui 现有的预览组件一致，只预览目录里公开分发的文件，并防止访问内网地址。上线后，mds-ui 只需设置 `NUXT_PUBLIC_DATA_PREVIEW_URL`。详见 `docs/iop-core-gaps.md` 的 G15。

### V2 第 2 步：适配新的界面设计

- [ ] 拿到新的界面设计（见"需要你决定的事"第 11 条），确认用哪个版本的联邦设计系统。
- [ ] 按新设计改造公开页面：首页、数据集搜索和详情、组织、showcase、博客、手册。
- [ ] 新设计也要包括 **showcase 提交表单**，虽然它到 V4 才实现。给设计师的依据是 `docs/ShowcaseSubmission.md`（字段、分组、提示、提交结果，以及要改进的地方）。这样 V4 只按新设计做一次。
- [ ] 把第 1 步恢复的筛选条件、排序和元数据下载放进新界面。
- [ ] 如果决定要数据预览（"需要你决定的事"第 3 条），在这一步按新界面重新做。

### V2 第 3 步：加强 SEO（可以和第 2 步同时进行）

这一步的结构化数据（schema.org、sitemap、DCAT 导出的链接）同时是 V6 第 1 步 GEO 的基础，GEO 本身放在 V6。

- [ ] 数据集和组织页面加上 `description` 和 Open Graph 标签，内容来自 iop-core 的描述。
- [ ] 所有页面通过 `useLocaleHead()` 加上 `hreflang` 和 `canonical`。
- [ ] 生成 sitemap，数据集列表从 iop-core 获取，带 `lastmod`。
- [ ] 数据集页面加上 schema.org `Dataset` 的 JSON-LD，并用 `<link rel="alternate">` 指向 DCAT 导出（G6）。
- [ ] 为爬虫流量调大 iop-core 的输出缓存，或者加 CDN。

## V3：登录、管理区框架和 public-ui 的功能

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
  - 注意（2026-10-19）：浏览器现在经 `/input-backend/` 代理访问 iop-core，代理不转发 `Authorization`。登录后的调用有两种做法：让代理为管理区的接口另列一份白名单，并转发 token；或者给 iop-core 开 CORS。到时候再定。
- [ ] 用户信息和权限从 iop-core 取，这一点比 admin-ui 做得更好：
  - `GET /api/Users/current`：姓名、邮箱、业务角色、所属组织；
  - `GET /api/Users/current-agents`：用户所属的组织；
  - `AllowActions`：能做哪些操作；
  - 不像 admin-ui 那样自己解析 token 里的角色，也不向 eIAM 取用户信息（admin-ui 设了 `loadUserInfo: true`）。
- [ ] token 只放在内存或 sessionStorage 里，不像 admin-ui 那样复制到 `localStorage`：放在那里，页面里任何被注入的脚本都能读到它。
- [ ] token 快过期时的处理，先照 admin-ui：弹窗提示用户重新登录。
- [ ] 管理区的页面只在浏览器里渲染（`routeRules` 里设 `ssr: false`），因为 token 只在浏览器里。这些页面也不需要 SEO。
- [ ] 管理区有一个入口（例如 `/admin` 下的登录页），给内部人员先用起来。页头的"登录"按钮暂时继续指向 input 门户，V5 切换时再改。

### V3 第 2 步：public-ui 的功能（清单，待选）

2026-10-16 加入：把 public-ui（i14y.admin.ch 的公开界面）的功能加进 mds-ui。**先列清单，搬哪些、mds-ui 是否取代 public-ui，见"需要你决定的事"第 16 条。**

public-ui 是 Angular 的单页应用，调用的是 Admin API（NSwag 生成的 `bfs-iop-admin-web-api-client`，地址 `IOP_ADMIN_API_BASE_URL`）。mds-ui 调用 iop-core，下表最后一列是 iop-core 客户端里有没有对应的接口。

| 功能 | public-ui 位置 | 代码行数 | iop-core 接口 | 粗估人天 |
|---|---|---|---|---|
| 全目录搜索：数据集、数据服务、公共服务、概念、映射表一起搜，按类型分开，带筛选 | `catalog/` | 1131 | 有：`/api/Search`、`/api/Search/count` | 5–8 |
| 数据集详情的其他部分：数据结构（类和属性的图和表）、质量信息、分发的访问服务 | `datasets/structure`、`qualityinfo`、`distributions` | 2684（整个 `datasets/`） | 有：`ModelGraph`、`QualityInformation`、`AccessServices`、`IsServedBy` | 8–13 |
| 概念和代码表：描述、代码表条目（树、搜索、注释）、版本、导出 | `concepts/` | 1510 | 有：`Concepts*`、`CodelistEntries*`、`StructureReferences` | 8–12 |
| 数据服务详情 | `data-services/` | 323 | 有 | 2–3 |
| 公共服务详情：渠道、关系 | `public-services/` | 633 | 有：`Channels`、`Relations` | 3–5 |
| 映射表详情：关系表、导出 | `mappingtables/` | 417 | 有：`MappingTablesRelations*` | 3–4 |
| 元搜索：同时搜 geocat 和 opendata.swiss | `metasearch/` | 396 | **没有**。public-ui 用 Admin API 的 `MetasearchClient`，要在 iop-core 加接口 | 3–5，另加后端 |
| 组织：列表、详情、统计 | `organisations/` | 577 | 有：`Agents`、`AgentsStatistics`。mds-ui 已有组织页，要合并 | 2–3 |
| I14Y 首页；新闻（只是跳到手册网站的新闻页） | `home/`、`news/` | 228 | — | 2–3 |
| 共用部分：富文本、关系表、筛选、列表视图、RDF 和 linked data 链接等 | `shared/` | 5779 | RDF 链接和 linked data URI 的接口（`getRdfLink…`、`getLdUri…`）在 iop-core 客户端里**没找到** | 含在上面各项里 |
| 如果 mds-ui 取代 public-ui：一个应用服务两个网站（域名、品牌、导航），i14y.admin.ch 旧网址的重定向 | — | — | — | 5–10 |
| **全部搬过来** | | | | **约 41–66** |

- [ ] 决定搬哪些功能，以及 mds-ui 是否取代 public-ui（第 16 条）。
- [ ] 元搜索和 RDF / linked data 链接：和后端团队确认 iop-core 要不要提供，要的话记进 `docs/iop-core-gaps.md`。
- [ ] 公开页面都由服务端渲染，和 opendata.swiss 的页面一样有 SEO。这也是相对 public-ui 的主要好处：public-ui 是单页应用，AI 爬虫读不到内容，不存在的网址也返回 200。

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
- [ ] 浏览器怎样调用这些写接口：浏览器经 mds-ui 的 `/input-backend/` 代理访问 iop-core，现在只允许 GET（2026-10-19）。要么在代理里为这些接口放行 `POST`、`PUT`，要么请运维在 iop-core 上给 mds-ui 的域名开 CORS。和 V3 第 1 步的 token 一起决定。

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

### V4 第 4 步：showcase 搬进 iop-core，页面、手册、博客用 Nuxt Studio

2026-10-13 决定。架构图、编辑流程、组件、风险和工作量见 `docs/CmsComparison.md` 的 "Decision"；`Showcase` 实体和接口见 `docs/iop-core-gaps.md` 的 G14。约 35–52 人天。

| 内容 | 放在哪里 | 用什么编辑 | 登录 |
|---|---|---|---|
| showcase | iop-core（G14） | mds-ui 管理区：表单 + Nuxt UI 的 `UEditor` | eIAM |
| 页面、手册、博客 | GitHub 内容仓库（Markdown + MDC） | Nuxt Studio，在网站上可视化编辑 | eIAM（Studio 的 Custom Auth） |

**先做两个原型**（共 2–3 天，可以同时做，都要用到 V3 的 eIAM 登录）：

- [ ] 原型 1：Nuxt UI 能不能放进 mds-ui。
  - 要确认：Nuxt UI（依赖 Tailwind CSS 4）只在管理区加载，公开页面的样式完全不受影响；一篇真实的 showcase 正文放进 `UEditor` 再保存出来，内容不变。
  - 不通过：showcase 编辑器改为直接用 Tiptap，界面用 Ods 组件自己做，多 4–6 人天。
- [ ] 原型 2：Nuxt Studio 能不能接 eIAM 和我们的内容仓库。
  - 要确认：能用单独的内容仓库（可能要把 `pages/`、`handbook/`、`blog/` 挪进仓库里的 `content/` 文件夹）；Custom Auth 能接受 eIAM 登录，并在 iop-core 检查编辑权限；GitHub token 能限制成只写内容仓库；能不能先发布到分支、审核以后再上线。
  - 不通过：在管理区自己做一个小编辑器（普通页面用 `UEditor`，带 MDC 区块的页面用源码视图，由 mds-ui 服务端通过 I14Y 的 GitHub App 提交），多 10–15 人天。

**showcase（G14）：**

- [x] ~~后端：`Showcase` 实体、接口、两种权限、AuditTrail、测试~~ 移到 V2 第 1 步（2026-10-16 决定）。
- [ ] 后端：匿名投稿接口，带 ALTCHA 验证码检查、限流和图片缩放。
- [ ] 管理区：showcase 列表、审核队列、表单、`UEditor`、数据集和机构的选择器、图片上传（`/api/Media`）、修改历史。
- [ ] 投稿表单：按 V2 的新设计重写 `/showcases/submit`，提交到 iop-core 的 `POST /api/ShowcaseSubmissions`。
  - 字段、分组、提示、提交结果，以及原来表单和 iop-core 字段的对应关系，见 `docs/ShowcaseSubmission.md`；文案用 V1 保留下来的翻译。
  - 修好那份文档里"Problems in the old form"列出的问题，主要是：验证码和限流；必填标记和校验一致（数据集、分类）；隐私说明；提交成功后的确认；不向公众显示技术错误。
  - 类型、分类和数据集改为查 iop-core。
  - 新投稿的邮件通知（给编辑）和确认邮件（给投稿人）用第 1 步的发信能力。
- [ ] 公开页面：V1 第 3 步的 `useShowcases()` 改为调用 iop-core（`swr` 缓存）；首页的 showcase 区块改用 iop-core 的 `Pinned` 标记，删掉首页里写死的 showcase 列表；机构页的数量和数据集页的"使用了这个数据集的 showcase"用 iop-core 的接口；sitemap 加上 showcase。
- [ ] 迁移：一次性脚本把内容仓库里的 showcase 写进 iop-core，保留 slug（`/showcase/{slug}` 的链接不会断）；图片上传到 `/api/Media`。核对以后，从内容仓库删除 `showcases/` 和对应的图片，从 `content.config.ts` 删除 `showcases` 集合。

**页面、手册、博客（Nuxt Studio）：**

- [ ] 安装 `nuxt-studio`，按原型 2 的结果配置内容仓库。
- [ ] eIAM 登录：mds-ui 服务端加一个 Studio Custom Auth 的路由，检查 eIAM token、在 iop-core 检查编辑权限，再建立 Studio 的会话。管理区的菜单里放"编辑页面"的入口。
- [ ] GitHub token：只允许写内容仓库的内容，定期更换。Studio 在编辑的浏览器里直接调用 GitHub，所以 token 会到浏览器里。
- [ ] 区块分组：用 Studio 的组件配置只开放 `app/components/content/` 里给编辑用的区块。
- [ ] 部署：V1 第 8 步已经做了"内容仓库合并后自动部署"，Studio 发布以后就走这条路。
- [ ] 把 V1 第 5 步写的 GitHub 编辑指南换成 Studio 和管理区的使用说明。

**密钥：**从 V4 开始，mds-ui 第一次有密钥：Studio 的会话密钥和 GitHub token。放进 Azure Key Vault，挂载到 pod 里；V1–V3 仍然没有密钥。

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

## V6（加分项）：GEO、AI 助手和推荐

2026-10-14 加上。**只在 V1–V5 完成、还有余量时做**，不算进承诺的工期。按步骤顺序做；余量用完就停，已经做完的步骤都能单独上线。

| 组 | 步骤 | 依赖 |
|---|---|---|
| GEO | 第 1 步 SEO 的普通扩展；第 2 步把数据和 showcase 推给 AI | 不依赖 V3–V5，需要的话可以提前 |
| AI 助手 | 第 3 步 AI 网关和元数据助手；第 4 步自然语言搜索 | 第 3 步要 V5 的数据集编辑；第 4 步要 V2 的筛选接口和搜索服务；都要先选定模型服务（第 15 条） |
| 推荐 | 第 5 步关系推荐和 newsletter 推荐；第 6 步个性排序 | V4 的订阅、新闻邮件和 showcase（G14）；第 6 步要第 5 步 |

最后一节"可能增加的功能"只是候选清单，没有排期。

### GEO

GEO（生成式引擎优化）的目标：AI 助手（ChatGPT、Copilot、Perplexity、Google 的 AI 概览等）回答"瑞士有没有关于某某的公开数据"这类问题时，能找到 opendata.swiss 上的数据集和 showcase，正确引用，并链接回来。**这没法保证**：AI 引用谁由各家决定。我们能做的，是让数据最容易被它们找到、读懂和信任。

#### V6 第 1 步：GEO 一，SEO 的普通扩展（约 3–5 人天）

在 V2 第 3 步的基础上（服务端渲染、schema.org `Dataset`、sitemap、`hreflang`、`canonical`、DCAT 导出的链接），只改网站本身：

- [ ] 更多结构化数据：目录（`DataCatalog`）、机构（`Organization`）、面包屑（`BreadcrumbList`）、博客文章（`Article`）、showcase（`CreativeWork`，带它用到的数据集）、手册里的常见问题（`FAQPage`）。
- [ ] 适合被引用的内容：
  - 每个数据集页面开头有一段简短、事实性的摘要：发布者、主题、时间范围、更新频率、许可证；
  - 手册改成清楚的问答结构；
  - 每个页面写明最后更新日期。
- [ ] 检查 Google 的结构化数据测试和 Google Dataset Search 是否能正确识别我们的数据集。

#### V6 第 2 步：GEO 二，把数据和 showcase 推给 AI（约 5–10 人天）

主动把我们的目录送到 AI 助手所依赖的地方，并提供它们可以直接查询的入口：

- [ ] **主动通知搜索引擎**：用 IndexNow 在数据集或 showcase 新增、更新时通知 Bing 等搜索引擎（Copilot 和 ChatGPT 的搜索部分依赖 Bing）；在 Google Search Console 和 Bing Webmaster Tools 提交 sitemap。
- [ ] **数据门户**：确认欧盟的 data.europa.eu 能通过 DCAT-AP 收录 opendata.swiss 的目录（I14Y 接手以后要重新检查这条通路）。AI 助手回答欧洲公开数据的问题时常常引用它。
- [ ] **知识库**：为重要的数据集系列、发布机构和 showcase 在 Wikidata 里建立或补全条目，链接回 opendata.swiss。很多 AI 模型的知识来自 Wikidata 和 Wikipedia。
- [ ] **给 AI 读的说明**：4 种语言的 `llms.txt`，说明 opendata.swiss 是什么、主要栏目、公开 API 和 DCAT 导出在哪里。
- [ ] **机器入口**：
  - 公开的只读 API 和它的 OpenAPI 文档；
  - 整个目录的 DCAT 导出，包括 showcase 和它们用到的数据集；
  - MCP 服务器不在 mds-ui 的计划里：另一个团队已经有了（2026-10-14）。如果它对外公开，在 `llms.txt` 里加上它的说明即可。
- [ ] **AI 爬虫政策**：`robots.txt` 里对 AI 爬虫（如 GPTBot、Google-Extended、PerplexityBot）允许什么，见"需要你决定的事"第 13 条。
- [ ] **监测**：
  - 在 Matomo 里统计从 AI 助手来的访问；
  - 每季度用一组典型问题（4 种语言）检查主要 AI 助手的回答有没有引用 opendata.swiss，引用得对不对。

### AI 助手

两个助手共用第 3 步建立的 AI 网关。原则：

- mds-ui 只调用 iop-core，浏览器里和 mds-ui 里都不放模型的密钥；
- 模型不直接发布任何内容：元数据助手的建议由人确认；自然语言搜索只把问题转换成筛选条件，结果来自真实数据；
- 凡是由 AI 生成、经人确认后发布的内容，按联邦的规定标明。

#### V6 第 3 步：AI 网关和元数据助手（约 10–15 人天）

**AI 网关**（iop-core 里的一个新模块）：

- [ ] 统一调用模型服务（按第 15 条决定，例如 Azure OpenAI 的瑞士区域），换服务商时只改这一层。
- [ ] 提示词模板放在仓库里、有版本；要求模型输出固定结构的 JSON，后端校验。
- [ ] 日志里不记录个人信息；每个功能有调用频率和每月费用的上限。
- [ ] 每个功能有一组测试问题和期望结果，模型或提示词改动后重新跑一遍。

**元数据助手**（放进 V5 的数据集编辑页）：

- [ ] 根据标题和描述建议关键词（4 种语言）、主题（只能从词表里选，不能自己编）和一段简短摘要。
- [ ] 把标题和描述从原文翻译成其他语言，作为草稿。
- [ ] 按 GOGD 的元数据质量指标（V5 第 3 步）列出缺少或不完整的信息，并说明怎样补全。
- [ ] 每条建议都可以单独采纳、修改或拒绝；记录采纳率，用来改进提示词。

#### V6 第 4 步：自然语言搜索（约 6–10 人天）

例如访客输入"伯尔尼 2010 年以来的租金数据"：

```
一句话的搜索 → AI 网关：转换成结构化条件（关键词、主题、机构、时间范围、格式）
            → Elasticsearch：关键词查询，可选再加向量查询，由 iop-core 用 RRF 公式合并（只用免费功能）
            → 搜索结果页：把识别出来的条件显示成筛选标签，访客可以修改或删除
```

- [ ] 只有像句子的输入才交给模型；短的关键词直接走普通搜索，省费用。
- [ ] 模型超时或出错时，自动退回普通搜索。
- [ ] 常见的输入缓存起来；按访客限流。
- [ ] 模型只负责理解问题，不写答案，所以不会编造数据。

**依赖：**V2 的筛选接口（G1–G5、G9）和后端的搜索服务；第 3 步的 AI 网关。

### 推荐

#### V6 第 5 步：基于关系远近的推荐和 newsletter 推荐（约 22–34 人天）

**思路：**I14Y 的数据本来就是一张关系图（iop-core 已经有链接数据和三元组库）。两个对象在图上离得越近、连接它们的关系越"强"，就越相关。

| 关系 | 例子 | 建议的强度 |
|---|---|---|
| 数据集使用同一个概念或代码表（数据结构里的属性引用的代码表） | 两个数据集都用职业分类 CH-ISCO-19 | 最强 |
| 概念之间有映射表 | 两个代码表之间有对照 | 强 |
| 被同一个 showcase 使用（G14） | 一个应用同时用了这两个数据集 | 强 |
| 数据集之间的明确关系、同一个数据服务 | DCAT 的 `qualifiedRelation` | 强 |
| 同一个发布机构 | 都来自 BFS | 中 |
| 同一个主题 | 都属于"人口" | 弱 |
| 同一个关键词 | 都标了"Miete" | 弱 |

**算法从简单开始：**

1. 加权的共同邻居：两个数据集共享的邻居越多、越强，分数越高；只看两步以内的路径。
2. 效果不够，再试个性化 PageRank（从一组起点出发、随机游走并不断回到起点）。
3. 最后才考虑图嵌入（如 node2vec），或者和描述文本的向量相似度混合。

每条推荐都带原因，例如"和你订阅的数据集使用同一个代码表 NOGA 2008"。这样推荐可以解释，也方便调整权重。

**两种用法：**

- **网站上的动态推荐：**
  - 数据集页的"相关数据集"、showcase 页的"相关数据集和 showcase"、搜索没有结果时的推荐；
  - 首页的"根据你最近看过的"：最近浏览只保存在访客自己的浏览器里，不上传、不跨会话追踪。
- **新闻邮件和摘要邮件：**
  - 对每个订阅者，从他订阅的数据集、主题和机构出发，在图上找出最近新发布或更新、离得最近的数据集，按分数排序，放进摘要邮件和新闻邮件的"您可能也感兴趣"一节；
  - 只用订阅时明确提供的信息，不用浏览记录；
  - 邮件里可以一键"不再推荐这一类"。

**要做的：**

- [ ] 后端：iop-core 每晚的任务从 Postgres（或现有的三元组库）构建关系图，算出每个数据集最相关的前 N 个对象和原因，存进一张表；数据变化时增量更新。
- [ ] 接口：
  - `GET /api/Datasets/{id}/related`：某个数据集的相关数据集和 showcase，带原因；
  - `POST /api/Recommendations`：输入一组数据集、主题或机构，返回推荐。给"最近看过的"和邮件用。
- [ ] 网站：相关数据集组件；首页"根据你最近看过的"，浏览记录只放在浏览器里。
- [ ] 邮件：在 V4 的摘要邮件和新闻邮件里加推荐一节，每条带原因。
- [ ] 评估：用 Matomo 事件统计网站推荐的点击率，统计邮件里推荐链接的点击率，按结果调整权重。
- [ ] 数据保护：不建立跨会话的个人画像；推荐怎样产生写进隐私声明；见"需要你决定的事"第 14 条。

**依赖：**V2 的 G2（主题）和 G4（关键词）；V4 的订阅、新闻邮件和 showcase（G14）；如果要把推荐放进搜索结果，还要看后端搜索服务的进度。

#### V6 第 6 步：个性排序（约 3–5 人天）

按访客或订阅者的兴趣**调整顺序**，不过滤、不隐藏任何内容。兴趣来自第 5 步算好的关系：和访客最近看过的、或订阅者订阅的对象越近，排得越靠前。

```
最终分数 = 原来的相关度 × (1 + α × 兴趣分)      α 很小，放在配置里
```

- [ ] **网站**：服务端先渲染不个性化的结果，所以 SEO 和缓存都不受影响；然后浏览器根据本地保存的最近浏览，重新排列搜索结果和首页列表的前几十条。页面上显示"已按你的兴趣排序"，可以一键关闭。
- [ ] **邮件**：摘要邮件和新闻邮件里的条目按订阅者的兴趣排序，最接近的排在前面。
- [ ] 数据保护和第 5 步一样：网站的兴趣只保存在访客自己的浏览器里，邮件只用订阅时明确提供的信息（第 14 条）。

### 可能增加的功能（候选，没有排期）

需要时再逐个评估工作量和优先级。

| 功能 | 说明 | 依赖 |
|---|---|---|
| CMS 翻译助手 | 在 Nuxt Studio 里，把页面和手册的修改翻译成其他语言的草稿 | V4 的 CMS、第 3 步的 AI 网关 |
| 代码表匹配 | 发布者上传数据结构时，把每一列匹配到合适的概念和代码表 | V5、AI 网关、第 5 步的关系图 |
| showcase 介绍起草 | 根据投稿人对 8 个问题的回答，起草 showcase 的介绍 | V4 的投稿、AI 网关 |
| 公开的问答助手 | 根据目录和手册回答问题并附上引用 | AI 网关、向量；先看 GEO 和另一个团队的 MCP 服务器的效果，可能不需要 |
| 公开的元数据质量等级 | 在数据集页显示质量等级，督促发布者改进 | V5 第 3 步的质量指标 |
| 失效链接报告 | 每周把失效的下载链接发给对应的发布者 | 质量指标里的链接检查、V4 的邮件 |
| 保存搜索和提醒 | 按一个搜索条件订阅，有新结果时发邮件 | V4 的订阅、G10 |
| 数据需求 | 公众提出想要但还没有公开的数据，OGD 办公室跟进 | V4 的 Portal 模块 |
| 数据集问题反馈 | 公众报告数据里的错误，转给发布者 | V4，和评论的方案一起考虑 |
| 数据预览和简单图表 | 在网站上直接看 CSV 表格和图表 | 第 3 条决定 |
| 发布者的使用统计 | 发布者在管理区看自己数据集的访问和下载 | Matomo、V5 的管理区 |
| 嵌入组件 | 其他网站可以嵌入数据集卡片或 showcase | V2 的新设计 |
| 罗曼什语 | 网站界面加第五种语言；iop-core 的多语言字段已经有罗曼什语 | 翻译的工作量 |

## 需要你决定的事

| # | 问题 | 最晚什么时候要决定 |
|---|---|---|
| 1 | ~~V4 用哪个内容系统~~ **已决定（2026-10-13）**：showcase 搬进 iop-core（G14），在管理区编辑；页面、手册、博客留在 GitHub，用 Nuxt Studio；所有编辑都用 eIAM 登录。见 `docs/CmsComparison.md` 的 "Decision"。<br>还没定的小问题：<br>- showcase 的两种权限（编辑、发布）由谁拥有；<br>- 公众投稿算在哪个机构（publisher）名下；<br>- 投稿人的联系方式保留多久；<br>- 页面、手册、博客的修改要不要第二个人审核。Nuxt Studio 本身没有审核步骤，要看原型 2 的结果。 | V4 开始之前 |
| 2 | ~~数据集网址用 DCAT 标识符还是 GUID~~ **已决定（2026-10-16）**：用标识符，和 public-ui 的网址和 IRI 一样。旧的 opendata.swiss 网址不做重定向，切换后返回 404。 | V1 第 8 步（部署）之前 |
| 3 | ~~数据预览~~ **已决定（2026-10-15）**：保留前端的预览组件，只去掉对原团队 piveau 预览服务的依赖；服务地址改为配置项，没有配置时不显示预览。预览服务由 iop-core 在 V2 提供（G15，按分发的地址只预览目录里的公开分发）。piveau 自己的预览服务有公开镜像，但源代码不公开、没有声明许可证，所以不用。 | — |
| 4 | G1：V1 没有按目录过滤，网站会显示 iop-core 里所有公开的数据集，不只是 opendata.swiss 的。V1 能不能接受？ | V1 上线之前 |
| 5 | 旧邮件里的链接：偏好设置页和 showcase 提交页删除后会返回 404，要不要重定向到一个说明页？<br>现有的订阅者和链接签名密钥都在原团队的 Listmonk 里，I14Y 拿不到，所以没法通知他们，也没法迁移；G12 做好后，订阅从零开始。 | V1 上线之前 |
| 6 | 后端的改动：`main-mvp` 上提交了 `Bfs.Iop.Core.Api.ClientGenerator/Program.cs` 里的 `StoreSwaggerJson = true`，以及生成的 `IopCoreApiClient.swagger.json`（约 2.2 万行）。要么保留，要么把 swagger 文件复制到 mds-ui，后端恢复原样。 | 合并进 `main` 之前 |
| 7 | 环境信息：<br>- 各环境的 iop-core 地址、登录地址和 mds-ui 的域名；<br>- AKS：用哪个集群和 namespace；域名和 TLS 证书放在哪个 Application Gateway 上；镜像仓库；GitHub Actions 怎样获得部署权限；集群由谁运维。<br>**dev 已经有答案（2026-10-20）**：集群 `aks-i14y-search-dev`、namespace `bfs-np-mds-ui-dev`、ACR `i14ycontainers`；域名和 TLS 在 Application Gateway `agw-i14y-dev` 上，见第 8 步。int、prod 的这些信息，以及 GitHub Actions 的部署权限，还要确认。 | V1 第 8 步（部署） |
| 8 | 评论功能的方案（G11），比较见 `docs/HyvorCommentsNote.md` 的 "Replacing Hyvor Talk"；以及是不是真的需要公开评论。 | V4 开始之前 |
| 9 | CMS 内容仓库：GitHub 上 `opendata-swiss` 组织由谁管理？I14Y 有没有 `opendata-swiss-cms-content`（以及 `-int`、`-test`）的写权限？<br>- 有：照常使用；<br>- 没有：把内容复制到 I14Y 自己的 GitHub 组织，并修改 `docker/Dockerfile.mds-ui` 的 `CMS_CONTENT_REPO`、`package.json` 的 `predev` 和 `src/admin/config.yml`。 | V1 第 3 步之前（要在内容仓库里做数据迁移） |
| 10 | 邮件订阅（G12）：<br>- 身份已经定了：公众用邮件里的链接，内部人员用 eIAM 账号，两条路在 V4 一起做；<br>- 还要决定：订阅存在哪里、谁发邮件，iop-core 加 SMTP，还是自己部署 Listmonk（需要许可证例外）。比较见 `docs/iop-core-gaps.md` 的 "Open for discussion"。 | V4 开始之前 |
| 11 | 新的界面设计：由谁设计、什么时候定稿、用哪个版本的联邦设计系统？V2 要按它改造公开页面，所以 V1 不要在界面细节上花太多工夫。设计范围也要包括 V4 才实现的 showcase 提交表单（依据 `docs/ShowcaseSubmission.md`）。 | V2 开始之前 |
| 12 | 从 V1 上线到 V4，编辑都要直接在 GitHub 上改 Markdown，这可能持续很久。OGD 办公室能不能接受？不能接受的话，可以把 V4 第 4 步里 Nuxt Studio 的部分提前到 V3 之后（它要用 V3 的 eIAM 登录）。不建议在 V1 之后加临时编辑器，那违背"每个功能只做一次"的原则。 | V1 上线之前 |
| 13 | GEO 第二步：<br>- `robots.txt` 里允许哪些 AI 爬虫读取 opendata.swiss？内容本来就是公开的，但要有法律和 OGD 办公室的意见；<br>- 由谁维护 Wikidata 条目？ | V6 第 2 步开始之前 |
| 14 | 推荐和个性排序的范围和数据保护：<br>- 网站上只用浏览器本地的最近浏览（建议），还是要在服务端记录访客行为？<br>- 邮件推荐只用订阅时明确提供的信息（建议）？<br>- 要不要先做数据保护审查。 | V6 第 5 步开始之前 |
| 15 | AI 助手的模型服务：<br>- 用哪个服务（例如 Azure OpenAI 的瑞士区域），数据存在哪里、会不会被用来训练；<br>- 数据保护审查，以及联邦关于使用 AI 的规定；<br>- AI 生成的内容怎样标明；<br>- 每月的费用上限，由谁的预算出。 | V6 第 3 步开始之前 |
| 16 | public-ui 的功能（V3 第 2 步的清单）：<br>- 搬哪些功能；<br>- mds-ui 是否取代 public-ui（一个应用服务 opendata.swiss 和 i14y.admin.ch 两个网站），还是只在 opendata.swiss 里加上这些对象，public-ui 继续运行；<br>- 取代的话：i14y.admin.ch 用哪套设计（public-ui 现在用 Oblique），旧网址怎么重定向，public-ui 什么时候删除；<br>- 元搜索（geocat、opendata.swiss）和 RDF / linked data 链接要不要由 iop-core 提供。 | V3 开始之前 |

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
- **2026-10-13**：决定 V4 的内容方案（"需要你决定的事"第 1 条）。
  - showcase 搬进 iop-core，新增 G14（`Showcase` 实体和接口），在管理区编辑；公众投稿直接写进 iop-core，不再需要 GitHub App。
  - 页面、手册、博客留在 GitHub 内容仓库，用 Nuxt Studio 可视化编辑，首页的 MDC 区块保持不变。
  - 所有编辑都用 eIAM 登录，Nuxt Studio 通过 Custom Auth 接入，编辑不需要 GitHub 账号。
  - V4 第 4 步改写：先做两个原型（Nuxt UI 能否放进 mds-ui、Nuxt Studio 能否接 eIAM 和我们的内容仓库），写明不通过时的备用方案。
  - V1 第 3 步：showcase 数据统一通过一个 composable 读取，V4 只换数据来源；内容仓库的 URI 迁移结果给 V4 的迁移脚本用。V1 第 5 步的 GitHub 编辑指南只用到 V4。
  - 工期：V4 的 CMS 部分从"11–16 或 35–50"改为 35–52 人天，V4 合计约 69–130 人天，总计约 210–375 人天；按 1 前端 + 1 后端仍是约 9–15 个月。
  - `docs/CmsComparison.md` 加上 "Decision" 一节（架构图、编辑流程、组件、工作量、原型、风险），原来的四个方案比较保留作背景。
  - 从 V4 开始，mds-ui 第一次有密钥（Studio 的会话密钥和 GitHub token），放进 Key Vault。
  - 新增 `docs/ShowcaseSubmission.md`（英文）：记下现在的 showcase 提交表单的设计（入口、字段、提示、提交结果）、和 iop-core 字段的对应、要改进的问题、V4 的要求，以及要保留的翻译。表单仍在 V1 第 1 步删除，不只是隐藏，因为它的服务端和下拉框都依赖已经不能用的服务。
    - V1 第 1 步：删除前先截图；保留表单的翻译。
    - V2 第 2 步和第 11 条决定：新设计包括提交表单。
    - V4 第 4 步：按这份文档和新设计重做表单，并修好里面列出的问题。
- **2026-10-14**：**加上 V6：GEO 和推荐**，放在 V5 之后。
  - GEO 从 V2 挪到 V6。V2 第 3 步只做 SEO，但它的结构化数据（schema.org、sitemap、DCAT 导出的链接）同时是 GEO 的基础。GEO 不依赖 V3–V5，需要的话可以提前。
  - 新的推荐功能：按数据集、概念和代码表、机构、主题、showcase 之间关系的远近来推荐，用在网站上的"相关数据集"和动态推荐，以及新闻邮件和摘要邮件里；每条推荐带原因。要用到 V4 的订阅、新闻邮件和 showcase。
  - 工期：V6 约 30–49 人天，总计约 240–425 人天；按 1 前端 + 1 后端约 10–17 个月。
  - "需要你决定的事"新增第 13 条（AI 爬虫政策和 MCP 服务器）和第 14 条（推荐的范围和数据保护）。
  - 同一天修改：**V6 是加分项**，只在 V1–V5 完成、还有余量时做；余量的作用是保证 V1–V5 完成。工期合计改回只算 V1–V5（约 210–375 人天，按 1 前端 + 1 后端约 9–15 个月），V6 单独列出。
  - GEO 分成两步：第 1 步是 SEO 的普通扩展（只改网站，约 3–5 人天）；第 2 步把数据和 showcase 推给 AI（IndexNow 和站长工具、data.europa.eu、Wikidata、`llms.txt`、机器入口、可选的 MCP 服务器、AI 爬虫政策、监测，约 5–10 人天，MCP 另加 3–5）。推荐改为第 3 步。
  - 同一天修改：MCP 服务器从计划里去掉，另一个团队已经有了。第 13 条决定只剩 AI 爬虫政策和 Wikidata。
  - 同一天修改：**V6 改为"GEO、AI 助手和推荐"**，分三组六步。
    - GEO：第 1、2 步不变。
    - AI 助手（新）：第 3 步 iop-core 里的 AI 网关和管理区的元数据助手；第 4 步自然语言搜索（模型只把问题转换成筛选条件，不写答案）。
    - 推荐：第 5 步关系推荐和 newsletter 推荐（原来的第 3 步）；第 6 步个性排序（新：只调顺序、不隐藏，网站上在浏览器里按本地的最近浏览重新排序，邮件按订阅内容排序）。
    - 新增"可能增加的功能"候选清单（13 项，没有排期）。
    - 工期：V6 约 49–79 人天，按 2 人约 2–3 个月；仍不算进 V1–V5 的合计。
    - "需要你决定的事"新增第 15 条（AI 助手的模型服务、数据保护、标明 AI 内容、费用上限）；第 14 条扩大到个性排序。
- **2026-10-15**：数据预览（第 3 条决定）。
  - 保留前端的预览组件，只去掉对原团队 piveau 预览服务的依赖：服务地址改为配置项 `NUXT_PUBLIC_DATA_PREVIEW_URL`，没有配置时不显示预览；图表库和样式只在分发页、真正显示预览时才加载（V1 第 2 步的图表库那一项因此按"保留预览"的方式修好）。
  - 预览服务由 iop-core 在 V2 提供：新增 G15，回答格式和现有组件一致，只预览目录里的公开分发，防止访问内网地址（SSRF）。V2 的估计加 6–12 人天，变为约 42–77 人天；V1–V5 合计约 220–385 人天，按 1 前端 + 1 后端约 9.5–15.5 个月。
  - piveau 自己的预览服务（`registry.gitlab.com/piveau/hub/piveau-hub-data-preview`，Spring Boot，需要 PostgreSQL）有公开镜像，但源代码不公开、没有声明许可证，所以不用。
  - V1 第 9 步的验收标准改为：不依赖 piveau 或原团队的任何后端服务；`piveau` 只允许出现在前端预览组件的包名里。
- **2026-10-16**：调整 V2、V3 和 showcase 的安排。
  - showcase 的前端改动暂停，V4 以后再做：组织页的数量和内容仓库的迁移不在 V1 做；内容里不加 `issued`、`modified`，按日期排序在 V4 由 iop-core 提供。
  - V2 加入 G14 的后端部分：在 iop-core 里建好 `Showcase`，和其他对象一样创建和管理（10–14 人天，从 V4 提前）。公众投稿接口、迁移和 mds-ui 的界面仍在 V4。V2 约 52–91 人天，V4 约 59–116 人天。
  - V3 加入"把 public-ui 的功能加进 mds-ui"。先列功能清单（V3 第 2 步，全部搬约 41–66 人天），搬哪些、是否取代 public-ui 见"需要你决定的事"第 16 条。iop-core 客户端里没有元搜索和 RDF / linked data 链接的接口。
  - V1 第 6 步的 piveau 部分提前做完：删除 piveau SDK、`app/piveau/`、piveau 的配置；showcase 的列表页和首页区块改读 Nuxt Content。
  - 第 2 条决定：数据集网址用标识符，和 public-ui 的网址和 IRI 一样；旧的 opendata.swiss 网址不做重定向。V1 第 4 步因此只剩第 5 条（被删除页面的处理）。
  - V1 第 5 步：Decap 先保留，等 V2 的 showcase 后端完成以后再删；Decap 里依赖 piveau 的选择器换成 Decap 自带的字段，piveau 组件删除。`docs/cms.md` 暂时不改。
  - showcase 的主题改用 I14Y 主题（`Concept_DATASET_THEME`，26 个，存代码），和 iop-core 里数据集等对象的主题一样：Decap 的选项、`useThemes()`（原来的 `useCategories()`）、详情页和列表页的筛选都已改。现有 showcase 里的 EU 数据主题 URI 暂时不处理，网站上不显示，以后迁移时再改。G14 的 `ShowcaseTheme` 也存 I14Y 主题代码。
  - V1 第 6 步清理的 1–4 项完成：删除 4 个没用的组件和 5 个没用的依赖，开发工具移到 `devDependencies`，许可证映射合并成一份。漏洞数从 64 降到 62；剩下的生产依赖漏洞主要来自 Decap 和 Nuxt 的构建工具。
- **2026-10-19**：mds-ui 第一次部署到 AKS（dev）：集群 `aks-i14y-search-dev`，namespace `bfs-np-mds-ui-dev`，镜像 `i14ycontainers…azurecr.io/mds-ui:0.1.0`。`k8s/` 改写为 base 加 dev overlay；Nuxt Content 的数据库移到 `.data`，根文件系统只读。从外面怎么访问（ingress 或 LoadBalancer）还没定。
- **2026-10-19 到 2026-10-20**：dev 可以从外面访问了，`https://mds.i14y.d.c.bfs.admin.ch`。
  - 访问路径：Application Gateway（TLS，通配符证书）→ 内部 LoadBalancer `10.71.0.10:80` → pod。集群里不需要 ingress controller，第 8 步的 Ingress 一项删除，第 7 条里关于 ingress controller 和 cert-manager 的问题也删除。DNS 已生效；两台跳板机所在的 VNet 已和 AKS 对等连接。
  - 网关上的修正：mds-ui 的 HTTP 设置改为 Http:80；从共用后端池 `bfs-bep-i14y-dev` 删除 `10.71.0.10`，它让 core 和 keycloak 的探测一直失败。
  - 浏览器经 mds-ui 服务端的 `/input-backend/` 代理访问 iop-core，不需要 iop-core 的 CORS。另外比较过两种做法：给 iop-core 开 CORS，要改共用的 core；改用 Admin API，接口不同，而且 Admin API 以后会删除。代理只开放 5 个 GET 接口，有 Swagger 文档 `/input-backend/docs`。镜像 `0.1.1`。
  - V3 第 1 步和 V4 第 1 步：登录后的调用和写接口，是经过代理还是改用 CORS，到时候再定。
  - 第 1 步的勾选按代码核对后补上：2026-10-14 已在 `8c1e23d` 完成；剩下的文档清理在 2026-10-20 做完，只剩截图一项没做。进度表和第 7 条决定也一起更新。
