using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace Com.Tapjoy.TapjoySampleApp
{
	[Activity (Label = "BaseActivity")]			
	public class BaseActivity : Activity
	{
		public EditText debugTextView;
		public string displayText = "";
		public bool update_text = false;

		// Need handler for callbacks to the UI thread
		public readonly Handler mHandler;
		public IRunnable mUpdateResults;

		public BaseActivity ()
		{
			mHandler = new Handler ();
			// Create runnable for posting
			mUpdateResults = new RunnableAnonymousInnerClassHelper(this) { };
		}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			// this should be replaced in each child activity
			debugTextView = new EditText (this);
			base.OnCreate (savedInstanceState);
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.Inflate (Resource.Menu.main, menu);
			return true;
		}
			
		public virtual void updateResultsInUi (){}

		protected void log (string text)
		{
			debugTextView.Text = debugTextView.Text + ("\n") + text;
		}

		class RunnableAnonymousInnerClassHelper : Java.Lang.Object, Java.Lang.IRunnable
		{
			readonly BaseActivity outerInstance;

			public RunnableAnonymousInnerClassHelper (BaseActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void Run ()
			{
				outerInstance.updateResultsInUi ();
			}
		}
	}
}
