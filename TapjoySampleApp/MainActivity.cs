using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Util;
using Android.Widget;
using System;
using Java.Util;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

namespace Com.Tapjoy.TapjoySampleApp
{
	using TapjoyConnect = Com.Tapjoy.TapjoyConnect;
	using TapjoyConnectFlag = Com.Tapjoy.TapjoyConnectFlag;
	using TapjoyConnectNotifier = Com.Tapjoy.ITapjoyConnectNotifier;
	using TapjoyLog = Com.Tapjoy.TapjoyLog;

	[Activity (Label = "TapjoySampleApp", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : BaseActivity, View.IOnClickListener, ITapjoyConnectNotifier
	{
		private Dictionary<string, Dictionary<string, object>> appInfoMap;

		private TextView appIdField;
		private TextView secretKeyField;
		private TextView currencyIdField;
		private CheckBox managedCheckbox;
		private List<string> appIdList;
		private List<string> currencyIdList;

		private Button connect;
		private Button selectAppButton;
		private Button selectCurrencyButton;
		private Button createAppButton;

		private string appId;
		private string secretKey;

		private Button buttonToReenable;

		private TapjoyConnectNotifier connectNotifier;

		private const string TAG = "MainActivity";

		public MainActivity ()
		{
			connectNotifier = this;	
			appInfoMap = new Dictionary<string, Dictionary<string, object>> ();
		}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.activity_main);

			debugTextView = (EditText)FindViewById (Resource.Id.debug_output_field);

			// Try to load previously saved apps
			loadAppInfo ();
			if (appInfoMap.Count == 0) {
				// If no saved apps found, initialize default available apps
				appInfoMap = new Dictionary<string, Dictionary<string, object>> ();
				Dictionary<string, object> managedAppMap = new Dictionary<string, object> ();
				managedAppMap [TapjoySampleConstants.SECRET_KEY] = TapjoySampleConstants.SECRET_KEY_MANAGED;
				managedAppMap [TapjoySampleConstants.MANAGED] = bool.TrueString;
				managedAppMap [TapjoySampleConstants.CURRENCY_IDS] = new List<string> (TapjoySampleConstants.CURRENCY_IDS_MANAGED);

				Dictionary<string, object> nonmanagedAppMap = new Dictionary<string, object> ();
				nonmanagedAppMap [TapjoySampleConstants.SECRET_KEY] = TapjoySampleConstants.SECRET_KEY_NONMANAGED;
				nonmanagedAppMap [TapjoySampleConstants.MANAGED] = bool.FalseString;
				nonmanagedAppMap [TapjoySampleConstants.CURRENCY_IDS] = new List<string> (TapjoySampleConstants.CURRENCY_IDS_NONMANAGED);

				appInfoMap [TapjoySampleConstants.APP_ID_NONMANAGED] = nonmanagedAppMap;
				appInfoMap [TapjoySampleConstants.APP_ID_MANAGED] = managedAppMap;
			}
				
			appIdList = new List<string> (appInfoMap.Keys.ToList ());

			// Set up default example
			appId = appIdList [0];
			appIdField = (TextView)FindViewById (Resource.Id.app_id_field);
			appIdField.Text = appId;

			Dictionary<string, object> defaultAppInfo = (Dictionary<string, object>)appInfoMap [appId];
			secretKey = (string)defaultAppInfo [TapjoySampleConstants.SECRET_KEY];

			secretKeyField = (TextView)FindViewById (Resource.Id.secret_key_field);
			secretKeyField.Text = secretKey;

			managedCheckbox = (CheckBox)FindViewById (Resource.Id.managed_currency_checkbox);
			managedCheckbox.Checked = bool.Parse ((string)defaultAppInfo [TapjoySampleConstants.MANAGED]);

			currencyIdField = (TextView)FindViewById (Resource.Id.currency_id_field);
			currencyIdList = new List<string> ((List<string>)defaultAppInfo [TapjoySampleConstants.CURRENCY_IDS]); //copy currencyIds to new list
			currencyIdField.Text = currencyIdList [0];

			// Set up buttons
			connect = (Button)FindViewById (Resource.Id.ConnectButton);
			connect.SetOnClickListener (this);

			selectAppButton = (Button)FindViewById (Resource.Id.SelectAppButton);
			selectAppButton.SetOnClickListener (this);

			selectCurrencyButton = (Button)FindViewById (Resource.Id.SelectCurrencyButton);
			selectCurrencyButton.SetOnClickListener (this);
			// if managed currency, disable option to select currency id
			if (isAppManaged ()) {
				selectCurrencyButton.Enabled = false;
			}

			createAppButton = (Button)FindViewById (Resource.Id.CreateAppButton);
			createAppButton.SetOnClickListener (this);

		}

		public virtual void OnClick (View v)
		{
			if (v is Button) {
				Button button = ((Button)v);
				int id = button.Id;

				if (id == Resource.Id.ConnectButton) {
					DisableButton (button);

					// OPTIONAL: For custom startup flags.
					Hashtable connectFlags = new Hashtable ();
					connectFlags.Put (TapjoyConnectFlag.EnableLogging, "true");

					// If you are not using Tapjoy Managed currency, you would set your own user ID here.
					//	connectFlags.put(TapjoyConnectFlag.USER_ID, "A_UNIQUE_USER_ID");

					// You can also set your event segmentation parameters here.
					//  Hashtable<String, String> segmentationParams = new Hashtable<String, String>();
					//  segmentationParams.put("iap", "true");
					//  connectFlags.put(TapjoyConnectFlag.SEGMENTATION_PARAMS, segmentationParams);

					TapjoyConnect.RequestTapjoyConnect (ApplicationContext, appId, secretKey, connectFlags, connectNotifier);
				} else if (id == Resource.Id.SelectAppButton) {
					AlertDialog.Builder builder = new AlertDialog.Builder (this);
					builder.SetTitle ("Select App ID");

					EventHandler<DialogClickEventArgs> handler = (s, e) => {
						appId = appIdList [e.Which];
						appIdField.Text = appId;
						Dictionary<string, object> selectedAppMap = appInfoMap [appId];
						secretKey = (string)selectedAppMap [TapjoySampleConstants.SECRET_KEY];
						secretKeyField.Text = secretKey;
						
						managedCheckbox.Checked = bool.Parse ((string)selectedAppMap [TapjoySampleConstants.MANAGED]);
						// if managed currency, disable option to select currency id
						selectCurrencyButton.Enabled = !isAppManaged ();
						
						currencyIdList.Clear ();
						currencyIdList.AddRange ((List<string>)selectedAppMap [TapjoySampleConstants.CURRENCY_IDS]);
						currencyIdField.Text = currencyIdList [0];
					};
					builder.SetItems (appIdList.ToArray (), handler);
					builder.Show ();
				} else if (id == Resource.Id.SelectCurrencyButton) {
					AlertDialog.Builder builder = new AlertDialog.Builder (this);
					builder.SetTitle ("Select Currency ID");
					EventHandler<DialogClickEventArgs> handler = (s, e) => {
						currencyIdField.Text = currencyIdList [e.Which];
					};
					builder.SetItems (currencyIdList.ToArray (), handler);
					builder.Show ();
				} else if (id == Resource.Id.CreateAppButton) {
					AlertDialog.Builder builder = new AlertDialog.Builder (this);
					builder.SetTitle ("Create New App");
					LayoutInflater inflater = this.LayoutInflater;
					View dialogView = inflater.Inflate (Resource.Layout.setup_layout, null);

					EventHandler<DialogClickEventArgs> positiveHandler = (s, e) => {
						// get new app info 
						EditText addAppIdField = (EditText)dialogView.FindViewById (Resource.Id.add_app_id_field);
						EditText addSecretKeyField = (EditText)dialogView.FindViewById (Resource.Id.add_secret_key_field);
						
						CheckBox addManaged = (CheckBox)dialogView.FindViewById (Resource.Id.add_managed_currency_checkbox);
						appId = addAppIdField.Text.ToString ();
						secretKey = addSecretKeyField.Text.ToString ();
						
						// Check for non-empty appID.
						if (appId.Length == 0) {
							Toast.MakeText (this, "Could not add app -- App ID is empty", ToastLength.Long).Show ();
							return;
						}
						
						// Check for non-empty secretKey.
						if (secretKey.Length == 0) {
							Toast.MakeText (this, "Could not add app -- Secret Key is empty", ToastLength.Long).Show ();
							return;
						}
						
						// add new app to available apps list
						appIdList.Add (appId);
						Dictionary<string, object> newAppMap = new Dictionary<string, object> ();
						newAppMap [TapjoySampleConstants.SECRET_KEY] = secretKey;
						newAppMap [TapjoySampleConstants.MANAGED] = addManaged.Checked;
						
						// if non-managed, get the list of entered currency ids
						List<string> currencyIds = new List<string> ();
						if (addManaged.Checked) {
							currencyIds.Add (appId);
						} else {
							EditText addCurrencyIdField = (EditText)dialogView.FindViewById (Resource.Id.add_currency_id_field);
							string currencyIdText = addCurrencyIdField.Text.ToString ();
							currencyIds.AddRange (currencyIdText.ToString ().Split (new string[] { "\\s*,\\s*" }, StringSplitOptions.None));
						}
						newAppMap [TapjoySampleConstants.CURRENCY_IDS] = currencyIds;
						
						appInfoMap [appId] = newAppMap;
						
						saveAppInfo ();
						
						// update main page UI with new app info
						appIdField.Text = appId;
						secretKeyField.Text = secretKey;
						managedCheckbox.Checked = addManaged.Checked;
						selectCurrencyButton.Enabled = !addManaged.Checked;
						currencyIdList.Clear ();
						currencyIdList.AddRange (currencyIds);
						currencyIdField.Text = currencyIdList [0];
						
						mHandler.Post (mUpdateResults);
					};

					EventHandler<DialogClickEventArgs> negativeHandler = (s, e) => {
						return;
					};
					builder.SetView (dialogView)
						.SetPositiveButton ("Done", positiveHandler)
						.SetNegativeButton ("Cancel", negativeHandler);
					builder.Show ();
				} else {
					log (((Button)v).Text);
				}
			}
		}

		private void DisableButton (Button button)
		{
			this.buttonToReenable = button;
			button.Enabled = false;
		}

		protected internal virtual void UpdateResultsInUi ()
		{
			// Back in the UI thread -- update our UI elements based on the data in mResults
			if (debugTextView != null) {
				// Update the display text.
				if (update_text) {
					log (displayText);
					update_text = false;
				}
			}

			if (buttonToReenable != null) {
				buttonToReenable.Enabled = true;
				buttonToReenable = null;
			}
		}

		// Callback Methods

		public void ConnectFail ()
		{
			update_text = true;
			displayText = "Connect Failed";
			mHandler.Post (mUpdateResults);
		}

		public void ConnectSuccess ()
		{
			// Pass info about selected app to the next Activity
			Intent intent = new Intent (ApplicationContext, typeof(FeaturesActivity));
			intent.PutExtra (TapjoySampleConstants.APP_ID, appId);
			intent.PutExtra (TapjoySampleConstants.MANAGED, isAppManaged ());
			intent.PutExtra (TapjoySampleConstants.ACTIVE_CURRENCY_ID, getActiveCurrency ());
			StartActivity (intent);
			mHandler.Post (mUpdateResults);
		}


		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			log (item.TitleFormatted.ToString ());
			Intent intent = new Intent (ApplicationContext, typeof(DeviceActivity));
			StartActivity (intent);
			return true;
		}

