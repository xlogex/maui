using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public partial class ButtonHandlerTests : HandlerTestBase<ButtonHandler, ButtonStub>
	{
		const int Precision = 4;

		[Fact(DisplayName = "Text Initializes Correctly")]
		public async Task TextInitializesCorrectly()
		{
			var button = new ButtonStub()
			{
				Text = "Test"
			};

			await ValidatePropertyInitValue(button, () => button.Text, GetNativeText, button.Text);
		}

		[Fact(DisplayName = "Text Color Initializes Correctly")]
		public async Task TextColorInitializesCorrectly()
		{
			var button = new ButtonStub()
			{
				Text = "Test",
				TextColor = Colors.Orange
			};

			await ValidatePropertyInitValue(button, () => button.TextColor, GetNativeTextColor, button.TextColor);
		}

		[Fact(DisplayName = "Null Text Color Doesn't Crash")]
		public async Task NullTextColorDoesntCrash()
		{
			var button = new ButtonStub()
			{
				Text = "Test",
				TextColor = null
			};

			await CreateHandlerAsync(button);
		}

		[Fact(DisplayName = "Click event fires Correctly")]
		public async Task ClickEventFires()
		{
			var clicked = false;

			var button = new ButtonStub();
			button.Clicked += delegate
			{
				clicked = true;
			};

			await PerformClick(button);

			Assert.True(clicked);
		}

		[Theory()]
		[InlineData("red.png", "#FF0000")]
		[InlineData("green.png", "#00FF00")]
		[InlineData("black.png", "#000000")]
		public async Task ImageSourceInitializesCorrectly(string filename, string colorHex)
		{
			var image = new ButtonStub
			{
				Background = new SolidPaintStub(Colors.Black),
				ImageSource = new FileImageSourceStub(filename),
			};

			var order = new List<string>();

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(image);

				bool imageLoaded = await Wait(() => ImageSourceLoaded(handler));

				Assert.True(imageLoaded);
				var expectedColor = Color.FromArgb(colorHex);
				await handler.PlatformView.AssertContainsColor(expectedColor);
			});
		}

		[Theory(DisplayName = "Padding Initializes Correctly")]
		[InlineData(0, 0, 0, 0)]
		[InlineData(1, 1, 1, 1)]
		[InlineData(10, 10, 10, 10)]
		[InlineData(5, 10, 15, 20)]
		public async Task PaddingInitializesCorrectly(double left, double top, double right, double bottom)
		{
			var user = new Thickness(left, top, right, bottom);

			var button = new ButtonStub
			{
				Text = "Test",
				Padding = user
			};

			var (expected, native) = await GetValueAsync(button, handler =>
			{
				var native = GetNativePadding(handler);
				var scaled = user;

#if __ANDROID__
				scaled = handler.PlatformView.Context!.ToPixels(scaled);
#endif

				return (scaled, native);
			});

			Assert.Equal(expected.Left, native.Left, Precision);
			Assert.Equal(expected.Top, native.Top, Precision);
			Assert.Equal(expected.Right, native.Right, Precision);
			Assert.Equal(expected.Bottom, native.Bottom, Precision);
		}

		[Fact(DisplayName = "LineBreakMode Initializes Correctly")]
		public async Task LineBreakModeInitializesCorrectly()
		{
			var xplatLineBreakMode = LineBreakMode.TailTruncation;

			var button = new ButtonStub()
			{
				LineBreakMode = xplatLineBreakMode
			};

			var expectedValue = xplatLineBreakMode.ToPlatform();

			var values = await GetValueAsync(button, (handler) =>
			{
				return new
				{
					ViewValue = button.LineBreakMode,
					NativeViewValue = GetNativeLineBreakMode(handler)
				};
			});

			Assert.Equal(xplatLineBreakMode, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue);
		}
	}
}