namespace Asteroids.Audio
{
    // One entry per volume slider. Each maps to an exposed AudioMixer parameter named "<Channel>Vol".
    public enum AudioChannel
    {
        Master,
        Music,
        SFX,
        UI
    }
}
