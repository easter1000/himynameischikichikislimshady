using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlacementGuide : MonoBehaviour
{
    public Button guideButton;

    public Button emptyButton;
    public Button grassButton;
    public Button waterButton;

    private int q;
    private int r;
    private MapEditor mapEditor;

    // Setup 함수를 통해 필요한 모든 정보를 받아옵니다.
    public void Setup(int q, int r, MapEditor editor)
    {
        this.q = q;
        this.r = r;
        this.mapEditor = editor;

        emptyButton.onClick.AddListener(() =>
        {
            mapEditor.CreateTileAt(this.q, this.r, TileType.Empty);
        });
        grassButton.onClick.AddListener(() =>
        {
            mapEditor.CreateTileAt(this.q, this.r, TileType.Grass);
        });
        waterButton.onClick.AddListener(() =>
        {
            mapEditor.CreateTileAt(this.q, this.r, TileType.Water);
        });
        emptyButton.gameObject.SetActive(false);
        grassButton.gameObject.SetActive(false);
        waterButton.gameObject.SetActive(false);
        guideButton.gameObject.SetActive(true);

        guideButton.onClick.AddListener(OnGuideClick);
    }

    // 가이드 버튼을 클릭했을 때 호출
    private void OnGuideClick()
    {
        // 선택 컨테이너를 활성화
        emptyButton.gameObject.SetActive(true);
        grassButton.gameObject.SetActive(true);
        waterButton.gameObject.SetActive(true);

        guideButton.gameObject.SetActive(false);

        // MapEditor에 현재 이 가이드가 활성화되었음을 알림
        mapEditor.SetActiveGuide(this);
    }
    
    public void ResetToGuideState()
    {
        emptyButton.gameObject.SetActive(false);
        grassButton.gameObject.SetActive(false);
        waterButton.gameObject.SetActive(false);
        guideButton.gameObject.SetActive(true);
    }
}