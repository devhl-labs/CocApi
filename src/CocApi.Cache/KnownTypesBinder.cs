using CocApi.Cache.Models;
using CocApi.Cache.Models.Clans;
using CocApi.Cache.Models.Villages;
using CocApi.Cache.Models.Wars;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace CocApi.Cache
{
    public sealed class KnownTypesBinder : ISerializationBinder
    {
        public IList<Type> KnownTypes { get; set; } = new List<Type>();

        public KnownTypesBinder()
        {
            //clan
            KnownTypes.Add(typeof(Clan));
            KnownTypes.Add(typeof(BadgeUrl));
            KnownTypes.Add(typeof(Location));
            KnownTypes.Add(typeof(ClanVillage));
            KnownTypes.Add(typeof(Donation));
            KnownTypes.Add(typeof(LeagueChange));
            KnownTypes.Add(typeof(RoleChange));
            KnownTypes.Add(typeof(SimpleClan));
            KnownTypes.Add(typeof(TopBuilderClan));
            KnownTypes.Add(typeof(TopClan));
            KnownTypes.Add(typeof(TopMainClan));

            //village
            KnownTypes.Add(typeof(Achievement));
            KnownTypes.Add(typeof(League));
            KnownTypes.Add(typeof(LeagueIcon));
            KnownTypes.Add(typeof(LegendLeagueResult));
            KnownTypes.Add(typeof(LegendLeagueStatistics));
            KnownTypes.Add(typeof(Spell));
            KnownTypes.Add(typeof(TopBuilderVillage));
            KnownTypes.Add(typeof(TopMainVillage));
            KnownTypes.Add(typeof(TopVillage));
            KnownTypes.Add(typeof(Troop));
            KnownTypes.Add(typeof(Village));
            KnownTypes.Add(typeof(VillageClan));

            //war
            KnownTypes.Add(typeof(Attack));
            KnownTypes.Add(typeof(CurrentWar));
            KnownTypes.Add(typeof(LeagueClan));
            KnownTypes.Add(typeof(LeagueGroup));
            KnownTypes.Add(typeof(LeagueGroupNotFound));
            KnownTypes.Add(typeof(LeagueVillage));
            KnownTypes.Add(typeof(LeagueWar));
            KnownTypes.Add(typeof(NotInWar));
            KnownTypes.Add(typeof(PrivateWarLog));
            KnownTypes.Add(typeof(Round));
            KnownTypes.Add(typeof(WarClan));
            KnownTypes.Add(typeof(WarLogEntry));
            KnownTypes.Add(typeof(WarLogEntryClan));
            KnownTypes.Add(typeof(WarVillage));
            KnownTypes.Add(typeof(WarLog));

            //other
            KnownTypes.Add(typeof(Cursor));
            KnownTypes.Add(typeof(Downloadable));
            KnownTypes.Add(typeof(Label));
            KnownTypes.Add(typeof(LabelUrl));
            KnownTypes.Add(typeof(Paging));
            KnownTypes.Add(typeof(ResponseMessage));
            KnownTypes.Add(typeof(WarLeague));

            //paged 
            KnownTypes.Add(typeof(Paginated<Label>));
            KnownTypes.Add(typeof(Paginated<League>));
            KnownTypes.Add(typeof(Paginated<Location>));
            KnownTypes.Add(typeof(Paginated<TopMainClan>));
            KnownTypes.Add(typeof(Paginated<TopBuilderClan>));
            KnownTypes.Add(typeof(Paginated<Clan>));
            KnownTypes.Add(typeof(Paginated<TopMainVillage>));
            KnownTypes.Add(typeof(Paginated<TopBuilderVillage>));
            KnownTypes.Add(typeof(Paginated<WarLeague>));
        }

        public Type BindToType(string? assemblyName, string typeName)
        {
            if (typeName == GetName(typeof(Paginated<Label>)))
                return typeof(Paginated<Label>);
            if (typeName == GetName(typeof(Paginated<League>)))
                return typeof(Paginated<League>);
            if (typeName == GetName(typeof(Paginated<Location>)))
                return typeof(Paginated<Location>);
            if (typeName == GetName(typeof(Paginated<TopMainClan>)))
                return typeof(Paginated<TopMainClan>);
            if (typeName == GetName(typeof(Paginated<TopBuilderClan>)))
                return typeof(Paginated<TopBuilderClan>);
            if (typeName == GetName(typeof(Paginated<Clan>)))
                return typeof(Paginated<Clan>);
            if (typeName == GetName(typeof(Paginated<TopMainVillage>)))
                return typeof(Paginated<TopMainVillage>);
            if (typeName == GetName(typeof(Paginated<TopBuilderVillage>)))
                return typeof(Paginated<TopBuilderVillage>);
            if (typeName == GetName(typeof(Paginated<WarLeague>)))
                return typeof(Paginated<WarLeague>);

            return KnownTypes.SingleOrDefault(t => t.Name == typeName);
        }

        public void BindToName(Type serializedType, out string? assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;

            if (serializedType == typeof(Paginated<Label>))
                typeName = GetName(typeof(Paginated<Label>));

            if (serializedType == typeof(Paginated<League>))
                typeName = GetName(typeof(Paginated<League>));

            if (serializedType == typeof(Paginated<Location>))
                typeName = GetName(typeof(Paginated<Location>));

            if (serializedType == typeof(Paginated<TopMainClan>))
                typeName = GetName(typeof(Paginated<TopMainClan>));

            if (serializedType == typeof(Paginated<TopBuilderClan>))
                typeName = GetName(typeof(Paginated<TopBuilderClan>));

            if (serializedType == typeof(Paginated<Clan>))
                typeName = GetName(typeof(Paginated<Clan>));

            if (serializedType == typeof(Paginated<TopMainVillage>))
                typeName = GetName(typeof(Paginated<TopMainVillage>));

            if (serializedType == typeof(Paginated<TopBuilderVillage>))
                typeName = GetName(typeof(Paginated<TopBuilderVillage>));

            if (serializedType == typeof(Paginated<WarLeague>))
                typeName = GetName(typeof(Paginated<WarLeague>));
        }

        private string GetName(Type type)
        {
            if (type == typeof(Paginated<Label>))
                return $"Paginated<{typeof(Label).Name}>";

            if (type == typeof(Paginated<League>))
                return $"Paginated<{typeof(League).Name}>";

            if (type == typeof(Paginated<Location>))
                return $"Paginated<{typeof(Location).Name}>";

            if (type == typeof(Paginated<TopMainClan>))
                return $"Paginated<{typeof(TopMainClan).Name}>";

            if (type == typeof(Paginated<TopBuilderClan>))
                return $"Paginated<{typeof(TopBuilderClan).Name}>";

            if (type == typeof(Paginated<Clan>))
                return $"Paginated<{typeof(Clan).Name}>";

            if (type == typeof(Paginated<TopMainVillage>))
                return $"Paginated<{typeof(TopMainVillage).Name}>";

            if (type == typeof(Paginated<TopBuilderVillage>))
                return $"Paginated<{typeof(TopBuilderVillage).Name}>";

            if (type == typeof(Paginated<WarLeague>))
                return $"Paginated<{typeof(WarLeague).Name}>";

            return type.Name;
        }
    }
}