namespace Asteroids.Audio
{
    // One entry per volume slider. Each maps to an exposed AudioMixer parameter named "<Channel>Vol".
    // UI sounds have no slider of their own: the mixer's UI group sits under SFX, so the SFX slider covers them.
    public enum AudioChannel
    {
        Master,
        Music,
        SFX
    }
}
