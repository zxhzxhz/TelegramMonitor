namespace TelegramMonitor
{
    /// <summary>
    /// 关键词配置类，用于定义关键词的匹配规则和文本样式
    /// </summary>
    public class KeywordConfig
    {
        private string? keywordContent;
        private KeywordType keywordType = KeywordType.Contains; // 默认为Contains
        private KeywordAction keywordAction = KeywordAction.Monitor; // 默认为Monitor

        /// <summary>
        /// 获取或设置关键词内容(文本/用户名/用户ID)
        /// </summary>
        /// <value>关键词字符串</value>
        public string? KeywordContent 
        { 
            get => keywordContent;
            set => keywordContent = value;
        }

        /// <summary>
        /// 获取或设置关键词匹配类型
        /// </summary>
        /// <value>关键词匹配类型枚举值</value>
        public KeywordType KeywordType 
        {
            get => keywordType;
            set => keywordType = value;
        }

        /// <summary>
        /// 获取或设置匹配后的处理方式
        /// </summary>
        /// <value>关键词处理动作枚举值</value>
        public KeywordAction KeywordAction
        {
            get => keywordAction;
            set => keywordAction = value;
        }

        /// <summary>
        /// 获取或设置是否区分大小写
        /// </summary>
        /// <value>为 true 时表示匹配时区分大小写；false 则不区分</value>
        public bool IsCaseSensitive { get; set; } = false;

        /// <summary>
        /// 获取或设置文本是否加粗
        /// </summary>
        public bool IsBold { get; set; } = false;

        /// <summary>
        /// 获取或设置文本是否斜体
        /// </summary>
        public bool IsItalic { get; set; } = false;

        /// <summary>
        /// 获取或设置文本是否底线
        /// </summary>
        public bool IsUnderline { get; set; } = false;

        /// <summary>
        /// 获取或设置文本是否删除线
        /// </summary>
        public bool IsStrikeThrough { get; set; } = false;

        /// <summary>
        /// 获取或设置文本是否引用
        /// </summary>
        public bool IsQuote { get; set; } = false;

        /// <summary>
        /// 获取或设置文本是否等宽
        /// </summary>
        public bool IsMonospace { get; set; } = false;

        /// <summary>
        /// 获取或设置文本是否剧透
        /// </summary>
        public bool IsSpoiler { get; set; } = false;
    }
}