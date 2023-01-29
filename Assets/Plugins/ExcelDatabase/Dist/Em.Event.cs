namespace ExcelDatabase
{
    public static partial class Em
    {
        public static class Event
        {
            public enum ParamType
            {
                None,
                Character_ID,
                Character_Type,
                Item_ID,
                Item_Type,
                Skill_ID,
                Skill_Type,
                Object_ID,
                Object_Type,
                Level_Type,
                Shop_ID,
                Shop_Type,
                TriggerCollider_ID,
                TriggerCollTypeer_Type,
                Event_ID,
                Event_Type,
            }

            public enum EventType
            {
                None,
                Character_Kill,
                Character_Interact_Success,
                Character_Interact_Fail,
                Item_Use,
                Item_Acquire,
                Item_Equip,
                Item_UnEquip,
                Skill_Start,
                Skill_End,
                Object_Interact_Success,
                Object_Interact_Fail,
                Object_Destroy,
                Object_StateChange,
                Player_LevelUp,
                Shop_Purchase_Success,
                Shop_Purchase_Fail,
                TriggerCollider_Enter,
                TriggerCollider_Exit,
                Event_Success,
                Event_Fail,
            }
        }
    }
}
