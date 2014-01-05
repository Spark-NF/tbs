using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using TBS.Screens;

namespace TBS
{
	class Clavier
	{
		private static Clavier _instance;
		public static Clavier Get()
		{ return _instance ?? (_instance = new Clavier()); }

		private Clavier()
		{ }

		public EventHandler<PlayerIndexEventArgs> TextEntered { get; set; }

		public string Text { get; set; }
		private bool _getText;
		public bool GetText
		{
			get { return _getText; }
			set
			{
				TextEntered = null;
				Text = "";
				_getText = value;
			}
		}
		private KeyboardState _oldState, _newState;

		public void Update(KeyboardState state)
		{
			_oldState = _newState;
			_newState = state;

			if (!_getText)
				return;

			var pressedKeys = _newState.GetPressedKeys();
			var shift = pressedKeys.Contains(Keys.RightShift) || pressedKeys.Contains(Keys.LeftShift);
			foreach (var key in pressedKeys.Where(key => _oldState.IsKeyUp(key)))
			{
				switch (key)
				{
					case Keys.Back:
						if (Text.Length > 0)
						{
							Text = Text.Remove(Text.Length - 1, 1);
							if (TextEntered != null)
								TextEntered(this, null);
						}
						break;

					default:
						var c = TranslateChar(key, shift, false, false);
						if (c != 0)
						{
							Text += c;
							if (TextEntered != null)
								TextEntered(this, null);
						}
						break;
				}
			}
		}

