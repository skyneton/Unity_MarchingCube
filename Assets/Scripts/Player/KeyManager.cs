using UnityEngine;

public class KeyManager
{
    public static KeyCode Jump { get; protected set; } = KeyCode.Space;
    public static KeyCode Forward { get; protected set; } = KeyCode.W;
    public static KeyCode Backward { get; protected set; } = KeyCode.S;
    public static KeyCode Left { get; protected set; } = KeyCode.A;
    public static KeyCode Right { get; protected set; } = KeyCode.D;
    public static KeyCode Run { get; protected set; } = KeyCode.R;
}