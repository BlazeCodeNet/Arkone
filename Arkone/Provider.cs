using DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkone
{
    public class Provider
    {
        public static async Task<bool> IsMasterUserAsync(DiscordMember target)
        {
            if ( target != null )
            {
                if ( target.Roles.Any( x => x.Name.Contains( "Master" ) ) || target.Id.Equals( Program.data.config.ownerDiscordId ) )
                {
                    return true;
                }
            }
            return false;
        }
    }
}
