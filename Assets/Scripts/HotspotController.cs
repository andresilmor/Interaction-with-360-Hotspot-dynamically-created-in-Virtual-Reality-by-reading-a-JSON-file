using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class HotspotController : MonoBehaviour
{
    [Header("Hotspot Initialization Config:")]
    [Tooltip("JSON file location with hotspot informations and map.")]
    [SerializeField] string filePath = "Assets/HotspotData.json";
    [Tooltip("All materials with the 360º images used in this hotspot.")]
    [SerializeField] List<Material> scenaryMaterials;
    [SerializeField] GameObject canvasMapping;
    [SerializeField] GameObject mappingButton;
    [SerializeField] GameObject canvasInfo;

    [SerializeField] string typeTextID = "text";
    [SerializeField] string typeImageID = "image";
    [SerializeField] string typeVideoID = "video";
    [SerializeField] string typeTransitionID = "transition";

    private string jsonText;
    private GameObject hotspotScenary;
    private MeshRenderer scenaryMeshRenderer;
    private Hotspot hotspot;

    private Camera camera;

    private byte actualIndex = 0;

    void Start() {

        camera = FindObjectOfType<Camera>();

        InitializeHotspot();

    }

    private void InitializeHotspot() {
        StreamReader reader = new StreamReader(filePath);
        jsonText = reader.ReadToEnd();
        reader.Close();

        if (jsonText.Trim().Length == 0) {
            Debug.LogError("JSON file is empty.");
            return;

        }

        hotspot = JsonUtility.FromJson<Hotspot>(jsonText);

        CreateHotspotScenary();

        canvasMapping = Instantiate(canvasMapping, gameObject.transform);
        canvasMapping.GetComponent<Canvas>().worldCamera = camera;

        scenaryMeshRenderer = hotspotScenary.GetComponent<MeshRenderer>();
        scenaryMeshRenderer.material = FindMaterial(hotspot.images[actualIndex].materialName);

        gameObject.name = "Hotspot Controller [" + hotspot.name + "]";

        MapImage();

    }

    private void CreateHotspotScenary() {
        hotspotScenary = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hotspotScenary.name = "Hotspot Scenary";
        hotspotScenary.transform.parent = gameObject.transform;
        hotspotScenary.transform.localScale = new Vector3(hotspot.scaleX, hotspot.scaleY, hotspot.scaleZ);

    }

    private void MapImage() {
        byte count = 0;

        foreach (Mapping map in hotspot.images[actualIndex].mapping) {
            GameObject newMappingButton = Instantiate(mappingButton, canvasMapping.transform);
            newMappingButton.transform.position = new Vector3(map.positionX, map.positionY, map.positionZ);

            newMappingButton.name = newMappingButton.name + " | " + count;

            newMappingButton.transform.LookAt(camera.transform, Vector3.one);

            if (map.type.Equals(typeTextID))
                InstantiateCanvasInfo(ref count, map, newMappingButton);

        }
    }

    private byte InstantiateCanvasInfo(ref byte count, Mapping map, GameObject newMappingButton) {
        GameObject newCanvasInfo = Instantiate(canvasInfo.gameObject, newMappingButton.transform);

        newCanvasInfo.name = newCanvasInfo.name + " | " + count++;

        newMappingButton.GetComponent<ObjectInformation>().SetInformation(map.information);
        newMappingButton.GetComponent<ObjectInformation>().SetInformationDisplay(newCanvasInfo.GetComponent<InformationDisplay>());
        return count;
    }

    private Material FindMaterial(string materialName) {
        foreach (Material mat in scenaryMaterials) {
            if (mat.name.Contains(materialName)) {
                return mat;

            }
        }
        return null;

    }
}
