namespace ExcelDatabase
{
    public static partial class Em
    {
        public static class Unit
        {
            public enum RandomType
            {
                None,
                All,
                RandomEach,
                RandomWeight,
            }

            public enum OpType
            {
                None,
                Equal,
                NotEqual,
                GreaterThan,
                LessThan,
                GreaterEqual,
                LessEqual,
                Include,
                Exclude,
                AND,
                OR,
                NOT,
                True,
                False,
            }

            public enum CalculateType
            {
                None,
                Plus,
                Minus,
                Multiply,
                Devide,
            }

            public enum ApplyType
            {
                None,
                Time,
                Once,
                While,
                Blend,
            }

            public enum NumberType
            {
                None,
                Value,
                Rate,
                Percentage,
            }

            public enum CoordinateType
            {
                None,
                World,
                Local,
            }

            public enum ColliderType
            {
                None,
                BoxCollider2D,
                CircleCollider2D,
                CapsuleCollider2D,
            }
        }
    }
}