		/// <summary>
		/// Convertit une valeur de l'énumération Keys en caractère.
		/// </summary>
		/// <param name="key">La Key à traduire.</param>
		/// <param name="shift">Si la touche shift est enfoncée.</param>
		/// <param name="capsLock">Si le verrouillage de majuscules est activé.</param>
		/// <param name="numLock">Si le verrouillage numérique est activé.</param>
		/// <returns>Le caractère équivalent.</returns>
		public static char TranslateChar(Keys key, bool shift, bool capsLock, bool numLock)
		{
			switch (key)
			{
				case Keys.A: return TranslateAlphabetic('a', shift, capsLock);
				case Keys.B: return TranslateAlphabetic('b', shift, capsLock);
				case Keys.C: return TranslateAlphabetic('c', shift, capsLock);
				case Keys.D: return TranslateAlphabetic('d', shift, capsLock);
				case Keys.E: return TranslateAlphabetic('e', shift, capsLock);
				case Keys.F: return TranslateAlphabetic('f', shift, capsLock);
				case Keys.G: return TranslateAlphabetic('g', shift, capsLock);
				case Keys.H: return TranslateAlphabetic('h', shift, capsLock);
				case Keys.I: return TranslateAlphabetic('i', shift, capsLock);
				case Keys.J: return TranslateAlphabetic('j', shift, capsLock);
				case Keys.K: return TranslateAlphabetic('k', shift, capsLock);
				case Keys.L: return TranslateAlphabetic('l', shift, capsLock);
				case Keys.M: return TranslateAlphabetic('m', shift, capsLock);
				case Keys.N: return TranslateAlphabetic('n', shift, capsLock);
				case Keys.O: return TranslateAlphabetic('o', shift, capsLock);
				case Keys.P: return TranslateAlphabetic('p', shift, capsLock);
				case Keys.Q: return TranslateAlphabetic('q', shift, capsLock);
				case Keys.R: return TranslateAlphabetic('r', shift, capsLock);
				case Keys.S: return TranslateAlphabetic('s', shift, capsLock);
				case Keys.T: return TranslateAlphabetic('t', shift, capsLock);
				case Keys.U: return TranslateAlphabetic('u', shift, capsLock);
				case Keys.V: return TranslateAlphabetic('v', shift, capsLock);
				case Keys.W: return TranslateAlphabetic('w', shift, capsLock);
				case Keys.X: return TranslateAlphabetic('x', shift, capsLock);
				case Keys.Y: return TranslateAlphabetic('y', shift, capsLock);
				case Keys.Z: return TranslateAlphabetic('z', shift, capsLock);

				case Keys.D0: return capsLock ^ shift ? '0' : 'à';
				case Keys.D1: return capsLock ^ shift ? '1' : '&';
				case Keys.D2: return capsLock ^ shift ? '2' : 'é';
				case Keys.D3: return capsLock ^ shift ? '3' : '"';
				case Keys.D4: return capsLock ^ shift ? '4' : '\'';
				case Keys.D5: return capsLock ^ shift ? '5' : '(';
				case Keys.D6: return capsLock ^ shift ? '6' : '-';
				case Keys.D7: return capsLock ^ shift ? '7' : 'è';
				case Keys.D8: return capsLock ^ shift ? '8' : '_';
				case Keys.D9: return capsLock ^ shift ? '9' : 'ç';

				case Keys.Add: return '+';
				case Keys.Divide: return '/';
				case Keys.Multiply: return '*';
				case Keys.Subtract: return '-';

				case Keys.Space: return ' ';
				case Keys.Tab: return '\t';

				case Keys.Decimal: if (numLock && !shift) return '.'; break;
				case Keys.NumPad0: if (numLock && !shift) return '0'; break;
				case Keys.NumPad1: if (numLock && !shift) return '1'; break;
				case Keys.NumPad2: if (numLock && !shift) return '2'; break;
				case Keys.NumPad3: if (numLock && !shift) return '3'; break;
				case Keys.NumPad4: if (numLock && !shift) return '4'; break;
				case Keys.NumPad5: if (numLock && !shift) return '5'; break;
				case Keys.NumPad6: if (numLock && !shift) return '6'; break;
				case Keys.NumPad7: if (numLock && !shift) return '7'; break;
				case Keys.NumPad8: if (numLock && !shift) return '8'; break;
				case Keys.NumPad9: if (numLock && !shift) return '9'; break;

				case Keys.OemBackslash:		return capsLock ^ shift ? '>' : '<';
				case Keys.OemCloseBrackets:	return capsLock ^ shift ? '¨' : '^';
				case Keys.OemComma:			return capsLock ^ shift ? '?' : ',';
				case Keys.OemOpenBrackets:	return capsLock ^ shift ? '°' : ')';
				case Keys.OemPeriod:		return capsLock ^ shift ? '.' : ';';
				case Keys.OemPipe:			return capsLock ^ shift ? 'µ' : '*';
				case Keys.OemPlus:			return capsLock ^ shift ? '+' : '=';
				case Keys.OemQuestion:		return capsLock ^ shift ? '/' : ':';
				case Keys.OemSemicolon:		return capsLock ^ shift ? '£' : '$';
				case Keys.OemTilde:			return capsLock ^ shift ? '%' : 'ù';
				case Keys.OemQuotes:		return '²';

				/*case Keys.OemBackslash: return shift ? '|' : '\\';
				case Keys.OemCloseBrackets: return shift ? '}' : ']';
				case Keys.OemComma: return shift ? '<' : ',';
				case Keys.OemMinus: return shift ? '_' : '-';
				case Keys.OemOpenBrackets: return shift ? '{' : '[';
				case Keys.OemPeriod: return shift ? '>' : '.';
				case Keys.OemPipe: return shift ? '|' : '\\';
				case Keys.OemPlus: return shift ? '+' : '=';
				case Keys.OemQuestion: return shift ? '?' : '/';
				case Keys.OemQuotes: return shift ? '"' : '\'';
				case Keys.OemSemicolon: return shift ? ':' : ';';
				case Keys.OemTilde: return shift ? '~' : '`';*/
			}

			return (char)0;
		}

		public static char TranslateAlphabetic(char baseChar, bool shift, bool capsLock)
		{ return capsLock ^ shift ? char.ToUpper(baseChar) : baseChar; }

		public bool Pressed(Keys button)
		{ return Static.Game.IsActive && _newState.IsKeyDown(button); }

		public bool Released(Keys button)
		{ return Static.Game.IsActive && _newState.IsKeyUp(button); }

		public bool NewPress(Keys button)
		{ return Static.Game.IsActive && _oldState.IsKeyUp(button) && _newState.IsKeyDown(button); }

		public bool NewPress()
		{ return Static.Game.IsActive && _newState.GetPressedKeys().Count() > _oldState.GetPressedKeys().Count(); }

		public bool NewRelease(Keys button)
		{ return Static.Game.IsActive && _oldState.IsKeyDown(button) && _newState.IsKeyUp(button); }

		public bool NewRelease()
		{ return Static.Game.IsActive && _newState.GetPressedKeys().Count() < _oldState.GetPressedKeys().Count(); }

		public bool Press()
		{ return Static.Game.IsActive && _newState.GetPressedKeys().Count() != 0; }

		public bool NoPress()
		{ return Static.Game.IsActive && !_newState.GetPressedKeys().Any(); }

		public override string ToString()
		{ return _newState.ToString(); }
	}
}