		// helper function to determine if active app has managed/non-managed currency
		public virtual bool isAppManaged ()
		{
			return managedCheckbox.Checked;
		}

		// helper function to retrieve active currency id
		public virtual string getActiveCurrency ()
		{
			return currencyIdField.Text.ToString ();
		}

		// Save all the app info to SharedPreferences
		public virtual void saveAppInfo ()
		{
			var prefs = GetSharedPreferences (TapjoySampleConstants.TAPJOY_SAMPLE_PREFS, 0);
			var editor = prefs.Edit ();

			try {
				editor.PutString (TapjoySampleConstants.TAPJOY_PREFS_KEY, SerializeObject (appInfoMap));
				editor.Commit ();
			} catch (Exception e) {
				Log.Error (TAG, "e: " + e.ToString ());
			}

		}

		// Load saved app info from SharedPreferences
		public virtual void loadAppInfo ()
		{
			var prefs = GetSharedPreferences (TapjoySampleConstants.TAPJOY_SAMPLE_PREFS, 0);

			try {
				string settings = prefs.GetString (TapjoySampleConstants.TAPJOY_PREFS_KEY, SerializeObject (new Dictionary<string, Dictionary<string, object>> ()));
				appInfoMap = (Dictionary<string, Dictionary<string, object>>)DeserializeObject<Dictionary<string, Dictionary<string, object>>> (settings);
			} catch (Exception e) {
				Log.Error (TAG, "e: " + e.ToString ());
			}
		}

