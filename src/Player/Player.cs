using Godot;
using System;

public class Player : KinematicBody2D
{
    private static readonly Vector2 Gravity = new Vector2(0, 1200);
    private static readonly Vector2 FloorNormal = new Vector2(0, -1);
    private static readonly Vector2 FaceLeftScale = new Vector2(-1, 1);
    private static readonly Vector2 FaceRightScale = new Vector2(1, 1);
    // private const float SlopeSlideStop = 25f;
    private const float SidingChangeSpeed = 10;

    [Export] public float WalkSpeed = 400;

    // Jumping parameters
    [Export] public float InitialJumpImpulse = 400;
    [Export] public float AdditionalJumpImpulse = 20;
    [Export] public int MaxAdditionalJumpImpulses = 20;

    private Vector2 linearVelocity = new Vector2();
    private string animation = "";
    private bool isJumping = false;
    private int additionalJumpImpulsesCount = 0;

    private AnimatedSprite sprite;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite>("Sprite");
    }

    public override void _PhysicsProcess(float delta)
    {
        // Apply gravity
        float gravityMultiplier = linearVelocity.y > 0 ? 1.5f : 1f;
        linearVelocity += Gravity * gravityMultiplier * delta;

        // Move the player
        linearVelocity = MoveAndSlide(linearVelocity, FloorNormal);

        bool onFloor = IsOnFloor();

        isJumping = onFloor ? true : isJumping;

        // Calculate next horizontal movement
        float targetSpeed = 0;
        if (Input.IsActionPressed("move_left")) { targetSpeed = -1; }
        if (Input.IsActionPressed("move_right")) { targetSpeed = 1; }

        targetSpeed *= WalkSpeed;
        linearVelocity.x = Mathf.Lerp(linearVelocity.x, targetSpeed, 0.5f);

        // When already jumped, apply additional impulse if action is still pressed.
        if (isJumping && Input.IsActionPressed("jump")
            && additionalJumpImpulsesCount < MaxAdditionalJumpImpulses)
        {
            linearVelocity.y -= AdditionalJumpImpulse;
            additionalJumpImpulsesCount++;
        }

        // Start jumping
        if (onFloor && Input.IsActionJustPressed("jump"))
        {
            linearVelocity.y = -InitialJumpImpulse;
            isJumping = true;
            additionalJumpImpulsesCount = 0;
        }

        // Animation
        var newAnimation = "stand";

        if (onFloor)
        {
            if (linearVelocity.x < -SidingChangeSpeed)
            {
                sprite.Scale = FaceLeftScale;
                newAnimation = "walk";
            }
            else if (linearVelocity.x > SidingChangeSpeed)
            {
                sprite.Scale = FaceRightScale;
                newAnimation = "walk";
            }
        }
        else
        {
            if (Input.IsActionPressed("move_left") && !Input.IsActionPressed("move_right"))
            {
                sprite.Scale = FaceLeftScale;
            }
            else if (Input.IsActionPressed("move_right") && !Input.IsActionPressed("move_left"))
            {
                sprite.Scale = FaceRightScale;
            }

            newAnimation = "jump";
        }

        if (newAnimation != animation)
        {
            animation = newAnimation;
            sprite.Play(animation);
        }
    }
}
