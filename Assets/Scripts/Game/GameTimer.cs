using System;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public event Action<int> ElapsedSecondsChanged;

    public float ElapsedTime { get; private set; }
    public int ElapsedSeconds { get; private set; }

    private void Update()
    {
        ElapsedTime += Time.deltaTime;

        int seconds = Mathf.FloorToInt(ElapsedTime);
        if (seconds == ElapsedSeconds) return;

        ElapsedSeconds = seconds;
        ElapsedSecondsChanged?.Invoke(seconds);
    }
}
