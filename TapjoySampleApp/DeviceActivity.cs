using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Provider;
using Android.Telephony;
using Android.Util;

namespace Com.Tapjoy.TapjoySampleApp
{
	[Activity (Label = "DeviceActivity")]			
	public class DeviceActivity : Activity
	{

		String info = "";
		private static string TAG = "DeviceActivity";

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.activity_device);

			TextView textView = (TextView)FindViewById (Resource.Id.DeviceInfo);
			info += "My Device Info: \n";
			info += "Tapjoy Device ID: " + TapjoyConnectCore.DeviceID + "\n";
			info += "IMEI/MEID: " + getIMEIorMEID() + "\n";
			info += "Android Serial: " + getAndroidSerial() + "\n";
			info += "ANDROID_ID: " + getAndroidID () + "\n";
			info += "Manufacturer: " + Android.OS.Build.Manufacturer + "\n";
			info += "Device Model: " + Android.OS.Build.Model + "\n";
			info += "Android Version: " + Android.OS.Build.VERSION.Release + "\n";
			info += "MAC Address: " + TapjoyConnectCore.MacAddress + "\n";

			textView.Text = info;

			Button copyButton = (Button)FindViewById (Resource.Id.CopyButton);

			copyButton.Click += (sender, e) => {
				ClipboardManager clipboardManager = (ClipboardManager)GetSystemService (Context.ClipboardService); 
				clipboardManager.Text = info;
				
				if (clipboardManager.HasText) {
					Toast.MakeText (ApplicationContext, clipboardManager.Text + " copied to the clipboard!", ToastLength.Long).Show ();
				}
			};
		}

		public String getIMEIorMEID ()
		{
			String deviceIMEIorMEID = "";
		
			try {
				TelephonyManager telephonyManager = (TelephonyManager)this.GetSystemService (Context.TelephonyService);
		
				if (telephonyManager != null) {
					deviceIMEIorMEID = telephonyManager.DeviceId;
				}
			} catch (Exception e) {
				Log.Error (TAG, "e: " + e.ToString ());
			}
		
			return deviceIMEIorMEID;
		}

		public String getAndroidSerial ()
		{
			String androidSerial = "";

			try {
				// Is there no IMEI or MEID?
				// Is this at least Android 2.3+?
				// Then let's get the serial.
				if (int.Parse (Android.OS.Build.VERSION.Sdk) >= 9) {
					// THIS CLASS IS ONLY LOADED FOR ANDROID 2.3+
					androidSerial = getSerial ();
				}
			} catch (Exception e) {
				Log.Error (TAG, "e: " + e.ToString ());
			}

			return androidSerial;
		}

		public String getAndroidID ()
		{
			// ANDROID_ID
			return Android.Provider.Settings.Secure.GetString (this.ContentResolver, Settings.Secure.AndroidId);
		}

		public String getSerial ()
		{
			String serial = null;
			try {
				serial = Android.OS.Build.Serial;
				TapjoyLog.I (TAG, "serial: " + serial);
			} catch (Exception e) {
				TapjoyLog.E (TAG, e.ToString ());
			}

			return serial;
		}
	}
}

