using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music Settings")]
    [SerializeField] private AudioClip[] musicTracks;
    [SerializeField] private AudioClip titleScreenTrack;
    [SerializeField] private float crossFadeDuration = 2.0f;
    [SerializeField] private float defaultVolume = 0.7f;
    
    [Header("Playback Settings")]
    [SerializeField] private bool continuousPlayback = true;
    [SerializeField] private bool randomizePlayback = false;
    [SerializeField] private float delayBetweenTracks = 1.0f;
    [SerializeField] private bool avoidRepeatingLastTrack = true;
    
    [Header("Current Track")]
    [SerializeField] private int currentTrackIndex = -1;
    
    // Components
    private AudioSource[] audioSources;
    private int activeSource = 0;
    private Coroutine fadeCoroutine;
    private Coroutine continuousPlaybackCoroutine;
    private List<int> playbackHistory = new List<int>();
    private int historyMaxSize = 3; // Remembers the last few tracks to avoid quick repeats
    private bool playingTitleScreen = false;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize audio sources
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeAudioSources()
    {
        // Create two audio sources for crossfading
        audioSources = new AudioSource[2];
        
        for (int i = 0; i < 2; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].loop = false; // Changed to false for continuous playback system
            audioSources[i].playOnAwake = false;
            audioSources[i].volume = 0f;
        }
    }
    
    private void Update()
    {
        // Check if current track has ended and we need to play the next one
        if (continuousPlayback && currentTrackIndex >= 0 && !playingTitleScreen)
        {
            AudioSource currentSource = audioSources[activeSource];
            
            // Check if the track has ended and no fade is in progress
            if (!currentSource.isPlaying && fadeCoroutine == null && 
                continuousPlaybackCoroutine == null)
            {
                continuousPlaybackCoroutine = StartCoroutine(PlayNextTrackAfterDelay());
            }
        }
        else if (continuousPlayback && playingTitleScreen)
        {
            // Check if title screen music has ended
            AudioSource currentSource = audioSources[activeSource];
            
            if (!currentSource.isPlaying && fadeCoroutine == null && 
                continuousPlaybackCoroutine == null)
            {
                // Play title screen music again (loop it)
                continuousPlaybackCoroutine = StartCoroutine(ReplayTitleScreenAfterDelay());
            }
        }
    }
    
    // Play a track by index with crossfade
    public void PlayTrack(int trackIndex)
    {
        // Validate track index
        if (trackIndex < 0 || trackIndex >= musicTracks.Length)
        {
            Debug.LogWarning("MusicManager: Invalid track index: " + trackIndex);
            return;
        }
        
        // Don't restart if already playing this track
        if (trackIndex == currentTrackIndex && audioSources[activeSource].isPlaying && !playingTitleScreen)
        {
            return;
        }
        
        // Cancel any pending track changes
        if (continuousPlaybackCoroutine != null)
        {
            StopCoroutine(continuousPlaybackCoroutine);
            continuousPlaybackCoroutine = null;
        }
        
        // Update current track and history
        UpdatePlaybackHistory(trackIndex);
        currentTrackIndex = trackIndex;
        playingTitleScreen = false;
        
        // Stop any current fade
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // Start crossfade
        fadeCoroutine = StartCoroutine(CrossFade(musicTracks[trackIndex]));
    }
    
    // Play a track by name with crossfade
    public void PlayTrack(string trackName)
    {
        // Find track by name
        for (int i = 0; i < musicTracks.Length; i++)
        {
            if (musicTracks[i] != null && musicTracks[i].name == trackName)
            {
                PlayTrack(i);
                return;
            }
        }
        
        Debug.LogWarning("MusicManager: Track not found: " + trackName);
    }

    // Play the title screen music
    public void PlayTitleScreen()
    {
        // Check if title screen track is assigned
        if (titleScreenTrack == null)
        {
            Debug.LogWarning("MusicManager: Title screen track is not assigned!");
            return;
        }
        
        // Don't restart if already playing the title screen music
        if (playingTitleScreen && audioSources[activeSource].isPlaying && 
            audioSources[activeSource].clip == titleScreenTrack)
        {
            return;
        }
        
        // Cancel any pending track changes
        if (continuousPlaybackCoroutine != null)
        {
            StopCoroutine(continuousPlaybackCoroutine);
            continuousPlaybackCoroutine = null;
        }
        
        // Reset track index since we're not playing from the regular tracks array
        currentTrackIndex = -1;
        playingTitleScreen = true;
        
        // Stop any current fade
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // Start crossfade to title screen track
        fadeCoroutine = StartCoroutine(CrossFade(titleScreenTrack));
    }
    
    // Replay title screen after delay (for looping)
    private IEnumerator ReplayTitleScreenAfterDelay()
    {
        yield return new WaitForSeconds(delayBetweenTracks);
        
        if (!playingTitleScreen) // Double-check we still want title screen music
        {
            continuousPlaybackCoroutine = null;
            yield break;
        }
        
        // Set up which source is which
        int newSource = 1 - activeSource;
        
        // Set the new source's track and start playing at zero volume
        audioSources[newSource].clip = titleScreenTrack;
        audioSources[newSource].volume = defaultVolume;
        audioSources[newSource].Play();
        
        // Swap active source
        activeSource = newSource;
        
        continuousPlaybackCoroutine = null;
    }
    
    // Play a random track
    public void PlayRandomTrack()
    {
        if (musicTracks.Length == 0) return;
        
        int randomIndex;
        
        if (avoidRepeatingLastTrack && musicTracks.Length > 1)
        {
            // Avoid playing tracks in recent history
            List<int> availableTracks = new List<int>();
            
            for (int i = 0; i < musicTracks.Length; i++)
            {
                if (!playbackHistory.Contains(i))
                {
                    availableTracks.Add(i);
                }
            }
            
            // If all tracks are in history, just avoid the current one
            if (availableTracks.Count == 0)
            {
                randomIndex = Random.Range(0, musicTracks.Length);
                while (randomIndex == currentTrackIndex && musicTracks.Length > 1)
                {
                    randomIndex = Random.Range(0, musicTracks.Length);
                }
            }
            else
            {
                randomIndex = availableTracks[Random.Range(0, availableTracks.Count)];
            }
        }
        else
        {
            // Completely random selection
            randomIndex = Random.Range(0, musicTracks.Length);
        }
        
        PlayTrack(randomIndex);
    }
    
    // Enable/disable continuous playback
    public void SetContinuousPlayback(bool enabled)
    {
        continuousPlayback = enabled;
    }
    
    // Enable/disable random playback
    public void SetRandomPlayback(bool enabled)
    {
        randomizePlayback = enabled;
    }
    
    // Update the playback history
    private void UpdatePlaybackHistory(int trackIndex)
    {
        playbackHistory.Add(trackIndex);
        
        // Keep history limited to recent tracks
        if (playbackHistory.Count > historyMaxSize)
        {
            playbackHistory.RemoveAt(0);
        }
    }
    
    // Play the next track after a delay
    private IEnumerator PlayNextTrackAfterDelay()
    {
        yield return new WaitForSeconds(delayBetweenTracks);
        
        if (randomizePlayback)
        {
            PlayRandomTrack();
        }
        else
        {
            // Play next track (loop back to beginning if at the end)
            int nextTrack = (currentTrackIndex + 1) % musicTracks.Length;
            PlayTrack(nextTrack);
        }
        
        continuousPlaybackCoroutine = null;
    }
    
    // Crossfade between tracks
    private IEnumerator CrossFade(AudioClip newClip)
    {
        // Set up which source is which
        int newSource = 1 - activeSource;
        
        // Set the new source's track and start playing at zero volume
        audioSources[newSource].clip = newClip;
        audioSources[newSource].volume = 0f;
        audioSources[newSource].Play();
        
        float timer = 0f;
        
        // Fade out the old source, fade in the new source
        while (timer < crossFadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / crossFadeDuration;
            
            audioSources[newSource].volume = Mathf.Lerp(0f, defaultVolume, t);
            
            if (audioSources[activeSource].isPlaying)
            {
                audioSources[activeSource].volume = Mathf.Lerp(defaultVolume, 0f, t);
            }
            
            yield return null;
        }
        
        // Stop the old source
        audioSources[activeSource].Stop();
        
        // Ensure final volumes are correct
        audioSources[newSource].volume = defaultVolume;
        audioSources[activeSource].volume = 0f;
        
        // Swap active source
        activeSource = newSource;
        
        // Clear the fade coroutine reference
        fadeCoroutine = null;
    }
    
    // Overload for backward compatibility
    private IEnumerator CrossFade(int newTrackIndex)
    {
        return CrossFade(musicTracks[newTrackIndex]);
    }
    
    // Stop all music with fade out
    public void StopMusic()
    {
        // Cancel any pending track changes
        if (continuousPlaybackCoroutine != null)
        {
            StopCoroutine(continuousPlaybackCoroutine);
            continuousPlaybackCoroutine = null;
        }
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeOut());
    }
    
    // Fade out routine
    private IEnumerator FadeOut()
    {
        float timer = 0f;
        float startVolume = audioSources[activeSource].volume;
        
        while (timer < crossFadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / crossFadeDuration;
            
            audioSources[activeSource].volume = Mathf.Lerp(startVolume, 0f, t);
            
            yield return null;
        }
        
        // Stop playing and reset
        audioSources[activeSource].Stop();
        currentTrackIndex = -1;
        playingTitleScreen = false;
        fadeCoroutine = null;
    }
    
    // Set volume level for music (0-1)
    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        defaultVolume = volume;
        
        // Adjust current playing volume if active
        if (audioSources[activeSource].isPlaying)
        {
            audioSources[activeSource].volume = volume;
        }
    }
    
    // Return the currently playing track index
    public int GetCurrentTrackIndex()
    {
        return currentTrackIndex;
    }
    
    // Return the currently active audio source index
    public int GetActiveSourceIndex()
    {
        return activeSource;
    }
    
    // Is playing title screen music?
    public bool IsPlayingTitleScreen()
    {
        return playingTitleScreen && audioSources[activeSource].isPlaying;
    }
    
    // Is music currently playing?
    public bool IsPlaying()
    {
        return (currentTrackIndex >= 0 || playingTitleScreen) && audioSources[activeSource].isPlaying;
    }
    
    // Clear the playback history
    public void ClearPlaybackHistory()
    {
        playbackHistory.Clear();
    }
}