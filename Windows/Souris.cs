using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TBS
{
	public enum MouseButton
	{
		Left,
		Middle,
		Right,
		X1,
		X2
	}

	public class Souris
	{
		private static Souris _instance;
		public static Souris Get()
		{ return _instance ?? (_instance = new Souris()); }

		private Souris()
		{ }

		private MouseState _oldState, _newState;
		private Rectangle _selection;
		public int X { get { return Static.Game.IsActive ? _newState.X : -1; } }
		public int Y { get { return Static.Game.IsActive ? _newState.Y : -1; } }
		public Point Position { get { return new Point(X, Y); } }
		public Rectangle Selection { get { return _selection; } }

		public void Update(MouseState state)
		{
			_oldState = _newState;
			_newState = state;
			if (_newState.LeftButton == ButtonState.Pressed)
			{
				if (_oldState.LeftButton == ButtonState.Released)
				{ _selection = new Rectangle(_newState.X, _newState.Y, 0, 0); }
				else
				{
					_selection.Width = _newState.X - _selection.X;
					_selection.Height = _newState.Y - _selection.Y;
				}
			}
			else
			{ _selection = Rectangle.Empty; }
		}

		public bool Pressed(MouseButton button)
		{
			if (!Static.Game.IsActive)
			{ return false;  }
			switch (button)
			{
				case MouseButton.Left:
					return _newState.LeftButton == ButtonState.Pressed;
				case MouseButton.Middle:
					return _newState.MiddleButton == ButtonState.Pressed;
				case MouseButton.Right:
					return _newState.RightButton == ButtonState.Pressed;
				case MouseButton.X1:
					return _newState.XButton1 == ButtonState.Pressed;
				case MouseButton.X2:
					return _newState.XButton2 == ButtonState.Pressed;
			}
			return false;
		}

		public bool Clicked(MouseButton button)
		{
			if (!Static.Game.IsActive)
			{ return false; }
			switch (button)
			{
				case MouseButton.Left:
					return _oldState.LeftButton == ButtonState.Released && _newState.LeftButton == ButtonState.Pressed;
				case MouseButton.Middle:
					return _oldState.MiddleButton == ButtonState.Released && _newState.MiddleButton == ButtonState.Pressed;
				case MouseButton.Right:
					return _oldState.RightButton == ButtonState.Released && _newState.RightButton == ButtonState.Pressed;
				case MouseButton.X1:
					return _oldState.XButton1 == ButtonState.Released && _newState.XButton1 == ButtonState.Pressed;
				case MouseButton.X2:
					return _oldState.XButton2 == ButtonState.Released && _newState.XButton2 == ButtonState.Pressed;
			}
			return false;
		}

		public bool Released(MouseButton button)
		{
			if (!Static.Game.IsActive)
			{ return false; }
			switch (button)
			{
				case MouseButton.Left:
					return _oldState.LeftButton == ButtonState.Pressed && _newState.LeftButton == ButtonState.Released;
				case MouseButton.Middle:
					return _oldState.MiddleButton == ButtonState.Pressed && _newState.MiddleButton == ButtonState.Released;
				case MouseButton.Right:
					return _oldState.RightButton == ButtonState.Pressed && _newState.RightButton == ButtonState.Released;
				case MouseButton.X1:
					return _oldState.XButton1 == ButtonState.Pressed && _newState.XButton1 == ButtonState.Released;
				case MouseButton.X2:
					return _oldState.XButton2 == ButtonState.Pressed && _newState.XButton2 == ButtonState.Released;
			}
			return false;
		}

		public bool Hold(MouseButton button, ButtonState status = ButtonState.Pressed)
		{
			if (!Static.Game.IsActive)
			{ return false; }
			switch (button)
			{
				case MouseButton.Left:
					return _oldState.LeftButton == status && _newState.LeftButton == status;
				case MouseButton.Middle:
					return _oldState.MiddleButton == status && _newState.MiddleButton == status;
				case MouseButton.Right:
					return _oldState.RightButton == status && _newState.RightButton == status;
				case MouseButton.X1:
					return _oldState.XButton1 == status && _newState.XButton1 == status;
				case MouseButton.X2:
					return _oldState.XButton2 == status && _newState.XButton2 == status;
			}
			return false;
		}

		public override string ToString()
		{ return _newState.ToString(); }
	}
}
