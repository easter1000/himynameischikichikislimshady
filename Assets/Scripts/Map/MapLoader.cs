// MapLoader.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapLoader : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject tilePrefab; 
    public GameObject guidePrefab;
    
    [Header("References")]
    public Transform mapContainer; // 생성된 타일들을 담을 부모 오브젝트

    [Header("Tile Settings")]
    [Tooltip("컨테이너가 아무리 커도 타일이 이 값(반지름)보다 커지지 않습니다.")]
    public float maxTileSize = 150f;
    [Tooltip("타일 사이의 여백 비율입니다. 0 = 여백 없음, 0.1 = 10% 여백")]
    [Range(0f, 1f)]
    public float tilePadding = 0.05f;
    private float tileSize; // 동적으로 계산된 타일 크기
    public void LoadMap(HexMapData mapData, bool isEditing = false, MapEditor editor = null)
    {
        ClearMap();

        if (mapData == null)
        {
            Debug.LogError("MapData is null!");
            return;
        }

        if (tilePrefab == null || guidePrefab == null)
        {
            Debug.LogError("Tile Prefab or Guide Prefab is not assigned in the MapLoader!");
            return;
        }

        if (mapData.tiles.Count == 0 && !isEditing) // 에디터 모드가 아닐 때만 경고
        {
            Debug.LogWarning("MapData has no tiles!");
            return;
        }

        CalculateMapBoundsAndTileSize(mapData.tiles);
        Vector2 gridOffset = CalculateGridOffset(mapData.tiles);

        float sqrt3 = Mathf.Sqrt(3);
        float spacingSize = tileSize * (1.0f + tilePadding);

        foreach (var tileData in mapData.tiles)
        {
            GameObject prefabToUse;
            if (tileData.tileType == TileType.Guide)
            {
                prefabToUse = guidePrefab;
            }
            else
            {
                prefabToUse = tilePrefab;
            }

            GameObject instance = Instantiate(prefabToUse, mapContainer);
            instance.name = $"{tileData.tileType}_{tileData.q}_{tileData.r}";

            if (tileData.tileType == TileType.Guide)
            {
                PlacementGuide guideScript = instance.GetComponent<PlacementGuide>();
                if (guideScript != null)
                {
                    guideScript.Setup(tileData.q, tileData.r, editor);
                }
            }
            else
            {
                EditableTile editableTile = instance.GetComponent<EditableTile>();
                if (editableTile != null)
                {
                    editableTile.Setup(tileData.q, tileData.r, editor, isEditing, tileData.tileType);
                }
            }

            RectTransform tileRect = instance.GetComponent<RectTransform>();
            Vector2 position = AxialToPixel(tileData.q, tileData.r, spacingSize);
            tileRect.anchoredPosition = position - gridOffset;
            
            float tileVisualWidth = tileSize * sqrt3;
            float tileVisualHeight = tileSize * 2f;
            tileRect.sizeDelta = new Vector2(tileVisualWidth, tileVisualHeight);
        }
    }

    public void ClearMap()
    {
        foreach (Transform child in mapContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public Vector2 AxialToPixel(int q, int r, float size)
    {
        float sqrt3 = Mathf.Sqrt(3);
        float x = size * (sqrt3 * q + sqrt3 / 2 * r);
        float y = size * (3f / 2f * r);
        return new Vector2(x, y);
    }
    
    public void CalculateMapBoundsAndTileSize(List<TileData> tiles)
    {
        if (mapContainer == null) return;
        if (tiles.Count == 0) // 타일이 없으면 기본 크기로 설정하거나 0으로 처리
        {
            tileSize = Mathf.Min(maxTileSize, 50f); // 혹은 다른 기본값
            return;
        }
        
        Rect containerRect = (mapContainer as RectTransform).rect;
        if (containerRect.width <= 0 || containerRect.height <= 0)
        {
            Debug.LogError("MapContainer has zero or negative size.");
            tileSize = 0;
            return;
        }
        
        List<Vector2> pixelPositions = tiles.Select(tile => AxialToPixel(tile.q, tile.r, 1f)).ToList();

        float minX = pixelPositions.Min(p => p.x);
        float maxX = pixelPositions.Max(p => p.x);
        float minY = pixelPositions.Min(p => p.y);
        float maxY = pixelPositions.Max(p => p.y);

        float sqrt3 = Mathf.Sqrt(3);
        float gridWidthInUnits = (maxX - minX) + sqrt3;
        float gridHeightInUnits = (maxY - minY) + 2f;
        
        if (gridWidthInUnits <= 0 || gridHeightInUnits <= 0)
        {
            tileSize = 0;
            return;
        }

        float spacingFactor = 1.0f + tilePadding;
        float widthRatio = containerRect.width / (gridWidthInUnits * spacingFactor);
        float heightRatio = containerRect.height / (gridHeightInUnits * spacingFactor);

        float calculatedSize = Mathf.Min(widthRatio, heightRatio);
        tileSize = Mathf.Min(calculatedSize, maxTileSize);
    }
    
    public Vector2 CalculateGridOffset(List<TileData> tiles)
    {
        if (tileSize == 0 || tiles.Count == 0) return Vector2.zero;

        float spacingSize = tileSize * (1.0f + tilePadding);
        List<Vector2> pixelPositions = tiles.Select(tile => AxialToPixel(tile.q, tile.r, spacingSize)).ToList();

        Vector2 totalPosition = Vector2.zero;
        foreach (var pos in pixelPositions)
        {
            totalPosition += pos;
        }

        return totalPosition / tiles.Count;
    }
}