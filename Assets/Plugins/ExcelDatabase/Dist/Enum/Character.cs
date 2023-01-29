namespace ExcelDatabase
{
    public static partial class Em
    {
        public static class Character
        {
            public enum StateType
            {
                None,
                Idle,
                Move,
                Chase,
                Attack,
                HitReaction,
                RunAway,
                Dead,
                KnockBack,
                Sturn,
            }

            public enum MoveType
            {
                None,
                Ground,
                Air,
                Fixed,
            }

            public enum HitReactionType
            {
                None,
                HitBack,
                HitAnimation,
                HitAniHitBack,
                SuperArmor,
            }

            public enum FactionType
            {
                None,
                Player,
                Soldier,
                HigherPeople,
                Mercenary,
                Resistance,
                LowerPeople,
                Zombie,
                Robot,
            }

            public enum FactionRelationType
            {
                None,
                Enemy,
                Frighten,
                Neutral,
            }

            public enum CharacterType
            {
                None,
                Player,
                Monster,
                BossMonster,
                NPC,
                Object,
            }
        }
    }
}
