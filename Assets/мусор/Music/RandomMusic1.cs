using UnityEngine;

public class RandomMusic1 : MonoBehaviour
{
    public AudioClip[] musicTracks; // Массив аудиотреков
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PlayRandomTrack();
    }

    void PlayRandomTrack()
    {
        if (musicTracks.Length == 0) return; // Проверка на наличие треков

        int randomIndex = Random.Range(0, musicTracks.Length); // Генерация случайного индекса
        audioSource.clip = musicTracks[randomIndex]; // Установка случайного трека
        audioSource.Play(); // Воспроизведение трека

        // Воспроизведение следующего трека после окончания текущего
        Invoke("PlayRandomTrack", audioSource.clip.length);
    }
}
