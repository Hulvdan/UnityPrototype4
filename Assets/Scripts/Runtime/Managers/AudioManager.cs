using System;
using System.Collections.Generic;
using BFG.Runtime.Entities;
using FMOD.Studio;
using FMODUnity;
using Foundation.Architecture;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace BFG.Runtime {
public enum Sound {
    UI_ButtonSelected,
    UI_ButtonDeselected,
    GP_RoadPlaced,
    GP_BuildingPlaced,
    GP_FlagPlaced,
    GP_CityHallSpawnedHuman,
    GP_HumanFootsteps,
    GP_ResourePlacedInsideBuilding,
}

[Serializable]
public class SoundBinding {
    public EventReference ev;

    public Sound sound;
}

public class AudioManager : MonoBehaviour {
    [SerializeField]
    List<SoundBinding> _sounds = new();

    public void Init() {
        if (!Application.isPlaying) {
            return;
        }

        _state = LoadAudioSettings();
        foreach (var sound in _sounds) {
            _bindings.Add(sound.sound, sound.ev);
        }

        _masterBus = RuntimeManager.GetBus("bus:/");
        _sfxBus = RuntimeManager.GetBus("bus:/SFX");
        _musicBus = RuntimeManager.GetBus("bus:/Music");

        _masterBus.setVolume(_state.masterVolume);
        _sfxBus.setVolume(_state.sfxVolume);
        _musicBus.setVolume(_state.musicVolume);

        DomainEvents<E_ButtonSelected>.Subscribe(_ => PlaySound(Sound.UI_ButtonSelected));
        DomainEvents<E_ButtonDeselected>.Subscribe(_ => PlaySound(Sound.UI_ButtonDeselected));
        DomainEvents<E_ItemPlaced>.Subscribe(data => {
            switch (data.item) {
                case ItemToBuildType.Road:
                    PlaySound(Sound.GP_RoadPlaced);
                    break;
                case ItemToBuildType.Building:
                    PlaySound(Sound.GP_BuildingPlaced);
                    break;
                case ItemToBuildType.Flag:
                    PlaySound(Sound.GP_FlagPlaced);
                    break;
            }
        });
        DomainEvents<E_CityHallCreatedHuman>.Subscribe(
            _ => PlaySound(Sound.GP_CityHallSpawnedHuman)
        );
        DomainEvents<E_HumanFootstep>.Subscribe(_ => PlaySound(Sound.GP_HumanFootsteps));
        DomainEvents<E_ResourcePlacedInsideBuilding>.Subscribe(
            _ => PlaySound(Sound.GP_ResourePlacedInsideBuilding)
        );
    }

    AudioState LoadAudioSettings() {
        return new() {
            masterVolume = PlayerPrefs.GetFloat(AudioState.KEY_MASTER_VOLUME, .5f),
            sfxVolume = PlayerPrefs.GetFloat(AudioState.KEY_SFX_VOLUME, .5f),
            musicVolume = PlayerPrefs.GetFloat(AudioState.KEY_MUSIC_VOLUME, .5f),
        };
    }

    void SaveAudioSettings() {
        PlayerPrefs.SetFloat(AudioState.KEY_MASTER_VOLUME, _state.masterVolume);
        PlayerPrefs.SetFloat(AudioState.KEY_SFX_VOLUME, _state.sfxVolume);
        PlayerPrefs.SetFloat(AudioState.KEY_MUSIC_VOLUME, _state.musicVolume);
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

    readonly Dictionary<Sound, EventReference> _bindings = new();
}

internal class AudioState {
    public const string KEY_MASTER_VOLUME = "Key_IsActive_MasterVolume";
    public const string KEY_SFX_VOLUME = "Key_IsActive_SFXVolume";
    public const string KEY_MUSIC_VOLUME = "Key_IsActive_MusicVolume";

    public float masterVolume { get; set; }
    public float sfxVolume { get; set; }
    public float musicVolume { get; set; }
}
}
