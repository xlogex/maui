using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using FlexLayout = Microsoft.Maui.Controls.Compatibility.FlexLayout;

	[TestFixture]
	public class FlexLayoutAlignSelfTest : BaseTestFixture
	{
		[Test]
		public void TestAlignSelfCenter()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,

				Direction = FlexDirection.Column,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 10, HeightRequest = 10 };
			FlexLayout.SetAlignSelf(view0, FlexAlignSelf.Center);
			layout.Children.Add(view0);

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rect(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rect(45, 0, 10, 10)));
		}

		[Test]
		public void TestAlignSelfFlexEnd()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,

				Direction = FlexDirection.Column,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 10, HeightRequest = 10 };
			FlexLayout.SetAlignSelf(view0, FlexAlignSelf.End);
			layout.Children.Add(view0);

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rect(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rect(90, 0, 10, 10)));
		}

		[Test]
		public void TestAlignSelfFlexStart()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,

				Direction = FlexDirection.Column,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 10, HeightRequest = 10 };
			FlexLayout.SetAlignSelf(view0, FlexAlignSelf.Start);
			layout.Children.Add(view0);

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rect(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rect(0, 0, 10, 10)));
		}

		[Test]
		public void TestAlignSelfFlexEndOverrideFlexStart()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,

				AlignItems = FlexAlignItems.Start,
				Direction = FlexDirection.Column,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 10, HeightRequest = 10 };
			FlexLayout.SetAlignSelf(view0, FlexAlignSelf.End);
			layout.Children.Add(view0);

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rect(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rect(90, 0, 10, 10)));
		}
	}
}