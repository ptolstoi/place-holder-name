using UnityEngine;

public enum Player
{
    PlayerNone,
    Player1,
    Player2,
    Player3,
    Player4,
    Player5,
    Player6,
    Player7,
    Player8
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
                return Color.red;
            case Player.Player2:
                return Color.blue;
            case Player.Player3:
                return Color.green;
            case Player.Player4:
                return Color.yellow;
            case Player.Player5:
                return Color.cyan;
            case Player.Player6:
                return Color.red + Color.green*0.5f;
            case Player.Player7:
                return Color.green * 0.5f + Color.blue;
            case Player.Player8:
                return Color.blue + Color.red*0.5f;
            case Player.PlayerNone:
                return Color.grey;             
            default:
                return Color.magenta;
        }
    }

}
