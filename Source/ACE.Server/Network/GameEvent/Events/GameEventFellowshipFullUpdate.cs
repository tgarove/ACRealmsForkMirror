using System;
using System.Collections.Generic;
using System.Linq;
using ACE.Server.Entity;
using ACE.Server.Network.Structure;
using ACE.Server.WorldObjects;

namespace ACE.Server.Network.GameEvent.Events
{
    public class GameEventFellowshipFullUpdate : GameEventMessage
    {
        private static readonly HashComparer FellowComparer = new HashComparer(16);

        public GameEventFellowshipFullUpdate(ISession session)
            : base(GameEventType.FellowshipFullUpdate, GameMessageGroup.UIQueue, session, 512) // 338 is the average seen in retail pcaps, 1,264 is the max seen in retail pcaps
        {
            var fellowship = session.Player.Fellowship;

            var fellows = fellowship.GetFellowshipMembers().ToDictionary(x => (uint)x.Key, x => x.Value);

            PackableHashTable.WriteHeader(Writer, fellows.Count, FellowComparer.NumBuckets);

            var sorted = new SortedDictionary<uint, Player>(fellows, FellowComparer);

            foreach (var fellow in sorted.Values)
            {
                WriteFellow(fellow);
            }

            Writer.WriteString16L(fellowship.FellowshipName);
            Writer.WriteGuid(new ACE.Entity.ObjectGuid(fellowship.FellowshipLeaderGuid));
            Writer.Write(Convert.ToUInt32(fellowship.ShareXP));
            Writer.Write(Convert.ToUInt32(fellowship.EvenShare));
            Writer.Write(Convert.ToUInt32(fellowship.Open));

            Writer.Write(Convert.ToUInt32(fellowship.IsLocked));

            Writer.Write(fellowship.DepartedMembers);

            Writer.Write(fellowship.FellowshipLocks);
        }

        public void WriteFellow(Player fellow)
        {
            Writer.WriteGuid(fellow.Guid);

            Writer.Write(0u); // TODO: cpCached - Perhaps cp stored up before distribution?
            Writer.Write(0u); // TODO: lumCached - Perhaps lum stored up before distribution?

            Writer.Write(fellow.Level ?? 1);

            Writer.Write(fellow.Health.MaxValue);
            Writer.Write(fellow.Stamina.MaxValue);
            Writer.Write(fellow.Mana.MaxValue);

            Writer.Write(fellow.Health.Current);
            Writer.Write(fellow.Stamina.Current);
            Writer.Write(fellow.Mana.Current);

            // todo: share loot with this fellow?
            Writer.Write((uint)0x10); // TODO: shareLoot - if 0 then noSharePhatLoot, if 16(0x0010) then sharePhatLoot

            Writer.WriteString16L(fellow.Name);
        }
    }
}
