using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditableTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("자식으로 있는 삭제 버튼 오브젝트를 연결")]
    public GameObject deleteButtonObject;

    private int q;
    private int r;
    private MapEditor mapEditor;
    private Button deleteButton;
    private bool isEditing = false;
    private TileType tileType;

    public void Setup(int q, int r, MapEditor editor, bool isEditing, TileType tileType)
    {
        this.q = q;
        this.r = r;
        this.mapEditor = editor;
        this.isEditing = isEditing;
        this.tileType = tileType;
        GetComponent<Image>().color = tileType == TileType.Empty ? Color.gray : tileType == TileType.Grass ? Color.green : Color.blue;

        if (deleteButtonObject != null)
        {
            deleteButton = deleteButtonObject.GetComponent<Button>();
            deleteButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
            deleteButton.onClick.AddListener(OnDelete);
            deleteButtonObject.SetActive(false); // 초기에는 숨김
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (deleteButtonObject != null && isEditing)
        {
            deleteButtonObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (deleteButtonObject != null && isEditing)
        {
            deleteButtonObject.SetActive(false);
        }
    }

    private void OnDelete()
    {
        if (isEditing)
        {
            mapEditor.DeleteTile(q, r);
        }
    }
}