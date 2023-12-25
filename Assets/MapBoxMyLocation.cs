using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using TMPro;
using Mapbox.Utils;

public class MapBoxMyLocation : MonoBehaviour
{
    public TMP_Text resultShow;
    public GameObject myLocationObj;
    public Slider slider;
    public string accessToken;
    public float centerLat = -33.8873f;
    public float centerLong = 151.2189f;
    public float zoom = 12f;
    public int bearing = 0;
    public int pitch = 0;

    public enum mapStyle { Light, Dark, Streets, Outdoors, Satellite, SatelliteStreets };

    public mapStyle mapType = mapStyle.Streets;

    public enum resolution { low = 1, high = 2 };
    public resolution mapResolution = resolution.low;

    public int mapWidth = 800;
    public int mapHeight = 600;

    string[] styleStr = new string[] { "light-v10", "dark-v10", "streets-v11", "outdoors-v11", "satellite-v9", "satellite-streets-11" };
    public string url = "";
    bool mapIsLoading = false;
    Rect rect;
    bool updateMap = true;

    string accessTokenLast;
    float centerLatLast = -33.8873f;
    float centerLongLast = 151.2189f;
    float zoomLast = 12f;
    int bearingLast = 0;
    int pitchLast = 0;
    mapStyle mapTypeLast = mapStyle.Streets;
    resolution mapResolutionLast = resolution.low;
    public bool isLocationServiceEnable;

    // check Distanc
    public float targetRadius = 4f;
    public Transform targetTransform;
    private Vector2 targetLocation;
    float startLat;
    float startLong;
    public Vector2d startPosition;
    public Vector2d curuntPosition;

    // VR Functions
    public GameObject XROrign;
    public GameObject ARSession;
    public GameObject Canvas, NormalCam;
    public GameObject ARBtn;
    public GameObject Player;

    private void Awake()
    {
        ARBtn.SetActive(false);
    }
    public void ZoomMap()
    {
        zoom = slider.value;
    }
    // Start is called before the first frame update
    void Start()
    {

        myLocationObj.SetActive(false);
        slider.value = zoom;

        //if (Application.isEditor)
        //    StartCoroutine(GetMapBox());
        ////else
        StartCoroutine(GetMyDeviceLocation());
        rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
        mapWidth = (int)Math.Round(rect.width);
        mapHeight = (int)Math.Round(rect.height);

        //InvokeRepeating("GetLiveLocationEveryFiveMin", 8, 4);
    }


    // Update is called once per frame
    void Update()
    {

        UpdateLocation();
        // get map as i move
        if (updateMap && (accessTokenLast != accessToken || !Mathf.Approximately(centerLatLast, centerLat) || !Mathf.Approximately(centerLongLast, centerLong) || zoomLast != zoom ||
           bearingLast != bearing || pitchLast != pitch || mapTypeLast != mapType || mapResolutionLast != mapResolution))
        {
            rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
            mapWidth = (int)Math.Round(rect.width);
            mapHeight = (int)Math.Round(rect.height);
            StartCoroutine(GetMapBox());
            updateMap = false;

        }

    }


