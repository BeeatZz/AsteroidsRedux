namespace Asteroids.Audio
{
    // One entry per volume slider. Each maps to an exposed AudioMixer parameter named "<Channel>Volume".
    public enum AudioChannel
    {
        Master,
        Music,
        SFX,
        UI
    }
}
