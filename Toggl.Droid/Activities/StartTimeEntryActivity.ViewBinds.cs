using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
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

            adapter = new StartTimeEntryRecyclerAdapter();
            recyclerView.SetLayoutManager(new UnpredictiveLinearLayoutManager(this));
            recyclerView.SetAdapter(adapter);

            SetupToolbar();
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_close);

            var spannable = new SpannableString(Shared.Resources.ClickOnFolderIconToAssignAProject);
            var indexOfDrawable = Shared.Resources.ClickOnFolderIconToAssignAProject.IndexOf("$");
            var span = new VerticallyCenteredImageSpan(this, Resource.Drawable.ic_project);
            span.Drawable.SetTint(ContextCompat.GetColor(this, Resource.Color.tooltipText));
            spannable.SetSpan(span, indexOfDrawable, indexOfDrawable + 1, SpanTypes.InclusiveExclusive);
            projectTooltip.TextFormatted = spannable;
        }
    }
}
