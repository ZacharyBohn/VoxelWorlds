using Godot;
using System;

public partial class PlayerCamera : Camera3D
{
    public float MouseSensitivity { get; set; }
    public float MinPitchAngle = -85f;
    public float MaxPitchAngle = 85f;
    public bool InvertY = false;

    // Constructor to set the mouse sensitivity
    public PlayerCamera(float mouseSensitivity)
    {
        MouseSensitivity = mouseSensitivity;
    }

    // Override the _Ready method to perform any setup if necessary
    public override void _Ready()
    {
        // Call the base class's _Ready if needed.
        // base._Ready();
    }

    // Override the _Input method to handle input events
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            float yDelta = -eventMouseMotion.Relative.y;

            if (InvertY)
            {
                yDelta *= -1;
            }

            float newPitch = RotationDegrees.x + (yDelta * MouseSensitivity);
            RotationDegrees = new Vector3(ClampDegrees(newPitch, MinPitchAngle, MaxPitchAngle), RotationDegrees.y, RotationDegrees.z);
        }
    }

    private float ClampDegrees(float degree, float min, float max)
    {
        return Mathf.Clamp(degree, min, max);
    }
}

// Extension methods to ease the conversion between radians and degrees
public static class MathExtensions
{
    public static float DegToRad(this float degree)
    {
        return Mathf.Deg2Rad(degree);
    }

    public static float RadToDeg(this float radian)
    {
        return Mathf.Rad2Deg(radian);
    }
}