using System;

public abstract class Timer
{
    protected float initialTime;
    public float time {  get; set; }
    public bool isRunning { get; protected set; }

    public float progress => time / initialTime;

    public Action OnTimerStart = delegate { };
    public Action OnTimerStop = delegate { };

    protected Timer(float value)
    {
        initialTime = value;
        isRunning = false;
    }

    public void Start()
    {
        time = initialTime;
        if (!isRunning)
        {
            isRunning = true;
            OnTimerStart?.Invoke();
        }
    }

    public void Stop()
    {
        if (isRunning)
        {
            isRunning = false;
            OnTimerStop?.Invoke();
        }
    }

    public void Resume() => isRunning = true;
    public void Pause() => isRunning = false;

    public abstract void Tick(float deltaTime);
}

public class CountdownTimer : Timer
{
    public CountdownTimer(float value) : base(value) { }

    public override void Tick(float deltaTime)
    {
        if (isRunning && time > 0)
        {
            time -= deltaTime;
        }

        if (isRunning && time <= 0)
        {
            Stop();
        }
    }

    public bool isFinished => time <= 0;

    public void Reset() => time = initialTime;

    public void Reset(float newTime)
    {
        initialTime = newTime;
        Reset();
    }
}

public class StopwatchTimer : Timer
{
    public StopwatchTimer() : base(0) { }

    public override void Tick(float deltaTime)
    {
        if (isRunning)
        {
            time += deltaTime;
        }
    }

    public void Reset() => time = 0;

    public float GetTime() => time;
}