using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyRebindUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private Button changeKeyButton;
    [SerializeField] private TMP_Text statusText;

    private bool isWaitingForOldKey;
    private bool isWaitingForNewKey;
    private KeyCode selectedOldKey = KeyCode.None;

    private void Awake()
    {
        if (changeKeyButton != null)
        {
            changeKeyButton.onClick.AddListener(BeginRebind);
        }

        UpdateStatus("Click Change Key to start.");
    }

    public void BeginRebind()
    {
        if (playerMove == null)
        {
            UpdateStatus("PlayerMove reference missing.");
            return;
        }

        isWaitingForOldKey = true;
        isWaitingForNewKey = false;
        selectedOldKey = KeyCode.None;
        UpdateStatus("Select a used key.");
    }

    public void OnKeyboardButtonClicked(string keyName)
    {
        if (!TryParseKeyCode(keyName, out KeyCode clickedKey))
        {
            UpdateStatus("Invalid key: " + keyName);
            Debug.LogWarning("KeyRebindUI: Invalid key name from UI: " + keyName);
            return;
        }

        Debug.Log("KeyRebindUI: Clicked key = " + clickedKey);

        if (isWaitingForOldKey)
        {
            if (!playerMove.IsKeyUsed(clickedKey))
            {
                UpdateStatus("This key is not currently used.");
                return;
            }

            selectedOldKey = clickedKey;
            isWaitingForOldKey = false;
            isWaitingForNewKey = true;
            UpdateStatus("Selected " + clickedKey + ". Now select an unused key.");
            return;
        }

        if (isWaitingForNewKey)
        {
            if (playerMove.IsKeyUsed(clickedKey))
            {
                UpdateStatus("This key is already used.");
                return;
            }

            bool success = playerMove.TryRebindKey(selectedOldKey, clickedKey);
            if (success)
            {
                UpdateStatus(selectedOldKey + " changed to " + clickedKey + ". Left=" + playerMove.MoveLeftKey + " Right=" + playerMove.MoveRightKey + " Jump=" + playerMove.JumpKey);
                Debug.Log("KeyRebindUI: Rebind success. Left=" + playerMove.MoveLeftKey + " Right=" + playerMove.MoveRightKey + " Jump=" + playerMove.JumpKey);
            }
            else
            {
                UpdateStatus("Rebind failed.");
                Debug.LogWarning("KeyRebindUI: Rebind failed from " + selectedOldKey + " to " + clickedKey);
            }

            isWaitingForNewKey = false;
            selectedOldKey = KeyCode.None;
        }
    }

    private bool TryParseKeyCode(string keyName, out KeyCode keyCode)
    {
        string normalizedKeyName = NormalizeKeyName(keyName);
        return Enum.TryParse(normalizedKeyName, true, out keyCode);
    }

    private string NormalizeKeyName(string keyName)
    {
        if (string.IsNullOrWhiteSpace(keyName))
        {
            return string.Empty;
        }

        string normalized = keyName.Trim();

        switch (normalized.ToUpperInvariant())
        {
            case " ":
            case "SPACE":
                return "Space";
            case "LEFT":
                return "LeftArrow";
            case "RIGHT":
                return "RightArrow";
            case "UP":
                return "UpArrow";
            case "DOWN":
                return "DownArrow";
            case "ESC":
                return "Escape";
            case "CTRL":
                return "LeftControl";
            case "SHIFT":
                return "LeftShift";
            case "ALT":
                return "LeftAlt";
            default:
                return normalized;
        }
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
