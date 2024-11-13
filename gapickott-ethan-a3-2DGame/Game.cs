using Game10003;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

public class Game
{
    // Canvas size
    public static int WindowWidth = 600;
    public static int WindowHeight = 600;

    // Background colours for phases
    Color Phase1Background = new Color(0x4a, 0x11, 0x24);  // Magenta
    Color Phase2Background1 = new Color(0x95, 0x35, 0x53);  // Red
    Color Phase2Background2 = new Color(0x22, 0x41, 0x93);  // Aqua

    public Player player;
    public List<Attack> attacks = new List<Attack>();
    bool isGameOver = false;
    int hitCount = 0;
    float invincibilityTime = 0f;
    float invincibilityDuration = 1f;
    float phaseTimer = 0f;

    public bool isPhase2 = false;
    public bool isPhase3 = false;
    public bool isWinPhase = false;
    public bool isEasterEggPhase = false;

    private float winPhaseTimer = 0f;
    private float phase1Duration = 13f; // Phase 1 Timeframe (Lasts 13s)

    // Timers and phase management for laser firing
    private float laserTimer = 0f;
    private float laserInterval = 0.25f;
    private float warningTimer = 0f;
    private float laserDuration = 5f;
    private float laserDurationTimer = 0f;
    private float laserDelayTimer = 0f;

    // Timer for phase transition
    private float phase3Timer = 0f;

    private GameAudio gameAudio;  // References gameaudio to play music
    private bool isGameStarted = false; // Ensure music starts only once

    // Pulse effect variables for background sync
    private float beatInterval = 60f / 72.5f;  // Time between beats for 72.5 BPM (half of song bpm)
    private bool isPulsing = false; // Checks if background currently pulses
    private float pulseDuration = 0.2f;  // Duration the pulse effect lasts
    private float pulseFadeOutTimer = 0f;
    private float pulseFadeDuration = 0.2f;

    // Pulse effects for all phases
    private float pulseTimer = 0f;
    public float pulseRedBlue = 0f;  // Pulse after phase 1

    public void Setup()
    {
        Window.SetTitle("Bullet Hell");
        Window.SetSize(WindowWidth, WindowHeight);

        // Initialize player with parameters (position, size, color)
        player = new Player(new Vector2(WindowWidth / 2 - 20, WindowHeight - 50), new Vector2(40, 40), Color.Green);

        // Initialize GameAudio
        gameAudio = new GameAudio();
    }

