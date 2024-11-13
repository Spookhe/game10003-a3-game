using System;
using System.Numerics;
using Game10003;


public class Player
{
    public Vector2 position;
    public Vector2 size;
    public Color color;
    public float speed = 4f;  // Player speed

    public Player(Vector2 position, Vector2 size, Color color)
    {
        this.position = position;
        this.size = size;
        this.color = color;
    }

    // Checks if player is in the game window
    public bool IsInWindow()
    {
        return position.X >= 0 && position.X + size.X <= Game.WindowWidth &&
               position.Y >= 0 && position.Y + size.Y <= Game.WindowHeight;
    }

    public void MovePlayer()
    {
        // Moves the player with WASD keys
        if (Input.IsKeyboardKeyDown(KeyboardInput.W)) position.Y -= speed;
        if (Input.IsKeyboardKeyDown(KeyboardInput.S)) position.Y += speed;
        if (Input.IsKeyboardKeyDown(KeyboardInput.A)) position.X -= speed;
        if (Input.IsKeyboardKeyDown(KeyboardInput.D)) position.X += speed;

        // Ensures the player does not move off-screen
        if (position.X < 0) position.X = 0;
        if (position.X + size.X > Game.WindowWidth) position.X = Game.WindowWidth - size.X;
        if (position.Y < 0) position.Y = 0;
        if (position.Y + size.Y > Game.WindowHeight) position.Y = Game.WindowHeight - size.Y;
    }

    public void DrawPlayer()
    {
        // Draws the player (Square)
        Draw.FillColor = color;
        Draw.Rectangle(position, size);
    }
}
