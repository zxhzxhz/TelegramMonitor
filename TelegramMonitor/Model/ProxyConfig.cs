using YamlDotNet.Serialization;

namespace TelegramMonitor.Models
{
    public class ProxyConfig
    {
        [YamlMember(Alias = "enabled")]
        public bool Enabled { get; set; } = false;

        [YamlMember(Alias = "type")]
        public string Type { get; set; } = "SOCKS"; // SOCKS 或 MTProto

        // SOCKS代理配置
        [YamlMember(Alias = "socksHost")]
        public string SocksHost { get; set; }

        [YamlMember(Alias = "socksPort")]
        public int SocksPort { get; set; }

        [YamlMember(Alias = "socksUsername")]
        public string SocksUsername { get; set; }

        [YamlMember(Alias = "socksPassword")]
        public string SocksPassword { get; set; }

        // MTProto代理配置
        [YamlMember(Alias = "mtprotoUrl")]
        public string MtprotoUrl { get; set; }
    }
}
