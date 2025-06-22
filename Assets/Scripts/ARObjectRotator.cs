using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

public class ARObjectRotator : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Dropdown objectDropdown;
    public Button rotateLeftButton;
    public Button rotateRightButton;
    public Button refreshButton;
    public Slider rotationSlider;
    public TMP_Text selectedObjectInfo;
    
    [Header("Rotation Settings")]
    public float rotationStep = 15f;
    public bool useWorldSpace = false;
    public bool smoothRotation = true;
    public float rotationSpeed = 2f;
    
    [Header("Object Detection")]
    public string objectTag = "ARObject";
    public float refreshInterval = 1f;
    public bool autoRefresh = true;
    
    private List<GameObject> detectedObjects = new List<GameObject>();
    private GameObject selectedObject;
    private bool isRotating = false;
    private Quaternion targetRotation;
    private float originalRotationY;
    
    // Visual feedback
    private Material originalMaterial;
    private Color originalColor;
    
    void Start()
    {
        InitializeComponents();
        SetupEventListeners();
        RefreshDetectedObjects();
        
        if (autoRefresh)
        {
            StartCoroutine(AutoRefreshCoroutine());
        }
    }
    
    void InitializeComponents()
    {
        // Initialize rotation slider if present
        if (rotationSlider != null)
        {
            rotationSlider.minValue = 0f;
            rotationSlider.maxValue = 360f;
            rotationSlider.wholeNumbers = true;
        }
        
        // Initialize info text
        UpdateSelectedObjectInfo();
    }
    
    void SetupEventListeners()
    {
        objectDropdown.onValueChanged.AddListener(OnDropdownChanged);
        rotateLeftButton.onClick.AddListener(() => RotateSelected(-rotationStep));
        rotateRightButton.onClick.AddListener(() => RotateSelected(rotationStep));
        
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshDetectedObjects);
        }
        
        if (rotationSlider != null)
        {
            rotationSlider.onValueChanged.AddListener(OnSliderRotationChanged);
        }
    }
    
    void Update()
    {
        // Handle smooth rotation
        if (smoothRotation && isRotating && selectedObject != null)
        {
            selectedObject.transform.rotation = Quaternion.Lerp(
                selectedObject.transform.rotation, 
                targetRotation, 
                Time.deltaTime * rotationSpeed
            );
            
            // Check if rotation is complete
            if (Quaternion.Angle(selectedObject.transform.rotation, targetRotation) < 0.1f)
            {
                selectedObject.transform.rotation = targetRotation;
                isRotating = false;
            }
        }
        
        // Update slider value to match current rotation
        if (rotationSlider != null && selectedObject != null && !isRotating)
        {
            float currentY = selectedObject.transform.eulerAngles.y;
            if (Mathf.Abs(rotationSlider.value - currentY) > 1f)
            {
                rotationSlider.SetValueWithoutNotify(currentY);
            }
        }
    }
    
    void RefreshDetectedObjects()
    {
        List<GameObject> newDetectedObjects = new List<GameObject>();
        
        // Search for objects with ARObject tag only
        try
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(objectTag);
            foreach (GameObject obj in objectsWithTag)
            {
                if (obj != null && obj.activeInHierarchy && !newDetectedObjects.Contains(obj))
                {
                    // Additional checks for AR objects
                    if (IsValidARObject(obj))
                    {
                        newDetectedObjects.Add(obj);
                    }
                }
            }
        }
        catch (UnityException e)
        {
            Debug.LogWarning($"Tag '{objectTag}' not found: {e.Message}");
        }
        
        // Sort by distance from camera for better UX
        if (Camera.main != null)
        {
            newDetectedObjects = newDetectedObjects
                .OrderBy(obj => Vector3.Distance(Camera.main.transform.position, obj.transform.position))
                .ToList();
        }
        
        detectedObjects = newDetectedObjects;
        UpdateDropdown();
        
        // Reselect object if it still exists
        if (selectedObject != null && !detectedObjects.Contains(selectedObject))
        {
            selectedObject = null;
        }
        
        // Select first object if none selected
        if (selectedObject == null && detectedObjects.Count > 0)
        {
            SelectObject(0);
        }
        
        UpdateSelectedObjectInfo();
        Debug.Log($"Refreshed: Found {detectedObjects.Count} AR objects with tag '{objectTag}'");
    }
    
    bool IsValidARObject(GameObject obj)
    {
        // Check if object has renderer and is visible
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && !renderer.isVisible)
        {
            return false;
        }
        
        // Check if object is too far from camera (optional)
        if (Camera.main != null)
        {
            float distance = Vector3.Distance(Camera.main.transform.position, obj.transform.position);
            if (distance > 50f) // Adjust max distance as needed
            {
                return false;
            }
        }
        
        // Check if object has required components (optional)
        // You can add more specific checks here
        
        return true;
    }
    
    void UpdateDropdown()
    {
        objectDropdown.ClearOptions();
        
        if (detectedObjects.Count == 0)
        {
            objectDropdown.AddOptions(new List<string> { "No objects found" });
            objectDropdown.interactable = false;
            return;
        }
        
        objectDropdown.interactable = true;
        List<string> options = new List<string>();
        
        for (int i = 0; i < detectedObjects.Count; i++)
        {
            GameObject obj = detectedObjects[i];
            string displayName = obj.name;
            
            // Add distance info for better identification
            if (Camera.main != null)
            {
                float distance = Vector3.Distance(Camera.main.transform.position, obj.transform.position);
                displayName += $" ({distance:F1}m)";
            }
            
            options.Add(displayName);
        }
        
        objectDropdown.AddOptions(options);
    }
    
    void OnDropdownChanged(int index)
    {
        SelectObject(index);
    }
    
    void SelectObject(int index)
    {
        // Remove highlight from previous object
        if (selectedObject != null)
        {
            RemoveHighlight(selectedObject);
        }
        
        if (index >= 0 && index < detectedObjects.Count)
        {
            selectedObject = detectedObjects[index];
            objectDropdown.SetValueWithoutNotify(index);
            
            // Add highlight to new selected object
            AddHighlight(selectedObject);
            
            // Update slider to match current rotation
            if (rotationSlider != null)
            {
                rotationSlider.SetValueWithoutNotify(selectedObject.transform.eulerAngles.y);
            }
            
            originalRotationY = selectedObject.transform.eulerAngles.y;
        }
        else
        {
            selectedObject = null;
        }
        
        UpdateSelectedObjectInfo();
    }
    
    void RotateSelected(float yAngle)
    {
        if (selectedObject == null) return;
        
        if (smoothRotation)
        {
            // Smooth rotation
            Vector3 currentEuler = selectedObject.transform.eulerAngles;
            Vector3 targetEuler = currentEuler + new Vector3(0f, yAngle, 0f);
            
            if (useWorldSpace)
            {
                targetRotation = Quaternion.Euler(targetEuler);
            }
            else
            {
                targetRotation = selectedObject.transform.rotation * Quaternion.Euler(0f, yAngle, 0f);
            }
            
            isRotating = true;
        }
        else
        {
            // Instant rotation
            if (useWorldSpace)
            {
                selectedObject.transform.Rotate(0f, yAngle, 0f, Space.World);
            }
            else
            {
                selectedObject.transform.Rotate(0f, yAngle, 0f, Space.Self);
            }
        }
        
        UpdateSelectedObjectInfo();
        Debug.Log($"Rotated {selectedObject.name} by {yAngle} degrees");
    }
    
    void OnSliderRotationChanged(float value)
    {
        if (selectedObject == null || isRotating) return;
        
        Vector3 currentEuler = selectedObject.transform.eulerAngles;
        Vector3 targetEuler = new Vector3(currentEuler.x, value, currentEuler.z);
        
        if (smoothRotation)
        {
            targetRotation = Quaternion.Euler(targetEuler);
            isRotating = true;
        }
        else
        {
            selectedObject.transform.rotation = Quaternion.Euler(targetEuler);
        }
        
        UpdateSelectedObjectInfo();
    }
    
    void AddHighlight(GameObject obj)
    {
        if (obj == null) return;
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Store original material/color
            originalMaterial = renderer.material;
            originalColor = renderer.material.color;
            
            // Apply highlight
            renderer.material.color = Color.yellow;
            
            // Optional: Add outline effect
            Outline outline = obj.GetComponent<Outline>();
            if (outline == null)
            {
                outline = obj.AddComponent<Outline>();
            }
            outline.effectColor = Color.cyan;
            outline.effectDistance = new Vector2(5f, 5f);
        }
    }
    
    void RemoveHighlight(GameObject obj)
    {
        if (obj == null) return;
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && originalMaterial != null)
        {
            renderer.material.color = originalColor;
        }
        
        // Remove outline
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            DestroyImmediate(outline);
        }
    }
    
    void UpdateSelectedObjectInfo()
    {
        if (selectedObjectInfo == null) return;
        
        if (selectedObject == null)
        {
            selectedObjectInfo.text = $"Objects found: {detectedObjects.Count}\nNo object selected";
        }
        else
        {
            Vector3 rotation = selectedObject.transform.eulerAngles;
            Vector3 position = selectedObject.transform.position;
            
            selectedObjectInfo.text = $"Selected: {selectedObject.name}\n" +
                                     $"Position: ({position.x:F1}, {position.y:F1}, {position.z:F1})\n" +
                                     $"Rotation Y: {rotation.y:F1}°\n" +
                                     $"Objects found: {detectedObjects.Count}";
        }
    }
    
    IEnumerator AutoRefreshCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshInterval);
            if (autoRefresh)
            {
                RefreshDetectedObjects();
            }
        }
    }
    
    // Public methods for external control
    public void SetRotationStep(float step)
    {
        rotationStep = step;
    }
    
    public void SetSelectedObject(GameObject obj)
    {
        if (detectedObjects.Contains(obj))
        {
            int index = detectedObjects.IndexOf(obj);
            SelectObject(index);
        }
    }
    
    public GameObject GetSelectedObject()
    {
        return selectedObject;
    }
    
    public List<GameObject> GetDetectedObjects()
    {
        return new List<GameObject>(detectedObjects);
    }
    
    void OnDestroy()
    {
        // Clean up highlights
        if (selectedObject != null)
        {
            RemoveHighlight(selectedObject);
        }
    }
}