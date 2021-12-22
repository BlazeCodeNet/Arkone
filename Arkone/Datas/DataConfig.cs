using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkone.Datas
{
    public class DataConfig
    {
        public string discordBotSecret { get; set; }
        public ulong ownerDiscordId { get; set; }
        public bool playerCountEnabled { get; set; }
        public ulong playerCountChannelDiscordId { get; set; }
        public ulong discordGuildId { get; set; }
        public bool rewardsEnabled { get; set; }
        public int rewardsCooldownSeconds { get; set; }
        public int rewardsAmount { get; set; }
        public string rconPrimaryAddress { get; set; }
        public string rconSecondaryAddress { get; set; }
        public string rconPassword { get; set; }
        public int rconQueryCooldownSeconds { get; set; }
    }
}
