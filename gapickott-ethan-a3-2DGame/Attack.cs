using System;
using System.Numerics;

namespace Game10003
{
    // Attack values
    public enum AttackType
    {
        Projectile,
        Laser,
        Triangle
    }

    public class Attack
    {
        public Vector2 position;
        public Vector2 size;
        public Color color;
        public int damage;
        public AttackType attackType;
        public Vector2 velocity;

        // Constructor to initialize attack
        public Attack(Vector2 position, Vector2 size, Color color, int damage, AttackType attackType)
        {
            this.position = position;
            this.size = size;
            this.color = color;
            this.damage = damage;
            this.attackType = attackType;

            // Sets the attack velocity

            // Phase1 velocity
            if (attackType == AttackType.Projectile)
            {
                velocity = new Vector2(0, 5);
            }

            // Phase2 velocity
            else if (attackType == AttackType.Laser)
            {
                velocity = new Vector2(5, 0);  // Horizontal movement for lasers
            }

            // Phase 3 velocity
            else if (attackType == AttackType.Triangle)
            {
                // Initial velocity for bouncing triangles
                velocity = new Vector2(Random.Float(-5, 5), Random.Float(-5, 5));  // Random directional bounce
            }
        }

        // Update the triangle positioning
        public void Move()
        {
            position += velocity;

            // Bounce logic for triangles
            if (attackType == AttackType.Triangle)
            {
                // Bounce off left and right edges
                if (position.X <= 0 || position.X + size.X >= Game.WindowWidth)
                {
                    velocity.X = -velocity.X;
                }

                // Bounce off top and bottom edges
                if (position.Y <= 0 || position.Y + size.Y >= Game.WindowHeight)
                {
                    velocity.Y = -velocity.Y;
                }
            }
        }

        // Method to check if the attack hits the player
        public bool HitsPlayer(Player player)
        {
            return position.X < player.position.X + player.size.X &&
                   position.X + size.X > player.position.X &&
                   position.Y < player.position.Y + player.size.Y &&
                   position.Y + size.Y > player.position.Y;
        }
    }
}
