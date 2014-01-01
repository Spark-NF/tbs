using Microsoft.Xna.Framework;

namespace TBS
{
	class UnitCreator
	{
		public static Unit Unit(int type, Player player, Vector2 position)
		{
			if (type == 0)
				return new Unit(type, 3, 2, player, position);
			if (type == 1)
				return new Unit(type, 2, 2, player, position);
			if (type == 2)
				return new Unit(type, 5, 2, player, position);
			if (type == 3)
				return new Unit(type, 6, 2, player, position);
			return null;
		}
	}
}
