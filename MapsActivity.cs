using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Locations;
using Android.Widget;
using Java.Lang;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Exception = System.Exception;
using Location = Android.Locations.Location;

namespace CarsDatabase
{
	[Activity(Label = "MapsActivity")]
	public class MapsActivity : Activity, IOnMapReadyCallback, ILocationListener
	{
		private GoogleMap _eMap; //Map
		private Button _recenterButton;
		private Button _routeButton;
		private EditText _destinationPointEditText;
		private LatLng _myCurrentLatLng; //Latitude and longitude of our position

		private LatLng _mapCenterLatLng; //CDN's center

		//private LatLng _latlngService; //variable that contain the latitude and longitude of the service
		private Marker _myCurrentPositionMarker; //marker of our position
		private Marker _destinationMarker;
		private LocationManager _locationManager;
		string _locationProvider = string.Empty;

		private string ApiKey = "AIzaSyC6cFXGWrJjL_Q2M5-IRNx4tHDDzlvMbjc";

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);

			// Create your application here
			SetContentView(Resource.Layout.maps_view);

			//Set up the map
			SetUpMap();

			//Inizialize the location 
			InitializeLocationManager();

			//recenter button
			_recenterButton = FindViewById<Button>(Resource.Id.recenter);
			_routeButton = FindViewById<Button>(Resource.Id.route);
			_destinationPointEditText = FindViewById<EditText>(Resource.Id.DestinationPointName);