    IEnumerator GetMapBox()
    {
        url = "https://api.mapbox.com/styles/v1/mapbox/" + styleStr[(int)mapType] + "/static/" + centerLong + "," + centerLat + "," + zoom + "," + bearing + "," + pitch + "/" + mapWidth + "x" +
            mapHeight + "?" + "access_token=" + accessToken;
        mapIsLoading = true;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("WWW Error" + www.error);

        }
        else
        {
            mapIsLoading = false;
            gameObject.GetComponent<RawImage>().texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            accessTokenLast = accessToken;
            centerLatLast = centerLat;
            centerLongLast = centerLong;
            zoomLast = zoom;
            bearingLast = bearing;
            pitchLast = pitch;
            mapTypeLast = mapType;
            mapResolutionLast = mapResolution;
            updateMap = true;
            myLocationObj.SetActive(true);

            // Start checking distance
            CheckDistance();
        }
    }

    void UpdateLocation()

    { 
        //Input.location.Start();
        //centerLat = Input.location.lastData.latitude;
        //centerLong = Input.location.lastData.longitude;
        //Update my loaction
        StartCoroutine(GetMyLiveLocation());

    }

    IEnumerator GetMyLiveLocation()
    {
        // Request location permissions
        if (!Input.location.isEnabledByUser)
        {
            myLocationObj.SetActive(false);
            isLocationServiceEnable = Input.location.isEnabledByUser;

            Debug.LogError("Location service is not enabled by the user.");
            resultShow.text = "Location service is not enabled by the user.";
            yield break;
        }

        // Start location service updates
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            myLocationObj.SetActive(false);
            Debug.LogError("Unable to initialize location services.");
            resultShow.text = "Unable to initialize location services.";
            yield break;
        }
        isLocationServiceEnable = Input.location.isEnabledByUser;

        // Access the current location data
        centerLat = Input.location.lastData.latitude;
        centerLong = Input.location.lastData.longitude;
        // Stop location service updates
        //Input.location.Stop();
    }

    IEnumerator GetMyDeviceLocation()
    {
        // Request location permissions
        if (!Input.location.isEnabledByUser)
        {
            myLocationObj.SetActive(false);
            isLocationServiceEnable = Input.location.isEnabledByUser;

            Debug.LogError("Location service is not enabled by the user.");
            resultShow.text = "Location service is not enabled by the user.";
            yield break;
        }

        // Start location service updates
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            myLocationObj.SetActive(false);
            Debug.LogError("Unable to initialize location services.");
            resultShow.text = "Unable to initialize location services.";
            yield break;
        }
        isLocationServiceEnable = Input.location.isEnabledByUser;

        // Access the current location data
        centerLat = Input.location.lastData.latitude;
        centerLong = Input.location.lastData.longitude;
        startLat = centerLat;
        startLong = centerLong;

        startPosition = new Vector2d(startLat, startLong);

        Debug.Log("Latitude: " + centerLat + ", Longitude: " + centerLong);
        resultShow.text = "Latitude: " + centerLat + ", Longitude: " + centerLong;
        // get my currunt location
        StartCoroutine(GetMapBox());
        OpenMap();
        // Stop location service updates
        //Input.location.Stop();
    }

    void CheckDistance()
    {
        curuntPosition = new Vector2d(centerLat, centerLong);
        float distance = CalculateHaversineDistance(startPosition, curuntPosition);

        if (distance > 4f)
        {
            resultShow.text = "you have moved: " + distance + " meters";
            // Open VR Camara and place Objct
            ARBtn.SetActive(true);

        }
        else
            ARBtn.SetActive(false);
        Debug.Log("Distance between start and end positions: " + distance + " meters");
    }

    float CalculateHaversineDistance(Vector2d start, Vector2d end)
    {
        const double EarthRadius = 6371000; // Earth radius in meters

        double lat1 = start.x * Mathf.Deg2Rad;
        double lon1 = start.y * Mathf.Deg2Rad;
        double lat2 = end.x * Mathf.Deg2Rad;
        double lon2 = end.y * Mathf.Deg2Rad;

        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;

        double a = Mathf.Pow(Mathf.Sin((float)(dLat / 2)), 2) +
                   Mathf.Cos((float)lat1) * Mathf.Cos((float)lat2) *
                   Mathf.Pow(Mathf.Sin((float)(dLon / 2)), 2);

        double c = 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1 - a)));

        float distance = (float)(EarthRadius * c);

        return distance;
    }


    public void OpenAR()
    {
        XROrign.SetActive(true);
        ARSession.SetActive(true);
        //Player.SetActive(true);
        Canvas.SetActive(false);
        NormalCam.SetActive(false);

    }

    public void OpenMap()
    {
        XROrign.SetActive(false);
        ARSession.SetActive(false);
        Canvas.SetActive(true);
        NormalCam.SetActive(true);

    }

}
