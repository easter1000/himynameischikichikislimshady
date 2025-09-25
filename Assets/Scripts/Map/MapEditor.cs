// MapEditor.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System.IO;

// TODO: 파일명 중복 확인, 빈 맵 저장 불가능

public class MapEditor : MonoBehaviour
{
    [Header("Core References")]
    public MapLoader mapLoader;
    public Button addMapButton;
    public Button saveMapButton;
    public Button cancelButton;
    public MapSelectionUI mapSelectionUI;

    [Header("Dialog References")]
    public ConfirmDialog confirmDialog;
    public InputDialog inputDialog;

    private HexMapData currentMapData;
    private PlacementGuide activeGuide = null;

    void Start()
    {
        currentMapData = ScriptableObject.CreateInstance<HexMapData>();
        cancelButton.onClick.AddListener(CancelEditing);
    }

    public void StartEditing()
    {
        currentMapData = ScriptableObject.CreateInstance<HexMapData>();
        RefreshMapDisplay();
        addMapButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(true);
        saveMapButton.GetComponentInChildren<TextMeshProUGUI>().text = "Save";

        saveMapButton.onClick.AddListener(SaveMap);
    }

    public void CancelEditing()
    {
        // 취소 확인 다이얼로그 표시
        confirmDialog.ShowDialog(
            "Cancel Edit",
            "Do you want to cancel editing? All unsaved changes will be lost.",
            () =>
            {
                // 사용자가 취소를 확인한 경우
                currentMapData = ScriptableObject.CreateInstance<HexMapData>();
                RefreshMapDisplay();
                addMapButton.gameObject.SetActive(true);
                cancelButton.gameObject.SetActive(false);
                saveMapButton.GetComponentInChildren<TextMeshProUGUI>().text = "Play";

                saveMapButton.onClick.RemoveListener(SaveMap);
            }
        );
    }

    public void SaveMap()
    {
        // 맵 이름 입력 다이얼로그 표시
        inputDialog.ShowDialog(
            "Save Map",
            "Enter map name:",
            currentMapData.mapName,
            (inputName) =>
            {
                ProcessMapSave(inputName);
            }
            // 취소시 아무것도 하지 않음
        );
    }

    private void ProcessMapSave(string mapName)
    {
        // 입력 검증
        if (string.IsNullOrEmpty(mapName.Trim()))
        {
            // 빈 이름을 입력한 경우 - 다시 입력 다이얼로그 표시
            confirmDialog.ShowDialog(
                "Error",
                "Map name is required.",
                () =>
                {
                    SaveMap(); // 다시 입력 다이얼로그 표시
                }
            );
            return;
        }

        mapName = mapName.Trim();

        // 파일명으로 사용할 수 없는 문자 검사
        if (mapName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            confirmDialog.ShowDialog(
                "Error",
                "Map name contains invalid characters.",
                () =>
                {
                    SaveMap(); // 다시 입력 다이얼로그 표시
                }
            );
            return;
        }

        // 맵 저장 실행
        currentMapData.mapName = mapName;

        // Resources 폴더에 맵 데이터를 저장 (런타임에서 사용 가능하도록)
        string resourcesPath = "Assets/Resources/Maps";
        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
        }

        // ScriptableObject를 JSON으로 저장하는 방법으로 변경 (에디터 의존성 제거)
        string fileName = mapName + ".json";
        string filePath = Path.Combine(resourcesPath, fileName);

        // HexMapData를 JSON으로 직렬화
        string jsonData = JsonUtility.ToJson(currentMapData, true);
        File.WriteAllText(filePath, jsonData);

        // 저장 완료 메시지
        confirmDialog.ShowDialog(
            "Save Complete",
            $"Map '{mapName}' has been saved successfully.",
            () =>
            {
                // 저장 후 편집 모드 종료 및 맵 리스트 새로고침
                FinishEditing();
                if (mapSelectionUI != null)
                {
                    mapSelectionUI.RefreshMapList();
                }
            },
            () =>
            {
                FinishEditing();
                if (mapSelectionUI != null)
                {
                    mapSelectionUI.RefreshMapList();
                }
            }
        );
    }

    private void FinishEditing()
    {
        currentMapData = ScriptableObject.CreateInstance<HexMapData>();
        RefreshMapDisplay();
        addMapButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(false);
        saveMapButton.GetComponentInChildren<TextMeshProUGUI>().text = "Play";

        saveMapButton.onClick.RemoveListener(SaveMap);
    }

    public void CreateTileAt(int q, int r, TileType type)
    {
        if (currentMapData.tiles.Any(t => t.q == q && t.r == r)) return;

        TileData newTile = new TileData { q = q, r = r, tileType = type };
        currentMapData.tiles.Add(newTile);

        RefreshMapDisplay();
    }

    public void SetActiveGuide(PlacementGuide guide)
    {
        if (activeGuide != null && activeGuide != guide)
        {
            activeGuide.ResetToGuideState();
        }
        activeGuide = guide;
    }

    public void DeleteTile(int q, int r)
    {
        currentMapData.tiles.RemoveAll(tile => tile.q == q && tile.r == r);
        RefreshMapDisplay();
    }

    private void RefreshMapDisplay()
    {
        // 1. 실제 타일 데이터로 리스트를 시작합니다.
        List<TileData> displayTiles = new List<TileData>(currentMapData.tiles);

        // 2. 가이드 타일의 위치를 계산합니다.
        HashSet<Vector2Int> occupiedCoords = new HashSet<Vector2Int>(currentMapData.tiles.Select(t => new Vector2Int(t.q, t.r)));
        HashSet<Vector2Int> guideCoords = new HashSet<Vector2Int>();

        if (currentMapData.tiles.Count == 0)
        {
            guideCoords.Add(Vector2Int.zero); // 타일이 없으면 (0,0)에 가이드 추가
        }
        else
        {
            foreach (var tile in currentMapData.tiles)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2Int neighbor = GetNeighborCoords(tile.q, tile.r, i);
                    if (!occupiedCoords.Contains(neighbor))
                    {
                        guideCoords.Add(neighbor);
                    }
                }
            }
        }

        // 3. 계산된 위치에 가이드 타일 '데이터'를 추가합니다.
        foreach (var coord in guideCoords)
        {
            displayTiles.Add(new TileData { q = coord.x, r = coord.y, tileType = TileType.Guide });
        }

        // 4. 가이드를 포함한 임시 맵 데이터를 생성합니다.
        HexMapData displayMapData = ScriptableObject.CreateInstance<HexMapData>();
        displayMapData.tiles = displayTiles;

        // 5. 이 임시 데이터를 MapLoader에 넘겨 화면을 그리게 합니다.
        mapLoader.LoadMap(displayMapData, true, this);
    }

    private static readonly Vector2Int[] axialDirections = {
        new Vector2Int(+1, 0), new Vector2Int(+1, -1), new Vector2Int(0, -1),
        new Vector2Int(-1, 0), new Vector2Int(-1, +1), new Vector2Int(0, +1)
    };

    private Vector2Int GetNeighborCoords(int q, int r, int direction)
    {
        Vector2Int dir = axialDirections[direction];
        return new Vector2Int(q + dir.x, r + dir.y);
    }
}