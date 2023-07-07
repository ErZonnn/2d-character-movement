/*Created by Pawe³ Mularczyk*/

using UnityEngine;

public enum SoundType
{
    effects,
    music,
    ui
};

public class VolumeMaster : MonoBehaviour
{
    [Header("VOLUME SETTINGS")]

    [Range(0, 1)]
    [SerializeField] private float _globalVolume = 1;
    [Range(0, 1)]
    [SerializeField] private float _effectsVolume = 1;
    [Range(0, 1)]
    [SerializeField] private float _musicVolume = 1;
    [Range(0, 1)]
    [SerializeField] private float _uiVolume = 1;

    public float GetVolume(SoundType soundType)
    {
        switch (soundType)
        {
            case SoundType.effects:
                return _effectsVolume * _globalVolume;

            case SoundType.music:
                return _musicVolume * _globalVolume;

            case SoundType.ui:
                return _uiVolume * _globalVolume;
        }

        return 0;
    }

    public void SetGlobalVolume(float value)
    {
        _globalVolume = value;
    }

    public void SetEffectsVolume(float value)
    {
        _effectsVolume = value;
    }

    public void SetMusicVolume(float value)
    {
        _musicVolume = value;
    }

    public void SetUIVolume(float value)
    {
        _uiVolume = value;
    }
}
