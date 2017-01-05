﻿using UnityEngine;
using System.Collections;
using RSG;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    public static IPromiseTimer promiseTimer;
    public static IPromiseTimer stoppablePromiseTimer;

    Substitution timeSubstitution;

    public float readonlyTimeScale;

    public bool pauseOnStart;
    static bool paused;
    public static bool Paused {
        get {
            return paused;
        }
        set {
            if (value) {
                Pause();
            } else {
                Unpause();
            }
        }
    }
    public static bool timestopped = false;

    public float gameTime;

    [SerializeField]
    public float stoppableGameTime;

    public static float GameTime {
        get {
            if (!Extensions.Editor()) {
                return instance.gameTime;
            } else {
                return FindObjectOfType<TimeManager>().gameTime;
            }
        }
    }

    public static float RealTime {
        get {
            return Time.realtimeSinceStartup;
        }
    }

    public static float StoppableGameTime {
        get {
            if (!Extensions.Editor()) {
                return instance.stoppableGameTime;
            } else {
                return FindObjectOfType<TimeManager>().gameTime;
            }
        }
    }

    public static float loosedFixedDeltaTime;
    public const float defaultFixedDeltaTime = 0.02f;

    static void UpdateTimeScale() {
        float timeScale = 1;
        if (Player.instance != null) {
            if (Player.instance.Rewind()) {
                timeScale *= Player.instance.current.rewind.timeMultiplyer;
            }
            if (Player.instance.current.slowmo != null && Player.instance.current.slowmo.on) {
                timeScale *= Player.instance.current.slowmo.timeMultiplyer;
            }
            if (Player.instance.current.transform.position.y < -1000f && !Player.instance.Undo()) {
                timeScale = 0;
            }
        }
        if (paused) {
            timeScale = 0;
        }
        Time.timeScale = timeScale;
        instance.readonlyTimeScale = timeScale;
        loosedFixedDeltaTime = defaultFixedDeltaTime * Mathf.Clamp(Time.timeScale, 1, float.PositiveInfinity);
        Time.fixedDeltaTime = defaultFixedDeltaTime * Mathf.Clamp(Time.timeScale, 0.01f, 1);
    }

    static void Pause()
    {
        paused = true;
        Time.timeScale = 0;
    }

    static void Unpause()
    {
        paused = false;
        Time.timeScale = 1;
    }

    public static float StoppableFixedDeltaTime {
        get {
            if (timestopped) {
                return 0;
            } else {
                return Time.fixedDeltaTime;
            }
        }
    }

    public static float FixedDeltaTime {
        get {
            return Time.fixedDeltaTime;
        }
    }

    static void SwitchPause()
    {
        if (Paused)
        {
            Unpause();
        }
        else
        {
            Pause();
        }
    }

    void FixedUpdate() {
        UpdateTimeScale();
        if (TimeManager.Paused) {
            return;
        }
        promiseTimer.Update(-100500);
        stoppablePromiseTimer.Update(-100500);

        if (Undoing()) {
            gameTime -= Time.fixedDeltaTime;
            if (gameTime < 0) {
                gameTime = 0;
            }
            onUndo();
        } else {
            if (Player.instance.current.transform.position.y < -1000) {
                Time.timeScale = 0;
            }
            Track();
            gameTime += Time.fixedDeltaTime;
            stoppableGameTime += StoppableFixedDeltaTime;
        }

        totalSampleCount = 0;
        onPushSampleCount();
    }

    void Awake() {
        instance = this;
        Paused = pauseOnStart;
        timestopped = false;
    }

    void Start() {
        promiseTimer = new UndoablePromiseTimer(() => gameTime);
        stoppablePromiseTimer = new UndoablePromiseTimer(() => stoppableGameTime);
        gameTime = 0;
        timeSubstitution = DynamicTextManager.instance.Substitute("#{gameTime}", () => {
            var span = TimeSpan.FromSeconds(stoppableGameTime);
            var result = string.Format("{0}:{1:00}.{2:00}", (int)span.TotalMinutes, span.Seconds, span.Milliseconds/10);
            if (Time.timeScale != 1) {
                result += string.Format(" (x{0:f1})", Time.timeScale);
            }
            return result;
        });
        new BoolTracker(v => timestopped = v, () => timestopped);
        new ValueTracker<float>(v => stoppableGameTime = gameTime + v, () => stoppableGameTime - gameTime);
    }

    void Update() {
        UpdateTimeScale();
        timeSubstitution.Recalculate();
    }

    public static IPromise WaitFor(float time) {
        return promiseTimer.WaitFor(time);
    }

    public static IPromise WaitForStoppable(float time) {
        return stoppablePromiseTimer.WaitFor(time);
    }

    public int totalSampleCount;

    public event Action onUndo = () => { };
    public event Action onDrop = () => { };
    public event Action onTrack = () => { };
    public event Action onPushSampleCount = () => { };

    public bool Undoing() {
        return Player.instance.current.undo != null && Player.instance.current.undo.Undoing();
    }

    public void Track() {
        onTrack();
    }

    /// <summary>
    /// Make it "It's always been like this" for current state of game
    /// </summary>
    public void DropUndoData() {
        onDrop();
        Debug.Log("Drop Undo Data");
    }
}