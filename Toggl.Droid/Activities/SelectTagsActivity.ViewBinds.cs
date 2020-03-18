using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Toggl.Droid.Adapters;
using Toggl.Droid.Extensions;
using Toggl.Droid.LayoutManagers;

namespace Toggl.Droid.Activities
{
    public partial class SelectTagsActivity
    {
        private readonly SelectTagsRecyclerAdapter selectTagsRecyclerAdapter = new SelectTagsRecyclerAdapter();

        private EditText textField;
        private RecyclerView selectTagsRecyclerView;

        protected override void InitializeViews()
        {
            textField = FindViewById<EditText>(Resource.Id.TextField);
            selectTagsRecyclerView = FindViewById<RecyclerView>(Resource.Id.SelectTagsRecyclerView);

            textField.Hint = Shared.Resources.AddFilterTags;
            var layoutManager = new UnpredictiveLinearLayoutManager(this);
            layoutManager.ItemPrefetchEnabled = true;
            layoutManager.InitialPrefetchItemCount = 4;
            selectTagsRecyclerView.SetLayoutManager(layoutManager);
            selectTagsRecyclerView.SetAdapter(selectTagsRecyclerAdapter);
            selectTagsRecyclerView.FitBottomMarginInset();

            SetupToolbar();
        }
    }
}
