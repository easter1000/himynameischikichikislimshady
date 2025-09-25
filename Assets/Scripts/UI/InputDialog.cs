using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InputDialog : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public TMP_InputField inputField;
    public Button confirmButton;
    public Button cancelButton;
    
    private Action<string> onConfirm;
    private Action onCancel;
    
    void Start()
    {
        // 버튼 이벤트 설정
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        
        // Enter 키로 확인 버튼 동작
        inputField.onSubmit.AddListener(OnInputSubmit);
    }
    
    public void ShowDialog(string title, string message, string defaultValue = "", Action<string> confirmAction = null, Action cancelAction = null)
    {
        titleText.text = title;
        messageText.text = message;
        inputField.text = defaultValue;
        onConfirm = confirmAction;
        onCancel = cancelAction;
        
        gameObject.SetActive(true);
        
        // 입력 필드에 포커스 설정
        inputField.Select();
        inputField.ActivateInputField();
    }
    
    private void OnInputSubmit(string value)
    {
        OnConfirmClicked();
    }
    
    private void OnConfirmClicked()
    {
        string inputValue = inputField.text;
        onConfirm?.Invoke(inputValue);
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