		//		public virtual string Serialize (ISerializable obj)
		//		{
		//			if (obj == null) {
		//				return "";
		//			}
		//
		//			try {
		//				ByteArrayOutputStream serialObj = new ByteArrayOutputStream ();
		//				using (ObjectOutputStream objStream = new ObjectOutputStream ((Java.IO.BufferedOutputStream)serialObj)) {
		//					objStream.WriteObject ((Java.Lang.Object)obj);
		//					objStream.Close ();
		//				}
		//				return Base64.EncodeToString (serialObj.ToByteArray (), 0);
		//
		//			} catch (Exception e) {
		//				Log.Error (TAG, "e: " + e.ToString ());
		//			}
		//
		//			return "";
		//		}
		//
		//		public virtual object Deserialize (string str)
		//		{
		//			if (string.IsNullOrEmpty (str))
		//				return null;
		//
		//			try {
		//				ByteArrayInputStream serialObj = new ByteArrayInputStream (Base64.Decode (str, 0));
		//				using (ObjectInputStream objStream = new ObjectInputStream (serialObj)) {
		//					return objStream.ReadObject ();
		//				}
		//			} catch (Exception e) {
		//				Log.Error (TAG, "e: " + e.ToString ());
		//			}
		//
		//			return null;
		//		}

		public static object DeserializeObject<T> (string toDeserialize)
		{
			//var xs = new System.Runtime.Serialization (typeof (T));
			XmlSerializer xmlSerializer = new XmlSerializer (typeof(T));
			StringReader textReader = new StringReader (toDeserialize);
			var des =  xmlSerializer.Deserialize (textReader);
			return des;
		}

		public static string SerializeObject<T> (T toSerialize)
		{
			XmlSerializer xmlSerializer = new XmlSerializer (typeof(T));
			StringWriter textWriter = new System.IO.StringWriter ();
			xmlSerializer.Serialize (textWriter, toSerialize);
			var text = textWriter.ToString ();
			return text;
		}
	}
}



