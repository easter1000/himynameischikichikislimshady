// MapSelectionUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MapSelectionUI : MonoBehaviour
{
    public MapLoader mapLoader; // 맵 로더 참조
    public GameObject buttonPrefab; // 맵 선택 버튼 프리팹
    public Transform buttonContainer; // 버튼들이 생성될 부모 UI 오브젝트 (e.g., Vertical Layout Group이 있는 Panel)

    private HexMapData[] allMaps;

    void Start()
    {
        RefreshMapList();
    }

    public void RefreshMapList()
    {
        // 기존 버튼들을 모두 제거
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // JSON 파일들을 읽어서 HexMapData로 변환
        string mapsPath = Path.Combine(Application.dataPath, "Resources/Maps");
        if (Directory.Exists(mapsPath))
        {
            string[] jsonFiles = Directory.GetFiles(mapsPath, "*.json");
            System.Collections.Generic.List<HexMapData> mapList = new System.Collections.Generic.List<HexMapData>();
            
            foreach (string filePath in jsonFiles)
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    HexMapData mapData = ScriptableObject.CreateInstance<HexMapData>();
                    JsonUtility.FromJsonOverwrite(jsonContent, mapData);
                    mapList.Add(mapData);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"맵 파일 로드 실패: {filePath}, 오류: {e.Message}");
                }
            }
            
            allMaps = mapList.ToArray();
        }
        else
        {
            allMaps = new HexMapData[0];
        }

        // 불러온 맵 데이터마다 버튼을 생성합니다.
        foreach (var mapData in allMaps)
        {
            GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
            
            // 버튼의 높이 설정
            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(buttonRect.sizeDelta.x, 50f); // 높이를 50으로 설정
            
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = mapData.mapName;

            HexMapData currentMap = mapData;
            buttonGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                mapLoader.LoadMap(currentMap, false, null);
            });
        }
    }
}