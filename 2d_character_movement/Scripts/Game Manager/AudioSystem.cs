/*Created by Pawe³ Mularczyk*/

using UnityEngine;
public class AudioSystem : MonoBehaviour
{
    [SerializeField] private AudioSource _mainAudioSource;
    [SerializeField] private VolumeMaster _volumeMaster;

    public void PlayAudio(AudioClip clip, SoundType soundType, Vector2 pitchRange = default)
    {
        if (!_mainAudioSource.isPlaying)
        {
            _mainAudioSource.clip = clip;
            _mainAudioSource.volume = _volumeMaster.GetVolume(soundType);

            if (pitchRange != default)
                _mainAudioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);

            _mainAudioSource.Play();
        }
        else
        {
            AudioSource tempA_Source = gameObject.AddComponent<AudioSource>();

            tempA_Source.clip = clip;
            tempA_Source.volume = _volumeMaster.GetVolume(soundType);

            if (pitchRange != default)
                tempA_Source.pitch = Random.Range(pitchRange.x, pitchRange.y);

            tempA_Source.Play();

            Destroy(tempA_Source, tempA_Source.clip.length);
        }
    }
}
