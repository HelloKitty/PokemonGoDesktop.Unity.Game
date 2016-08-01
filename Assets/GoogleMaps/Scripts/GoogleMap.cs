using UnityEngine;
using System.Collections;

public class GoogleMap : MonoBehaviour
{
	public enum MapType
	{
		RoadMap,
		Satellite,
		Terrain,
		Hybrid
	}
	public bool loadOnStart = true;
	public bool autoLocateCenter = true;
	public GoogleMapLocation centerLocation;
	public int zoom = 13;
	public MapType mapType;
	public int size = 512;
	public bool doubleResolution = false;
	public GoogleMapMarker[] markers;
	public GoogleMapPath[] paths;

	public bool shouldRefresh = true;
	
	void Start() {
		if(loadOnStart) Refresh();	
	}
	
	public void Refresh() {
		if(autoLocateCenter && (markers.Length == 0 && paths.Length == 0)) {
			Debug.LogError("Auto Center will only work if paths or markers are used.");	
		}
		StartCoroutine(_Refresh());
	}

	void Update()
	{
		if (shouldRefresh)
			StartCoroutine(_Refresh());
	}
	
	IEnumerator _Refresh ()
	{
		var url = "http://maps.googleapis.com/maps/api/staticmap";
		var qs = "";
		if (!autoLocateCenter) {
			if (centerLocation.address != "")
				qs += "center=" + WWW.UnEscapeURL(centerLocation.address);
			else {
				qs += "center=" + WWW.UnEscapeURL(string.Format("{0},{1}", centerLocation.latitude, centerLocation.longitude));
			}
		
			qs += "&zoom=" + zoom.ToString ();
		}
		qs += "&size=" + WWW.UnEscapeURL(string.Format("{0}x{0}", size));
		qs += "&scale=" + (doubleResolution ? "2" : "1");
		qs += "&maptype=" + mapType.ToString ().ToLower ();
		var usingSensor = false;
#if UNITY_IPHONE
		usingSensor = Input.location.isEnabledByUser && Input.location.status == LocationServiceStatus.Running;
#endif
		qs += "&sensor=" + (usingSensor ? "true" : "false");
		qs += @"&style=feature:all|element:labels|visibility:off";
		qs += @"&style=feature:poi|element:all|visibility:off";
		qs += @"&style=feature:landscape|element:all|visibility:off";
		qs += @"&style=feature:transit.line|element:all|visibility:off";
		qs += @"&style=feature:road|element:geometry.stroke|color:0x880000|saturation:100|visibility:simplified";
		qs += @"&style=feature:highway|element:geometry.stroke|color:0x880000|saturation:100|visibility:simplified";
		qs += @"&style=feature:road|element:geometry.fill|color:0xFF0000|saturation:100|visibility:simplified";
		qs += @"&style=feature:highway|element:geometry.fill|color:0xFF0000|saturation:100|visibility:simplified";
		qs += @"format=png32";
		


		WWW req = new WWW(url + "?" + qs);

		yield return req;
		if (req.error == null)
		{
			var tex = new Texture2D(size, size);
			req.LoadImageIntoTexture(tex);

			Destroy(GetComponent<Renderer>().material.mainTexture);
			GetComponent<Renderer>().material.mainTexture = tex;
		}

		req.Dispose();
		
		shouldRefresh = false;
	}
}

public enum GoogleMapColor
{
	black,
	brown,
	green,
	purple,
	yellow,
	blue,
	gray,
	orange,
	red,
	white
}

[System.Serializable]
public class GoogleMapLocation
{
	public string address;
	public double latitude;
	public double longitude;
}

[System.Serializable]
public class GoogleMapMarker
{
	public enum GoogleMapMarkerSize
	{
		Tiny,
		Small,
		Mid
	}
	public GoogleMapMarkerSize size;
	public GoogleMapColor color;
	public string label;
	public GoogleMapLocation[] locations;
	
}

[System.Serializable]
public class GoogleMapPath
{
	public int weight = 5;
	public GoogleMapColor color;
	public bool fill = false;
	public GoogleMapColor fillColor;
	public GoogleMapLocation[] locations;	
}