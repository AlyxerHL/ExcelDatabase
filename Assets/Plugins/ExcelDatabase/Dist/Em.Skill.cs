namespace ExcelDatabase
{
    public static partial class Em
    {
        public static class Skill
        {
            public enum StepType
            {
                None,
                Ready,
                Wait,
                Action,
                Recover,
            }

            public enum SkillType
            {
                None,
                Melee,
                Projectile,
                Guard,
                Move,
                Passive,
                Active,
                CC,
                Interact,
                Common,
            }

            public enum LoopTimeType
            {
                None,
                AnimationEnd,
                StepEnd,
                Time,
                HoldEnd,
            }
        }
    }
}
