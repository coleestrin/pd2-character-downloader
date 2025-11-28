using System.Collections.Generic;
using Newtonsoft.Json;

namespace D2SLib.Model.Api
{
    public class D2Item
    {
        [JsonProperty("defense")]
        public D2Defense Defense { get; set; }

        [JsonProperty("is_identified")]
        public bool IsIdentified { get; set; }

        [JsonProperty("is_socketed")]
        public bool IsSocketed { get; set; }

        [JsonProperty("is_new")]
        public bool IsNew { get; set; }

        [JsonProperty("is_ethereal")]
        public bool IsEthereal { get; set; }

        [JsonProperty("base")]
        public D2ItemBase Base { get; set; }

        [JsonProperty("socketed_count")]
        public int SocketedCount { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("item_level")]
        public int ItemLevel { get; set; }

        [JsonProperty("unique")]
        public UniqueO Unique { get; set; }

        [JsonProperty("quality")]
        public D2ItemQuality Quality { get; set; }

        [JsonProperty("runeword")]
        public D2Runeword Runeword { get; set; }

        [JsonProperty("durability")]
        public D2Durability Durability { get; set; }

        [JsonProperty("socket_count")]
        public int SocketCount { get; set; }

        [JsonProperty("modifiers")]
        public List<D2ItemModifier> Modifiers { get; set; } = new();

        [JsonProperty("socketed")]
        public List<D2Item> Socketed { get; set; } = new();

        [JsonProperty("location")]
        public D2ItemLocation Location { get; set; }

        [JsonProperty("position")]
        public D2ItemPosition Position { get; set; }
    }

    public class D2ItemBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("codes")]
        public D2ItemCodes Codes { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("requirements")]
        public D2ItemRequirements Requirements { get; set; }

        [JsonProperty("damage")]
        public D2ItemDamage Damage { get; set; }
    }

    public class UniqueO
    {
        [JsonProperty("id")]
        public uint Id { get; set; }
    }

    public class D2ItemCodes
    {
        [JsonProperty("normal")]
        public string Normal { get; set; }

        [JsonProperty("exceptional")]
        public string Exceptional { get; set; }

        [JsonProperty("elite")]
        public string Elite { get; set; }
    }

    public class D2ItemRequirements
    {
        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("strength")]
        public int Strength { get; set; }
    }

    public class D2ItemDamage
    {
        [JsonProperty("one_handed")]
        public D2DamageRange OneHanded { get; set; }

        public class D2DamageRange
        {
            [JsonProperty("minimum")]
            public int Minimum { get; set; }

            [JsonProperty("maximum")]
            public int Maximum { get; set; }
        }
    }

    public class D2ItemQuality
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class D2Runeword
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("runes")]
        public List<string> Runes { get; set; } = new();
    }

    public class D2Defense
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("base")]
        public int Base { get; set; }
    }

    public class D2Durability
    {
        [JsonProperty("maximum")]
        public int Maximum { get; set; }

        [JsonProperty("current")]
        public int Current { get; set; }
    }

    public class D2ItemModifier
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("values")]
        public List<float> Values { get; set; } = new();

        [JsonProperty("label")]
        public string Label { get; set; }
    }

    public class D2ItemLocation
    {
        [JsonProperty("equipment_id")]
        public int EquipmentId { get; set; }

        [JsonProperty("zone")]
        public string Zone { get; set; }
    }

    public class D2ItemPosition
    {
        [JsonProperty("column")]
        public int Column { get; set; }

        [JsonProperty("row")]
        public int Row { get; set; }
    }
}