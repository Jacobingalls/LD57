using UnityEngine;

public static class AudioManagerExtension
{
    public static AudioSource Play2D(this AudioManager am, string id, bool loop = false, float pitchMin = 1.0f, float pitchMax = 1.0f, float volumeMin = 1.0f, float volumeMax = 1.0f, bool isMusic = false, Vector3? position = null, float minDistance = 0.01f, float maxDistance = 20.0f)
    {
        if (position != null)
        {
            position = new(Camera.main.transform.position.x, position.Value.y, Camera.main.transform.position.z);
        }
        return am.Play(id, loop, pitchMin, pitchMax, volumeMin, volumeMax, isMusic, position, minDistance, maxDistance);
    }


}
