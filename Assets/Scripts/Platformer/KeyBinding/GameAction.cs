namespace Platformer.KeyBinding
{
    /// <summary>
    /// Every gameplay action that can be allocated to a physical key. This is the single
    /// extension point for new remappable actions: add an entry here and it automatically
    /// gets a slot in KeyBindingManager and shows up in the key-remap UI once given a
    /// default key.
    ///
    /// Menu navigation itself (arrows/Enter/Escape used inside the key-remap screen) is
    /// intentionally NOT part of this enum - it stays on fixed physical keys so remapping
    /// can never lock the player out of the remap screen.
    /// </summary>
    public enum GameAction
    {
        MoveLeft,
        MoveRight,
        Jump,
        Interact,
    }
}
