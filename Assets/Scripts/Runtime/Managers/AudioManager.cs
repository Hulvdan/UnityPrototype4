﻿using System;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;
using Foundation.Architecture;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public enum Sound {
    UI_ButtonSelected,
    UI_ButtonDeselected,
    GP_RoadPlaced,
    GP_BuildingPlaced,
    GP_FlagPlaced,
    GP_CityHallSpawnedHuman,
    GP_HumanFootsteps,
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
        if (!Application.isPlaying) {
            return;
        }

        _state = LoadAudioSettings();
        foreach (var sound in _sounds) {
            _bindings.Add(sound.Sound, sound.Event);
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
            switch (data.Item) {
                case SelectedItemType.Road:
                    PlaySound(Sound.GP_RoadPlaced);
                    break;
                case SelectedItemType.Building:
                    PlaySound(Sound.GP_BuildingPlaced);
                    break;
                case SelectedItemType.Flag:
                    PlaySound(Sound.GP_FlagPlaced);
                    break;
            }
        });
        DomainEvents<E_CityHallCreatedHuman>.Subscribe(
            _ => PlaySound(Sound.GP_CityHallSpawnedHuman)
        );
        DomainEvents<E_HumanFootstep>.Subscribe(_ => PlaySound(Sound.GP_HumanFootsteps));
    }

    AudioState LoadAudioSettings() {
        return new() {
            masterVolume = PlayerPrefs.GetFloat(AudioState.Key_MasterVolume, .5f),
            sfxVolume = PlayerPrefs.GetFloat(AudioState.Key_SfxVolume, .5f),
            musicVolume = PlayerPrefs.GetFloat(AudioState.Key_MusicVolume, .5f),
        };
    }

    void SaveAudioSettings() {
        PlayerPrefs.SetFloat(AudioState.Key_MasterVolume, _state.masterVolume);
        PlayerPrefs.SetFloat(AudioState.Key_SfxVolume, _state.sfxVolume);
        PlayerPrefs.SetFloat(AudioState.Key_MusicVolume, _state.musicVolume);
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

internal class AudioState {
    public const string Key_MasterVolume = "Key_IsActive_MasterVolume";
    public const string Key_SfxVolume = "Key_IsActive_SFXVolume";
    public const string Key_MusicVolume = "Key_IsActive_MusicVolume";

    public float masterVolume { get; set; }
    public float sfxVolume { get; set; }
    public float musicVolume { get; set; }
}
}