using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Android.Text;
using Toggl.Core.Autocomplete;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Extensions;
using Toggl.Droid.Extensions.Reactive;
using Toggl.Droid.Helper;
using Toggl.Core.UI.Extensions;
using Toggl.Droid.Presentation;
using Toggl.Shared.Extensions;
using Android.Graphics;

namespace Toggl.Droid.Activities
{
    [Activity(Theme = "@style/Theme.Splash",
              ScreenOrientation = ScreenOrientation.Portrait,
              WindowSoftInputMode = SoftInput.StateVisible,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed partial class StartTimeEntryActivity : ReactiveActivity<StartTimeEntryViewModel>
    {
        private static readonly TimeSpan typingThrottleDuration = TimeSpan.FromMilliseconds(300);

        public StartTimeEntryActivity() : base(
            Resource.Layout.StartTimeEntryActivity,
            Resource.Style.AppTheme,
            Transitions.SlideInFromBottom)
        {
        }

        public StartTimeEntryActivity(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected override void InitializeBindings()
        {
            ViewModel.Suggestions
                .SubscribeOn(AndroidDependencyContainer.Instance.SchedulerProvider.BackgroundScheduler)
                .Subscribe(adapter.Rx().Items())
                .DisposedBy(DisposeBag);

            adapter.ItemTapObservable
                .Subscribe(ViewModel.SelectSuggestion.Inputs)
                .DisposedBy(DisposeBag);

            adapter.ToggleTasks
                .Subscribe(ViewModel.ToggleTasks.Inputs)
                .DisposedBy(DisposeBag);

            // Displayed time
            ViewModel.DisplayedTime
                .Subscribe(durationLabel.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            // Toggle project suggestions toolbar button
            selectProjectToolbarButton.Rx()
                .BindAction(ViewModel.ToggleProjectSuggestions)
                .DisposedBy(DisposeBag);

            ViewModel.IsSuggestingProjects
                .Select(isSuggesting => isSuggesting ? Resource.Drawable.ic_project_active : Resource.Drawable.ic_project)
                .Subscribe(selectProjectToolbarButton.SetImageResource)
                .DisposedBy(DisposeBag);

            // Toggle tag suggestions toolbar button
            selectTagToolbarButton.Rx()
                .BindAction(ViewModel.ToggleTagSuggestions)
                .DisposedBy(DisposeBag);

            ViewModel.IsSuggestingTags
                .Select(isSuggesting => isSuggesting ? Resource.Drawable.ic_tag_active : Resource.Drawable.ic_tag)
                .Subscribe(selectTagToolbarButton.SetImageResource)
                .DisposedBy(DisposeBag);

            // Billable toolbar button
            ViewModel.IsBillable
                .Select(isSuggesting => isSuggesting ? Resource.Drawable.ic_billable_active : Resource.Drawable.ic_billable)
                .Subscribe(selectBillableToolbarButton.SetImageResource)
                .DisposedBy(DisposeBag);

            ViewModel.IsBillableAvailable
                .Select(isAvailable => Color.ParseColor(isAvailable ? "#757575" : "#A5A5A5").ToArgb())
                .Subscribe(selectBillableToolbarButton.Drawable.SetTint)
                .DisposedBy(DisposeBag);

            ViewModel.IsBillablePremiumTooltipVisibile
                .Subscribe(billableTooltip.Rx().IsVisible())
                .DisposedBy(DisposeBag);

            selectBillableToolbarButton.Rx()
                .BindAction(ViewModel.BillableTapped)
                .DisposedBy(DisposeBag);

            billableTooltip.Rx()
                .BindAction(ViewModel.DismissBillableTooltip)
                .DisposedBy(DisposeBag);

            // Description text field
            descriptionField.Hint = ViewModel.PlaceholderText;

            ViewModel.TextFieldInfo
                .Subscribe(onTextFieldInfo)
                .DisposedBy(DisposeBag);

            durationLabel.Rx()
                .BindAction(ViewModel.ChangeTime)
                .DisposedBy(DisposeBag);

            descriptionField.TextObservable
                .SubscribeOn(ThreadPoolScheduler.Instance)
                .Sample(typingThrottleDuration)
                .Select(text => text.AsImmutableSpans(descriptionField.SelectionStart))
                .Subscribe(ViewModel.SetTextSpans)
                .DisposedBy(DisposeBag);

            ViewModel.ProjectsTooltipCondition.ConditionMet
                .Subscribe(projectTooltip.Rx().IsVisible())
                .DisposedBy(DisposeBag);

            projectTooltip.Rx().Tap()
               .Subscribe(ViewModel.ProjectsTooltipCondition.Dismiss)
               .DisposedBy(DisposeBag);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.OneButtonMenu, menu);
            var doneMenuItem = menu.FindItem(Resource.Id.ButtonMenuItem);
            doneMenuItem.SetTitle(Shared.Resources.Done);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.ButtonMenuItem)
            {
                ViewModel.Done.Execute();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnResume()
        {
            base.OnResume();
            descriptionField.RequestFocus();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            DisposeBag?.Dispose();
        }

        private void onTextFieldInfo(TextFieldInfo textFieldInfo)
        {
            var (formattedText, cursorPosition) = textFieldInfo.AsSpannableTextAndCursorPosition();
            if (descriptionField.TextFormatted is ISpannable currentFormattedText && formattedText.IsEqualTo(currentFormattedText))
                return;

            descriptionField.TextFormatted = formattedText;
            descriptionField.SetSelection(cursorPosition);
        }
    }
}
