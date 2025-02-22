namespace EnumCenter
{
    public enum RoomType
    {
        BasicRoom=0,
        BirthRoom=1,
        EnemyRoom = 2,
        BossRoom=3,
        TeleportRoom=4,
        TreasureRoom=5,
        ShopRoom=6,
        Corridor=7,
    }

    public enum CameraType
    {
        StaticCamera=0,
        SelectCamera=1,
        FollowCamera=2,
    }

    public enum PlayerType
    {
        Knight=1,
    }

    public enum PlayerWeaponType
    {
        BadPistol=1,
        AK47=2,
        DoubleBladeSword=3,
    }
    
    public enum InteractiveObjectType
    {
        Weapon=1,
    }
    
    public enum GameModeType
    {
        SingleMode=1<<0,
        MultipleMode=1<<1,
    }
}
