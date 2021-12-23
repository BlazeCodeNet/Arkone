using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkone.Datas
{
    public class DataConfig
    {
        public string discordBotSecret { get; set; } = "";
        public ulong ownerDiscordId { get; set; } = 0;
        public bool playerCountEnabled { get; set; } = true;
        public ulong playerCountChannelDiscordId { get; set; } = 0;
        public int discordModifyCooldownSeconds { get; private set; } = 120;
        public ulong discordGuildId { get; set; } = 0;
        public bool rewardsEnabled { get; set; } = true;
        public int rewardsCooldownSeconds { get; set; } = 900;
        public int rewardsAmount { get; set; } = 10;
        public string rconPrimaryAddress { get; set; } = "127.0.0.1:27015";
        public string rconSecondaryAddress { get; set; } = "127.0.0.1:27016";
        public string rconPassword { get; set; } = "RCON PASSWORD123";
        public int rconQueryCooldownSeconds { get; set; } = 5;
    }
}
