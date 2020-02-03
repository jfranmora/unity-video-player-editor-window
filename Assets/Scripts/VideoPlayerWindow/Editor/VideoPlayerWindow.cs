using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerWindow : EditorWindow
{
    private GameObject _tempGO;
    private VideoPlayer _videoPlayer;
    private RenderTexture _renderTexture;

    private string _url;
    private bool _wasPlaying;
    private double _currentTime;

    [MenuItem("Window/Video Player")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<VideoPlayerWindow>().Show();
    }

    private void OnEnable()
    {
        _renderTexture = new RenderTexture(1920 / 4, 1080 / 4, 0);

        _tempGO = new GameObject();
        _tempGO.hideFlags = HideFlags.HideAndDontSave;

        _videoPlayer = _tempGO.AddComponent<VideoPlayer>();
        _videoPlayer.targetTexture = _renderTexture;

        RegisterUnityEvents();
    }

    private void RegisterUnityEvents()
    {
        EditorApplication.update += OnUpdate;
        AssemblyReloadEvents.beforeAssemblyReload += BeforeAssemblyReload;
        AssemblyReloadEvents.afterAssemblyReload += AfterAssemblyReload;
    }

    private void UnregisterUnityEvents()
    {
        EditorApplication.update -= OnUpdate;
        AssemblyReloadEvents.beforeAssemblyReload -= BeforeAssemblyReload;
        AssemblyReloadEvents.afterAssemblyReload -= AfterAssemblyReload;
    }

    private void OnDisable()
    {
        if (_renderTexture != null)
        {
            DestroyImmediate(_renderTexture);
        }

        if (_tempGO != null)
        {
            DestroyImmediate(_tempGO);
        }

        UnregisterUnityEvents();
    }

	#region Unity Event Handling

	private void OnUpdate()
    {
        if (_videoPlayer.isPlaying)
        {
            Repaint();
        }
    }

    private void BeforeAssemblyReload()
    {
        _currentTime = _videoPlayer.time;
        _wasPlaying = _videoPlayer.isPlaying;        
    }

    private void AfterAssemblyReload()
    {
        _videoPlayer.time = _currentTime;

        if (_wasPlaying)
        {
            _videoPlayer.Play();
        }
    }

    #endregion

    private void OnGUI()
    {
        DrawURL();

        GUILayout.Box(_renderTexture);

        DrawButtons();
        DrawTimeInfo();
    }

    private void DrawURL()
    {
        EditorGUILayout.BeginHorizontal();

        _url = GUILayout.TextField(_url);

        if (GUILayout.Button("Search", GUILayout.Width(100)))
        {
            var path = EditorUtility.OpenFilePanelWithFilters("Open video", null, new[] { "Video files", "asf,avi,dv,m4v,mov,mp4,mpg,mpeg,ogv,vp8,webm,wmv" });
            if (!string.IsNullOrEmpty(path))
            {
                _url = path;
                Play();
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawButtons()
    {
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = !_videoPlayer.isPlaying;
        if (GUILayout.Button("Play"))
        {
            Play();
        }

        GUI.enabled = _videoPlayer.isPlaying;
        if (GUILayout.Button("Pause"))
        {
            Pause();
        }

        GUI.enabled = _videoPlayer.isPlaying || _videoPlayer.isPaused;
        if (GUILayout.Button("Stop"))
        {
            Stop();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    private void DrawTimeInfo()
    {
        var current = TimeSpan.FromSeconds(_videoPlayer.time);
        var total = TimeSpan.FromSeconds(_videoPlayer.length);

        EditorGUI.BeginChangeCheck();
        float time = GUILayout.HorizontalSlider((float)_videoPlayer.time, 0, (float)_videoPlayer.length);
        if (EditorGUI.EndChangeCheck())
        {
            _videoPlayer.time = time;
        }

        EditorGUILayout.Space();
        GUILayout.Label($"{current:g} / {total:g}");
    }

    private void Play()
    {
        _videoPlayer.url = _url;
        _videoPlayer.Play();
    }

    private void Stop()
    {
        _videoPlayer.Stop();
    }

    private void Pause()
    {
        _videoPlayer.Pause();
    }
}
