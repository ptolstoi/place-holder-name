using UnityEngine;

public enum Player
{
    PlayerNone,
    Player1,
    Player2,
    Player3,
    Player4
}

public static class ColorPalette {
    
    public static Color GetColor(int i)
    {
        return GetColor((Player)i);
    }

    public static Color GetColor(this Player player)
    {
        switch (player)
        {
            case Player.Player1:
                return Utils.ToColor(240, 0.5f, 0.9f, 1);
            case Player.Player2:
                return Utils.ToColor(40, 1, 1, 1);
            case Player.Player3:
                return Utils.ToColor(80, 1, 1, 1);
            case Player.Player4:
                return Utils.ToColor(-40, 1, 1,1);
            case Player.PlayerNone:
                return Color.grey;             
            default:
                return Color.magenta;
        }
    }

}