    public void Update()
    {
        if (isGameOver)
        {
            Draw.FillColor = Color.Red;
            Text.Draw("GAME OVER", WindowWidth / 2 - 50, WindowHeight / 2);
            gameAudio.StopSong();  // Stop the music when the game is over
            return;
        }

        // Starts music when player enters the game window
        if (!isGameStarted && player != null && player.IsInWindow())  // Checks if the player is in the window
        {
            gameAudio.PlaySong();  // Starts the song when player enters the window
            isGameStarted = true;  // Sets the game as started
        }

        if (invincibilityTime > 0)
        {
            invincibilityTime -= Time.DeltaTime;
        }

        // Pulse effect for background color
        pulseTimer += Time.DeltaTime;

        // Applys a pulse effect for phase 1
        if (pulseTimer >= beatInterval && !isPhase2)
        {
            pulseTimer = 0f;
            isPulsing = true;
            pulseFadeOutTimer = pulseDuration;
        }
        if (isPulsing)
        {
            Draw.FillColor = new Color(175, 23, 64, 100);  // Magenta Pulse
            isPulsing = false;
        }
        else if (pulseFadeOutTimer > 0f)
        {
            pulseFadeOutTimer -= Time.DeltaTime;
            Draw.FillColor = new Color(93, 11, 40, (byte)(255 * (pulseFadeOutTimer / pulseFadeDuration)));
        }
        else
        {
            Draw.FillColor = Phase1Background;
        }

        // Pulse effect for the rest of the game after phase 1
        if (isPhase2)
        {
            pulseRedBlue += Time.DeltaTime;
            float PulseEffect = (float)(System.Math.Sin(pulseRedBlue * 2 * System.Math.PI / 2f) * 0.5 + 0.5);

            // Background colours for pulsing
            Draw.FillColor = new Color(
                (byte)(Phase2Background1.R * PulseEffect + Phase2Background2.R * (1 - PulseEffect)),
                (byte)(Phase2Background1.G * PulseEffect + Phase2Background2.G * (1 - PulseEffect)),
                (byte)(Phase2Background1.B * PulseEffect + Phase2Background2.B * (1 - PulseEffect))
            );
        }

        // Background handler
        Draw.Rectangle(0, 0, WindowWidth, WindowHeight);

        // Health bar (top left)
        Draw.FillColor = Color.Red;
        Draw.Rectangle(10, 10, 100, 20);
        Draw.FillColor = Color.Green;
        Draw.Rectangle(10, 10, 100 * (3 - hitCount) / 3, 20);

        // After phase 3, stop the player from moving
        if (!isWinPhase && !isEasterEggPhase)
        {
            // Draws the player as a green square
            player.MovePlayer();
            player.DrawPlayer();
        }

        // Manage phases and timers
        phaseTimer += Time.DeltaTime;

        // Handle phase transitions after timer
        if (phaseTimer >= phase1Duration && !isPhase2)
        {
            isPhase2 = true;
            attacks.Clear(); // Clear all attacks before phase 2
        }

        // Trigger phase 3
        phase3Timer += Time.DeltaTime;
        if (phase3Timer >= 26f && !isPhase3)
        {
            isPhase3 = true;
            attacks.Clear();  // Clear all attacks before phase 3
        }

        // Phase 3 triangles attack the player
        if (isPhase3 && !isWinPhase && !isEasterEggPhase)  // Clear all triangles from phase 3 after game is won
        {
            TriangleAttack();
        }

        // Phase 2 laser handler
        if (isPhase2 && !isPhase3)
        {
            if (warningTimer > 0)
            {
                warningTimer -= Time.DeltaTime;
                Draw.FillColor = Color.Yellow;
            }
            else
            {
                laserDelayTimer += Time.DeltaTime;
            }

            // After 7 seconds, fire lasers to sync with song
            if (laserDelayTimer >= 7f)
            {
                LaserAttackLogic();  // Allows lasers to start firing
            }
        }

        // Handles red circle spawns in phase 1
        if (!isPhase2)
        {
            if (Random.Float(0, 1) < 0.15f) // Randomizes chance for red circles in phase 1
            {
                attacks.Add(new Attack(new Vector2(Random.Float(0, WindowWidth), 0), new Vector2(10, 10), Color.Red, 100, AttackType.Projectile));
            }
        }

        // Handles attacks and on-hit attacks
        foreach (var attack in attacks)
        {
            attack.Move();
            DrawAttack(attack);

            if (attack.HitsPlayer(player))
            {
                if (invincibilityTime <= 0)
                {
                    hitCount++;
                    invincibilityTime = invincibilityDuration;

                    if (hitCount >= 3)
                    {
                        isGameOver = true;
                    }
                }
            }
        }

        // Handles the winphase after 7 seconds from phase3
        if (isPhase3 && !isWinPhase && !isEasterEggPhase)
        {
            winPhaseTimer += Time.DeltaTime;
            if (winPhaseTimer >= 7f)
            {
                // Enter the win phase if the player survives phase3 and was hit atleast once during gameplay
                if (hitCount > 0)
                {
                    isWinPhase = true;
                    attacks.Clear();  // Clear all attacks during the win phase
                }
                else
                {
                    // If the player never got hit, trigger the easteregg phase instead
                    isEasterEggPhase = true;
                    attacks.Clear();  // Clear all attacks during easteregg phase
                }
            }
        }

        // End screen for winphase
        if (isWinPhase && !isEasterEggPhase)
        {
            Text.Draw("You Win!", WindowWidth / 2 - 50, WindowHeight / 2);
        }
        if (!isWinPhase && isEasterEggPhase)
        {
            Text.Draw("You won without ever being hit?", WindowWidth / 2 - 250, WindowHeight / 2);
            Text.Draw("Congratulations!", WindowWidth / 2 - 125, WindowHeight / 2 + 30);
        }
    }

    private void TriangleAttack()
    {
        // Random chance to spawn a triangle
        if (Random.Float(0, 1) < 0.1f)
        {
            attacks.Add(new Attack(new Vector2(Random.Float(0, WindowWidth), 0), new Vector2(20, 20), Color.Yellow, 100, AttackType.Triangle));
        }
    }

    private void LaserAttackLogic()
    {
        // Laser firing logic
        if (laserDelayTimer >= 6f)
        {
            if (laserDurationTimer < laserDuration)
            {
                laserDurationTimer += Time.DeltaTime;

                if (laserTimer <= 0)
                {
                    if (laserDurationTimer < 5f)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            attacks.Add(new Attack(new Vector2(0, Random.Float(0, WindowHeight)), new Vector2(30, 5), Color.Cyan, 100, AttackType.Laser));
                        }
                    }

                    laserTimer = laserInterval;
                }
                else
                {
                    laserTimer -= Time.DeltaTime;
                }
            }
            else
            {
                laserDurationTimer = 0f;
            }
        }
    }

    // Handles phase1, phase2 and, phase3 attacks
    private void DrawAttack(Attack attack)
    {
        // Phase1 attacks
        if (attack.attackType == AttackType.Projectile)
        {
            Draw.FillColor = attack.color;
            Draw.Circle(attack.position.X, attack.position.Y, attack.size.X);
        }
        // Phase2 attacks
        else if (attack.attackType == AttackType.Laser)
        {
            Draw.FillColor = attack.color;
            Draw.Rectangle(attack.position.X, attack.position.Y, attack.size.X, attack.size.Y);
        }
        //Phase3 attacks
        else if (attack.attackType == AttackType.Triangle)
        {
            Draw.FillColor = attack.color;
            Draw.Triangle(attack.position.X, attack.position.Y, attack.position.X + attack.size.X, attack.position.Y, attack.position.X + (attack.size.X / 2), attack.position.Y - attack.size.Y);
        }
    }
}