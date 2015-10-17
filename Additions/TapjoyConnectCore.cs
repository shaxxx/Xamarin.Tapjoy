using System;

namespace Com.Tapjoy
{
	public partial class TapjoyConnectCore
	{
		public static bool VideoEnabled{
			get { 
				return isVideoEnabledOverload();
			}
			set { 
				var instance = TapjoyConnectCore.Instance;
				instance.setVideoEnabledOverload(value);
			}
		}
	}
}

