using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Util;

namespace  Com.Tapjoy.TapjoySampleApp
{

	using AlertDialog = Android.App.AlertDialog;
	using DialogInterface = Android.Content.DialogInterface;
	using Intent = Android.Content.Intent;
	using Bundle = Android.OS.Bundle;
	using Log = Android.Util.Log;
	using View = Android.Views.View;

	using TJError = Com.Tapjoy.TJError;
	using TJEvent = Com.Tapjoy.TJEvent;
	using TJEventCallback = Com.Tapjoy.ITJEventCallback;
	using TJEventRequest = Com.Tapjoy.TJEventRequest;

	[Activity (Label = "EventsActivity")]		
	public class EventsActivity : BaseActivity, ITJEventCallback
	{
		private string[] items = { "test_unit", "video_unit", "message_unit" };
		private TJEvent tapjoyEvent;
		private string lastSelectedEvent;

		internal ITJEventCallback eventCallback;
		internal Button selectEventButton;
		internal Button sendEventButton;

		public EventsActivity ()
		{
			eventCallback = this;
		}

		private const string TAG = "EventsActivity";

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_events);

			base.debugTextView = (EditText)FindViewById (Resource.Id.debug_output_field_events);
			selectEventButton = (Button)FindViewById (Resource.Id.select_event_button);
			sendEventButton = (Button)FindViewById (Resource.Id.send_event_button);

			EventHandler<DialogClickEventArgs> handler = (s, o) => {
				// save off selected event
				selectEventButton.Text = items [o.Which];
				lastSelectedEvent = items [o.Which];
				sendEventButton.Enabled = true;
			};

			selectEventButton.Click += (sender, e) => {
				AlertDialog.Builder builder = new AlertDialog.Builder (this);
				builder.SetTitle ("Select Event");
				builder.SetItems (items, handler);
				builder.Show();
			};
				

			// set up click listener for when event is sent
			sendEventButton.Click += (sender, e) => {
				tapjoyEvent = new TJEvent(this, lastSelectedEvent, null, eventCallback);
				tapjoyEvent.EnableAutoPresent(true);
				tapjoyEvent.Send();
			};

		}

		public override void updateResultsInUi ()
		{
			// Back in the UI thread -- update our UI elements based on the data in mResults
			if (debugTextView != null) {
				// Update the display text.
				if (update_text) {
					log (displayText);
					update_text = false;
				}
			}
		}

		// Callback Methods

		public  void ContentDidShow (TJEvent @event)
		{
			displayText = "Content did show \neventName: " + @event.Name;
			update_text = true;
			mHandler.Post (mUpdateResults);
		}

		public  void ContentDidDisappear (TJEvent @event)
		{
			displayText = "Content did disappear \neventName: " + @event.Name;
			update_text = true;
			mHandler.Post (mUpdateResults);
		}

		public void DidRequestAction (TJEvent @event, TJEventRequest request)
		{
//			// Dismiss the event content
			Intent intent = new Intent(ApplicationContext,typeof(EventsActivity));
			intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
			StartActivity(intent);
			string message = "Type: " + request.Type + ", Identifier: " + request.Identifier + ", Quantity: " + request.Quantity;

			EventHandler<DialogClickEventArgs> handler = (s, o) => {
				Dialog d = (Dialog)s;
				d.Dismiss ();
			};
			var builder = new AlertDialog.Builder(this)
				.SetTitle("Got Action Callback")
				.SetMessage(message)
				.SetPositiveButton("Okay", handler);	
		}

		public void SendEventCompleted (TJEvent @event, bool contentAvailable)
		{
			displayText = "Send event completed \neventName: " + @event.Name + "\ncontentAvailable: " + contentAvailable;
			update_text = true;
			mHandler.Post (mUpdateResults);
		}

		public  void SendEventFail (TJEvent @event, TJError error)
		{
			update_text = true;
			displayText = "Send event failed [eventId: " + @event.Name + "] error: " + string.Format (error.Code + ": " + error.Message);
			mHandler.Post (mUpdateResults);		
			Log.Info (TAG, displayText);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			log (item.TitleFormatted.ToString ());
			Intent intent = new Intent (ApplicationContext, typeof(DeviceActivity));
			StartActivity (intent);
			return true;
		}

		public void ContentIsReady (TJEvent @event, int statusPreloadIncomplete)
		{


		}

	}
}