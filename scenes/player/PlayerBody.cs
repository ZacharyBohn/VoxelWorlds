using Godot;
using System;

public partial class PlayerBody : CharacterBody3D
{
    // You may wish to load your helper class differently in C#
    // It's assumed here that PlayerBodyHelpers is a static class
    // const Helpers = preload("res://scripts/player_body_helpers.gd")

    private float strafeSpeed = 5.0f;
    // TODO: re-implement
    private bool flight = false;

    // TODO: should not be hard coded
    private Vector3 planetCenter = new(0, 0, -2);
    private float gravityStrength = 30.0f;
    private float jumpSpeed = 10.0f;
    // this is to allow easy access to this node
    private Camera3D camera;
    private float mouseSensitivity = 0.2f;

    // up_direction will change per tick based on relation
    // to the planet. however, the characterbody should not
    // rotate to match up_direction until the characterbody
    // is touching the floor. then the characterbody should
    // be rotated until up_direction is actually up for the
    // characterbody. this variable keep track of which direction
    // the characterbody has locked as "up".
    // the way that these can be misaligned, is if the characterbody
    // is basically laying perpendicular to the ground. gravity
    // will pull toward the planet and up_direction will be away
    // from the planet, but locked_up_direction will always be
    // "up" according to which direction the playerbody head is
    // pointing.
    private Vector3 lockedUpDirection = Vector3.Up;

    // gravity vars
    private float t = 0.0f;
    private float duration = 1.0f; // Transition duration in seconds
    private Vector3 oldGravity;
    private Vector3 newGravity;
    private bool transitioning = false;
    private Basis targetGravityRotation;

    public override void _Ready()
    {
        Transform = new Transform(Transform.basis, new Vector3(0, 20, 0));
        var collisionShape = new CollisionShape3D();
        collisionShape.Shape = new CapsuleShape3D();
        AddChild(collisionShape);
        camera = new PlayerCamera(mouseSensitivity); // Make sure this matches your actual Camera class
        AddChild(camera);

        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Pressed && keyEvent.Scancode == (int)KeyList.Escape)
            {
                GetTree().Quit();
            }
        }
        if (@event is InputEventMouseMotion mouseEvent)
        {
            RotateObjectLocal(Vector3.Up, Mathf.Deg2Rad(-mouseEvent.Relative.x * mouseSensitivity));
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        var rightDir = Transform.basis.x.Normalized();
        var forwardDir = -Transform.basis.z.Normalized();

        var direction = Vector3.Zero;
        if (!transitioning)
        {
            if (Input.IsActionPressed("move_forward"))
                direction += forwardDir;
            if (Input.IsActionPressed("move_backward"))
                direction -= forwardDir;
            if (Input.IsActionPressed("move_left"))
                direction -= rightDir;
            if (Input.IsActionPressed("move_right"))
                direction += rightDir;
            if (Input.IsActionPressed("move_up"))
                direction += lockedUpDirection;
            if (Input.IsActionPressed("move_down"))
                direction -= lockedUpDirection;
            if (Input.IsActionJustPressed("special"))
                flight = !flight;
            if (IsOnFloor() && Input.IsActionJustPressed("jump"))
                Velocity = PlayerBodyHelpers.Jump(Velocity, lockedUpDirection, jumpSpeed);
            Velocity = PlayerBodyHelpers.Strafe(direction, Velocity, strafeSpeed, lockedUpDirection);
            Velocity = PlayerBodyHelpers.ApplyGravity(lockedUpDirection, Velocity, gravityStrength, delta);
        }
        else
        {
            Velocity = Vector3.Zero;
        }

        var newGravityDirection = PlayerBodyHelpers.GetGravityDirection(
            -lockedUpDirection,
            GlobalTransform.origin,
            planetCenter
        );
        lockedUpDirection = (-newGravityDirection).Normalized();
        if (lockedUpDirection != this.lockedUpDirection && IsOnFloor())
        {
            StartGravityRealign(-this.lockedUpDirection, -lockedUpDirection);
            this.lockedUpDirection = lockedUpDirection;
        }

        if (transitioning)
        {
            t += delta / duration;
            Transform = new Transform(PlayerBodyHelpers.InterpolateGravityRealign(Transform.basis, targetGravityRotation, t),
                                      Transform.origin);
            if (t > 1.0f)
            {
                t = 1.0f;
                transitioning = false;
            }
        }

        // More flight and gravity logic may go here

        MoveAndSlide();
    }

    private void StartGravityRealign(Vector3 old, Vector3 @new)
    {
        oldGravity = old;
        newGravity = @new;
        t = 0.0f;
        targetGravityRotation = PlayerBodyHelpers.GetRotationToNewGravity(old, @new, Transform.basis);
        transitioning = true;
    }
}