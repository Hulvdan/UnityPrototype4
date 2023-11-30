using System;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using Sirenix.OdinInspector;
using FMODUnity;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace BFG.Runtime {
public class AudioState {
    public const string Key_MasterVolume = "Key_IsActive_MasterVolume";
    public const string Key_SFXVolume = "Key_IsActive_SFXVolume";
    public const string Key_MusicVolume = "Key_IsActive_MusicVolume";

    public float MasterVolume;
    public float SFXVolume;
    public float MusicVolume;
}

public enum Sound {
    UI_ButtonClick,
    UI_ButtonUnclick,
}

[Serializable]
public class SoundBinding {
    public EventReference Event;
    public Sound Sound;
}

public class AudioManager : MonoBehaviour {
    [SerializeField]
    List<SoundBinding> _sounds = new();

    public void Init() {
        _state = LoadAudioSettings();

        foreach (var sound in _sounds) {
            _bindings.Add(sound.Sound, sound.Event);
        }

        _masterBus = RuntimeManager.GetBus("bus:/");
        _sfxBus = RuntimeManager.GetBus("bus:/SFX");
        _musicBus = RuntimeManager.GetBus("bus:/Music");

        _masterBus.setVolume(_state.MasterVolume);
        _sfxBus.setVolume(_state.SFXVolume);
        _musicBus.setVolume(_state.MusicVolume);
    }

    public AudioState LoadAudioSettings() {
        return new() {
            MasterVolume = PlayerPrefs.GetFloat(AudioState.Key_MasterVolume, .5f),
            SFXVolume = PlayerPrefs.GetFloat(AudioState.Key_SFXVolume, .5f),
            MusicVolume = PlayerPrefs.GetFloat(AudioState.Key_MusicVolume, .5f),
        };
    }

    public void SaveAudioSettings() {
        PlayerPrefs.SetFloat(AudioState.Key_MasterVolume, _state.MasterVolume);
        PlayerPrefs.SetFloat(AudioState.Key_SFXVolume, _state.SFXVolume);
        PlayerPrefs.SetFloat(AudioState.Key_MusicVolume, _state.MusicVolume);
    }

    void PlaySound(Sound sound) {
        Assert.IsTrue(_bindings.ContainsKey(sound));
        PlayOneShot(_bindings[sound], Vector3.zero);
    }

    void PlayOneShot(EventReference sound, Vector3 worldPos) {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    AudioState _state;
    Bus _masterBus;
    Bus _musicBus;
    Bus _sfxBus;

    Dictionary<Sound, EventReference> _bindings = new();
}
}
