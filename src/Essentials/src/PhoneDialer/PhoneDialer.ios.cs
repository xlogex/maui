using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class PhoneDialerImplementation : IPhoneDialer
	{
		const string noNetworkProviderCode = "65535";

		public bool IsSupported => UIApplication.SharedApplication.CanOpenUrl(CreateNsUrl(new string('0', 10)));

		public async void Open(string number)
		{
			ValidateOpen(number);

			var nsUrl = CreateNsUrl(number);
			await Launcher.Default.OpenAsync(nsUrl);
		}

		static NSUrl CreateNsUrl(string number) => new NSUrl(new Uri($"tel:{number}").AbsoluteUri);
	}
}
