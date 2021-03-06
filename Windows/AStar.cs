﻿using System;
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
		public bool Friendly;
		public int Unit;
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

		private static float Heuristic(Point point1, Point point2)
		{
			return Math.Abs(point1.X - point2.X) +
				   Math.Abs(point1.Y - point2.Y);
		}

		private void InitializeSearchNodes(Terrain[,] map, List<Unit> units, Unit unit)
		{
			_searchNodes = new Node[_levelWidth, _levelHeight];
			for (var y = 0; y < _levelHeight; y++)
			{
				for (var x = 0; x < _levelWidth; x++)
				{
					var under = units != null ? units.FirstOrDefault(u => (int)u.Position.X == x && (int)u.Position.Y == y) : null;
					var node = new Node
					{
						Position = new Point(x, y),
						Weight = unit.WeightFromType(map[y, x]),
						Occupied = under != null && under.Player != unit.Player,
						Friendly = under != null && under.Player == unit.Player && (x != (int)unit.Position.X || y != (int)unit.Position.Y),
						Unit = under != null ? (under.Moved ? 2 : 1) : 0
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
					if (node == null || node.Weight < 0)
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

			for (var y = 0; y < _levelHeight; ++y)
			{
				for (var x = 0; x < _levelWidth; ++x)
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

			foreach (var t in _openList)
			{
				if (!(t.DistanceToGoal < smallestDistanceToGoal))
					continue;

				currentTile = t;
				smallestDistanceToGoal = currentTile.DistanceToGoal;
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

		public List<Node> FindPath(Point startPoint, Point endPoint, int initialDistance = 0, bool friendly = true, bool ennemy = false)
		{
			if (startPoint == endPoint)
				return null;
			ResetSearchNodes();

			var startNode = _searchNodes[startPoint.X, startPoint.Y];
			var endNode = _searchNodes[endPoint.X, endPoint.Y];

			startNode.InOpenList = true;
			startNode.DistanceToGoal = Heuristic(startPoint, endPoint);
			startNode.DistanceTraveled = initialDistance;
			_openList.Add(startNode);

			while (_openList.Count > 0)
			{
				var currentNode = FindBestNode();
				if (currentNode == null)
					break;
				if (currentNode == endNode)
					return FindFinalPath(startNode, endNode);
				if ((!currentNode.Occupied || ennemy) && (!currentNode.Friendly || friendly))
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

		public List<Node> FindIntermediatePath(Point startPoint, Point endPoint, int maxDistance, int initialDistance = 0, bool friendly = true)
		{
			if (startPoint == endPoint)
				return null;
			ResetSearchNodes();

			var startNode = _searchNodes[startPoint.X, startPoint.Y];
			var endNode = _searchNodes[endPoint.X, endPoint.Y];

			startNode.InOpenList = true;
			startNode.DistanceToGoal = Heuristic(startPoint, endPoint);
			startNode.DistanceTraveled = initialDistance;
			_openList.Add(startNode);

			while (_openList.Count > 0)
			{
				var currentNode = FindBestNode();
				if (currentNode == null)
					break;
				if (currentNode == endNode || currentNode.DistanceTraveled >= maxDistance)
					return FindFinalPath(startNode, currentNode);
				if (!currentNode.Occupied && (!currentNode.Friendly || friendly))
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
