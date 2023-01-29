namespace ExcelDatabase
{
    public static partial class Em
    {
        public static class LevelObj
        {
            public enum InteractionPointType
            {
                None,
                Center,
                FixedPoint,
                NearPoint,
            }

            public enum LifeTimeType
            {
                None,
                Collision,
                CollisionToCharacter,
                CollisionToWall,
                Time,
            }

            public enum LevelObjType
            {
                None,
                Item,
                Trigger,
            }
        }
    }
}
