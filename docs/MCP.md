# 小黄豆 CRM MCP 接入

MCP 已经集成在 `XHD.Core.View` 中，不需要单独启动服务或占用新端口。

WorkBuddy 配置：

```json
{
  "mcpServers": {
    "my-crm": {
      "type": "http",
      "url": "https://crm.yourdomain.com/mcp",
      "headers": {
        "Authorization": "Bearer xhdcrm_你的TOKEN"
      }
    }
  }
}
```

暂时不需要 `X-API-Key`。

## 1. 在 Web 后台生成 Token

登录 CRM 后，点击右上角用户名：

**AI 接入令牌** → **新建 Token**

建议每个 AI 客户端单独建一个 Token，例如：

- WorkBuddy
- Cursor
- Claude Desktop
- 企业微信机器人

Token 默认有效期 10 年，创建时完整显示一次；关闭窗口后只显示“前缀 + ******** + 尾部”。可以随时撤销，撤销后立即失效。

## 2. API 鉴权

API 控制器现在同时支持两种 Token：

- Web 后台生成的长期 Bearer Token：`xhdcrm_...`
- 原 APP 使用的登录 Token

Web 页面仍然使用原来的 Cookie 登录，其他 Controller 完全不受影响。

## 3. MCP 转发

`/mcp` 由 `XHD.Core.View` 直接托管。MCP 工具只做参数转发：

```text
WorkBuddy -> /mcp -> MCP Tool -> /API/xxx -> 原业务 Service / FreeSql
```

业务逻辑仍然只在 API / Service 里维护，MCP 不复制一套 CRM 逻辑。

当前工具包括：

- CRM 经营概览
- 全局搜索
- 客户列表、客户详情
- 订单列表、订单详情
- 合同列表
- 客户跟进、订单售后
- 收款计划、收款记录
- 财务汇总
- 新增/修改客户
- 新增客户跟进
- 新增订单
- 更新订单状态
- 新增合同
- 新增收款计划
- 登记收款
- 新增订单售后
- 员工、数据字典、数据结构说明

所有工具都会经过 API 的 `checkToken`，并继续执行原系统的数据权限、菜单权限和按钮权限。

## 4. 部署配置

`1.UI/XHD.Core.View/appsettings.json`：

```json
"Mcp": {
  "ApiBaseUrl": ""
}
```

一般留空即可，MCP 会使用当前请求的协议和域名转发到同站 API。

如果 CRM 部署在反向代理后面，服务器访问自己的公网域名不方便，可以配置内网地址：

```json
"Mcp": {
  "ApiBaseUrl": "http://127.0.0.1:5001"
}
```

## 5. 安全说明

- Token 只保存 SHA-256 哈希，数据库泄露也拿不到明文 Token。
- 每个用户只能看到和撤销自己的 Token。
- 账号禁用或 Token 撤销后立即失效。
- 不要多个用户共用一个 Token；按 AI/人员分别创建，便于审计。
- 写入工具仍然受按钮权限限制，例如没有 `Sale_Order|add` 的账号不能新增订单。
- 目前不需要 `X-API-Key`，后续如需额外防护可在网关层增加。
