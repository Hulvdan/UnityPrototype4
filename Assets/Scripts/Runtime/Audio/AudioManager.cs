using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BFG.Runtime {
public class AudioState {
    public const string Key_MasterVolume = "Key_IsActive_MasterVolume";
    public const string Key_SFXVolume = "Key_IsActive_SFXVolume";
    public const string Key_MusicVolume = "Key_IsActive_MusicVolume";

    public float MasterVolume;
    public float SFXVolume;
    public float MusicVolume;
}

public enum SoundBindingType {
    UI,
    Gameplay,
    Music,
}

[Serializable]
public class SoundBinding {
    public EventReference Event;

    SoundBindingType _type;

    [ShowIf("_type", SoundBindingType.UI)]
    public Sounds.UI soundUI;
}

public class AudioManager : MonoBehaviour {
    AudioState _state;

    public void Init() {
        _state = LoadAudioSettings();

        _masterBus = RuntimeManager.GetBus("bus:/");
        _musicBus = RuntimeManager.GetBus("bus:/Music");
        _ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
        _sfxBus = RuntimeManager.GetBus("bus:/SFX");
    }

    public AudioState LoadAudioSettings() {
        return new() {
            MasterVolume = PlayerPrefs.GetFloat(AudioState.Key_MasterVolume, .5f),
            SFXVolume = PlayerPrefs.GetFloat(AudioState.Key_SFXVolume, .5f),
            MusicVolume = PlayerPrefs.GetFloat(AudioState.Key_MusicVolume, .5f),
        };
    }
}
}
