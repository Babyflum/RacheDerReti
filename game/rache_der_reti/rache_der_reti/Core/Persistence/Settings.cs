using System;
using Newtonsoft.Json;
using rache_der_reti.Game.Global;

namespace rache_der_reti.Core.Persistence;

[Serializable]
public class Settings
{
    [JsonProperty]
    public bool BackgroundMusicEnabled { get; set; } = true;
    [JsonProperty]
    public bool SoundEffectsEnabled { get; set; } = true;
    [JsonProperty]
    public float BackgroundMusicVolume { get; set; } = 1f;
    
    [JsonProperty]
    public float SoundEffectVolume { get; set; } = 1f;
    [JsonProperty]
    public bool DebugEnabled { get; set; }


    public void Save()
    {
        // save into file
        Serialize serializer = new Serialize();
        serializer.SerializeObject(this, "Settings");
        Apply();
    }

    public void Apply()
    {
        Globals.mSoundManager.BackgroundMusicEnabled = BackgroundMusicEnabled;
        Globals.mSoundManager.SoundVolume = BackgroundMusicVolume;
        Globals.mSoundManager.SoundEffectsEnabled = SoundEffectsEnabled;
    }

    public static Settings Load()
    {
        Serialize serializer = new Serialize();
        Settings settingsBuilder = new Settings();

        var jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            NullValueHandling = NullValueHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        };
        Settings settings = (Settings)serializer.PopulateObject(settingsBuilder, "Settings", jsonSerializerSettings) ??
                            settingsBuilder;

        return settings;
    }
}