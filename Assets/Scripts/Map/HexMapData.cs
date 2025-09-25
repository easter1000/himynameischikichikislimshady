// HexMapData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New HexMapData", menuName = "Hex Map/Map Data")]
public class HexMapData : ScriptableObject
{
    public string mapName;

    public List<TileData> tiles = new List<TileData>();
}