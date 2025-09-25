using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmDialog : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button confirmButton;
    public Button cancelButton;
    
    private Action onConfirm;
    private Action onCancel;
    
    void Start()
    {
        // 버튼 이벤트 설정
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }
    
    public void ShowDialog(string title, string message, Action confirmAction, Action cancelAction = null)
    {
        titleText.text = title;
        messageText.text = message;
        onConfirm = confirmAction;
        onCancel = cancelAction;
        
        gameObject.SetActive(true);
    }
    
    private void OnConfirmClicked()
    {
        onConfirm?.Invoke();
        gameObject.SetActive(false);
    }
    
    private void OnCancelClicked()
    {
        onCancel?.Invoke();
        gameObject.SetActive(false);
    }
    
    public void HideDialog()
    {
        gameObject.SetActive(false);
    }
}
