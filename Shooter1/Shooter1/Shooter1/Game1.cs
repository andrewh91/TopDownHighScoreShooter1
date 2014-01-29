using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Shooter1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        
        Rectangle MovementField = new Rectangle((int)50, (int)50, (int)300, (int)300);
        Texture2D MovementFieldSprite;
        Texture2D MirrorSprite;
        Vector2 MirrorHinge = new Vector2((int)-20, (int)-50);
        Texture2D PenaltyZoneSprite;
        Vector2 PenaltyZoneHinge = new Vector2((int)333, (int)362);
        Random random = new Random();
        
        public class Player
        {
            public Texture2D Sprite;
            public Vector2 Position= new Vector2(50,330);
            public Vector2 Speed;
            public float Acceleration = 0.1f;
            public float Decceleration = 0.05f;
            public float TopSpeed = 2;
            public float ResetTopSpeed = 2;
            public float StunnedSpeed = 0.2f;
            public float StunnedTimer = 5000;
            public float HitTimer=0;
            public Rectangle rect
            {
                get
                {
                    int left = (int)Position.X ;
                    int width = Sprite.Width ;
                    int top = (int)Position.Y ;
                    int height = Sprite.Height;
                    return new Rectangle(left, top, width, height);
                }
            }
            public Rectangle MovementRange;
            public Rectangle BulletRange = new Rectangle(30, 30, 360, 360);
            public float ChargeTimer = 0;
            public float MinimumChargeTime = 200;
            public float MaximumChargeTime = 2000;
            public byte PlayerNo;
            public Rectangle Mirror;
            public Rectangle PenaltyZone;
            public float Multiplier=1;
            public float MaxMultiplier=10;
            public string ScoreText;
            public float ScoreValue = 0;
            public Vector2 ScorePosition;
            public float ScoreIncrement = 10;
            public float ScoreDecrement = 10;
            public float SuccessiveHits=0;

            public Player(byte playerNo)
            {
                Position = Vector2.Zero;
                PlayerNo = playerNo;
            }
            public Vector2 GetPosition()
            {
                return Position;
            }
            public Texture2D GetSprite()
            {
                return Sprite;
            }
            public Bullet[] BulletArray ;
            public void PrepareBullet(Bullet bullet, byte PlayerNo)
            {
                bullet.Position = GetPosition();
                if (PlayerNo == 1)
                {
                    bullet.Position.Y = bullet.Position.Y - (GetSprite().Height);
                    bullet.Position.X = bullet.Position.X + (GetSprite().Width / 2) - bullet.Sprite.Width / 2;
                }
                else if (PlayerNo == 2)
                {
                    bullet.Position.X = bullet.Position.X - (GetSprite().Width);
                    bullet.Position.Y = bullet.Position.Y + (GetSprite().Height / 2 - bullet.Sprite.Height / 2);
                }
            }
            public void FireBullet(byte PlayerNo)
            {
                
                foreach (Bullet bullet in BulletArray)
                {
                    if (!bullet.Alive && ChargeTimer <= MinimumChargeTime)
                    {
                        PrepareBullet( bullet, PlayerNo);
                        bullet.Bounce = 1;
                        bullet.Scale.X = 1;
                        bullet.Scale.Y = 1;
                        bullet.Alive = true;
                        bullet.Popped = false;
                        bullet.Ricochet = 0;
                        ChargeTimer = 0;
                        return;
                    }
                    else if (!bullet.Alive && ChargeTimer > MinimumChargeTime && ChargeTimer < MaximumChargeTime)
                    {
                        PrepareBullet(bullet, PlayerNo);
                        bullet.Bounce = 1;
                        if (PlayerNo == 1)
                        {
                            bullet.Scale.X = ChargeTimer / 200;
                            bullet.Scale.Y = 1;
                        }
                        else
                        {
                            bullet.Scale.Y = ChargeTimer / 200;
                            bullet.Scale.X = 1;
                        }
                        bullet.Alive = true;
                        bullet.Popped = false;
                        ChargeTimer = 0;
                        return;
                    }
                    else if (!bullet.Alive && ChargeTimer > MaximumChargeTime)
                    {
                        PrepareBullet(bullet, PlayerNo);
                        bullet.Bounce = 1;
                        if (PlayerNo == 1)
                        {
                            bullet.Scale.X = MaximumChargeTime / 200;
                            bullet.Scale.Y = 1;
                        }
                        else
                        {
                            bullet.Scale.Y = MaximumChargeTime / 200;
                            bullet.Scale.X = 1;
                        }
                        bullet.Alive = true;
                        bullet.Popped = false;
                        ChargeTimer = 0;
                        return;
                    }
                }
            }
            public void UpdateBullet(Vector2 Direction, GameTime gameTime,Mirror SmallMirror)
            {
                
                foreach (Bullet bullet in BulletArray)
                {
                    if (bullet.Alive)
                    {
                        bullet.Position += (bullet.Speed * (Direction + (new Vector2(1,1)*bullet.Ricochet)) * bullet.Bounce);
                    }
                    if (bullet.Alive&&SmallMirror.Alive&&bullet.Rect.Intersects(SmallMirror.Rect))
                    {
                        if (bullet.Popped)//if the bullet hits the mirror from the rear..
                        {
                            bullet.Alive = false;//it disappears
                        }
                        else
                        {
                            SmallMirror.Position += new Vector2(10, 10)*Direction;
                            bullet.Ricochet = 1;
                        }
                    }
                    if (bullet.Alive && bullet.Rect.Intersects(Mirror))
                    {
                        if (bullet.Popped)
                        {
                            bullet.Alive = false;
                        }
                        else
                            bullet.Bounce = -1;
                    }
                    if (bullet.Alive&&bullet.Rect.Intersects(PenaltyZone))
                    {
                        bullet.Alive = false;
                        if (!bullet.Popped)
                        {                            
                            ScoreValue -= ScoreDecrement;
                            SuccessiveHits = 0;
                        }
                    }

                    foreach (ScoreBubble scoreBubble in ScoreBubbleArray)
                    {
                        if (scoreBubble.Alive&&bullet.Alive&&bullet.Rect.Intersects(scoreBubble.Rect))
                        {
                            ScoreValue += ScoreIncrement * Multiplier;
                            SuccessiveHits++;
                            bullet.Bounce = 1;
                            bullet.Popped = true;   
                            scoreBubble.Alive = false;
                            scoreBubble.Popped = true;
                        }
                    }
                    if (LargeScoreBubble.Alive&&bullet.Alive&&bullet.Rect.Intersects(LargeScoreBubble.Rect))
                    {
                        ScoreValue += ScoreIncrement * 10 * Multiplier;
                        SuccessiveHits++;
                        bullet.Popped = true;
                        LargeScoreBubble.Alive = false;
                        LargeScoreBubble.Popped = true;
                    }
                    if (SuccessiveHits > 1 && Multiplier < MaxMultiplier + 1)
                    {
                        Multiplier = 1 + (int)(SuccessiveHits / (MaxMultiplier + (SuccessiveHits / MaxMultiplier)));
                    }
                    else
                    {
                        Multiplier = 1;
                    }
                    if (bullet.Alive&&bullet.Rect.Intersects(rect))
                    {
                        HitTimer = StunnedTimer;
                        bullet.Alive = false;
                    }
                    if (HitTimer > 0)
                    {
                        SuccessiveHits = 0;
                        TopSpeed = StunnedSpeed;
                        HitTimer -= gameTime.ElapsedGameTime.Milliseconds;
                    }
                    else
                    {
                        TopSpeed = ResetTopSpeed;
                    }
                }
            }
            public ScoreBubble[] ScoreBubbleArray;
            public ScoreBubble LargeScoreBubble;
        }
        Player Player01;
        Player Player02;
        public class Bullet
        {
            public Texture2D Sprite;
            public Vector2 Position;
            public Vector2 Speed = new Vector2(6,6);
            public bool Alive = false;
            public short Bounce;
            public short Ricochet = 0;
            public bool Popped;
            public Vector2 Scale;
            public Rectangle Rect
            {
                get
                {
                    int left = (int)Position.X - ((Sprite.Width * (int)Scale.X) / 2)+(Sprite.Width/2);
                    int width = Sprite.Width * (int)Scale.X;
                    int top = (int)Position.Y - ((Sprite.Height * (int)Scale.Y) / 2) + (Sprite.Height / 2);
                    int height = Sprite.Height * (int)Scale.Y;
                    return new Rectangle(left, top, width, height);
                }
            }
            
            public Bullet(Texture2D LoadedSprite)
            {
                Sprite = LoadedSprite;
                //Rect = new Rectangle(0, 0, Sprite.Width, Sprite.Height);
            }
        }
        public class ScoreBubble
        {
            public Texture2D Sprite;
            public Vector2 Position;
            public Vector2 RandomPosition;
            public bool Alive = false;
            public bool Popped = false;
            public float SpawnDelay;
            public float SpawnTimer;
            public double ElapsedAnimationTime;
            public double AnimationDelay=100;
            public int AnimationFrameNumber;
            public int AnimationFrameCount;
            public Texture2D AnimationTexture;
            public Vector2 AnimationPosition;
            public Rectangle Rect
            {
                get
                {
                    int left = (int)Position.X;
                    int width = Sprite.Width;
                    int top = (int)Position.Y;
                    int height = Sprite.Height;
                    return new Rectangle(left, top, width, height);
                }
            }
            public ScoreBubble(Texture2D LoadedSprite,Texture2D LoadedAnimation,float spawnTimerValue,float spawnDelayValue)
            {
                Sprite = LoadedSprite;
                AnimationTexture = LoadedAnimation;
                AnimationFrameCount = AnimationTexture.Width / AnimationTexture.Height;
                SpawnDelay = spawnDelayValue;
                SpawnTimer = spawnTimerValue;
            }
            public void Spawn(GameTime gameTime, Random random, byte PlayerNo, byte Large)
            {
                SpawnTimer -= ((float)gameTime.ElapsedGameTime.TotalMilliseconds);
                if (SpawnTimer < 0)
                {
                    if (Alive==false)
                    {
                        Alive = true;
                        Popped = false;
                        AnimationFrameNumber = 0;
                        if (PlayerNo==1)
                        {
                            RandomPosition.X = 50 + (float)(random.NextDouble() * (300 - (Sprite.Width)));
                            RandomPosition.Y = 30f+(Large*(311));
                        }
                        else if(PlayerNo ==2)
                        {
                            RandomPosition.X = 30f + (Large * (311));
                            RandomPosition.Y = 50 + (float)(random.NextDouble() * (300 - (Sprite.Height)));
                        }

                        Position = new Vector2(RandomPosition.X, RandomPosition.Y);
                    }
                    SpawnTimer = SpawnDelay * (1 + (float)random.NextDouble() * 0.2f);
                }
                
            }
        }
        public class Mirror
        {
            public float SpawnTimer;
            public bool Alive;
            public bool Popped;
            public Vector2 RandomPosition;
            public Vector2 Position;
            public Texture2D Sprite;
            public float SpawnDelay;
            public byte PlayerNo;
            public Rectangle Rect
            {
                get
                {
                    int left = (int)Position.X;
                    int width = Sprite.Width;
                    int top = (int)Position.Y;
                    int height = Sprite.Height;
                    return new Rectangle(left, top, width, height);
                }
            }
            public Mirror(Texture2D LoadedTexture, float spawnDelayValue, float spawnTimerValue)
            {
                Sprite = LoadedTexture;
                SpawnDelay = spawnDelayValue;
                SpawnTimer = spawnTimerValue;
            }
            public void Spawn(GameTime gameTime, Random random )
            {
                SpawnTimer -= ((float)gameTime.ElapsedGameTime.TotalMilliseconds);
                if (SpawnTimer < 0)
                {
                    if (Alive == false)
                    {
                        Alive = true;
                        Popped = false;
                        
                        RandomPosition.X = 50 + (float)(random.NextDouble() * (300 - (Sprite.Width)));
                        RandomPosition.Y = 50f + (float)(random.NextDouble() * (300 - (Sprite.Height)));

                        Position = new Vector2(RandomPosition.X, RandomPosition.Y);
                    }
                    SpawnTimer = SpawnDelay * (1 + (float)random.NextDouble() * 0.2f);
                }

            }
        }
        Mirror Mirror01;
        
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Player01 = new Player(1);
            Player02 = new Player(2);
            Player01.ScorePosition = new Vector2(50, 420);
            Player02.ScorePosition = new Vector2(420,50 );
            Player01.Sprite = Content.Load<Texture2D>("Player01");
            Player02.Sprite = Content.Load<Texture2D>("Player02");
            Player01.MovementRange = new Rectangle((int)50, (int)350 - Player01.Sprite.Height, (int)300 - Player01.Sprite.Width, (int)Player01.Sprite.Height);
            Player02.MovementRange = new Rectangle((int)350 - Player02.Sprite.Width, (int)50, (int)Player02.Sprite.Width, (int)300 - Player02.Sprite.Height);
            Player01.Mirror = new Rectangle(50, 0, 300, 30);
            Player02.Mirror = new Rectangle(0, 50, 30, 300);
            Player01.PenaltyZone = new Rectangle(50, 383, 300, 30);
            Player02.PenaltyZone = new Rectangle(383, 50, 30, 300);
            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("SpriteFont");//Score01 should really be named after the font, rather than the string it draws :$
            MovementFieldSprite = Content.Load<Texture2D>("WhiteBackground");
            MirrorSprite = Content.Load<Texture2D>("Mirror");
            PenaltyZoneSprite = Content.Load<Texture2D>("PenaltyZone");
            Texture2D BulletTexture = Content.Load<Texture2D>("Bullet01");
            Player01.BulletArray = new Bullet[12];
            for (int i = 0; i < 12; i++)
            {
                Player01.BulletArray[i] = new Bullet(BulletTexture);
            }
            Player02.BulletArray = new Bullet[12];
            for (int i = 0; i < 12; i++)
            {
                Player02.BulletArray[i] = new Bullet(BulletTexture);
            }
            Texture2D ScoreBubbleAnimation = Content.Load<Texture2D>("ScoreBubbleAnimation");
            Texture2D LargeScoreBubbleAnimation = Content.Load<Texture2D>("LargeScoreBubbleAnimation");
            Texture2D ScoreBubbleTexture = Content.Load<Texture2D>("ScoreBubble01");
            Texture2D LargeScoreBubbleTexture = Content.Load<Texture2D>("LargeScoreBubble");
            Player01.ScoreBubbleArray = new ScoreBubble[6];
            for (int i = 0; i < 6; i++)
            {
                Player01.ScoreBubbleArray[i] = new ScoreBubble(ScoreBubbleTexture, ScoreBubbleAnimation,5000,5000);
            }
            Player02.ScoreBubbleArray = new ScoreBubble[6];
            for (int i = 0; i < 6; i++)
            {
                Player02.ScoreBubbleArray[i] = new ScoreBubble(ScoreBubbleTexture, ScoreBubbleAnimation,5000,5000);
            }
            Player01.LargeScoreBubble = new ScoreBubble(LargeScoreBubbleTexture, LargeScoreBubbleAnimation,35000,35000);
            Player02.LargeScoreBubble = new ScoreBubble(LargeScoreBubbleTexture, LargeScoreBubbleAnimation,35000,35000);
            Texture2D MirrorTexture = Content.Load<Texture2D>("SmallMirror");
            Mirror01 = new Mirror(MirrorTexture,1000,1000);

            

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            //Player01 controls
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                Player01.Speed.X -= Player01.Acceleration;
                if (Player01.Speed.X < -Player01.TopSpeed)
                {
                    Player01.Speed.X = -Player01.TopSpeed;
                }
                Player01.Position.X += Player01.Speed.X;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                Player01.Speed.X += Player01.Acceleration;
                if (Player01.Speed.X > Player01.TopSpeed)
                {
                    Player01.Speed.X = Player01.TopSpeed;
                }
                Player01.Position.X += Player01.Speed.X;
            }
            else
            {
                if (Player01.Speed.X > Player01.TopSpeed)
                {
                    Player01.Speed.X = Player01.TopSpeed;
                }
                if (Player01.Speed.X < -Player01.TopSpeed)
                {
                    Player01.Speed.X = -Player01.TopSpeed;
                }
                if (Player01.Speed.X < 0)
                {
                    Player01.Speed.X += Player01.Decceleration;
                    if (Player01.Speed.X > 0)
                    {
                        Player01.Speed.X = 0;
                    }
                }
                if (Player01.Speed.X > 0)
                {
                    Player01.Speed.X -= Player01.Decceleration;
                }
                Player01.Position.X += Player01.Speed.X;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Player01.ChargeTimer += ((float)gameTime.ElapsedGameTime.TotalMilliseconds);
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.W) && Player01.ChargeTimer > 0)
            {
                Player01.FireBullet(Player01.PlayerNo);
                Player01.ChargeTimer = 0;
            }
            //Player02 Controls
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                Player02.Speed.Y -= Player02.Acceleration;
                if (Player02.Speed.Y < -Player02.TopSpeed)
                {
                    Player02.Speed.Y = -Player02.TopSpeed;
                }
                Player02.Position.Y += Player02.Speed.Y;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                Player02.Speed.Y += Player02.Acceleration;
                if (Player02.Speed.Y > Player02.TopSpeed)
                {
                    Player02.Speed.Y = Player02.TopSpeed;
                }
                Player02.Position.Y += Player02.Speed.Y;
            }
            else
            {
                if (Player02.Speed.Y > Player02.TopSpeed)
                {
                    Player02.Speed.Y = Player02.TopSpeed;
                }
                if (Player02.Speed.Y < -Player02.TopSpeed)
                {
                    Player02.Speed.Y = -Player02.TopSpeed;
                }
                if (Player02.Speed.Y < 0)
                {
                    Player02.Speed.Y += Player02.Decceleration;
                    if (Player02.Speed.Y > 0)
                    {
                        Player02.Speed.Y = 0;
                    }
                }
                if (Player02.Speed.Y > 0)
                {
                    Player02.Speed.Y -= Player02.Decceleration;
                }
                Player02.Position.Y += Player02.Speed.Y;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                Player02.ChargeTimer += ((float)gameTime.ElapsedGameTime.TotalMilliseconds);
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.Left) && Player02.ChargeTimer > 0)
            {
                Player02.FireBullet(Player02.PlayerNo);
                Player02.ChargeTimer = 0;
            }
            //Player01 bounding
            if (Player01.Position.X > Player01.MovementRange.Right)
            {
                Player01.Speed.X = -1 * Player01.Speed.X;
                Player01.Position.X = Player01.MovementRange.Right;
            }
            if (Player01.Position.X < Player01.MovementRange.Left)
            {
                Player01.Speed.X = -1 * Player01.Speed.X;
                Player01.Position.X = Player01.MovementRange.Left;
            }
            Player01.Position.Y = Player01.MovementRange.Top;
            //Player02 bounding
            if (Player02.Position.Y > Player02.MovementRange.Bottom)
            {
                Player02.Speed.Y = -1 * Player02.Speed.Y;
                Player02.Position.Y = Player02.MovementRange.Bottom;
            }
            if (Player02.Position.Y < Player02.MovementRange.Top)
            {
                Player02.Speed.Y = -1 * Player02.Speed.Y;
                Player02.Position.Y = Player02.MovementRange.Top;
            }
            Player02.Position.X = Player02.MovementRange.Left;
            //Player01 Bullet
            
            Player01.UpdateBullet(new Vector2(0, -1),gameTime,Mirror01);
            Player02.UpdateBullet(new Vector2(-1, 0),gameTime,Mirror01);
            Player01.ScoreText = ("Player 1 Score: " + Player01.ScoreValue+" Multiplier: "+Player01.Multiplier+ " Succesive hits: " + Player01.SuccessiveHits);
            Player02.ScoreText = ("Player 2 Score: " + Player02.ScoreValue);
            for (int i = 0; i < 6; i++)
            {
                Player01.ScoreBubbleArray[i].Spawn(gameTime, random,Player01.PlayerNo,0);
                Player02.ScoreBubbleArray[i].Spawn(gameTime, random,Player02.PlayerNo,0);
            }
            Player01.LargeScoreBubble.Spawn(gameTime, random, Player01.PlayerNo,1);
            Player02.LargeScoreBubble.Spawn(gameTime, random, Player02.PlayerNo,1);
            Mirror01.Spawn(gameTime, random);
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            spriteBatch.Draw(MovementFieldSprite, MovementField, Color.White);
            spriteBatch.Draw(MirrorSprite, Player02.Mirror, Color.White);
            spriteBatch.Draw(MirrorSprite, Player02.Mirror, null, Color.White, -1.57079633f, MirrorHinge, 0, 0);
            spriteBatch.Draw(PenaltyZoneSprite, Player01.PenaltyZone, Color.White);
            spriteBatch.Draw(PenaltyZoneSprite, Player01.PenaltyZone, null, Color.White, 1.57079633f, PenaltyZoneHinge, 0, 0); 
            if (Mirror01.Alive)
            {
                spriteBatch.Draw(Mirror01.Sprite, Mirror01.Rect, Color.White);
            }
            spriteBatch.Draw(Player01.Sprite, Player01.rect, Color.Green);
            spriteBatch.Draw(Player02.Sprite, Player02.rect, Color.Red);
            spriteBatch.DrawString(spriteFont,Player01.ScoreText, Player01.ScorePosition, Color.White);
            spriteBatch.DrawString(spriteFont ,Player02.ScoreText, Player02.ScorePosition, Color.White);

            foreach (Bullet bullet in Player01.BulletArray)
            {
                if (bullet.Alive)
                {
                    spriteBatch.Draw(bullet.Sprite, bullet.Rect,Color.Green);
                }
            }
            foreach (Bullet bullet in Player02.BulletArray)
            {
                if (bullet.Alive)
                {
                    spriteBatch.Draw(bullet.Sprite, bullet.Rect, Color.Red);
                }
            }
            foreach (ScoreBubble scoreBubble in Player01.ScoreBubbleArray)
            {
                if (scoreBubble.Alive)
                {
                    spriteBatch.Draw(scoreBubble.Sprite, scoreBubble.Rect, Color.White);
                }
                if (scoreBubble.Popped)
                {
                    scoreBubble.ElapsedAnimationTime += gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (scoreBubble.ElapsedAnimationTime >= scoreBubble.AnimationDelay)
                    {
                        if (scoreBubble.AnimationFrameNumber < scoreBubble.AnimationFrameCount)
                        {
                            scoreBubble.AnimationFrameNumber = (scoreBubble.AnimationFrameNumber + 1);
                        }
                        scoreBubble.ElapsedAnimationTime = 0;
                    }

                    Rectangle rect = new Rectangle(
                        scoreBubble.AnimationFrameNumber * scoreBubble.AnimationTexture.Height,
                        0,
                        scoreBubble.AnimationTexture.Height,
                        scoreBubble.AnimationTexture.Height);

                    scoreBubble.AnimationPosition.X = scoreBubble.Position.X - ((scoreBubble.AnimationTexture.Height-scoreBubble.Sprite.Width)/2);
                    scoreBubble.AnimationPosition.Y = scoreBubble.Position.Y - ((scoreBubble.AnimationTexture.Height - scoreBubble.Sprite.Height) / 2);

                    spriteBatch.Draw(scoreBubble.AnimationTexture, scoreBubble.AnimationPosition, rect, Color.White);

                }
            }
            foreach (ScoreBubble scoreBubble in Player02.ScoreBubbleArray)
            {
                if (scoreBubble.Alive)
                {
                    spriteBatch.Draw(scoreBubble.Sprite, scoreBubble.Rect, Color.White);
                } 
                if (scoreBubble.Popped)
                {
                    scoreBubble.ElapsedAnimationTime += gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (scoreBubble.ElapsedAnimationTime >= scoreBubble.AnimationDelay)
                    {
                        if (scoreBubble.AnimationFrameNumber < scoreBubble.AnimationFrameCount)
                        {
                            scoreBubble.AnimationFrameNumber = (scoreBubble.AnimationFrameNumber + 1);
                        }
                        scoreBubble.ElapsedAnimationTime = 0;
                    }

                    Rectangle rect = new Rectangle(
                        scoreBubble.AnimationFrameNumber * scoreBubble.AnimationTexture.Height,
                        0,
                        scoreBubble.AnimationTexture.Height,
                        scoreBubble.AnimationTexture.Height);

                    scoreBubble.AnimationPosition.X = scoreBubble.Position.X - ((scoreBubble.AnimationTexture.Height - scoreBubble.Sprite.Width) / 2);
                    scoreBubble.AnimationPosition.Y = scoreBubble.Position.Y - ((scoreBubble.AnimationTexture.Height - scoreBubble.Sprite.Height) / 2);

                    spriteBatch.Draw(scoreBubble.AnimationTexture, scoreBubble.AnimationPosition, rect, Color.White);

                }
            }
            if (Player01.LargeScoreBubble.Alive)
            {
                spriteBatch.Draw(Player01.LargeScoreBubble.Sprite, Player01.LargeScoreBubble.Rect, Color.White);
            }
            if (Player01.LargeScoreBubble.Popped)
            {
                Player01.LargeScoreBubble.ElapsedAnimationTime += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (Player01.LargeScoreBubble.ElapsedAnimationTime >= Player01.LargeScoreBubble.AnimationDelay)
                {
                    if (Player01.LargeScoreBubble.AnimationFrameNumber < Player01.LargeScoreBubble.AnimationFrameCount)
                    {
                        Player01.LargeScoreBubble.AnimationFrameNumber = (Player01.LargeScoreBubble.AnimationFrameNumber + 1);
                    }
                    Player01.LargeScoreBubble.ElapsedAnimationTime = 0;
                }

                Rectangle rect = new Rectangle(
                    Player01.LargeScoreBubble.AnimationFrameNumber * Player01.LargeScoreBubble.AnimationTexture.Height,
                    0,
                    Player01.LargeScoreBubble.AnimationTexture.Height,
                    Player01.LargeScoreBubble.AnimationTexture.Height);

                Player01.LargeScoreBubble.AnimationPosition.X = Player01.LargeScoreBubble.Position.X - ((Player01.LargeScoreBubble.AnimationTexture.Height - Player01.LargeScoreBubble.Sprite.Width) / 2);
                Player01.LargeScoreBubble.AnimationPosition.Y = Player01.LargeScoreBubble.Position.Y - ((Player01.LargeScoreBubble.AnimationTexture.Height - Player01.LargeScoreBubble.Sprite.Height) / 2);

                spriteBatch.Draw(Player01.LargeScoreBubble.AnimationTexture, Player01.LargeScoreBubble.AnimationPosition, rect, Color.White);

            }
            if (Player02.LargeScoreBubble.Alive)
            {
                spriteBatch.Draw(Player02.LargeScoreBubble.Sprite, Player02.LargeScoreBubble.Rect, Color.White);
            }
            if (Player02.LargeScoreBubble.Popped)
            {
                Player02.LargeScoreBubble.ElapsedAnimationTime += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (Player02.LargeScoreBubble.ElapsedAnimationTime >= Player02.LargeScoreBubble.AnimationDelay)
                {
                    if (Player02.LargeScoreBubble.AnimationFrameNumber < Player02.LargeScoreBubble.AnimationFrameCount)
                    {
                        Player02.LargeScoreBubble.AnimationFrameNumber = (Player02.LargeScoreBubble.AnimationFrameNumber + 1);
                    }
                    Player02.LargeScoreBubble.ElapsedAnimationTime = 0;
                }

                Rectangle rect = new Rectangle(
                    Player02.LargeScoreBubble.AnimationFrameNumber * Player02.LargeScoreBubble.AnimationTexture.Height,
                    0,
                    Player02.LargeScoreBubble.AnimationTexture.Height,
                    Player02.LargeScoreBubble.AnimationTexture.Height);

                Player02.LargeScoreBubble.AnimationPosition.X = Player02.LargeScoreBubble.Position.X - ((Player02.LargeScoreBubble.AnimationTexture.Height - Player02.LargeScoreBubble.Sprite.Width) / 2);
                Player02.LargeScoreBubble.AnimationPosition.Y = Player02.LargeScoreBubble.Position.Y - ((Player02.LargeScoreBubble.AnimationTexture.Height - Player02.LargeScoreBubble.Sprite.Height) / 2);

                spriteBatch.Draw(Player02.LargeScoreBubble.AnimationTexture, Player02.LargeScoreBubble.AnimationPosition, rect, Color.White);
                

            }
            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
