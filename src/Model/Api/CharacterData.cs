using System.Collections.Generic;
using Newtonsoft.Json;

namespace D2SLib.Model.Api
{
    public class CharacterData
    {
        [JsonProperty("file")]
        public FileInfo File { get; set; }

        [JsonProperty("character")]
        public Character Character { get; set; }

        [JsonProperty("mercenary")]
        public Mercenary Mercenary { get; set; }

        [JsonProperty("items")]
        public List<D2Item> Items { get; set; } = new();
    }

    public class FileInfo
    {
        [JsonProperty("header")]
        public long Header { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("filesize")]
        public int FileSize { get; set; }

        [JsonProperty("checksum")]
        public long Checksum { get; set; }

        [JsonProperty("updated_at")]
        public long UpdatedAt { get; set; }
    }

    public class Character
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public CharacterStatus Status { get; set; }

        [JsonProperty("class")]
        public CharacterClass Class { get; set; }

        [JsonProperty("attributes")]
        public Attributes Attributes { get; set; }

        [JsonProperty("gold")]
        public Gold Gold { get; set; }

        [JsonProperty("points")]
        public Points Points { get; set; }

        [JsonProperty("life")]
        public int Life { get; set; }

        [JsonProperty("mana")]
        public int Mana { get; set; }

        [JsonProperty("stamina")]
        public int Stamina { get; set; }

        [JsonProperty("experience")]
        public long Experience { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("skills")]
        public List<Skill> Skills { get; set; } = new();
    }

    public class CharacterStatus
    {
        [JsonProperty("is_hardcore")]
        public bool IsHardcore { get; set; }

        [JsonProperty("is_dead")]
        public bool IsDead { get; set; }

        [JsonProperty("is_expansion")]
        public bool IsExpansion { get; set; }

        [JsonProperty("is_ladder")]
        public bool IsLadder { get; set; }
    }

    public class CharacterClass
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Attributes
    {
        [JsonProperty("strength")]
        public int Strength { get; set; }

        [JsonProperty("dexterity")]
        public int Dexterity { get; set; }

        [JsonProperty("vitality")]
        public int Vitality { get; set; }

        [JsonProperty("energy")]
        public int Energy { get; set; }
    }

    public class Gold
    {
        [JsonProperty("character")]
        public int Character { get; set; }

        [JsonProperty("stash")]
        public int Stash { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }

    public class Points
    {
        [JsonProperty("stat")]
        public int Stat { get; set; }

        [JsonProperty("skill")]
        public int Skill { get; set; }
    }

    public class Skill
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }
    }

    public class Mercenary
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name_id")]
        public int NameId { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("experience")]
        public long Experience { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("items")]
        public List<D2Item> Items { get; set; } = new();
    }
}