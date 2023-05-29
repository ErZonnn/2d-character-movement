/*Created by Pawe³ Mularczyk*/

using UnityEngine;

public class MusicSystem : MonoBehaviour
{
    [SerializeField] private SoundType _soundType;
    [SerializeField] private VolumeMaster _volumeMaster;
    [SerializeField] private AudioSource _musicAudioSource;
    [SerializeField] private AudioClip[] _musicClips;

    private float _saveVolume;

    private void Start()
    {
        PlayMusic();
    }

    private void Update()
    {
        if (_saveVolume != _volumeMaster.GetVolume(_soundType))
        {
            _musicAudioSource.volume = _volumeMaster.GetVolume(_soundType);
            _saveVolume = _volumeMaster.GetVolume(_soundType);
        }
    }

    public void PlayMusic(float clipID = default)
    {
        if(clipID != default)
        {

        }
        else
        {
            _musicAudioSource.clip = _musicClips[Random.Range(0, _musicClips.Length)];
            _musicAudioSource.volume = _volumeMaster.GetVolume(_soundType);

            _saveVolume = _volumeMaster.GetVolume(_soundType);

            _musicAudioSource.Play();
        }
    }
}
