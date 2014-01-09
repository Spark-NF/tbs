using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TBS.ScreenManager;

namespace TBS.Screens
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
	    private bool _isAnimating;

	    private Sprite _cursor, _move, _attack;
	    private int _gridWidth, _gridHeight;
		private readonly Terrain[] _terrains;
		private readonly Dictionary<string, Sprite> _texturesBuildings = new Dictionary<string, Sprite>();
		private readonly Dictionary<string, Sprite> _texturesUnitsSmall = new Dictionary<string, Sprite>();
		private readonly Dictionary<string, Sprite> _texturesUnitsPreview = new Dictionary<string, Sprite>();
		private readonly Dictionary<string, Sprite> _texturesUnitsBig = new Dictionary<string, Sprite>();
		private Texture2D _backgroundTexture;
		private SpriteFont _font, _fontDebug, _fontPopup;
	    private Sprite _fontLife, _capturing;
		private Vector2 _cursorPos, _curMovePath;
		private Unit _selectedUnit;
		private int _currentPlayer;
		private int _turn;
	    private readonly int _mapHeight, _mapWidth;

		private Sprite _popupLeft, _popupMid, _popupRight, _popupLeftOn, _popupMidOn, _popupRightOn;

	    private Vector2 _camera;
		private readonly Player[] _players;
		private readonly Terrain[,] _mapTerrains;
		private bool[,] _availableMoves;
		private int[,] _availableAttacks, _availableAttacksOther;
		private readonly Building[,] _mapBuildings;
	    private readonly List<Unit> _units;
	    private Unit _attacksShowing;

		private bool _showContextMenu;
	    private string _contextMenuContext;
	    private string[] _contextMenus;
		private Point _contextMenuPos;
	    private int _contextMaxWidth;

		private int _fpsFrameRate;
		private int _fpsFrameCounter;
		private TimeSpan _fpsElapsed = TimeSpan.Zero;
		//private readonly Dictionary<Color, Texture2D> _colors = new Dictionary<Color, Texture2D>();
		private List<Node> _movePath;

	    public GameplayScreen(string map)
        {
		    TransitionOnTime = TimeSpan.FromSeconds(1.5);
			TransitionOffTime = TimeSpan.FromSeconds(0.5);

			UnitCreator.Initialize();

			_nullCursor = new Vector2(-1, -1);
			_terrains = new[]
		    {
			    new Terrain("Plains", false, 1, 1, 1, 2, 1, 1, 1, -1, -1),
			    new Terrain("Road", false, 0, 1, 1, 1, 1, 1, 1, -1, -1),
			    new Terrain("Wood", true, 3, 1, 1, 3, 3, 2, 1, -1, -1),
			    new Terrain("Mountain", false, 4, 2, 1, -1, -1, -1, 1, -1, -1),
			    new Terrain("Wasteland", false, 2, 1, 1, 3, 3, 2, 1, -1, -1),
			    new Terrain("Ruins", true, 1, 1, 1, 2, 1, 1, 1, -1, -1),
			    new Terrain("Sea", false, 0, -1, -1, -1, -1, -1, 1, 1, 1),
			    new Terrain("BridgeSea", false, 0, 1, 1, 1, 1, 1, 1, 1, 1),
			    new Terrain("BridgeRiver", false, 0, 1, 1, 1, 1, 1, 1, 1, 1),
			    new Terrain("River", false, 0, 2, 1, -1, -1, -1, 1, -1, -1),
			    new Terrain("Beach", false, 0, 1, 1, 2, 2, 1, 1, -1, 1),
			    new Terrain("Rough Sea", false, 0, -1, -1, -1, -1, -1, 1, 2, 2),
			    new Terrain("Mist", true, 0, -1, -1, -1, -1, -1, 1, 1, 1),
			    new Terrain("Reef", true, 0, -1, -1, -1, -1, -1, 1, 2, 2)
		    };

			// Load map data from TXT file
		    var flines = File.ReadLines("Content/Maps/" + map + ".txt")
				.Where(l => l.Length != 0 && !l.StartsWith("#"))
				.ToList();
			//_mapName = flines[0].Trim();
			_mapWidth = Convert.ToInt32(flines[1].Trim());
			_mapHeight = Convert.ToInt32(flines[2].Trim());
		    var plyers = flines.GetRange(3, 4).Where(l => !l.StartsWith("0")).ToArray();

		    _players = new Player[plyers.Length];
		    for (var i = 0; i < _players.Length; ++i)
		    {
				if (i != 0)
					_players[i] = new PlayerAI(i + 1, Convert.ToInt32(plyers[i].Trim()));
				else
					_players[i] = new Player(i + 1, Convert.ToInt32(plyers[i].Trim()));
		    }
		    var terrainLines = new string[_mapHeight];
		    var read = 0;
			for (var i = 7; i < _mapHeight + 7; ++i)
		    {
				var line = flines[i].Trim();
				terrainLines[read++] = line;
		    }
		    var buildings = flines;
			buildings.RemoveRange(0, _mapHeight + 7);

			// Generate terrain according to map data
			_mapTerrains = new Terrain[_mapHeight, _mapWidth];
			for (var y = 0; y < _mapHeight; ++y)
				for (var x = 0; x < _mapWidth; ++x)
				{
					var type = terrainLines[y][x] >= '0' && terrainLines[y][x] <= '9'
						? terrainLines[y][x] - '0'
						: terrainLines[y][x] - 'a' + 10;
					_mapTerrains[y, x] = _terrains[type];
				}

		    var bOrder = new[] { "Headquarter", "City", "Factory", "Port", "Airport" };
			_mapBuildings = new Building[_mapHeight, _mapWidth];
			_units = new List<Unit>();
			foreach (var data in buildings.Select(b => b.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)))
			{
				var p = Convert.ToInt32(data[2]);
			    if (data.Length == 5)
			    {
					var u = UnitCreator.Unit(
						data[3],
						_players[p - 1],
						new Vector2(
							Convert.ToInt32(data[1]),
							Convert.ToInt32(data[0])));
				    u.Life = Convert.ToInt32(data[4]) * 10;
					_units.Add(u);
			    }
				else if (data.Length == 4)
				{
					var x = Convert.ToInt32(data[1]);
					var y = Convert.ToInt32(data[0]);
					_mapBuildings[y, x] = new Building(bOrder[Convert.ToInt32(data[3])], p == 0 ? null : _players[p - 1], new Vector2(x, y));
				}
			}

			_availableMoves = new bool[_mapHeight, _mapWidth];
			_availableAttacks = new int[_mapHeight, _mapWidth];
			_availableAttacksOther = new int[_mapHeight, _mapWidth];
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

			_backgroundTexture = _content.Load<Texture2D>("Menu/Background");
			_cursor = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Cursor"), 1, 2);
			_move = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Move"));
			_attack = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Attack"));
			_capturing = new Sprite(_content.Load<Texture2D>("Capturing"));

	        _terrains[0].Texture = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Plains"));
			_terrains[1].Texture = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Road"), 16);
			_terrains[2].Texture = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Wood"));
			_terrains[3].Texture = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Mountain"));
			_terrains[4].Texture = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Wasteland"));
			_terrains[5].Texture = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Ruins"));
			_terrains[6].Texture = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Sea"), 16);
			_terrains[7].Texture = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Bridge"), 16);
			_terrains[8].Texture = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Bridge"), 16);
			_terrains[9].Texture = new Sprite(_content.Load<Texture2D>("Terrains/Medium/River"), 16);
			_terrains[10].Texture = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Beach"));
			_terrains[11].Texture = new Sprite(_content.Load<Texture2D>("Terrains/Medium/RoughSea"));
			_terrains[12].Texture = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Mist"));
			_terrains[13].Texture = new Sprite(_content.Load<Texture2D>("Terrains/Medium/Reef"));
			_gridWidth = _terrains[0].Texture.Texture.Width;
			_gridHeight = _terrains[0].Texture.Texture.Width;

			_texturesBuildings.Add("Headquarter", new Sprite(_content.Load<Texture2D>("Buildings/Headquarter"), 6, 4));
			_texturesBuildings.Add("City", new Sprite(_content.Load<Texture2D>("Buildings/City"), 6, 4));
			_texturesBuildings.Add("Factory", new Sprite(_content.Load<Texture2D>("Buildings/Factory"), 6, 4));
			_texturesBuildings.Add("Port", new Sprite(_content.Load<Texture2D>("Buildings/Port"), 6, 4));
			_texturesBuildings.Add("Airport", new Sprite(_content.Load<Texture2D>("Buildings/Airport"), 6, 4));

	        var units = new[]
	        {
		        new Tuple<string,string>("Anti-Air", "AntiAir"),
		        new Tuple<string,string>("Anti-Tank", "AntiTank"),
		        new Tuple<string,string>("Artillery", "Artillery"),
		        new Tuple<string,string>("Battleship", "Battleship"),
		        new Tuple<string,string>("Mech", "Bazooka1"),
		        new Tuple<string,string>("Bomber", "Bomber"),
		        new Tuple<string,string>("Carrier", "Carrier"),
		        new Tuple<string,string>("Cruiser", "Cruiser"),
		        new Tuple<string,string>("Duster", "Duster"),
		        new Tuple<string,string>("Fighter", "Fighter"),
		        new Tuple<string,string>("Battle Copter", "FightHeli"),
		        new Tuple<string,string>("Flare", "Flare"),
		        new Tuple<string,string>("Gunboat", "Gunboat"),
		        new Tuple<string,string>("Infantry", "Inf1"),
		        new Tuple<string,string>("Lander", "Lander"),
		        new Tuple<string,string>("Medium Tank", "MediumTank"),
		        new Tuple<string,string>("Missiles", "Missiles"),
		        new Tuple<string,string>("Bike", "Moto1"),
		        new Tuple<string,string>("Recon", "Recon"),
		        new Tuple<string,string>("Rig", "Rig"),
		        new Tuple<string,string>("Rockets", "Rockets"),
		        new Tuple<string,string>("Seaplane", "Seaplane"),
		        new Tuple<string,string>("Submarine", "Submarine"),
		        new Tuple<string,string>("Tank", "Tank"),
		        new Tuple<string,string>("Transport Copter", "TransHeli"),
		        new Tuple<string,string>("War Tank", "WarTank")
	        };
	        foreach (var u in units)
			{
				//_texturesUnitsSmall.Add(u.Item1, new Sprite(_content.Load<Texture2D>("Units/Small/" + u.Item2), 6, 3, 200));
				_texturesUnitsPreview.Add(u.Item1, new Sprite(_content.Load<Texture2D>("Units/Preview/" + u.Item2), 2, 3, 400));
				_texturesUnitsBig.Add(u.Item1, new Sprite(_content.Load<Texture2D>("Units/Big/" + u.Item2), 6, 3, 200));
	        }

			_font = _content.Load<SpriteFont>("Fonts/Game");
			_fontDebug = _content.Load<SpriteFont>("Fonts/Debug");
			_fontPopup = _content.Load<SpriteFont>("Fonts/Popup");
			_fontLife = new Sprite(_content.Load<Texture2D>("Fonts/Life"), 12);

			_popupLeft = new Sprite(_content.Load<Texture2D>("Popup/Left"));
			_popupLeftOn = new Sprite(_content.Load<Texture2D>("Popup/LeftOn"));
			_popupMid = new Sprite(_content.Load<Texture2D>("Popup/Mid"));
			_popupMidOn = new Sprite(_content.Load<Texture2D>("Popup/MidOn"));
			_popupRight = new Sprite(_content.Load<Texture2D>("Popup/Right"));
			_popupRightOn = new Sprite(_content.Load<Texture2D>("Popup/RightOn"));

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
			for (var i = 0; i < _terrains.GetLength(0); ++i)
				_terrains[i].Texture.Update(gameTime);
			for (var i = 0; i < _texturesBuildings.Count; ++i)
				_texturesBuildings.Values.ElementAt(i).Update(gameTime);
			for (var i = 0; i < _texturesUnitsBig.Count; ++i)
				_texturesUnitsBig.Values.ElementAt(i).Update(gameTime);
			for (var i = 0; i < _texturesUnitsPreview.Count; ++i)
				_texturesUnitsPreview.Values.ElementAt(i).Update(gameTime);
			for (var i = 0; i < _texturesUnitsSmall.Count; ++i)
				_texturesUnitsSmall.Values.ElementAt(i).Update(gameTime);
			_cursor.Update(gameTime);

			// Update moving animations
			if (_units.Any(u => u.UpdateAnimation(gameTime)))
			{
				_isAnimating = false;
				return;
			}
		    if (_isAnimating)
			    return;

		    // Context menu
		    var oldShow = _showContextMenu;
		    var noSelect = false;
		    if (_showContextMenu && Souris.Get().Clicked(MouseButton.Left))
		    {
				var rect = new Rectangle(_contextMenuPos.X + 2 - (int)_camera.X, _contextMenuPos.Y + 2 - (int)_camera.Y, _contextMaxWidth + 2 + _popupLeft.Width + _popupRight.Width, 16 * _contextMenus.Length);
			    if (rect.Contains(Souris.Get().Position))
			    {
				    var index = (int)Math.Floor((double)(Souris.Get().Y - rect.Y) / 16f);
					var selected = _contextMenus[index];
				    if (_contextMenuContext == "Build")
					{
						if (selected != "Cancel")
						{
							var unit = UnitCreator.Unit(
								selected.Substring(0, selected.IndexOf(" - ", StringComparison.Ordinal)),
								_players[_currentPlayer - 1],
								_cursorPos,
								true);
							if (unit != null)
							{
								unit.Moved = true;
								_units.Add(unit);
							}
						}
					}
					else if (selected == "Move")
					{
						var texture = _texturesUnitsBig[_selectedUnit.Type];
						_selectedUnit.Animate(_gridWidth * _selectedUnit.Position - _camera - new Vector2(
							(int)Math.Floor((double)(_gridWidth + texture.Width) / 2) - _gridWidth,
							texture.Height - _gridHeight), 1000);
						_isAnimating = true;
						_selectedUnit.Move(_cursorPos);
						_availableMoves = new bool[_mapHeight, _mapWidth];
						_selectedUnit = null;
						_movePath = null;
					}
					else if (selected == "Attack")
					{
						var unitUnder = _units.FirstOrDefault(t =>
							Math.Abs(t.Position.X - _cursorPos.X) < 0.1
							&& Math.Abs(t.Position.Y - _cursorPos.Y) < 0.1);
						if (_movePath != null && _movePath.Count > 1 && _contextMenuContext == "Direct")
						{
							_selectedUnit.Move(new Vector2(_movePath[_movePath.Count - 2].Position.X, _movePath[_movePath.Count - 2].Position.Y));
							_availableMoves = new bool[_mapHeight, _mapWidth];
						}
						_selectedUnit.Moved = true;
						if (unitUnder != null)
						{
							_selectedUnit.Attack(unitUnder, _mapTerrains);
							if (_selectedUnit.Life <= 0)
								_units.Remove(_selectedUnit);
							if (unitUnder.Life <= 0)
								_units.Remove(unitUnder);
						}
						_selectedUnit = null;
						_movePath = null;
					}
					else if (selected == "Capture")
					{
						_selectedUnit.Move(_cursorPos);
						_availableMoves = new bool[_mapHeight, _mapWidth];
						_selectedUnit.Capture(_mapBuildings[(int)_cursorPos.Y, (int)_cursorPos.X]);
						_selectedUnit = null;
						_movePath = null;
					}
					else if (selected == "Wait")
					{
						_selectedUnit.Wait();
						_availableMoves = new bool[_mapHeight, _mapWidth];
						_selectedUnit = null;
						_movePath = null;
					}
					else if (selected == "End turn")
						NextTurn();
			    }
				_showContextMenu = false;
			    noSelect = true;
		    }

			// Update cursor position
	        if (!_showContextMenu)
	        {
		        var curPos = new Vector2((int)((Souris.Get().X + _camera.X) / _gridWidth), (int)((Souris.Get().Y + _camera.Y) / _gridHeight));
		        _cursorPos = Souris.Get().X + _camera.X >= 0 && Souris.Get().Y + _camera.Y >= 0
		                     && curPos.X < _mapWidth && curPos.Y < _mapHeight
						   ? curPos
						   : _nullCursor;
	        }

			// Right click
		    if (_attacksShowing != null && !Souris.Get().Pressed(MouseButton.Right))
			{
				_availableAttacksOther = new int[_mapHeight, _mapWidth];
				_attacksShowing = null;
		    }
		    else if (Souris.Get().Clicked(MouseButton.Right))
		    {
			    if (_showContextMenu)
				    _showContextMenu = false;
			    else
				{
					var unitUnder = _units.FirstOrDefault(t =>
						Math.Abs(t.Position.X - _cursorPos.X) < 0.1
						&& Math.Abs(t.Position.Y - _cursorPos.Y) < 0.1);
					if (unitUnder != null)
					{
						_attacksShowing = unitUnder;
						SetAvailableAttacks(unitUnder, true, false);
					}
					else
						_selectedUnit = null;
				}
		    }

			// Possible path to mouse
			if (_selectedUnit != null && !_selectedUnit.Moved && _cursorPos != _nullCursor && _cursorPos != _curMovePath)
			{
				_curMovePath = _cursorPos;
			    var init = new Point((int)_selectedUnit.Position.X, (int)_selectedUnit.Position.Y);
			    if (_movePath != null && _movePath.Any())
				{
					var pf = new AStar(_mapTerrains, _units, _selectedUnit);
				    var nodes = pf.FindPath(
						_movePath.Last().Position,
						new Point((int)_cursorPos.X, (int)_cursorPos.Y),
						(int)_movePath.Last().DistanceTraveled);
					while (nodes != null && nodes.Any() && _movePath.Any()
						&& (nodes.Last().DistanceTraveled > _selectedUnit.MovingDistance
						|| nodes.Count > 1 && nodes.Last().Occupied && nodes[nodes.Count - 2].Friendly
						|| _movePath.Count > 0 && nodes.Last().Occupied && _movePath.Last().Friendly))
					{
						_movePath.RemoveAt(_movePath.Count - 1);
						nodes = pf.FindPath(
							_movePath.Any() ? _movePath.Last().Position : init,
							new Point((int)_cursorPos.X, (int)_cursorPos.Y),
							_movePath.Any() ? (int)_movePath.Last().DistanceTraveled : 0);
						if (nodes != null && nodes.Count > 1 && nodes.Last().Occupied && nodes[nodes.Count - 2].Friendly)
						{
							var n = pf.FindPath(
								_movePath.Any() ? _movePath.Last().Position : init,
								new Point((int)_cursorPos.X, (int)_cursorPos.Y),
								_movePath.Any() ? (int)_movePath.Last().DistanceTraveled : 0,
								false);
							if (n != null)
								nodes = n;
						}
					}
					if (_movePath.Any() && nodes != null)
					{
						var movePathNoNodes = _movePath.Select(u => u.Position).ToList();
						_movePath.AddRange(nodes);

						// Remove duplicates
						foreach (var n in nodes)
						{
							if (n.Position == new Point((int)_selectedUnit.Position.X, (int)_selectedUnit.Position.Y))
							{
								_movePath.Clear();
								break;
							}
							var index = movePathNoNodes.IndexOf(n.Position);
							if (index < 0)
								continue;
							_movePath.RemoveRange(index + 1, _movePath.Count - index - 1);
							movePathNoNodes.RemoveRange(index + 1, movePathNoNodes.Count - index - 1);
						}
					}
					else
						_movePath = nodes;
				}
				if (_movePath == null || !_movePath.Any())
				{
					var pf = new AStar(_mapTerrains, _units, _selectedUnit);
					var nodes = pf.FindPath(init, new Point((int)_cursorPos.X, (int)_cursorPos.Y));
					if (nodes != null && nodes.Any() &&
						(nodes.Last().DistanceTraveled <= _selectedUnit.MovingDistance
						|| nodes.Count > 1 && nodes[nodes.Count - 2].DistanceTraveled <= _selectedUnit.MovingDistance
						&& _availableAttacks[nodes.Last().Position.Y, nodes.Last().Position.X] == 2))
						_movePath = nodes;
			    }
			}

		    // Mouse click
			if (Souris.Get().Clicked(MouseButton.Left) && _cursorPos != _nullCursor && !noSelect)
	        {
				var unitUnder = _units.FirstOrDefault(t =>
					Math.Abs(t.Position.X - _cursorPos.X) < 0.1
					&& Math.Abs(t.Position.Y - _cursorPos.Y) < 0.1);
				var buildingUnder = _mapBuildings[(int)_cursorPos.Y, (int)_cursorPos.X];
		        if (Souris.Get().Clicked(MouseButton.Left) && (_selectedUnit == null || _selectedUnit.Moved) && unitUnder != null && !unitUnder.Moved && unitUnder.Player.Number == _currentPlayer)
				{
			        _selectedUnit = unitUnder;
					SetAvailableAttacks(unitUnder);
					_availableMoves = new bool[_mapHeight, _mapWidth];
			        _availableMoves[(int)_selectedUnit.Position.Y, (int)_selectedUnit.Position.X] = true;
			        var pf = new AStar(_mapTerrains, _units, _selectedUnit);
			        for (var y = (int)Math.Max(_selectedUnit.Position.Y - _selectedUnit.MovingDistance, 0); y <= (int)Math.Min(_selectedUnit.Position.Y + _selectedUnit.MovingDistance, _mapHeight - 1); ++y)
				        for (var x = (int)Math.Max(_selectedUnit.Position.X - _selectedUnit.MovingDistance, 0); x <= (int)Math.Min(_selectedUnit.Position.X + _selectedUnit.MovingDistance, _mapWidth - 1); ++x)
				        {
					        var nodes = pf.FindPath(new Point((int)_selectedUnit.Position.X, (int)_selectedUnit.Position.Y), new Point(x, y));
					        if (nodes != null && nodes.Any() && nodes.Last().DistanceTraveled <= _selectedUnit.MovingDistance && !nodes.Last().Occupied)
						        _availableMoves[y, x] = true;
				        }
		        }
				else if (!oldShow && Souris.Get().Clicked(MouseButton.Left)
					&& _selectedUnit != null && !_selectedUnit.Moved
					&& (_availableMoves[(int)_cursorPos.Y, (int)_cursorPos.X] || _availableAttacks[(int)_cursorPos.Y, (int)_cursorPos.X] == 2))
				{
			        if (unitUnder == null)
			        {
						if (buildingUnder != null && buildingUnder.Player != _selectedUnit.Player && _selectedUnit.CanCapture)
							SetContextMenu("Capture", "Capture", "Move", "Cancel");
						else
							SetContextMenu("Move", "Move", "Cancel");
					}
					else if (unitUnder.Player != _selectedUnit.Player && _selectedUnit.CanMoveAndAttack())
						SetContextMenu("Direct", "Attack", "Cancel");
					else if (unitUnder.Player != _selectedUnit.Player && _availableAttacks[(int)_cursorPos.Y, (int)_cursorPos.X] == 2)
						SetContextMenu("Distance", "Attack", "Cancel");
			        else if (unitUnder == _selectedUnit)
			        {
				        if (buildingUnder != null && buildingUnder.Player != _selectedUnit.Player && _selectedUnit.CanCapture)
					        SetContextMenu("Capture", "Capture", "Wait", "Cancel");
				        else
				        {
					        var inRange = false;
							for (var y = 0; y < _mapHeight; ++y)
								for (var x = 0; x < _mapHeight; ++x)
									if (_availableAttacks[y, x] == 2)
									{
										inRange = true;
										break;
									}
					        if (inRange)
								SetContextMenu("Wait", "Attack", "Wait", "Cancel");
							else
								SetContextMenu("Wait", "Wait", "Cancel");
				        }
			        }
			        else if (unitUnder.Type == _selectedUnit.Type)
						SetContextMenu("Merge", "Merge", "Cancel");
				}
				else if (!oldShow && !_showContextMenu && Souris.Get().Clicked(MouseButton.Left)
					&& (unitUnder == null || unitUnder.Moved || unitUnder.Player.Number != _currentPlayer))
				{
					if (_selectedUnit == null && buildingUnder != null && unitUnder == null
					    && buildingUnder.Player != null && buildingUnder.Player.Number == _currentPlayer)
					{
						var prices = UnitCreator.GetPrices(buildingUnder.Type);
						if (prices.Count > 0)
						{
							var buy = new string[prices.Count + 1];
							for (var i = 0; i < prices.Count; ++i)
								buy[i] = prices.Keys.ElementAt(i) + " - " + prices.Values.ElementAt(i) + " €";
							buy[prices.Count] = "Cancel";
							SetContextMenu("Build", buy);
						}
						else
							SetContextMenu("End turn", "End turn");
					}
					else if (_selectedUnit != null)
					{
						_selectedUnit = null;
						_movePath = null;
					}
					else
						SetContextMenu("End turn", "End turn");
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

			spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, graphics.Viewport.Width, graphics.Viewport.Height), Color.White);

			// Draw terrain and bridges
			for (var y = 0; y < _mapHeight; ++y)
				for (var x = 0; x < _mapWidth; ++x)
				{
					var terrain = _mapTerrains[y, x];
					var tex = terrain.Type == "Mist" || terrain.Type == "BridgeSea"
						? _terrains[6].Texture
						: (terrain.Type == "BridgeRiver"
							? _terrains[9].Texture
							: (terrain.Type == "Road"
								? _terrains[0].Texture
								: terrain.Texture));
					var number = 0;
					if (terrain.Type == "Sea" || terrain.Type == "BridgeSea")
						number = (y > 0 && !_mapTerrains[y - 1, x].IsSea() ? 8 : 0)
								 + (x < _mapWidth - 1 && !_mapTerrains[y, x + 1].IsSea() ? 4 : 0)
								 + (y < _mapHeight - 1 && !_mapTerrains[y + 1, x].IsSea() ? 2 : 0)
								 + (x > 0 && !_mapTerrains[y, x - 1].IsSea() ? 1 : 0);
					else if (terrain.Type == "River" || terrain.Type == "BridgeRiver")
						number = (y > 0 && !_mapTerrains[y - 1, x].IsRiver() ? 8 : 0)
								 + (x < _mapWidth - 1 && !_mapTerrains[y, x + 1].IsRiver() ? 4 : 0)
								 + (y < _mapHeight - 1 && !_mapTerrains[y + 1, x].IsRiver() ? 2 : 0)
								 + (x > 0 && !_mapTerrains[y, x - 1].IsRiver() ? 1 : 0);
					tex.Draw(
						spriteBatch,
						y / 100f + 0.1f,
						new Vector2(
							_gridWidth * x - _camera.X,
							_gridHeight * y - _camera.Y + _gridHeight - _mapTerrains[y, x].Texture.Height),
						number);
					if ((terrain.Type == "Mist" || terrain.Type == "BridgeSea"
						|| terrain.Type == "BridgeRiver" || terrain.Type == "Road")
						&& _mapBuildings[y, x] == null)
						_mapTerrains[y, x].Texture.Draw(
							spriteBatch,
							y / 100f + 0.101f,
							new Vector2(
								_gridWidth * x - _camera.X,
								_gridHeight * y - _camera.Y + _gridHeight - _mapTerrains[y, x].Texture.Height),
							terrain.Type == "Road" || terrain.Type == "BridgeSea" || terrain.Type == "BridgeRiver"
								? (y > 0 && !_mapTerrains[y - 1, x].IsRoad() ? 8 : 0)
								  + (x < _mapWidth - 1 && !_mapTerrains[y, x + 1].IsRoad() ? 4 : 0)
								  + (y < _mapHeight - 1 && !_mapTerrains[y + 1, x].IsRoad() ? 2 : 0)
								  + (x > 0 && !_mapTerrains[y, x - 1].IsRoad() ? 1 : 0)
								: 0);
				}

	        // Draw available displacements
			if (_selectedUnit != null && !_selectedUnit.Moved)
				for (var y = 0; y < _mapHeight; ++y)
					for (var x = 0; x < _mapWidth; ++x)
						if (_availableMoves[y, x])
							_move.Draw(
								spriteBatch,
								0.80f,
								new Vector2(
									_gridWidth * x - _camera.X,
									_gridHeight * y - _camera.Y));

			// Draw available attacks
			for (var y = 0; y < _mapHeight; ++y)
				for (var x = 0; x < _mapWidth; ++x)
					if (_attacksShowing != null && _availableAttacksOther[y, x] != 0 && (_selectedUnit != _attacksShowing || _selectedUnit != null && _selectedUnit.Moved || !_availableMoves[y, x])
						|| _selectedUnit != null && _availableAttacks[y, x] == 2)
						_attack.Draw(
							spriteBatch,
							0.81f,
							new Vector2(
								_gridWidth * x - _camera.X,
								_gridHeight * y - _camera.Y));

			// Draw current moving path
			if (_movePath != null && _selectedUnit != null && _movePath != null && (_movePath.Any() && !_movePath.Last().Occupied || _selectedUnit.CanMoveAndAttack()))
				foreach (var n in _movePath.Where(n => _availableMoves[n.Position.Y, n.Position.X] || _availableAttacks[n.Position.Y, n.Position.X] == 2))
				{
					spriteBatch.DrawString(_fontPopup, "" + n.DistanceTraveled,
						new Vector2(
							_gridWidth * n.Position.X - _camera.X,
							_gridHeight * n.Position.Y - _camera.Y),
							n.Friendly ? Color.Green : (n.Occupied ? Color.Black : Color.White),
							0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
					_attack.Draw(
						spriteBatch,
						0.82f,
						new Vector2(
							_gridWidth * n.Position.X - _camera.X,
							_gridHeight * n.Position.Y - _camera.Y));
				}

			// Draw buildings
			for (var y = 0; y < _mapHeight; ++y)
				for (var x = 0; x < _mapWidth; ++x)
					if (_mapBuildings[y, x] != null)
					{
						var texture = _texturesBuildings[_mapBuildings[y, x].Type];
						var pos = new Vector2(
							_gridWidth * x - _camera.X + (int)Math.Floor((double)(_gridWidth + texture.Width) / 2) - _gridWidth,
							_gridHeight * y - texture.Height + _gridHeight - _camera.Y);
						texture.Draw(
							spriteBatch,
							y / 100f + 0.103f,
							pos,
							_mapBuildings[y, x].Player == null ? 0 : _mapBuildings[y, x].Player.Version);
						if (_mapBuildings[y, x].CaptureStatus < 20)
							_capturing.Draw(spriteBatch, 0.91f, pos + new Vector2(0, texture.Height - 8));
					}

	        // Draw units
	        foreach (var u in _units)
	        {
		        if (u == _selectedUnit)
		        {
			        var texture = _texturesUnitsBig[u.Type];
					var pos = _gridWidth * u.Position - _camera - new Vector2(
						(int)Math.Floor((double)(_gridWidth + texture.Width) / 2) - _gridWidth,
						texture.Height - _gridHeight);
			        u.Draw(
						texture,
						spriteBatch,
						pos,
						u.Position.Y / 100f + 0.106f,
				        3 * (u.Player.Version - 1),
				        u.Moved && u.Player.Number == _currentPlayer ? new Color(.6f, .6f, .6f) : Color.White,
				        u.Player.Number == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
		        }
		        else
			        _texturesUnitsPreview[u.Type].Draw(
				        spriteBatch,
				        u.Position.Y / 100f + 0.106f,
				        _gridWidth * u.Position - _camera,
						u.Player.Version - 1,
				        u.Moved && u.Player.Number == _currentPlayer ? new Color(.6f, .6f, .6f) : Color.White,
				        u.Player.Number == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
				if (u.Life <= 90)
					_fontLife.Draw(
						spriteBatch,
						0.91f,
						_gridWidth * u.Position - _camera + new Vector2(_gridWidth - 8, _gridHeight - 8),
						(int)Math.Ceiling((double)u.Life / 10));
	        }

	        // User Interface (15px per number)
			spriteBatch.DrawString(_font, "Day " + _turn, new Vector2(13, 9), Color.Black, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.90f);
			spriteBatch.DrawString(_font, "Day " + _turn, new Vector2(12, 8), Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.91f);
			spriteBatch.DrawString(_font, "Player 1: " + _players[0].Money + " €", new Vector2(graphics.Viewport.Width - 220, 9), Color.Black, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.90f);
			spriteBatch.DrawString(_font, "Player 1: " + _players[0].Money + " €", new Vector2(graphics.Viewport.Width - 221, 8), Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.91f);
			spriteBatch.DrawString(_font, "Player 2: " + _players[1].Money + " €", new Vector2(graphics.Viewport.Width - 220, 39), Color.Black, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.90f);
			spriteBatch.DrawString(_font, "Player 2: " + _players[1].Money + " €", new Vector2(graphics.Viewport.Width - 221, 38), Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.91f);
			
			// Context menu (popup)
	        if (_showContextMenu)
			{
				for (var i = 0; i < _contextMenus.Length; ++i)
				{
					var on = new Rectangle(_contextMenuPos.X + 2 - (int)_camera.X, _contextMenuPos.Y + 2 + 16 * i - (int)_camera.Y, _contextMaxWidth + 2 + _popupLeft.Width + _popupRight.Width, 16).Contains(Souris.Get().Position);
					var dec = on ? 0.001f : 0;
					(on ? _popupLeftOn : _popupLeft).Draw(spriteBatch, 0.94f + dec, new Vector2(_contextMenuPos.X, _contextMenuPos.Y + 16 * i) - _camera);
					for (var j = 0; j < _contextMaxWidth + 2; ++j)
						(on ? _popupMidOn : _popupMid).Draw(spriteBatch, 0.94f + dec, new Vector2(_contextMenuPos.X + j + _popupLeft.Width, _contextMenuPos.Y + 16 * i) - _camera);
					(on ? _popupRightOn : _popupRight).Draw(spriteBatch, 0.94f + dec, new Vector2(_contextMenuPos.X + _contextMaxWidth + 2 + _popupLeft.Width, _contextMenuPos.Y + 16 * i) - _camera);
					spriteBatch.DrawString(_fontPopup, _contextMenus[i], new Vector2(_contextMenuPos.X + 9, _contextMenuPos.Y + 16 * i + 3) - _camera, Color.Black, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.95f);
					spriteBatch.DrawString(_fontPopup, _contextMenus[i], new Vector2(_contextMenuPos.X + 8, _contextMenuPos.Y + 16 * i + 2) - _camera, Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.96f);
				}
	        }

			// FPS Counter
			_fpsFrameCounter++;
			var str = string.Format("fps: {0} mem: {1} cam: ({2},{3}) trv: {4}", _fpsFrameRate, GC.GetTotalMemory(false), _camera.X, _camera.Y, _movePath != null && _movePath.Any() ? _movePath.Last().DistanceTraveled : -1);
			spriteBatch.DrawString(_fontDebug, str, new Vector2(13, graphics.Viewport.Height - 27), Color.Black, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.999f);
			spriteBatch.DrawString(_fontDebug, str, new Vector2(12, graphics.Viewport.Height - 28), Color.Orange, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);

			// Cursor
			if (_cursorPos != _nullCursor)
				_cursor.Draw(spriteBatch, 0.899f, _cursorPos * _gridWidth - _camera);

			spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || _pauseAlpha > 0)
				ScreenManager.FadeBackBufferToBlack(MathHelper.Lerp(1f - TransitionAlpha, 1f, _pauseAlpha / 2));
        }

		/*private Texture2D Color2Texture2D(Color color)
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
	    }*/

		/*private Rectangle ContextMenuRect(int dec = 0, Vector2? camera = null)
	    {
			return new Rectangle(
				_contextMenuPos.X + dec - (camera.HasValue ? (int)camera.Value.X : 0),
				_contextMenuPos.Y + dec - (camera.HasValue ? (int)camera.Value.Y : 0),
				20 + _contextMaxWidth - dec * 2,
				10 + 30 * _contextMenus.Length - dec * 2);
	    }*/

		private void SetContextMenu(string context, params string[] menus)
		{
			_contextMenuContext = context;
			_contextMenus = menus;
			_showContextMenu = true;
			_contextMenuPos = new Point(Souris.Get().X + (int)_camera.X, Souris.Get().Y + (int)_camera.Y);

			_contextMaxWidth = 0;
			foreach (var m in menus)
			{
				var len = (int)_fontPopup.MeasureString(m).X;
				if (len > _contextMaxWidth)
					_contextMaxWidth = len;
			}
		}

	    private void NextTurn()
	    {
			// Clean units
		    _units.RemoveAll(u => u.Life <= 0);

			// Next player
			_currentPlayer++;
		    if (_currentPlayer > _players.Length)
		    {
			    _currentPlayer = 1;
			    _turn++;
		    }

			// Update buildings capture count
		    for (var y = 0; y < _mapHeight; ++y)
				for (var x = 0; x < _mapWidth; ++x)
					if (_mapBuildings[y, x] != null)
						_mapBuildings[y, x].NextTurn();

			// Heal units on friendly buildings
			foreach (var u in _units.Where(u => 
					u.Player.Number == _currentPlayer
					&& _mapBuildings[(int)u.Position.Y, (int)u.Position.X] != null
					&& _mapBuildings[(int)u.Position.Y, (int)u.Position.X].Player == u.Player
					&& (_mapBuildings[(int)u.Position.Y, (int)u.Position.X].Type == u.Building
					|| u.Building == "Factory")))
				u.Heal();

			// Next turn
		    _players[_currentPlayer - 1].NextTurn();
		    _showContextMenu = false;

			// IA thinking
		    if (_players[_currentPlayer - 1].IsAI)
		    {
			    var ai = _players[_currentPlayer - 1] as PlayerAI;
			    if (ai == null)
				    return;
				ai.Think(_mapTerrains, _mapBuildings, _units);
			    NextTurn();
		    }
	    }

	    private void SetAvailableAttacks(Unit unit, bool other = false, bool real = true)
		{
			// Resets the array before filling it
			if (other)
				_availableAttacksOther = new int[_mapHeight, _mapWidth];
			else
				_availableAttacks = new int[_mapHeight, _mapWidth];

			var pf = new AStar(_mapTerrains, _units, unit);
			for (var y = (int)Math.Max(unit.Position.Y - unit.MovingDistance, 0); y <= (int)Math.Min(unit.Position.Y + unit.MovingDistance, _mapHeight - 1); ++y)
				for (var x = (int)Math.Max(unit.Position.X - unit.MovingDistance, 0); x <= (int)Math.Min(unit.Position.X + unit.MovingDistance, _mapWidth - 1); ++x)
				{
					var nodes = pf.FindPath(new Point((int)unit.Position.X, (int)unit.Position.Y), new Point(x, y));
					if ((nodes == null || !nodes.Any()
						 || !(nodes.Last().DistanceTraveled <= unit.MovingDistance)
						 || !unit.CanMoveAndAttack()) && (y != (int)unit.Position.Y || x != (int)unit.Position.X))
						continue;
					if (nodes != null && nodes.Any() && (nodes.Last().Occupied || nodes.Last().Friendly && nodes.Last().Unit == 2 && real))
						continue;
					var ymin = Math.Max(y - unit.RangeMax, 0);
					var ymax = Math.Min(y + unit.RangeMax, _mapHeight - 1);
					var xmin = Math.Max(x - unit.RangeMax, 0);
					var xmax = Math.Min(x + unit.RangeMax, _mapWidth - 1);
					for (var iy = ymin; iy <= ymax; ++iy)
						for (var ix = xmin; ix <= xmax; ++ix)
						{
							var diff = Math.Abs(y - iy) + Math.Abs(x - ix);
							if (diff >= unit.RangeMin && diff <= unit.RangeMax)
							{
								var data = _units.Any(t =>
									t.Player != unit.Player
									&& Math.Abs(t.Position.X - ix) < 0.1
									&& Math.Abs(t.Position.Y - iy) < 0.1)
									? 2
									: 1;
								if (other)
									_availableAttacksOther[iy, ix] = data;
								else
									_availableAttacks[iy, ix] = data;
							}
						}
				}
	    }
    }
}
