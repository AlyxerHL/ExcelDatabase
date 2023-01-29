namespace ExcelDatabase
{
    public static partial class Em
    {
        public static class Target
        {
            public enum TargetSpecificType
            {
                None,
                Player,
                Self,
                MainTarget,
                MySpawnPoint,
                MySocket,
                FilterWithCollider,
                FilterWithRecognizeTargetList,
            }

            public enum TargetRelationType
            {
                None,
                Self,
                Enemy,
                Frighten,
                Neutral,
                ALL,
            }

            public enum PriorityType
            {
                None,
                Near,
                Far,
                LowCurHp,
                HighCurHP,
                LowMaxHp,
                HighMaxHp,
                LowAtk,
                HighAtk,
                FactionRelation,
                Random,
            }
        }
    }
}
