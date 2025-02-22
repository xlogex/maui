﻿using System;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, MauiTextView>
	{
		static readonly int BaseHeight = 30;

		protected override MauiTextView CreatePlatformView() => new MauiTextView();

		protected override void ConnectHandler(MauiTextView platformView)
		{
			platformView.ShouldChangeText += OnShouldChangeText;
			platformView.Started += OnStarted;
			platformView.Ended += OnEnded;
			platformView.TextSetOrChanged += OnTextPropertySet;
		}

		protected override void DisconnectHandler(MauiTextView platformView)
		{
			platformView.ShouldChangeText -= OnShouldChangeText;
			platformView.Started -= OnStarted;
			platformView.Ended -= OnEnded;
			platformView.TextSetOrChanged -= OnTextPropertySet;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			new SizeRequest(new Size(widthConstraint, BaseHeight));

		public static void MapText(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateText(editor);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, editor);
		}

		public static void MapTextColor(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateTextColor(editor);

		public static void MapPlaceholder(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholder(editor);

		public static void MapPlaceholderColor(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholderColor(editor);

		public static void MapCharacterSpacing(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateCharacterSpacing(editor);

		public static void MapMaxLength(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateMaxLength(editor);

		public static void MapIsReadOnly(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsReadOnly(editor);

		public static void MapIsTextPredictionEnabled(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(editor);

		public static void MapFont(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateFont(editor, handler.GetRequiredService<IFontManager>());

		public static void MapHorizontalTextAlignment(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(editor);

		[MissingMapper]
		public static void MapVerticalTextAlignment(IEditorHandler handler, IEditor editor)
		{
		}

		public static void MapCursorPosition(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateCursorPosition(editor);

		public static void MapSelectionLength(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateSelectionLength(editor);

		public static void MapKeyboard(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateKeyboard(editor);

		public static void MapFormatting(IEditorHandler handler, IEditor editor)
		{
			handler.PlatformView?.UpdateMaxLength(editor);

			// Update all of the attributed text formatting properties
			handler.PlatformView?.UpdateCharacterSpacing(editor);
		}

		bool OnShouldChangeText(UITextView textView, NSRange range, string replacementString) =>
			VirtualView.TextWithinMaxLength(textView.Text, range, replacementString);

		void OnStarted(object? sender, EventArgs eventArgs)
		{
			if (VirtualView != null)
				VirtualView.IsFocused = true;
		}

		void OnEnded(object? sender, EventArgs eventArgs)
		{
			if (VirtualView != null)
			{
				VirtualView.IsFocused = false;

				VirtualView.Completed();
			}
		}

		void OnTextPropertySet(object? sender, EventArgs e) =>
			VirtualView.UpdateText(PlatformView.Text);
	}
}