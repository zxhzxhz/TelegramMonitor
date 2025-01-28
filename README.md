### AD -- 机场推广

**机场 - 老百姓自己的机场**：[https://老百姓自己的机场.com](https://老百姓自己的机场.com)  
咱老百姓就得用自己的机场 **老百姓自己的机场** 做用的起的机场



# Telegram关键词监控 使用说明

## 系统与环境要求
- 最新发布版下载：https://github.com/Riniba/TelegramMonitor/releases/latest
- 发布包提供常见的系统版本已经包含运行时。  
- 如需其他可自行编译
- 请注意 使用时需具备**全局代理**或能**直连 Telegram**。  

## 账号与频道准备
- 准备一个 Telegram 账号，该账号需拥有一个或多个可发布消息的频道或者群组。
- 该账号需对所选频道或者群组拥有发布消息的权限。
- 该账号需加入多个群组（Group），软件将监听这些群组中的消息。
- 当群组中出现匹配关键字的消息时，软件会将该消息内容转发至指定频道。

## 使用步骤
1. 确认已完成上述准备工作。
2. 双击运行 `TelegramMonitor.exe`。
3. 输入已经准备好的 Telegram 账号的手机号码，例如：`+8618888888888`。
4. 根据提示输入验证码或二级密码（首次登录时需要，成功登录后再次运行则无需）。
5. 成功登录后，程序会列出您拥有发布权限的频道/群组名称和对应的频道/群组 ID，并提示您选择。
6. 最新版的需要用键盘↑↓键选择你要发布消息的频道/群组，然后按 Enter 键选择就行了。
7. 选择完成后，软件会提示已经开始工作，并在所选频道/群组发布一条消息“开始工作”作为标记。
8. 此后请保持软件运行，关闭软件将无法继续监听群消息。
9. 如果需要停止软件请在那个黑框框里面输入stop 然后按Enter键就可以退出软件了 当然最简单的是直接右上角点X关闭

## 关键词设置

- 关键词文件为 `keyword.yaml`，软件开始工作后会自动在同级目录创建该文件 无需自己创建 
- 请系统自行创建后在进行修改添加
- 每次修改关键词文件无需重启软件即可生效。

> [!NOTE]
>
> 批量关键词(单配置)设置在线生成
>
> [https://riniba.net/GenerateFromKeywords.html](https://riniba.net/GenerateFromKeywords.html)
>
> 批量关键词(多配置)设置在线生成
>
> [https://riniba.net//GenerateFromList.html](https://riniba.net//GenerateFromList.html)



```yaml
# 关键词监控配置文件
# -----------------------------

# 配置文件说明
# KeywordContent:   关键词内容，默认为空字符串
#                  可以是普通文本、正则表达式、用户名或用户ID
#
# KeywordType:      关键词匹配类型，默认为 Contains
#                  - Contains:  包含匹配，检查消息中是否包含关键词
#                  - Regex:     正则表达式匹配，使用正则表达式进行匹配
#                  - Fuzzy:     模糊匹配，使用 ? 分隔多个关键词，所有词都匹配才算匹配
#                  - FullWord:  全字匹配，消息需要完全等于关键词
#                  - User:      用户匹配，可使用用户名或用户ID进行匹配
#
# KeywordAction:    匹配后的动作，默认为 Monitor
#                  - Monitor:   监控消息，匹配时进行记录和通知 如果是用户 则该用户的所有消息会被记录
#                  - Exclude:   排除匹配的内容，用于过滤不需要的内容 如果是用户 则该用户的所有消息不会被记录
#
# 文本样式选项:      以下选项默认都为 false
#   IsCaseSensitive:   是否区分大小写
#   IsBold:            是否使用粗体
#   IsItalic:          是否使用斜体
#   IsUnderline:       是否添加下划线
#   IsStrikeThrough:   是否添加删除线
#   IsQuote:           是否作为引用显示
#   IsMonospace:       是否使用等宽字体
#   IsSpoiler:         是否作为剧透内容处理

# ===== 示例配置 =====

- keywordContent: '你好世界'            # 示例1: 包含匹配关键词
  keywordType: Contains                # 包含匹配
  keywordAction: Monitor               # 监控消息
  isCaseSensitive: false               # 不区分大小写
  isBold: false                        # 不使用粗体
  isItalic: false                      # 不使用斜体
  isUnderline: false                   # 不添加下划线
  isStrikeThrough: false               # 不添加删除线
  isQuote: false                       # 不作为引用显示
  isMonospace: false                   # 不使用等宽字体
  isSpoiler: false                     # 不作为剧透内容处理

- keywordContent: '\b1[3-9]\d{9}\b'    # 示例2: 正则表达式匹配手机号
  keywordType: Regex                   # 正则包含匹配
  keywordAction: Monitor               # 监控消息
  isCaseSensitive: true                # 区分大小写
  isBold: true                         # 使用粗体
  isItalic: false                      # 不使用斜体
  isUnderline: false                   # 不添加下划线
  isStrikeThrough: false               # 不添加删除线
  isQuote: false                       # 不作为引用显示
  isMonospace: true                    # 使用等宽字体
  isSpoiler: false                     # 不作为剧透内容处理

- keywordContent: '早上?好?问候'        # 示例3: 模糊匹配关键词
  keywordType: Fuzzy                   # 模糊匹配
  keywordAction: Monitor               # 监控消息
  isCaseSensitive: false               # 不区分大小写
  isBold: false                        # 不使用粗体
  isItalic: true                       # 使用斜体
  isUnderline: true                    # 添加下划线
  isStrikeThrough: false               # 不添加删除线
  isQuote: true                        # 作为引用显示
  isMonospace: false                   # 不使用等宽字体
  isSpoiler: false                     # 不作为剧透内容处理

- keywordContent: 'riniba'             # 示例4: 用户监控
  keywordType: User                    # 用户匹配
  keywordAction: Monitor               # 监控消息
  isCaseSensitive: false               # 不区分大小写
  isBold: false                        # 不使用粗体
  isItalic: true                       # 使用斜体
  isUnderline: false                   # 不添加下划线
  isStrikeThrough: false               # 不添加删除线
  isQuote: false                       # 不作为引用显示
  isMonospace: false                   # 不使用等宽字体
  isSpoiler: false                     # 不作为剧透内容处理

- keywordContent: '广告'               # 示例5: 内容排除
  keywordType: Contains                # 包含匹配
  keywordAction: Exclude               # 排除匹配
  isCaseSensitive: false               # 不区分大小写
  isBold: false                        # 不使用粗体
  isItalic: false                      # 不使用斜体
  isUnderline: false                   # 不添加下划线
  isStrikeThrough: true                # 添加删除线
  isQuote: false                       # 不作为引用显示
  isMonospace: false                   # 不使用等宽字体
  isSpoiler: false                     # 不作为剧透内容处理
```

> **重要提示：**  
> 只会监控群组的消息。请保持软件运行，以持续监听。

## 其他说明
- 本软件免费无毒，可在虚拟机中运行进行长期挂机。
- 有问题请联系 Telegram：[https://t.me/Riniba](https://t.me/Riniba)
- Telegram交流群组 [https://t.me/RinibaGroup](https://t.me/RinibaGroup)

  

---

