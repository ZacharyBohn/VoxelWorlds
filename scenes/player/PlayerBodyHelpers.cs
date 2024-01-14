using Godot;
using System;

public class PlayerBodyHelpers
{
    public enum Axis { X, Y, Z }

    public enum Face { Forward, Back, Left, Right, Up, Down }

    public static Vector3 ApplyGravity(
        Vector3 upDirection,
        Vector3 velocity,
        float gravityStrength,
        float delta)
    {
        var newVelocity = velocity;
        if (upDirection == Vector3.Up)
            newVelocity.y -= gravityStrength * delta;
        if (upDirection == Vector3.Down)
            newVelocity.y += gravityStrength * delta;
        if (upDirection == Vector3.Forward)
            newVelocity.z += gravityStrength * delta;
        if (upDirection == Vector3.Back)
            newVelocity.z -= gravityStrength * delta;
        if (upDirection == Vector3.Left)
            newVelocity.x += gravityStrength * delta;
        if (upDirection == Vector3.Right)
            newVelocity.x -= gravityStrength * delta;
        return newVelocity;
    }

    public static Vector3 Strafe(
        Vector3 moveDirection,
        Vector3 velocity,
        float moveSpeed,
        Vector3 upDirection)
    {
        var newVelocity = velocity;
        if (upDirection == Vector3.Up || upDirection == Vector3.Down)
        {
            newVelocity.x = moveDirection.x * moveSpeed;
            newVelocity.z = moveDirection.z * moveSpeed;
        }
        if (upDirection == Vector3.Forward || upDirection == Vector3.Back)
        {
            newVelocity.x = moveDirection.x * moveSpeed;
            newVelocity.y = moveDirection.y * moveSpeed;
        }
        if (upDirection == Vector3.Left || upDirection == Vector3.Right)
        {
            newVelocity.y = moveDirection.y * moveSpeed;
            newVelocity.z = moveDirection.z * moveSpeed;
        }
        return newVelocity;
    }

    public static Vector3 Jump(
        Vector3 velocity,
        Vector3 upDirection,
        float jumpSpeed)
    {
        var down = Vector3.Down;
        var back = Vector3.Back;
        var up = Vector3.Up;
        var forward = Vector3.Forward;
        var left = Vector3.Left;
        var right = Vector3.Right;

        var targetVelocity = velocity;

        if (upDirection == up)
            targetVelocity.y = jumpSpeed;
        if (upDirection == down)
            targetVelocity.y = -jumpSpeed;
        if (upDirection == left)
            targetVelocity.x = jumpSpeed;
        if (upDirection == right)
            targetVelocity.x = -jumpSpeed;
        if (upDirection == forward)
            targetVelocity.z = -jumpSpeed;
        if (upDirection == back)
            targetVelocity.z = jumpSpeed;

        return targetVelocity;
    }

    public static Vector3 FaceToVector(Face face)
    {
        switch (face)
        {
            case Face.Forward: return Vector3.Forward;
            case Face.Back: return Vector3.Back;
            case Face.Left: return Vector3.Left;
            case Face.Right: return Vector3.Right;
            case Face.Up: return Vector3.Up;
            case Face.Down: return Vector3.Down;
            default: throw new ArgumentOutOfRangeException(nameof(face), face, null);
        }
    }

    public static Axis GetStrongestAxis(Vector3 vector)
    {
        if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y) && Mathf.Abs(vector.x) > Mathf.Abs(vector.z))
            return Axis.X;
        else if (Mathf.Abs(vector.y) > Mathf.Abs(vector.x) && Mathf.Abs(vector.y) > Mathf.Abs(vector.z))
            return Axis.Y;
        else
            return Axis.Z;
    }

    // Note: The function for rotation might need tweaking, as it relies on Basis which has changed in Godot 4.
    // Ensure you've updated the syntax and functionality of rotated, deg_to_rad, slerp according to Godot 4.
    public static Basis GetRotationToNewGravity(Vector3 oldUp, Vector3 newUp, Basis basis)
    {
        var targetRotation = new Basis(basis);

        float degToRad(float deg) => (Mathf.Pi / 180) * deg;

        // Back
        if (oldUp == Vector3.Down && newUp == Vector3.Back ||
            oldUp == Vector3.Back && newUp == Vector3.Up ||
            oldUp == Vector3.Up && newUp == Vector3.Forward ||
            oldUp == Vector3.Forward && newUp == Vector3.Down)
            targetRotation = targetRotation.Rotated(Vector3.Right, degToRad(-90));

        // Forward
        // ... (continue adding similar conditions for each case)

        return targetRotation;
    }

    public static Basis InterpolateGravityRealign(
        Basis basis,
        Basis targetRotation,
        float rotationDelta)
    {
        var interpolatedRotation = basis.Slerp(targetRotation, rotationDelta);
        return new Basis(interpolatedRotation);
    }

    public static Vector3 GetGravityDirection(
        Vector3 currentFace,
        Vector3 playerPosition,
        Vector3 planetCenter)
    {
        var directionToCenter = (planetCenter - playerPosition).Normalized();
        var absDirection = directionToCenter.Abs();

        // TODO: Adjust padding logic if needed
        float padding = 0.0f;

        if (currentFace == Vector3.Down || currentFace == Vector3.Up)
            absDirection.y += padding;
        if (currentFace == Vector3.Left || currentFace == Vector3.Right)
            absDirection.x += padding;
        if (currentFace == Vector3.Forward || currentFace == Vector3.Back)
            absDirection.z += padding;

        var strongestAxis = GetStrongestAxis(absDirection);

        Vector3 gravityDirection = Vector3.Zero;

        switch (strongestAxis)
        {
            case Axis.X:
                gravityDirection = directionToCenter.x > 0 ? Vector3.Right : Vector3.Left;
                break;
            case Axis.Y:
                gravityDirection = directionToCenter.y > 0 ? Vector3.Up : Vector3.Down;
                break;
            case Axis.Z:
                gravityDirection = directionToCenter.z > 0 ? Vector3.Back : Vector3.Forward;
                break;
        }

        return gravityDirection;
    }
}