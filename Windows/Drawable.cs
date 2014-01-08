using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TBS
{
	abstract class Drawable
	{
		private bool _animating;
		private Vector2 _animatingFrom;
		private double _animationStatus;
		private double _animationDuration;

		public void Animate(Vector2 position, int duration)
		{
			_animating = true;
			_animatingFrom = position;
			_animationStatus = 0;
			_animationDuration = duration;
		}

		public bool UpdateAnimation(GameTime gameTime)
		{
			if (!_animating)
				return false;
			_animationStatus += gameTime.ElapsedGameTime.TotalMilliseconds;
			if (_animationStatus < _animationDuration)
				return false;
			_animating = false;
			return true;
		}

		public void Draw(Sprite texture, SpriteBatch spriteBatch, Vector2 position, float layer, int version = -1, Color? color = null, SpriteEffects? effects = null)
		{
			var pos = _animating
				? _animatingFrom + (position - _animatingFrom) * (float)(_animationStatus / _animationDuration)
				: position;
			texture.Draw(
				spriteBatch,
				layer,
				pos,
				version,
				color,
				effects);
		}
	}
}
