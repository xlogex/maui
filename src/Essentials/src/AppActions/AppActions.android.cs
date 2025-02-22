using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Runtime;

namespace Microsoft.Maui.ApplicationModel
{
	class AppActionsImplementation : IAppActions, IPlatformAppActions
	{
		public const string IntentAction = "ACTION_XE_APP_ACTION";

		public bool IsSupported => OperatingSystem.IsAndroidVersionAtLeast(25);

		public Task<IEnumerable<AppAction>> GetAsync()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

#if __ANDROID_25__
			if (Application.Context.GetSystemService(Context.ShortcutService) is not ShortcutManager manager)
				throw new FeatureNotSupportedException();

			return Task.FromResult(manager.DynamicShortcuts.Select(s => s.ToAppAction()));
#else
			return Task.FromResult<IEnumerable<AppAction>>(null);
#endif
		}

		public Task SetAsync(IEnumerable<AppAction> actions)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

#if __ANDROID_25__
			if (Application.Context.GetSystemService(Context.ShortcutService) is not ShortcutManager manager)
				throw new FeatureNotSupportedException();

			using var list = new JavaList<ShortcutInfo>(actions.Select(a => a.ToShortcutInfo()));
			manager.SetDynamicShortcuts(list);
#endif
			return Task.CompletedTask;
		}

		public event EventHandler<AppActionEventArgs> AppActionActivated;

		public void OnResume(Intent intent) =>
			OnNewIntent(intent);

		public void OnNewIntent(Intent intent)
		{
			if (intent?.Action == IntentAction)
			{
				var appAction = intent.ToAppAction();

				if (!string.IsNullOrEmpty(appAction?.Id))
					AppActionActivated?.Invoke(null, new AppActionEventArgs(appAction));
			}
		}
	}

	static partial class AppActionsExtensions
	{
		internal static AppAction ToAppAction(this ShortcutInfo shortcutInfo) =>
			new AppAction(shortcutInfo.Id, shortcutInfo.ShortLabel, shortcutInfo.LongLabel);

		const string extraAppActionId = "EXTRA_XE_APP_ACTION_ID";
		const string extraAppActionTitle = "EXTRA_XE_APP_ACTION_TITLE";
		const string extraAppActionSubtitle = "EXTRA_XE_APP_ACTION_SUBTITLE";
		const string extraAppActionIcon = "EXTRA_XE_APP_ACTION_ICON";

		internal static AppAction ToAppAction(this Intent intent)
			=> new AppAction(
				intent.GetStringExtra(extraAppActionId),
				intent.GetStringExtra(extraAppActionTitle),
				intent.GetStringExtra(extraAppActionSubtitle),
				intent.GetStringExtra(extraAppActionIcon));

		internal static ShortcutInfo ToShortcutInfo(this AppAction action)
		{
			var context = Application.Context;

			var shortcut = new ShortcutInfo.Builder(context, action.Id)
				.SetShortLabel(action.Title);

			if (!string.IsNullOrWhiteSpace(action.Subtitle))
			{
				shortcut.SetLongLabel(action.Subtitle);
			}

			if (!string.IsNullOrWhiteSpace(action.Icon))
			{
				var iconResId = context.Resources.GetIdentifier(action.Icon, "drawable", context.PackageName);

				shortcut.SetIcon(Icon.CreateWithResource(context, iconResId));
			}

			var intent = new Intent(AppActionsImplementation.IntentAction);
			intent.SetPackage(context.PackageName);
			intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
			intent.PutExtra(extraAppActionId, action.Id);
			intent.PutExtra(extraAppActionTitle, action.Title);
			intent.PutExtra(extraAppActionSubtitle, action.Subtitle);
			intent.PutExtra(extraAppActionIcon, action.Icon);

			shortcut.SetIntent(intent);

			return shortcut.Build();
		}
	}
}
