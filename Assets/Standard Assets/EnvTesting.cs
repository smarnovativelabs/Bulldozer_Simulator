using System.Collections.Generic;
using UnityEngine;
public class EnvTesting : MonoBehaviour
{
    public GameObject testPanel;
    public Material daySkybox;
    public Material nightSkybox;
    public Color dayFogColor;
    public Color nightFogColor;

    public Texture2D[] dayTimeDir;
    public Texture2D[] dayTimecolor;

    public Texture2D[] nightTimeDir;
    public Texture2D[] nightTimecolor;

    LightmapData[] dayTimeMaps;
    LightmapData[] nightTimeMaps;


    // Start is called before the first frame update
    void Start()
    {
        GetLightMaps();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ApplyDayTime();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ApplyNightTime();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            SetPanel();
        }

    }
    void GetLightMaps()
    {
        List<LightmapData> _dLMaps = new List<LightmapData>();
        for(int i = 0; i < dayTimeDir.Length; i++)
        {
            LightmapData _map = new LightmapData();
            _map.lightmapDir = dayTimeDir[i];
            _map.lightmapColor = dayTimecolor[i];
            _dLMaps.Add(_map);
        }
        dayTimeMaps = _dLMaps.ToArray();

        List<LightmapData> _nLMaps = new List<LightmapData>();
        for (int i = 0; i < nightTimeDir.Length; i++)
        {
            LightmapData _map = new LightmapData();
            _map.lightmapDir = nightTimeDir[i];
            _map.lightmapColor = nightTimecolor[i];
            _nLMaps.Add(_map);
        }
        nightTimeMaps = _nLMaps.ToArray();


    }
    void ApplyDayTime()
    {
        LightmapSettings.lightmaps = dayTimeMaps;
        RenderSettings.skybox = daySkybox;
        RenderSettings.fogColor = dayFogColor;


    }
    void ApplyNightTime()
    {
        LightmapSettings.lightmaps = nightTimeMaps;
        RenderSettings.skybox = nightSkybox;
        RenderSettings.fogColor = nightFogColor;
    }
    void SetPanel()
    {
        testPanel.GetComponent<RectTransform>().offsetMax= new Vector2 (0,0f);
        testPanel.GetComponent<RectTransform>().offsetMax = testPanel.GetComponent<RectTransform>().offsetMax-new Vector2(0, 80f);

    }
}
