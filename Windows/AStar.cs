using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TBS
{
	class Node
	{
		public Point Position;
		public int Weight;
		public Node[] Neighbors;
		public Node Parent;
		public bool InOpenList;
		public bool InClosedList;
		public float DistanceToGoal;
		public float DistanceTraveled;
		public bool Occupied;
	}

	class AStar
	{
		private Node[,] _searchNodes;
		private readonly int _levelWidth;
		private readonly int _levelHeight;
		private readonly List<Node> _openList = new List<Node>();
		private readonly List<Node> _closedList = new List<Node>();

		public AStar(Terrain[,] map, List<Unit> units, Unit unit)
		{
			_levelWidth = map.GetLength(1);
			_levelHeight = map.GetLength(0);
			InitializeSearchNodes(map, units, unit);
		}

		private float Heuristic(Point point1, Point point2)
		{
			return Math.Abs(point1.X - point2.X) +
				   Math.Abs(point1.Y - point2.Y);
		}

		private void InitializeSearchNodes(Terrain[,] map, List<Unit> units, Unit unit)
		{
			_searchNodes = new Node[_levelHeight, _levelWidth];
			for (var y = 0; y < _levelHeight; y++)
			{
				for (var x = 0; x < _levelWidth; x++)
				{
					var node = new Node
					{
						Position = new Point(x, y),
						Weight = unit.WeightFromType(map[y, x]),
						Occupied = units.Any(u => u.Player != unit.Player && (int)u.Position.X == x && (int)u.Position.Y == y)
					};
					if (node.Weight < 0)
						continue;
					node.Neighbors = new Node[4];
					_searchNodes[x, y] = node;
				}
			}
			for (var y = 0; y < _levelHeight; y++)
			{
				for (var x = 0; x < _levelWidth; x++)
				{
					var node = _searchNodes[x, y];
					if (node == null || node.Weight < 0 || node.Occupied)
						continue;
					var neighbors = new[]
					{
						new Point (x, y - 1),
						new Point (x, y + 1),
						new Point (x - 1, y),
						new Point (x + 1, y)
					};
					for (var i = 0; i < neighbors.Length; i++)
					{
						var position = neighbors[i];
						if (position.X < 0 || position.X > _levelWidth - 1
								|| position.Y < 0 || position.Y > _levelHeight - 1)
							continue;
						var neighbor = _searchNodes[position.X, position.Y];
						if (neighbor == null || neighbor.Weight < 0)
							continue;
						node.Neighbors[i] = neighbor;
					}
				}
			}
		}

		private void ResetSearchNodes()
		{
			_openList.Clear();
			_closedList.Clear();

			for (var y = 0; y < _levelHeight; y++)
			{
				for (var x = 0; x < _levelWidth; x++)
				{
					var node = _searchNodes[x, y];

					if (node == null)
						continue;

					node.InOpenList = false;
					node.InClosedList = false;

					node.DistanceTraveled = float.MaxValue;
					node.DistanceToGoal = float.MaxValue;
				}
			}
		}

		private Node FindBestNode()
		{
			var currentTile = _openList[0];
			var smallestDistanceToGoal = float.MaxValue;

			foreach (Node t in _openList)
			{
				if (t.DistanceToGoal < smallestDistanceToGoal)
				{
					currentTile = t;
					smallestDistanceToGoal = currentTile.DistanceToGoal;
				}
			}
			return currentTile;
		}

		private List<Node> FindFinalPath(Node startNode, Node endNode)
		{
			_closedList.Add(endNode);
			var parentTile = endNode.Parent;

			while (parentTile != startNode)
			{
				_closedList.Add(parentTile);
				parentTile = parentTile.Parent;
			}

			var finalPath = new List<Node>();
			for (var i = _closedList.Count - 1; i >= 0; i--)
				finalPath.Add(_closedList[i]);

			return finalPath;
		}

		public List<Node> FindPath(Point startPoint, Point endPoint)
		{
			if (startPoint == endPoint)
				return null;
			ResetSearchNodes();

			var startNode = _searchNodes[startPoint.X, startPoint.Y];
			var endNode = _searchNodes[endPoint.X, endPoint.Y];

			startNode.InOpenList = true;
			startNode.DistanceToGoal = Heuristic(startPoint, endPoint);
			startNode.DistanceTraveled = 0;
			_openList.Add(startNode);

			while (_openList.Count > 0)
			{
				var currentNode = FindBestNode();
				if (currentNode == null)
					break;
				if (currentNode == endNode)
					return FindFinalPath(startNode, endNode);
				foreach (var neighbor in currentNode.Neighbors)
				{
					if (neighbor == null || neighbor.Weight < 0)
						continue;
					var distanceTraveled = currentNode.DistanceTraveled + neighbor.Weight;
					var heuristic = Heuristic(neighbor.Position, endPoint);
					if (neighbor.InOpenList == false && neighbor.InClosedList == false)
					{
						neighbor.DistanceTraveled = distanceTraveled;
						neighbor.DistanceToGoal = distanceTraveled + heuristic;
						neighbor.Parent = currentNode;
						neighbor.InOpenList = true;
						_openList.Add(neighbor);
					}
					else if (neighbor.InOpenList || neighbor.InClosedList)
					{
						if (!(neighbor.DistanceTraveled > distanceTraveled))
							continue;
						neighbor.DistanceTraveled = distanceTraveled;
						neighbor.DistanceToGoal = distanceTraveled + heuristic;
						neighbor.Parent = currentNode;
					}
				}
				_openList.Remove(currentNode);
				currentNode.InClosedList = true;
			}
			return null;
		}
	}
}
