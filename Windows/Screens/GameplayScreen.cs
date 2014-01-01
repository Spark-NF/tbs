#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace TBS
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        ContentManager _content;
		float _pauseAlpha;

		private const int CameraSpeed = 5;
		private readonly Vector2 _nullCursor;

		private Sprite _cursor, _move;
	    private Sprite[] _texturesTerrains, _texturesBuildings;
		private Dictionary<string, Sprite> _texturesUnitsSmall = new Dictionary<string, Sprite>();
		private Dictionary<string, Sprite> _texturesUnitsBig = new Dictionary<string, Sprite>();
		private SpriteFont _font, _fontDebug, _fontLife;
		private Vector2 _cursorPos;
		private Unit _selectedUnit;
		private int _currentPlayer;
		private int _turn;

		private Vector2 _camera;
		private Player[] _players;
		private int[,] _mapTerrains;
		private bool[,] _availableMoves;
		private Building[,] _mapBuildings;
		private List<Unit> _units;

		private bool _showContextMenu;
	    private string[] _contextMenus;
		private Point _contextMenuPos;
	    private int _contextMaxWidth;

		private int _fpsFrameRate;
		private int _fpsFrameCounter;
		private TimeSpan _fpsElapsed = TimeSpan.Zero;
		private readonly Dictionary<Color, Texture2D> _colors = new Dictionary<Color, Texture2D>();

	    /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
			TransitionOffTime = TimeSpan.FromSeconds(0.5);

			_nullCursor = new Vector2(-1, -1);
			_players = new[]
			{
				new Player(1, false),
				new Player(2, true)
			};
			_mapTerrains = new[,]
			{
				{ 0, 0, 0, 0, 0, 0, 0 },
				{ 0, 1, 1, 1, 1, 1, 0 },
				{ 0, 1, 3, 3, 3, 1, 0 },
				{ 0, 1, 3, 4, 3, 1, 0 },
				{ 0, 1, 3, 3, 3, 1, 0 },
				{ 0, 1, 2, 2, 2, 1, 0 },
				{ 0, 0, 0, 0, 0, 0, 0 }
			};
			_availableMoves = new bool[7, 7];
			_mapBuildings = new Building[7, 7];
			_mapBuildings[1, 1] = new Building(0, _players[0]);
			_mapBuildings[5, 5] = new Building(0, _players[1]);
			_mapBuildings[5, 1] = new Building(1, null);
			_mapBuildings[1, 5] = new Building(1, null);
			_units = new List<Unit>
			{
				UnitCreator.Unit("Infantry", _players[0], new Vector2(2, 1)),
				UnitCreator.Unit("Mech", _players[0], new Vector2(1, 2)),
				UnitCreator.Unit("Bike", _players[1], new Vector2(5, 4)),
				UnitCreator.Unit("Battle Copter", _players[1], new Vector2(4, 5))
			};
			_camera = new Vector2(-50, -80);
			_turn = 1;
			_currentPlayer = 1;
			_players[_currentPlayer - 1].NextTurn();
			_selectedUnit = null;
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (_content == null)
				_content = new ContentManager(ScreenManager.Game.Services, "Content");

			_cursor = new Sprite(_content.Load<Texture2D>("Cursor"), 1, 2);
			_move = new Sprite(_content.Load<Texture2D>("Move"));
			_texturesTerrains = new[]
			{
				new Sprite(_content.Load<Texture2D>("Terrains/Water")),
				new Sprite(_content.Load<Texture2D>("Terrains/Grass")),
				new Sprite(_content.Load<Texture2D>("Terrains/Road")),
				new Sprite(_content.Load<Texture2D>("Terrains/Forest")),
				new Sprite(_content.Load<Texture2D>("Terrains/Mountain"))
			};
			_texturesBuildings = new[]
			{
				new Sprite(_content.Load<Texture2D>("Buildings/Headquarters"), 5, 2),
				new Sprite(_content.Load<Texture2D>("Buildings/City"), 5, 2)
			};
	        _texturesUnitsSmall.Add("Infantry", new Sprite(_content.Load<Texture2D>("Units/Small/Inf1"), 3, 3, 200));
	        _texturesUnitsSmall.Add("Mech", new Sprite(_content.Load<Texture2D>("Units/Small/Bazooka1"), 3, 3, 200));
			_texturesUnitsSmall.Add("Bike", new Sprite(_content.Load<Texture2D>("Units/Small/Moto1"), 3, 3, 200));
			_texturesUnitsSmall.Add("Battle Copter", new Sprite(_content.Load<Texture2D>("Units/Small/FightHeli"), 3, 3, 200));
			_texturesUnitsSmall.Add("Transport Copter", new Sprite(_content.Load<Texture2D>("Units/Small/TransHeli"), 3, 3, 200));
	        _texturesUnitsBig.Add("Infantry", new Sprite(_content.Load<Texture2D>("Units/Big/Inf1"), 3, 3, 200));
			_texturesUnitsBig.Add("Mech", new Sprite(_content.Load<Texture2D>("Units/Big/Bazooka1"), 3, 3, 200));
			_texturesUnitsBig.Add("Bike", new Sprite(_content.Load<Texture2D>("Units/Big/Moto1"), 3, 3, 200));
			_texturesUnitsBig.Add("Battle Copter", new Sprite(_content.Load<Texture2D>("Units/Big/FightHeli"), 3, 3, 200));
			_texturesUnitsBig.Add("Transport Copter", new Sprite(_content.Load<Texture2D>("Units/Big/TransHeli"), 3, 3, 200));
			_font = _content.Load<SpriteFont>("Fonts/Game");
			_fontDebug = _content.Load<SpriteFont>("Fonts/Debug");

			GC.Collect();
            ScreenManager.Game.ResetElapsedTime();
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            _content.Unload();
        }

	    /// <summary>
	    /// Updates the state of the game. This method checks the GameScreen.IsActive
	    /// property, so the game will stop updating when the pause menu is active,
	    /// or if you tab away to a different application.
	    /// </summary>
	    public override void Update(GameTime gameTime, bool otherScreenHasFocus,
		    bool coveredByOtherScreen)
	    {
		    base.Update(gameTime, otherScreenHasFocus, false);

		    // Gradually fade in or out depending on whether we are covered by the pause screen.
		    _pauseAlpha = coveredByOtherScreen
			    ? Math.Min(_pauseAlpha + 1f / 32, 1)
			    : Math.Max(_pauseAlpha - 1f / 32, 0);

		    if (!IsActive)
			    return;

		    // FPS counter
		    _fpsElapsed += gameTime.ElapsedGameTime;
		    if (_fpsElapsed > TimeSpan.FromSeconds(1))
		    {
			    _fpsElapsed -= TimeSpan.FromSeconds(1);
			    _fpsFrameRate = _fpsFrameCounter;
			    _fpsFrameCounter = 0;
			}

			// Update textures for animations
			for (var i = 0; i < _texturesTerrains.GetLength(0); ++i)
				_texturesTerrains[i].Update(gameTime);
			for (var i = 0; i < _texturesBuildings.GetLength(0); ++i)
				_texturesBuildings[i].Update(gameTime);
			for (var i = 0; i < _texturesUnitsBig.Count; ++i)
				_texturesUnitsBig.Values.ElementAt(i).Update(gameTime);
			for (var i = 0; i < _texturesUnitsSmall.Count; ++i)
				_texturesUnitsSmall.Values.ElementAt(i).Update(gameTime);
			_cursor.Update(gameTime);

		    // Hide context menu
		    var oldShow = _showContextMenu;
		    if (_showContextMenu && Souris.Get().Clicked(MouseButton.Left))
		    {
			    var rect = ContextMenuRect(5, _camera);
			    if (rect.Contains(Souris.Get().Position))
			    {
					var selected = _contextMenus[(int)Math.Floor((double)(Souris.Get().Y - rect.Y) / 30f)];
					if (selected == "Move")
					{
						_selectedUnit.Move(_cursorPos);
						_availableMoves = new bool[7, 7];
						_selectedUnit = null;
					}
					else if (selected == "Capture")
					{
						_selectedUnit.Move(_cursorPos);
						_availableMoves = new bool[7, 7];
						var buildingUnder = _mapBuildings[(int)_cursorPos.Y, (int)_cursorPos.X];
						buildingUnder.Player = _selectedUnit.Player;
						_selectedUnit = null;
					}
					else if (selected == "Wait")
					{
						_selectedUnit.Move(_cursorPos);
						_availableMoves = new bool[7, 7];
						_selectedUnit = null;
					}
					else if (selected == "End turn")
					{
						_currentPlayer++;
						if (_currentPlayer > _players.Length)
						{
							_currentPlayer = 1;
							_turn++;
						}
						_players[_currentPlayer - 1].NextTurn();
						_showContextMenu = false;
						return;
					}
			    }
				_showContextMenu = false;
			}

			// Update cursor position
	        if (!_showContextMenu)
	        {
		        var curPos = new Vector2((int)((Souris.Get().X + _camera.X) / 32), (int)((Souris.Get().Y + _camera.Y) / 32));
		        _cursorPos = Souris.Get().X + _camera.X >= 0 && Souris.Get().Y + _camera.Y >= 0
		                     && curPos.X < _mapTerrains.GetLength(0) && curPos.Y < _mapTerrains.GetLength(1)
						   ? curPos
						   : _nullCursor;
	        }

			// Cancel with right click
		    if (Souris.Get().Clicked(MouseButton.Right))
		    {
			    if (_showContextMenu)
				    _showContextMenu = false;
				else
				    _selectedUnit = null;
		    }

		    // Mouse click
	        if (Souris.Get().Clicked(MouseButton.Left) && _cursorPos != _nullCursor)
	        {
		        Unit unitUnder = null;
		        foreach (var t in _units.Where(t =>
			        Math.Abs(t.Position.X - _cursorPos.X) < 0.1
			        && Math.Abs(t.Position.Y - _cursorPos.Y) < 0.1))
			        unitUnder = t;
		        if (Souris.Get().Clicked(MouseButton.Left) && (_selectedUnit == null || _selectedUnit.Moved) && unitUnder != null && !unitUnder.Moved && unitUnder.Player.Number == _currentPlayer)
		        {
			        _selectedUnit = unitUnder;
			        _availableMoves = new bool[7, 7];
			        _availableMoves[(int)_selectedUnit.Position.Y, (int)_selectedUnit.Position.X] = true;
			        var pf = new AStar(_mapTerrains, _units, _selectedUnit);
			        for (var y = (int)Math.Max(_selectedUnit.Position.Y - _selectedUnit.MovingDistance, 0); y <= (int)Math.Min(_selectedUnit.Position.Y + _selectedUnit.MovingDistance, _mapTerrains.GetLength(1) - 1); ++y)
				        for (var x = (int)Math.Max(_selectedUnit.Position.X - _selectedUnit.MovingDistance, 0); x <= (int)Math.Min(_selectedUnit.Position.X + _selectedUnit.MovingDistance, _mapTerrains.GetLength(0) - 1); ++x)
				        {
					        var nodes = pf.FindPath(new Point((int)_selectedUnit.Position.X, (int)_selectedUnit.Position.Y), new Point(x, y));
					        if (nodes != null && nodes.Any() && nodes.Last().DistanceTraveled <= _selectedUnit.MovingDistance)
						        _availableMoves[y, x] = true;
				        }
		        }
				else if (!oldShow && Souris.Get().Clicked(MouseButton.Left) && _selectedUnit != null && !_selectedUnit.Moved && _availableMoves[(int)_cursorPos.Y, (int)_cursorPos.X])
		        {
			        if (unitUnder == null)
			        {
				        var buildingUnder = _mapBuildings[(int)_cursorPos.Y, (int)_cursorPos.X];
						if (buildingUnder != null && buildingUnder.Player != _selectedUnit.Player && _selectedUnit.CanCapture)
							SetContextMenu("Capture", "Move", "Cancel");
						else
							SetContextMenu("Move", "Cancel");
			        }
			        else if (unitUnder.Player != _selectedUnit.Player)
				        SetContextMenu("Attack", "Cancel");
			        else if (unitUnder == _selectedUnit)
				        SetContextMenu("Wait", "Cancel");
			        else if (unitUnder.Type == _selectedUnit.Type)
				        SetContextMenu("Merge", "Cancel");
		        }
				else if (!oldShow && !_showContextMenu && Souris.Get().Clicked(MouseButton.Left) && unitUnder == null)
				{
					if (_selectedUnit != null)
						_selectedUnit = null;
					else
						SetContextMenu("End turn");
				}
	        }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            var playerIndex = ControllingPlayer != null ? (int)ControllingPlayer.Value : 0;
            var gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            var gamePadDisconnected = !gamePadState.IsConnected
				&& input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            else
            {
				// Camera movement
				var movement = Vector2.Zero;
				movement.X += gamePadState.ThumbSticks.Left.X;
				movement.Y += gamePadState.ThumbSticks.Left.Y;
				if (Clavier.Get().Pressed(Keys.Left))
					movement.X--;
				if (Clavier.Get().Pressed(Keys.Right))
					movement.X++;
				if (Clavier.Get().Pressed(Keys.Up))
					movement.Y--;
				if (Clavier.Get().Pressed(Keys.Down))
					movement.Y++;
				_camera += movement * CameraSpeed;
            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            var graphics = ScreenManager.GraphicsDevice;
            var spriteBatch = ScreenManager.SpriteBatch;
			
			graphics.Clear(ClearOptions.Target, Color.CornflowerBlue, 0, 0);
			spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

			// Draw terrain
			for (var y = 0; y < _mapTerrains.GetLength(0); ++y)
				for (var x = 0; x < _mapTerrains.GetLength(1); ++x)
					_texturesTerrains[_mapTerrains[y, x]].Draw(
						spriteBatch,
						y / 100f,
						new Vector2(
							32 * x - _camera.X,
							32 * y - _camera.Y));

			// Draw available displacements
			if (_selectedUnit != null && !_selectedUnit.Moved)
				for (var y = 0; y < _availableMoves.GetLength(0); ++y)
					for (var x = 0; x < _availableMoves.GetLength(1); ++x)
						if (_availableMoves[y, x])
							_move.Draw(
								spriteBatch,
								y / 100f + 0.005f,
								new Vector2(
									32 * x - _camera.X,
									32 * y - _camera.Y));

			// Draw buildings
			for (var y = 0; y < _mapTerrains.GetLength(0); ++y)
				for (var x = 0; x < _mapTerrains.GetLength(1); ++x)
					if (_mapBuildings[y, x] != null)
						_texturesBuildings[_mapBuildings[y, x].Type].Draw(
							spriteBatch,
							y / 100f + 0.003f,
							new Vector2(
								32 * x - _camera.X + 4,
								32 * y - _texturesBuildings[_mapBuildings[y, x].Type].Height + 32 - _camera.Y),
							_mapBuildings[y, x].Player == null ? 0 : _mapBuildings[y, x].Player.Number);

			// Draw units
	        foreach (var u in _units)
	        {
		        _texturesUnitsBig[u.Type].Draw(
			        spriteBatch,
			        u.Position.Y / 100f + 0.006f,
			        32 * u.Position - _camera - new Vector2(0, 4),
			        0,
			        u.Moved && u.Player.Number == _currentPlayer ? new Color(.6f, .6f, .6f) : Color.White,
			        u.Player.Number == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
				if (u.Life < 10)
					spriteBatch.DrawString(_fontLife, "" + u.Life, 32 * u.Position - _camera - new Vector2(16, 24), Color.White);
	        }

	        // User Interface
			spriteBatch.DrawString(_font, "Day " + _turn, new Vector2(13, 9), Color.Black, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.90f);
			spriteBatch.DrawString(_font, "Day " + _turn, new Vector2(12, 8), Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.91f);
			spriteBatch.DrawString(_font, "Player 1: " + _players[0].Money + " €", new Vector2(graphics.Viewport.Width - 205, 9), Color.Black, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.90f);
			spriteBatch.DrawString(_font, "Player 1: " + _players[0].Money + " €", new Vector2(graphics.Viewport.Width - 206, 8), Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.91f);
			spriteBatch.DrawString(_font, "Player 2: " + _players[1].Money + " €", new Vector2(graphics.Viewport.Width - 205, 39), Color.Black, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.90f);
			spriteBatch.DrawString(_font, "Player 2: " + _players[1].Money + " €", new Vector2(graphics.Viewport.Width - 206, 38), Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.91f);
			
			// Context menu
	        if (_showContextMenu)
			{
				spriteBatch.Draw(
					Color2Texture2D(new Color(.3f, .3f, .3f)),
					ContextMenuRect(0, _camera),
					new Rectangle(0, 0, 1, 1),
					Color.White,
					0f,
					Vector2.Zero,
					SpriteEffects.None,
					0.950f);
				spriteBatch.Draw(
					Color2Texture2D(new Color(.9f, .9f, .9f)),
					ContextMenuRect(1, _camera),
					new Rectangle(0, 0, 1, 1),
					Color.White,
					0f,
					Vector2.Zero,
					SpriteEffects.None,
					0.955f);
				for (var i = 0; i < _contextMenus.Length; ++i)
				{
					spriteBatch.DrawString(_font, _contextMenus[i], new Vector2(_contextMenuPos.X + 9, _contextMenuPos.Y + 30 * i + 5) - _camera, Color.LightGray, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.95f);
					spriteBatch.DrawString(_font, _contextMenus[i], new Vector2(_contextMenuPos.X + 8, _contextMenuPos.Y + 30 * i + 4) - _camera, Color.Black, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.96f);
				}
	        }

			// FPS Counter
			_fpsFrameCounter++;
			var str = string.Format(string.Format("fps: {0} mem: {1} cam: ({2},{3})", _fpsFrameRate, GC.GetTotalMemory(false), _camera.X, _camera.Y), _fpsFrameRate, GC.GetTotalMemory(false));
			spriteBatch.DrawString(_fontDebug, str, new Vector2(13, graphics.Viewport.Height - 27), Color.Black, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.999f);
			spriteBatch.DrawString(_fontDebug, str, new Vector2(12, graphics.Viewport.Height - 28), Color.Orange, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);

			// Cursor
			if (_cursorPos != _nullCursor)
				_cursor.Draw(spriteBatch, 0.899f, _cursorPos * 32 - _camera);

			spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || _pauseAlpha > 0)
				ScreenManager.FadeBackBufferToBlack(MathHelper.Lerp(1f - TransitionAlpha, 1f, _pauseAlpha / 2));
        }

		private Texture2D Color2Texture2D(Color color)
		{
			if (!_colors.ContainsKey(color))
				_colors.Add(color, CreateRectangle(1, 1, color));
			return _colors[color];
		}
		private Texture2D CreateRectangle(int width, int height, Color col, Color border, int size = 1)
		{
			var rectangleTexture = new Texture2D(ScreenManager.GraphicsDevice, width, height, false, SurfaceFormat.Color);
			var color = new Color[width * height];
			for (var i = 0; i < color.Length; i++)
				color[i] = (i < width * size || i >= color.Length - width * size || i % width < size || i % width >= width - size ? border : col);
			rectangleTexture.SetData(color);
			return rectangleTexture;
		}
	    private Texture2D CreateRectangle(int width, int height, Color col)
	    {
		    return CreateRectangle(width, height, col, col);
	    }

		private Rectangle ContextMenuRect(int dec = 0, Vector2? camera = null)
	    {
			return new Rectangle(
				_contextMenuPos.X + dec - (camera.HasValue ? (int)camera.Value.X : 0),
				_contextMenuPos.Y + dec - (camera.HasValue ? (int)camera.Value.Y : 0),
				20 + _contextMaxWidth - dec * 2,
				10 + 30 * _contextMenus.Length - dec * 2);
	    }

	    private void SetContextMenu(params string[] menus)
	    {
		    _contextMenus = menus;
		    _showContextMenu = true;
			_contextMenuPos = new Point(Souris.Get().X + (int)_camera.X, Souris.Get().Y + (int)_camera.Y);
		    _contextMaxWidth = 0;
			foreach (var m in menus)
		    {
			    var len = (int)_font.MeasureString(m).X;
				if (len > _contextMaxWidth)
				    _contextMaxWidth = len;
		    }
	    }
    }
}
