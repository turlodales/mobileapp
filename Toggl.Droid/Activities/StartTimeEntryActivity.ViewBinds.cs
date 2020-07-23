using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Java.Lang;
using Toggl.Droid.Adapters;
using Toggl.Droid.Extensions;
using Toggl.Droid.LayoutManagers;
using Toggl.Droid.Views;
using static Toggl.Droid.Resource.Id;

namespace Toggl.Droid.Activities
{
    public partial class StartTimeEntryActivity
    {
        private ImageView selectTagToolbarButton;
        private ImageView selectProjectToolbarButton;
        private ImageView selectBillableToolbarButton;

        private ViewGroup durationCard;

        private TextView durationLabel;

        private RecyclerView recyclerView;

        private AutocompleteEditText descriptionField;
        private StartTimeEntryRecyclerAdapter adapter;

        private TooltipLayout projectTooltip;
        private TooltipLayout billableTooltip;

        protected override void InitializeViews()
        {
            selectTagToolbarButton = FindViewById<ImageView>(ToolbarTagButton);
            selectProjectToolbarButton = FindViewById<ImageView>(ToolbarProjectButton);
            selectBillableToolbarButton = FindViewById<ImageView>(ToolbarBillableButton);

            durationCard = FindViewById<ViewGroup>(DurationCard);
            durationCard.FitBottomPaddingInset();

            durationLabel = FindViewById<TextView>(DurationText);

            recyclerView = FindViewById<RecyclerView>(SuggestionsRecyclerView);

            descriptionField = FindViewById<AutocompleteEditText>(DescriptionTextField);
            projectTooltip = FindViewById<TooltipLayout>(ProjectTooltip);
            projectTooltip.FitBottomPaddingInset();

            billableTooltip = FindViewById<TooltipLayout>(BillableTooltip);
            billableTooltip.FitBottomPaddingInset();

            adapter = new StartTimeEntryRecyclerAdapter();
            recyclerView.SetLayoutManager(new UnpredictiveLinearLayoutManager(this));
            recyclerView.SetAdapter(adapter);

            SetupToolbar();
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_close);

            projectTooltip.TextFormatted = createProjectTooltipFormattedText();

            billableTooltip.Text = Shared.Resources.BillableAwareness;
            billableTooltip.ButtonTextFormatted = createBillableTooltipButtonFormattedText();
        }

        private ICharSequence createProjectTooltipFormattedText()
        {
            var spannable = new SpannableString(Shared.Resources.ClickOnFolderIconToAssignAProject);
            var indexOfDrawable = Shared.Resources.ClickOnFolderIconToAssignAProject.IndexOf("$");
            var span = new VerticallyCenteredImageSpan(this, Resource.Drawable.ic_project);
            span.Drawable.SetTint(ContextCompat.GetColor(this, Resource.Color.tooltipText));
            spannable.SetSpan(span, indexOfDrawable, indexOfDrawable + 1, SpanTypes.InclusiveExclusive);
            return spannable;
        }

        private ICharSequence createBillableTooltipButtonFormattedText()
        {
            var rawText = Shared.Resources.Details;
            var foregroundColor = new Color(ContextCompat.GetColor(this, Resource.Color.tooltipText));
            var spannable = new SpannableString(rawText).SetClickableSpan(
                0, rawText.Length,
                ViewModel.OpenPlanSettings
            );
            spannable.SetSpan(
                new ForegroundColorSpan(foregroundColor),
                0, rawText.Length,
                SpanTypes.ExclusiveExclusive);

            return spannable;
        }
    }
}
