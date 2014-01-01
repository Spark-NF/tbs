using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TBS
{
	class Sprite
	{
		public Texture2D Texture { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		public int Versions { get; set; }
		public int Version { get; set; }
		public int Animations { get; set; }
		public int Speed { get; set; }
		private double _timeCounter;
		private int _currentAnimation;

		public Sprite(Texture2D texture, int versions = 1, int animations = 1, int speed = 500, int version = 0)
		{
			Texture = texture;
			Width = texture.Width / animations;
			Height = texture.Height / versions;

			Versions = versions;
			Version = version % versions;
			Animations = animations;
			Speed = speed;
			_timeCounter = 0;
			_currentAnimation = 0;
		}

		public void Update(GameTime gameTime)
		{
			if (Animations == 0)
				return;
			_timeCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
			if (_timeCounter < Speed)
				return;
			_currentAnimation = (_currentAnimation + 1) % Animations;
			_timeCounter -= Speed;
		}

		public void Draw(SpriteBatch spriteBatch, float layer, Vector2 position, int version = -1, Color? color = null, SpriteEffects? effect = null)
		{
			if (version < 0)
				version = Version;
			spriteBatch.Draw(
				Texture,
				position,
				new Rectangle(
					Width * _currentAnimation,
					Height * version,
					Width,
					Height),
				color.HasValue ? color.Value : Color.White,
				0.0f,
				Vector2.Zero,
				1.0f,
				effect.HasValue ? effect.Value : SpriteEffects.None,
				layer);
		}
	}
}
