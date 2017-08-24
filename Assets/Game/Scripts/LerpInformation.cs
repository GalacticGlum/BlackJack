using System;

public class LerpInformation<T>
{
    public T Source { get; }
    public T Destination { get; }
    public float Duration { get; }
    public float TimeLeft { get; set; }
    public float LerpFactor => 1 - TimeLeft / Duration;

    private readonly Func<T, T, float, T> lerpHandler;

    public LerpInformation(T source, T destination, float duration, Func<T, T, float, T> lerpHandler)
    {
        Source = source;
        Destination = destination;
        Duration = duration;
        TimeLeft = duration;

        this.lerpHandler = lerpHandler;
    }

    public T Step(float deltaTime)
    {
        T result = lerpHandler(Source, Destination, LerpFactor);
        TimeLeft -= deltaTime;

        return result;
    }
}