
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
using Android.Util;

namespace Com.Tapjoy.TapjoySampleApp
{

	using TapjoyAwardPointsNotifier = Com.Tapjoy.ITapjoyAwardPointsNotifier;
	using TapjoyConnect = Com.Tapjoy.TapjoyConnect;
	using TapjoyEarnedPointsNotifier = Com.Tapjoy.ITapjoyEarnedPointsNotifier;
	using TapjoyNotifier = Com.Tapjoy.ITapjoyNotifier;
	using TapjoySpendPointsNotifier = Com.Tapjoy.ITapjoySpendPointsNotifier;

	[Activity (Label = "VirtualCurrencyActivity")]	
	public class VirtualCurrencyActivity : BaseActivity, View.IOnClickListener, TapjoyNotifier, TapjoySpendPointsNotifier, TapjoyAwardPointsNotifier, TapjoyEarnedPointsNotifier
	{
		internal bool earnedPoints = false;
		internal int earnedAmount = 0;

		private const string TAG = "VirtualCurrencyActivity";

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.activity_virtual_currency);

			debugTextView = (EditText)FindViewById (Resource.Id.debug_output_field_currency);

			// Set up button click listeners
			Button @get = (Button)FindViewById (Resource.Id.GetCurrencyButton);
			@get.SetOnClickListener (this);

			Button award = (Button)FindViewById (Resource.Id.AwardCurrencyButton);
			award.SetOnClickListener (this);

			Button spend = (Button)FindViewById (Resource.Id.SpendCurrencyButton);
			spend.SetOnClickListener (this);

			// Set our earned points notifier to this class.
			TapjoyConnect.TapjoyConnectInstance.SetEarnedPointsNotifier (this);
		}

		public void OnClick (View v)
		{
			if (v is Button) {
				int id = ((Button)v).Id;

				if (id == Resource.Id.GetCurrencyButton) {
					// Retrieve the virtual currency amount from the server.
					TapjoyConnect.TapjoyConnectInstance.GetTapPoints (this);
				} else if (id == Resource.Id.AwardCurrencyButton) {
					// Award virtual currency.
					TapjoyConnect.TapjoyConnectInstance.AwardTapPoints (10, this);
				} else if (id == Resource.Id.SpendCurrencyButton) {
					// Spend virtual currency.
					TapjoyConnect.TapjoyConnectInstance.SpendTapPoints (25, this);
				}
			}
		}

		public void GetUpdatePoints (string currencyName, int pointTotal)
		{
			update_text = true;

			displayText = "getTapPoints succeeded \ngetUpdatePoints returned \n" + currencyName + ": " + pointTotal;

			mHandler.Post (mUpdateResults);
		}

		public void GetUpdatePointsFailed (string error)
		{
			update_text = true;
			displayText = "getTapPoints failed with error: " + error + "\nUnable to retrieve tap points from server.";

			mHandler.Post (mUpdateResults);

			Log.Info (TAG, displayText);
		}

		public void EarnedTapPoints (int amount)
		{
			earnedPoints = true;
			earnedAmount = amount;
			mHandler.Post (mUpdateResults);
		}

		public void GetAwardPointsResponse (string currencyName, int pointTotal)
		{
			update_text = true;
			displayText = "awardTapPoints succeeded \ngetAwardPointsResponse returned \n" + currencyName + ": " + pointTotal;
			mHandler.Post (mUpdateResults);
		}

		public void GetAwardPointsResponseFailed (string error)
		{
			update_text = true;
			displayText = "awardTapPoints failed with error: " + error + "\ngetAwardPointsResponseFailed returned";

			mHandler.Post (mUpdateResults);

			Log.Info (TAG, displayText);
		}

		public void GetSpendPointsResponse (string currencyName, int pointTotal)
		{
			update_text = true;
			displayText = "spendTapPoints succeeded \ngetSpendPointsResponse returned \n" + currencyName + ": " + pointTotal;

			mHandler.Post (mUpdateResults);
		}

		public void GetSpendPointsResponseFailed (string error)
		{
			update_text = true;
			displayText = "spendTapPoints failed with error: " + error + "\ngetSpendPointsResponseFailed returned";

			mHandler.Post (mUpdateResults);

			Log.Info (TAG, displayText);
		}

		protected internal virtual void UpdateResultsInUi ()
		{
//			// Back in the UI thread -- update our UI elements based on the data in mResults
//
//			// display alert dialog if points are earned to avoid overlap w/ getTapPoints log messages
			if (earnedPoints) {
				AlertDialog.Builder builder = new AlertDialog.Builder (this);
				builder.SetTitle ("Currency Earned");
				builder.SetMessage ("You have earned " + earnedAmount + "points!");
				builder.SetCancelable (true);

				EventHandler<DialogClickEventArgs> handler = (s, e) => {
					Dialog dialog = (Dialog)s;
					dialog.Cancel ();
				};
					
				builder.SetNeutralButton ("OK", handler);
				builder.Show ();

				// separate logging of earnedPoints, otherwise displayText gets overwritten by getUpdatePoints
				if (debugTextView != null) {
					log ("earnedTapPoints returned amount: " + earnedAmount);
				}
				earnedPoints = false;
			}

			if (debugTextView != null) {
				// Update the display text.
				if (update_text) {
					log (displayText);
					update_text = false;
				}
			}
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			log (item.TitleFormatted.ToString ());
			Intent intent = new Intent (ApplicationContext, typeof(DeviceActivity));
			StartActivity (intent);
			return true;
		}
	}
}