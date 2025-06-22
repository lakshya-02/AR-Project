using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class DebugARObjectRotator : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Dropdown objectDropdown;
    public Button rotateLeftButton;
    public Button rotateRightButton;
    public Button refreshButton;
    public TMP_Text debugText;
    
    [Header("Settings")]
    public string objectTag = "Player";
    public float rotationStep = 15f;
    
    private List<GameObject> detectedObjects = new List<GameObject>();
    private GameObject selectedObject;
    
    void Start()
    {
        Debug.Log("DebugARObjectRotator started");
        
        // Setup button listeners
        if (rotateLeftButton != null)
            rotateLeftButton.onClick.AddListener(() => RotateSelected(-rotationStep));
        
        if (rotateRightButton != null)
            rotateRightButton.onClick.AddListener(() => RotateSelected(rotationStep));
        
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshObjects);
        
        if (objectDropdown != null)
            objectDropdown.onValueChanged.AddListener(OnDropdownChanged);
        
        // Initial refresh
        RefreshObjects();
        
        // Auto refresh every 2 seconds for testing
        InvokeRepeating("RefreshObjects", 2f, 2f);
    }
    
    public void RefreshObjects()
    {
        Debug.Log("=== Refreshing Objects ===");
        
        // Clear previous list
        detectedObjects.Clear();
        
        // Find all objects with ARObject tag
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(objectTag);
        Debug.Log($"Found {foundObjects.Length} objects with tag '{objectTag}'");
        
        foreach (GameObject obj in foundObjects)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                detectedObjects.Add(obj);
                Debug.Log($"Added object: {obj.name} at position {obj.transform.position}");
            }
        }
        
        // Sort by name for consistent ordering
        detectedObjects = detectedObjects.OrderBy(obj => obj.name).ToList();
        
        UpdateDropdown();
        UpdateDebugText();
        
        Debug.Log($"Total objects in list: {detectedObjects.Count}");
    }
    
    void UpdateDropdown()
    {
        if (objectDropdown == null)
        {
            Debug.LogWarning("Dropdown is null!");
            return;
        }
        
        objectDropdown.ClearOptions();
        
        if (detectedObjects.Count == 0)
        {
            objectDropdown.AddOptions(new List<string> { "No objects found" });
            objectDropdown.interactable = false;
            Debug.Log("No objects found - dropdown disabled");
            return;
        }
        
        List<string> options = new List<string>();
        for (int i = 0; i < detectedObjects.Count; i++)
        {
            string objectName = detectedObjects[i].name;
            options.Add($"{i + 1}. {objectName}");
        }
        
        objectDropdown.AddOptions(options);
        objectDropdown.interactable = true;
        
        // Auto-select first object
        if (detectedObjects.Count > 0)
        {
            selectedObject = detectedObjects[0];
            objectDropdown.SetValueWithoutNotify(0);
        }
        
        Debug.Log($"Dropdown updated with {options.Count} options");
    }
    
    void OnDropdownChanged(int index)
    {
        Debug.Log($"Dropdown changed to index: {index}");
        
        if (index >= 0 && index < detectedObjects.Count)
        {
            selectedObject = detectedObjects[index];
            Debug.Log($"Selected object: {selectedObject.name}");
        }
        else
        {
            selectedObject = null;
            Debug.Log("No valid object selected");
        }
        
        UpdateDebugText();
    }
    
    void RotateSelected(float angle)
    {
        if (selectedObject == null)
        {
            Debug.LogWarning("No object selected for rotation");
            return;
        }
        
        Vector3 oldRotation = selectedObject.transform.eulerAngles;
        selectedObject.transform.Rotate(0, angle, 0, Space.Self);
        Vector3 newRotation = selectedObject.transform.eulerAngles;
        
        Debug.Log($"Rotated {selectedObject.name} by {angle}° - Old Y: {oldRotation.y:F1}°, New Y: {newRotation.y:F1}°");
        
        UpdateDebugText();
    }
    
    void UpdateDebugText()
    {
        if (debugText == null) return;
        
        string debugInfo = $"Tag: '{objectTag}'\n";
        debugInfo += $"Objects Found: {detectedObjects.Count}\n";
        
        if (selectedObject != null)
        {
            debugInfo += $"Selected: {selectedObject.name}\n";
            debugInfo += $"Position: {selectedObject.transform.position}\n";
            debugInfo += $"Rotation Y: {selectedObject.transform.eulerAngles.y:F1}°\n";
        }
        else
        {
            debugInfo += "Selected: None\n";
        }
        
        debugInfo += "\nObjects List:\n";
        for (int i = 0; i < detectedObjects.Count; i++)
        {
            debugInfo += $"{i + 1}. {detectedObjects[i].name}\n";
        }
        
        debugText.text = debugInfo;
    }
    
    // Manual test functions you can call from inspector
    [ContextMenu("Test Find Objects")]
    public void TestFindObjects()
    {
        RefreshObjects();
    }
    
    [ContextMenu("List All GameObjects")]
    public void ListAllGameObjects()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        Debug.Log($"=== All GameObjects in scene ({allObjects.Length}) ===");
        
        foreach (GameObject obj in allObjects)
        {
            Debug.Log($"Name: {obj.name}, Tag: {obj.tag}, Active: {obj.activeInHierarchy}");
        }
    }
    
    [ContextMenu("Test Rotation")]
    public void TestRotation()
    {
        if (selectedObject != null)
        {
            RotateSelected(45f);
        }
        else
        {
            Debug.Log("No object selected for test rotation");
        }
    }
}