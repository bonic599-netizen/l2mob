using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private int _currentEventPriority = -1;
    [SerializeField] private EventReference _currentMusicEvent;
    private Dictionary<EventReference, EventInstance> _musicInstances;
    public int CurrentEventPriority { get { return _currentEventPriority; } }
    public EventReference CurrentMusicEvent { get { return _currentMusicEvent; } }

    private static MusicManager _instance;
    public static MusicManager Instance
    {
        get
        {
            if (_instance == null)
            {
#if UNITY_2023_1_OR_NEWER
                _instance = Object.FindFirstObjectByType<MusicManager>();
#else
                _instance = Object.FindObjectOfType<MusicManager>();
#endif
                if (_instance == null)
                {
                    GameObject go = new GameObject("MusicManager");
                    _instance = go.AddComponent<MusicManager>();
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return _instance;
        }
    }

    private void Awake() {
        if (_instance == null) {
            _instance = this;
            if (transform.parent == null && Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        } else if(_instance != this){
            Destroy(gameObject);
            return;
        }

        if (_musicInstances == null)
        {
            _musicInstances = new Dictionary<EventReference, EventInstance>();
        }
    }

    void OnDestroy() {
        Clear();
    }

    public void Clear() {
        ResetPriority();
        StopMusic(CurrentMusicEvent);
        if (_musicInstances != null)
        {
            _musicInstances.Clear();
        }
    }

    public void PlayMusic(EventReference musicEvent, int priority) {
        if(priority < _currentEventPriority) {
            return;
        }

        if (musicEvent.IsNull) {
            return;
        }

        _currentEventPriority = priority;
        _currentMusicEvent = musicEvent;

        if (_musicInstances == null)
        {
            _musicInstances = new Dictionary<EventReference, EventInstance>();
        }

        if (_musicInstances.ContainsKey(musicEvent)) {
            _musicInstances[musicEvent].start();
        } else {
            EventInstance musicInstance = RuntimeManager.CreateInstance(musicEvent);
            _musicInstances.Add(musicEvent, musicInstance);
            musicInstance.start();
        }

        Debug.Log("Playing music " + musicEvent + " with priority " + priority);
    }

    public void StopMusic(EventReference musicEvent) {
        if(_musicInstances != null && _musicInstances.ContainsKey(musicEvent)) {
            _musicInstances[musicEvent].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }             
    }

    public void ResetPriority() {
        _currentEventPriority = -1;
    }
}
