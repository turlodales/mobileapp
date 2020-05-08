#if !USE_PRODUCTION_API
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.IO;
using System.Reactive.Disposables;
using Java.IO;
using Toggl.Core.Services;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Extensions;
using Toggl.Droid.Extensions.Reactive;
using Toggl.Droid.Fragments;
using Toggl.Shared.Extensions;
using Environment = System.Environment;
using File = Java.IO.File;

namespace Toggl.Droid.Debug
{
    public sealed class ErrorTriggeringFragment : ReactiveDialogFragment<ViewModel>
    {
        private readonly CompositeDisposable disposeBag = new CompositeDisposable();

        public ErrorTriggeringFragment()
        {
        }

        public ErrorTriggeringFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var matchParent = ViewGroup.LayoutParams.MatchParent;

            var view = new LinearLayout(Context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new ViewGroup.LayoutParams(matchParent, matchParent),
            };

            var tokenReset = createTextView("Token reset error");
            var noWorkspace = createTextView("No workspace error");
            var noDefaultWorkspace = createTextView("No default workspace error");
            var outdatedApp = createTextView("Outdated client error");
            var outdatedApi = createTextView("Outdated API error");
            var outdatedAppPermanently = createTextView("Permanent outdated client error");
            var outdatedApiPermanently = createTextView("Permanent outdated API error");
            var unsyncedDataDump = createTextView("Share unsynced data dump");

            tokenReset.Rx().Tap()
                .Subscribe(dismissAndThenRun(tokenResetErrorTriggered))
                .DisposedBy(disposeBag);

            noWorkspace.Rx().Tap()
                .Subscribe(dismissAndThenRun(noWorkspaceErrorTriggered))
                .DisposedBy(disposeBag);

            noDefaultWorkspace.Rx().Tap()
                .Subscribe(dismissAndThenRun(noDefaultWorkspaceErrorTriggered))
                .DisposedBy(disposeBag);

            outdatedApp.Rx().Tap()
                .Subscribe(dismissAndThenRun(outdatedAppErrorTriggered))
                .DisposedBy(disposeBag);

            outdatedApi.Rx().Tap()
                .Subscribe(dismissAndThenRun(outdatedApiErrorTriggered))
                .DisposedBy(disposeBag);

            outdatedAppPermanently.Rx().Tap()
                .Subscribe(dismissAndThenRun(outdatedAppPermanentlyErrorTriggered))
                .DisposedBy(disposeBag);

            outdatedApiPermanently.Rx().Tap()
                .Subscribe(dismissAndThenRun(outdatedApiPermanentlyErrorTriggered))
                .DisposedBy(disposeBag);

            unsyncedDataDump.Rx().Tap()
                .Subscribe(dismissAndThenRun(unsyncedDataDumpTriggered))
                .DisposedBy(disposeBag);

            view.AddView(tokenReset);
            view.AddView(noWorkspace);
            view.AddView(noDefaultWorkspace);
            view.AddView(outdatedApp);
            view.AddView(outdatedApi);
            view.AddView(outdatedAppPermanently);
            view.AddView(outdatedApiPermanently);
            view.AddView(unsyncedDataDump);

            return view;
        }

        public override void OnCancel(IDialogInterface dialog)
        {
            Dismiss();
        }

        private TextView createTextView(string text)
        {
            var layoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);

            var textView = new TextView(Context)
            {
                Text = text,
                LayoutParameters = layoutParameters
            };

            var padding = 20.DpToPixels(Context);
            textView.SetPadding(padding, padding, padding, padding);

            var outValue = new TypedValue();
            Context.Theme.ResolveAttribute(
                Android.Resource.Attribute.SelectableItemBackground, outValue, true);
            textView.SetBackgroundResource(outValue.ResourceId);

            return textView;
        }

        private Action dismissAndThenRun(Action callback)
            => () =>
            {
                Dismiss();
                callback?.Invoke();
            };

        private void tokenResetErrorTriggered()
        {
            var container = AndroidDependencyContainer.Instance;
            container.NavigationService.Navigate<TokenResetViewModel>(null);
        }

        private void noWorkspaceErrorTriggered()
        {
            var container = AndroidDependencyContainer.Instance;
            var noWorkspaceViewModel = container.ViewModelLoader.Load<NoWorkspaceViewModel>();
            var noWorkspaceFragment = new NoWorkspaceFragment();
            container.ViewModelCache.Cache(noWorkspaceViewModel);
            noWorkspaceFragment.Show(FragmentManager, nameof(NoWorkspaceFragment));
        }

        private async void noDefaultWorkspaceErrorTriggered()
        {
            var container = AndroidDependencyContainer.Instance;
            var noWorkspaceViewModel = container.ViewModelLoader.Load<SelectDefaultWorkspaceViewModel>();
            await noWorkspaceViewModel.Initialize();
            var noWorkspaceFragment = new SelectDefaultWorkspaceFragment();
            container.ViewModelCache.Cache(noWorkspaceViewModel);
            noWorkspaceFragment.Show(FragmentManager, nameof(SelectDefaultWorkspaceFragment));
        }

        private void outdatedAppErrorTriggered()
        {
            var container = AndroidDependencyContainer.Instance;
            container.NavigationService.Navigate<OutdatedAppViewModel>(null);
        }

        private void outdatedApiErrorTriggered()
        {
            var container = AndroidDependencyContainer.Instance;
            container.NavigationService.Navigate<OutdatedAppViewModel>(null);
        }

        private void outdatedAppPermanentlyErrorTriggered()
        {
            var container = AndroidDependencyContainer.Instance;
            container.AccessRestrictionStorage.SetApiOutdated();
            outdatedAppErrorTriggered();
        }

        private void outdatedApiPermanentlyErrorTriggered()
        {
            var container = AndroidDependencyContainer.Instance;
            container.AccessRestrictionStorage.SetClientOutdated();
            outdatedApiErrorTriggered();
        }

        private void unsyncedDataDumpTriggered()
        {
            void copyFile(File src, File dst) {
                FileInputStream inStream = new FileInputStream(src);
                FileOutputStream outStream = new FileOutputStream(dst);
                inStream.Channel.TransferTo(0, inStream.Channel.Size(), outStream.Channel);
                inStream.Close();
                outStream.Close();
            }

            AndroidDependencyContainer
                .Instance
                .UnsyncedDataPersistenceService
                .PersistUnsyncedData()
                .Wait();

            var originalFile = new File(IUnsyncedDataPersistenceService.UnsyncedDataFilePath);
            var tempCopy = File.CreateTempFile("unsynced_dump", ".json", RequireContext().ExternalCacheDir);
            copyFile(originalFile, tempCopy);
            tempCopy.SetReadable(true, false);
            var email = new Intent(Intent.ActionSend);
            email.PutExtra(Intent.ExtraSubject, "Unsynced data dump");
            email.PutExtra(Intent.ExtraText, "See attached file");
            email.PutExtra(Intent.ExtraStream, Android.Net.Uri.FromFile(tempCopy));
            email.SetType("message/rfc822");
            StartActivityForResult(email, 1);
        }

        protected override void InitializeViews(View view)
        {
        }
    }
}
#endif
