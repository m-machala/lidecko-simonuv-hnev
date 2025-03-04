using System.Collections.Generic;

public static class ObstacleData
{
    public static readonly List<UnityEngine.Vector2> Positions = new List<UnityEngine.Vector2>
    {
        // Left border
        new UnityEngine.Vector2(-1, -1),
        new UnityEngine.Vector2(-1, 0),
        new UnityEngine.Vector2(-1, 1),
        new UnityEngine.Vector2(-1, 2),
        new UnityEngine.Vector2(-1, 3),
        new UnityEngine.Vector2(-1, 4),
        new UnityEngine.Vector2(-1, 5),
        new UnityEngine.Vector2(-1, 6),
        new UnityEngine.Vector2(-1, 7),
        new UnityEngine.Vector2(-1, 8),
        new UnityEngine.Vector2(-1, 9),
        new UnityEngine.Vector2(-1, 10),
        new UnityEngine.Vector2(-1, 11),
        new UnityEngine.Vector2(-1, 12),
        new UnityEngine.Vector2(-1, 13),
        new UnityEngine.Vector2(-1, 14),
        new UnityEngine.Vector2(-1, 15),       

        // Bottom border
        new UnityEngine.Vector2(-1, -1),
        new UnityEngine.Vector2(0, -1),
        new UnityEngine.Vector2(1, -1),
        new UnityEngine.Vector2(2, -1),
        new UnityEngine.Vector2(3, -1),
        new UnityEngine.Vector2(4, -1),
        new UnityEngine.Vector2(5, -1),
        new UnityEngine.Vector2(6, -1),
        new UnityEngine.Vector2(7, -1),
        new UnityEngine.Vector2(8, -1),
        new UnityEngine.Vector2(9, -1),
        new UnityEngine.Vector2(10, -1),
        new UnityEngine.Vector2(11, -1),
        new UnityEngine.Vector2(12, -1),
        new UnityEngine.Vector2(13, -1),
        new UnityEngine.Vector2(14, -1),
        new UnityEngine.Vector2(15, -1),

        // Right border
        new UnityEngine.Vector2(15, -1),
        new UnityEngine.Vector2(15, 0),
        new UnityEngine.Vector2(15, 1),
        new UnityEngine.Vector2(15, 2),
        new UnityEngine.Vector2(15, 3),
        new UnityEngine.Vector2(15, 4),
        new UnityEngine.Vector2(15, 5),
        new UnityEngine.Vector2(15, 6),
        new UnityEngine.Vector2(15, 7),
        new UnityEngine.Vector2(15, 8),
        new UnityEngine.Vector2(15, 9),
        new UnityEngine.Vector2(15, 10),
        new UnityEngine.Vector2(15, 11),
        new UnityEngine.Vector2(15, 12),
        new UnityEngine.Vector2(15, 13),
        new UnityEngine.Vector2(15, 14),
        new UnityEngine.Vector2(15, 15),

        // Top border 
        new UnityEngine.Vector2(-1, 15),
        new UnityEngine.Vector2(0, 15),
        new UnityEngine.Vector2(1, 15),
        new UnityEngine.Vector2(2, 15),
        new UnityEngine.Vector2(3, 15),
        new UnityEngine.Vector2(4, 15),
        new UnityEngine.Vector2(5, 15),
        new UnityEngine.Vector2(6, 15),
        new UnityEngine.Vector2(7, 15),
        new UnityEngine.Vector2(8, 15),
        new UnityEngine.Vector2(9, 15),
        new UnityEngine.Vector2(10, 15),
        new UnityEngine.Vector2(11, 15),
        new UnityEngine.Vector2(12, 15),
        new UnityEngine.Vector2(13, 15),
        new UnityEngine.Vector2(14, 15),
        new UnityEngine.Vector2(15, 15),

        // Inner obstacles
        new UnityEngine.Vector2(0, 11),
        new UnityEngine.Vector2(0, 7),
        
        new UnityEngine.Vector2(1, 11),
        new UnityEngine.Vector2(1, 7),
        new UnityEngine.Vector2(1, 3),
        
        new UnityEngine.Vector2(2, 11),
        new UnityEngine.Vector2(2, 7),
        new UnityEngine.Vector2(2, 3),
        
        new UnityEngine.Vector2(3, 14),
        new UnityEngine.Vector2(3, 13),
        new UnityEngine.Vector2(3, 11),
        new UnityEngine.Vector2(3, 9),
        new UnityEngine.Vector2(3, 8),
        new UnityEngine.Vector2(3, 7),
        new UnityEngine.Vector2(3, 6),
        new UnityEngine.Vector2(3, 3),
        new UnityEngine.Vector2(3, 2),
        new UnityEngine.Vector2(3, 1),
        
        new UnityEngine.Vector2(4, 11),
        new UnityEngine.Vector2(4, 6),
        new UnityEngine.Vector2(4, 5),
        
        new UnityEngine.Vector2(5, 13),
        new UnityEngine.Vector2(5, 12),
        new UnityEngine.Vector2(5, 11),
        new UnityEngine.Vector2(5, 5),
        new UnityEngine.Vector2(5, 4),
        
        new UnityEngine.Vector2(6, 12),
        new UnityEngine.Vector2(6, 9),
        new UnityEngine.Vector2(6, 8),
        new UnityEngine.Vector2(6, 7),
        
        new UnityEngine.Vector2(7, 9),
        new UnityEngine.Vector2(7, 7),
        
        new UnityEngine.Vector2(8, 12),
        new UnityEngine.Vector2(8, 9),
        new UnityEngine.Vector2(8, 8),
        new UnityEngine.Vector2(8, 7),
        
        new UnityEngine.Vector2(9, 13),
        new UnityEngine.Vector2(9, 12),
        new UnityEngine.Vector2(9, 11),
        new UnityEngine.Vector2(9, 5),
        new UnityEngine.Vector2(9, 4),
        
        new UnityEngine.Vector2(10, 11),
        new UnityEngine.Vector2(10, 6),
        new UnityEngine.Vector2(10, 5),
        
        new UnityEngine.Vector2(11, 14),
        new UnityEngine.Vector2(11, 13),
        new UnityEngine.Vector2(11, 11),
        new UnityEngine.Vector2(11, 9),
        new UnityEngine.Vector2(11, 8),
        new UnityEngine.Vector2(11, 7),
        new UnityEngine.Vector2(11, 6),
        new UnityEngine.Vector2(11, 3),
        new UnityEngine.Vector2(11, 2),
        new UnityEngine.Vector2(11, 1),
        
        new UnityEngine.Vector2(12, 11),
        new UnityEngine.Vector2(12, 7),
        new UnityEngine.Vector2(12, 3),
        
        new UnityEngine.Vector2(13, 11),
        new UnityEngine.Vector2(13, 7),
        new UnityEngine.Vector2(13, 3),
        
        new UnityEngine.Vector2(14, 11),
        new UnityEngine.Vector2(14, 7)
    };
}