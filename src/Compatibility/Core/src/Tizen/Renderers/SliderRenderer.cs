using System;
using System.ComponentModel;
using EColor = ElmSharp.Color;
using ESize = ElmSharp.Size;
using ESlider = ElmSharp.Slider;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class SliderRenderer : ViewRenderer<Slider, ESlider>
	{
		EColor _defaultMinColor;
		EColor _defaultMaxColor;
		EColor _defaultThumbColor;

		protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
		{
			if (Control == null)
			{
				SetNativeControl(CreateNativeControl());
				Control.ValueChanged += OnValueChanged;
				Control.DragStarted += OnDragStarted;
				Control.DragStopped += OnDragStopped;
				_defaultMinColor = Control.GetBarColor();
				_defaultMaxColor = Control.GetBackgroundColor();
				_defaultThumbColor = Control.GetHandlerColor();
			}

			UpdateMinimum();
			UpdateMaximum();
			UpdateValue();
			UpdateSliderColors();
			base.OnElementChanged(e);
		}

		protected virtual ESlider CreateNativeControl()
		{
			return new ESlider(Forms.NativeParent);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Slider.MinimumProperty.PropertyName)
			{
				UpdateMinimum();
			}
			else if (e.PropertyName == Slider.MaximumProperty.PropertyName)
			{
				UpdateMaximum();
			}
			else if (e.PropertyName == Slider.ValueProperty.PropertyName)
			{
				UpdateValue();
			}
			else if (e.PropertyName == Slider.MinimumTrackColorProperty.PropertyName)
			{
				UpdateMinimumTrackColor();
			}
			else if (e.PropertyName == Slider.MaximumTrackColorProperty.PropertyName)
			{
				UpdateMaximumTrackColor();
			}
			else if (e.PropertyName == Slider.ThumbColorProperty.PropertyName)
			{
				UpdateThumbColor();
			}
			base.OnElementPropertyChanged(sender, e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.ValueChanged -= OnValueChanged;
					Control.DragStarted -= OnDragStarted;
					Control.DragStopped -= OnDragStopped;
				}
			}
			base.Dispose(disposing);
		}

		protected override ESize Measure(int availableWidth, int availableHeight)
		{
			return new ESize(Math.Min(200, availableWidth), 50);
		}

		void OnValueChanged(object sender, EventArgs e)
		{
			Element.SetValueFromRenderer(Slider.ValueProperty, Control.Value);
		}

		void OnDragStarted(object sender, EventArgs e)
		{
			((ISliderController)Element)?.SendDragStarted();
		}

		void OnDragStopped(object sender, EventArgs e)
		{
			((ISliderController)Element)?.SendDragCompleted();
		}

		protected void UpdateValue()
		{
			Control.Value = Element.Value;
		}

		protected void UpdateMinimum()
		{
			Control.Minimum = Element.Minimum;
		}

		protected void UpdateMaximum()
		{
			Control.Maximum = Element.Maximum;
		}

		protected virtual void UpdateMinimumTrackColor()
		{
			var color = Element.MinimumTrackColor.IsDefault ? _defaultMinColor : Element.MinimumTrackColor.ToPlatform();
			Control.SetBarColor(color);
		}

		protected virtual void UpdateMaximumTrackColor()
		{
			Control.SetBackgroundColor(Element.MaximumTrackColor.IsDefault ? _defaultMaxColor : Element.MaximumTrackColor.ToPlatform());
		}

		protected virtual void UpdateThumbColor()
		{
			var color = Element.ThumbColor.IsDefault ? _defaultThumbColor : Element.ThumbColor.ToPlatform();
			Control.SetHandlerColor(color);
		}

		protected void UpdateSliderColors()
		{
			// Changing slider color is only available on mobile profile. Otherwise ignored.
			UpdateMinimumTrackColor();
			UpdateMaximum();
			UpdateThumbColor();
		}
	}
}
