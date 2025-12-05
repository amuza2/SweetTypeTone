using System.Text.Json.Serialization;
using SweetTypeTone.Core.Models;

namespace SweetTypeTone.Core;

[JsonSerializable(typeof(SoundPack))]
[JsonSerializable(typeof(MechvibesConfig))]
[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(SoundDefinition))]
[JsonSerializable(typeof(Dictionary<int, SoundDefinition>))]
[JsonSerializable(typeof(List<SoundPack>))]
[JsonSerializable(typeof(List<string>))]
public partial class AppJsonContext : JsonSerializerContext
{
}