			_recenterButton.Click += RecenterButtonClick;
			_routeButton.Click += RouteButtonClick;
		}


		public override void OnBackPressed()
		{
			_eMap.Clear();
			this.Finish();
		}

		void InitializeLocationManager()
		{
			// initialise the location manager 
			_locationManager = (LocationManager) GetSystemService(LocationService);
			// define its Criteria
			Criteria criteriaForLocationService = new Criteria
			{
				Accuracy = Accuracy.Coarse,
				PowerRequirement = Power.Medium
			};
			// find a location provider (GPS, wi-fi, etc.)
			IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);
			// if we have any, use the first one
			if (acceptableLocationProviders.Any())
				_locationProvider = acceptableLocationProviders.First();
			else
				_locationProvider = string.Empty;
		}

		public void OnLocationChanged(Location location)
		{
			//if location has changed
			if (location == null) return;
			
			var latitudine = location.Latitude;
			var longitudine = location.Longitude;
			if (!(latitudine > 0) || !(longitudine > 0)) return;
			//if the map already exists
			if (_eMap == null) return;
			_myCurrentLatLng = new LatLng(latitudine, longitudine);
			//If already exists a marker, delete it
			if (_myCurrentPositionMarker != null)
			{
				_myCurrentPositionMarker.Remove();
				_myCurrentPositionMarker = null;
			}

			_myCurrentPositionMarker = _eMap.AddMarker(new MarkerOptions().SetPosition(_myCurrentLatLng)
				.SetTitle("My location")
				.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueCyan)));
		}

		protected override void OnResume()
		{
			base.OnResume();
			if (_locationProvider != string.Empty)
				_locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
		}

		protected override void OnPause()
		{
			base.OnPause();
			_locationManager.RemoveUpdates(this);
		}

		private void SetUpMap()
		{
			if (_eMap == null)
			{
				FragmentManager.FindFragmentById<MapFragment>(Resource.Id.map).GetMapAsync(this);
			}
		}

		public void OnMapReady(GoogleMap googleMap)
		{
			_eMap = googleMap;

			//CDN's center
			_mapCenterLatLng = new LatLng(50.484001, 30.636866);

			//Set the firt view of the CDN
			CameraPosition INIT = new CameraPosition.Builder()
				.Target(_mapCenterLatLng)
				.Zoom(13.2F)
				.Bearing(82F)
				.Build();

			_eMap.AnimateCamera(CameraUpdateFactory.NewCameraPosition(INIT));
		}

		public void OnStatusChanged(string provider, Availability status, Bundle extras)
		{
		}

		public void OnProviderDisabled(string provider)
		{
		}

		public void OnProviderEnabled(string provider)
		{
		}

		private async void BuildPath(string destinationAddress)
		{
			try
			{
				_eMap.Clear();
				_myCurrentPositionMarker = _eMap.AddMarker(new MarkerOptions().SetPosition(_myCurrentLatLng)
					.SetTitle("My location")
					.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueCyan)));

				var destinationLocations = await Geocoding.GetLocationsAsync(destinationAddress);
				var destinationLocation = destinationLocations?.FirstOrDefault();

				var pointCoordinates = new LatLng(destinationLocation.Latitude, destinationLocation.Longitude);
				_destinationMarker = _eMap.AddMarker(new MarkerOptions().SetPosition(pointCoordinates)
					.SetTitle("Destination point")
					.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed)));

				var strGoogleDirectionUrlConstraction =
					"https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={2},{3}&key={4}";
				var strGoogleDirectionUrl = string.Format(strGoogleDirectionUrlConstraction,
					_myCurrentLatLng.Latitude, _myCurrentLatLng.Longitude,
					destinationLocation.Latitude, destinationLocation.Longitude, ApiKey);

				var strJSONDirectionResponse = await FnHttpRequest(strGoogleDirectionUrl);

				FnUpdateCameraPosition(pointCoordinates);
				FnSetDirectionQuery(strJSONDirectionResponse);
			}
			catch (Exception ex)
			{
				Toast.MakeText(ApplicationContext, ex.Message, ToastLength.Long)
					?.Show();
			}
		}

		private void RecenterButtonClick(object sender, EventArgs e)
		{
			var camera = CameraUpdateFactory.NewLatLngZoom(_mapCenterLatLng, 13.2F);
			_eMap.MoveCamera(camera);
		}

		private void RouteButtonClick(object sender, EventArgs e)
		{
			var destinationPointName = _destinationPointEditText.Text;
			BuildPath(destinationPointName);
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
			Android.Content.PM.Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		private void FnSetDirectionQuery(string strJSONDirectionResponse)
		{
			var objRoutes = JsonConvert.DeserializeObject<GoogleDirectionClass>(strJSONDirectionResponse);

			if (objRoutes.routes.Count <= 0) return;
			var encodedPoints = objRoutes.routes[0].overview_polyline.points;

			var lstDecodedPoints = FnDecodePolylinePoints(encodedPoints);
			//convert list of location point to array of LatLng type
			var latLngPoints = new LatLng[lstDecodedPoints.Count];
			var index = 0;
			foreach (var loc in lstDecodedPoints)
			{
				latLngPoints[index++] = loc;
			}

			var polylineOptions = new PolylineOptions();
			polylineOptions.InvokeColor(Android.Graphics.Color.Green);
			polylineOptions.Geodesic(true);
			polylineOptions.Add(latLngPoints);
			RunOnUiThread(() =>
				_eMap.AddPolyline(polylineOptions));
		}

		private static List<LatLng> FnDecodePolylinePoints(string encodedPoints)
		{
			if (string.IsNullOrEmpty(encodedPoints))
				return null;
			var poly = new List<LatLng>();
			var polylineCharArray = encodedPoints.ToCharArray();
			var index = 0;

			var currentLat = 0;
			var currentLng = 0;


			while (index < polylineCharArray.Length)
			{
				// calculate next latitude
				var sum = 0;
				var shifter = 0;
				int next5bits;
				do
				{
					next5bits = (int) polylineCharArray[index++] - 63;
					sum |= (next5bits & 31) << shifter;
					shifter += 5;
				} while (next5bits >= 32 && index < polylineCharArray.Length);

				if (index >= polylineCharArray.Length)
					break;

				currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

				//calculate next longitude
				sum = 0;
				shifter = 0;
				do
				{
					next5bits = (int) polylineCharArray[index++] - 63;
					sum |= (next5bits & 31) << shifter;
					shifter += 5;
				} while (next5bits >= 32 && index < polylineCharArray.Length);

				if (index >= polylineCharArray.Length && next5bits >= 32)
					break;

				currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

				var lat = Convert.ToDouble(currentLat) / 100000.0;
				var lng = Convert.ToDouble(currentLng) / 100000.0;
				LatLng p = new LatLng(lat, lng);
				poly.Add(p);
			}

			return poly;
		}

		private void FnUpdateCameraPosition(LatLng pos)
		{
			try
			{
				var builder = CameraPosition.InvokeBuilder();
				builder.Target(pos);
				builder.Zoom(12);
				builder.Bearing(45);
				builder.Tilt(10);
				CameraPosition cameraPosition = builder.Build();
				CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
				_eMap.AnimateCamera(cameraUpdate);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
		
		private async Task<string> FnHttpRequest(string strUri)
		{
			var webclient = new WebClient();
			string strResultData;
			try
			{
				strResultData = await webclient.DownloadStringTaskAsync(new Uri(strUri));
				Console.WriteLine(strResultData);
			}
			catch
			{
				strResultData = "Exeption!";
			}
			finally
			{
				webclient.Dispose();
			}

			return strResultData;
		}
	}
}