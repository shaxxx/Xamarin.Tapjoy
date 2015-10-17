using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Util;
using Android.Widget;

namespace Com.Tapjoy.TapjoySampleApp
{
	using TJError = Com.Tapjoy.TJError;
	using TJEvent = Com.Tapjoy.TJEvent;
	using TJEventCallback = Com.Tapjoy.ITJEventCallback;
	using TJEventRequest = Com.Tapjoy.TJEventRequest;
	using TapjoyConnect = Com.Tapjoy.TapjoyConnect;
	using TapjoyLog = Com.Tapjoy.TapjoyLog;
	using TapjoyOffersNotifier = Com.Tapjoy.ITapjoyOffersNotifier;

	[Activity (Label = "FeaturesActivity")]		
	public class FeaturesActivity : BaseActivity, View.IOnClickListener, ITapjoyOffersNotifier
	{

		private Button offers;
		private Button events;
		private Button getDirectPlayVideoAd;
		private Button vc;

		private Com.Tapjoy.ITapjoyOffersNotifier offersNotifier;

		private Button buttonToReenable;

		internal const string TAG = "FeaturesActivity";

		public FeaturesActivity ()
		{
			offersNotifier = this;
		}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			TapjoyLog.EnableLogging (true);

			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_features);

			debugTextView = (EditText)FindViewById (Resource.Id.debug_output_field_features);

			// Set up API button click listeners
			offers = (Button)FindViewById (Resource.Id.OffersButton);
			offers.SetOnClickListener (this);

			events = (Button)FindViewById (Resource.Id.EventsButton);
			events.SetOnClickListener (this);

			getDirectPlayVideoAd = (Button)FindViewById (Resource.Id.GetDirectPlayVideoAd);
			getDirectPlayVideoAd.SetOnClickListener (this);

			vc = (Button)FindViewById (Resource.Id.VirtualCurrencyButton);
			vc.SetOnClickListener (this);
			if (!isAppManaged ()) {
				vc.Visibility = ViewStates.Gone;
			}
		}

		public virtual void OnClick (View v)
		{
			if (v is Button) {
				Button button = ((Button)v);
				int id = button.Id;
				// --------------------------------------------------------------------------------
				// Events
				// --------------------------------------------------------------------------------
				if (id == Resource.Id.EventsButton) {
					Intent intent = new Intent (ApplicationContext, typeof(EventsActivity));
					StartActivity (intent);
				} else if (id == Resource.Id.OffersButton) {
					// Disable button
					disableButton (button);
					if (isAppManaged ()) {
						log ("Method Called: showOffers");
						TapjoyConnect.TapjoyConnectInstance.ShowOffers (offersNotifier);
					} else {
						log ("Method Called: showOffersWithCurrencyID \nCurrency ID: " + getActiveCurrency ());
						TapjoyConnect.TapjoyConnectInstance.ShowOffersWithCurrencyID (getActiveCurrency (), false, offersNotifier);
					}
				} else if (id == Resource.Id.GetDirectPlayVideoAd) {
					// Disable button
					disableButton (button);

					// Shows a direct play video
					TJEvent directPlayEvent = new TJEvent (this, "video_unit", new CustomTJEventCallback (TAG, this));
				
					// By default, ad content will be shown automatically on a successful send. For finer control of when content should be shown, call:
					directPlayEvent.EnableAutoPresent (false);

					directPlayEvent.Send ();
				} else if (id == Resource.Id.VirtualCurrencyButton) {
					Intent intent = new Intent (ApplicationContext, typeof(VirtualCurrencyActivity));
					StartActivity (intent);
				} else {
					log (((Button)v).Text.ToString ());
				}
			}
		}

		private void disableButton (Button button)
		{
			this.buttonToReenable = button;
			button.Enabled = false;
		}

		protected internal void UpdateResultsInUi ()
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
			return Intent.GetBooleanExtra (TapjoySampleConstants.MANAGED, true);
		}

		// helper function to retrieve active currency id
		public virtual string getActiveCurrency ()
		{
			return Intent.GetStringExtra (TapjoySampleConstants.ACTIVE_CURRENCY_ID);
		}

		public void GetOffersResponse ()
		{
			update_text = true;
			displayText = "showOffers Succeeded";

			// We must use a handler since we cannot update UI elements from a different thread.
			mHandler.Post (mUpdateResults);
		}

		public void GetOffersResponseFailed (string error)
		{
			update_text = true;
			displayText = "showOffers error: " + error;

			// We must use a handler since we cannot update UI elements from a different thread.
			mHandler.Post (mUpdateResults);
		}

	}

	internal class CustomTJEventCallback : Java.Lang.Object, TJEventCallback
	{

		internal string TAG;
		FeaturesActivity parent;

		public CustomTJEventCallback (string tag, FeaturesActivity activity)
		{
			TAG = tag;
			parent = activity;
		}

		public void SendEventCompleted (TJEvent @event, bool contentAvailable)
		{
			Log.Info (TAG, "Tapjoy send event 'video_unit' completed, contentAvailable: " + contentAvailable);
		
			parent.update_text = true;
			parent.displayText = "Tapjoy send event 'video_unit' completed, contentAvailable: " + contentAvailable;
		
			// We must use a handler since we cannot update UI elements from a different thread.
			parent.mHandler.Post (parent.mUpdateResults);
		
			if (contentAvailable) {
				// If enableAutoPresent is set to false for the event, we need to present the event's content ourselves
				@event.ShowContent ();
			}
		}

		public void SendEventFail (TJEvent @event, TJError error)
		{
			Log.Info (TAG, "Tapjoy send event 'video_unit' failed with error: " + error.Message);
		
			parent.update_text = true;
			parent.displayText = "Tapjoy send event 'video_unit' failed with error: " + error.Message;
		
			// We must use a handler since we cannot update UI elements from a different thread.
			parent.mHandler.Post (parent.mUpdateResults);
		}

		public void ContentDidShow (TJEvent @event)
		{
			Log.Info (TAG, "Tapjoy direct play content did show");
		
			parent.update_text = true;
			parent.displayText = "Tapjoy direct play content did show";
		
			// We must use a handler since we cannot update UI elements from a different thread.
			parent.mHandler.Post (parent.mUpdateResults);
		}

		public void ContentDidDisappear (TJEvent @event)
		{
			Log.Info (TAG, "Tapjoy direct play content did disappear");
		
			parent.update_text = true;
			parent.displayText = "Tapjoy direct play content did disappear";
		
			// We must use a handler since we cannot update UI elements from a different thread.
			parent.mHandler.Post (parent.mUpdateResults);
		}

		public void DidRequestAction (TJEvent @event, TJEventRequest request)
		{
			// Does nothing
		}

		public void ContentIsReady (TJEvent @event, int statusPreloadIncomplete)
		{
		
		}
		
	}


}