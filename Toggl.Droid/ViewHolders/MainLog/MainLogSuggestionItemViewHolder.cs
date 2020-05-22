using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Toggl.Core.Suggestions;
using Toggl.Core.UI.ViewModels;
using Toggl.Core.UI.ViewModels.MainLog;
using Toggl.Droid.Extensions;
using Toggl.Droid.Helper;

namespace Toggl.Droid.ViewHolders.MainLog
{
    public sealed class MainLogSuggestionItemViewHolder : BaseRecyclerViewHolder<MainLogItemViewModel>
    {
        public ISubject<SuggestionLogItemViewModel> ContinueSuggestionSubject { get; set; }

        private TextView descriptionLabel;
        private TextView projectLabel;
        private TextView clientLabel;

        public MainLogSuggestionItemViewHolder(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public MainLogSuggestionItemViewHolder(View itemView) : base(itemView)
        {
        }

        protected override void InitializeViews()
        {
            descriptionLabel = ItemView.FindViewById<TextView>(Resource.Id.DescriptionLabel);
            projectLabel = ItemView.FindViewById<TextView>(Resource.Id.ProjectLabel);
            clientLabel = ItemView.FindViewById<TextView>(Resource.Id.ClientLabel);
            ItemView.Click += onItemClick;
        }

        protected override void UpdateView()
        {
            var viewModel = (SuggestionLogItemViewModel) Item;
            descriptionLabel.Text = viewModel.Suggestion.Description;
            prefixWithProviderNameInDebug(viewModel);
            descriptionLabel.Visibility = (!string.IsNullOrWhiteSpace(viewModel.Suggestion.Description)).ToVisibility();

            var foregroundColor = Shared.Color.ParseAndAdjustToLabel(viewModel.Suggestion.ProjectColor, ActiveTheme.Is.DarkTheme).ToNativeColor();
            projectLabel.Text = viewModel.Suggestion.ProjectName;
            projectLabel.SetTextColor(foregroundColor);
            projectLabel.Visibility = viewModel.Suggestion.HasProject.ToVisibility();

            clientLabel.Text = viewModel.Suggestion.ClientName;
            clientLabel.Visibility = viewModel.Suggestion.HasProject.ToVisibility();
        }

        private void onItemClick(object sender, EventArgs e)
        {
            ContinueSuggestionSubject.OnNext((SuggestionLogItemViewModel) Item);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing)
                return;

            ItemView.Click -= onItemClick;
        }

        [Conditional("DEBUG")]
        private void prefixWithProviderNameInDebug(SuggestionLogItemViewModel viewModel)
        {
            var prefix = viewModel.Suggestion.ProviderType.ToString().Substring(0, 4);
            descriptionLabel.Text = $"{prefix} {viewModel.Suggestion.Description}";
        }
    }
}
