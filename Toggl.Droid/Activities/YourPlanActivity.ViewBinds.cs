using Android.Content.Res;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Java.Lang;
using System;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Adapters;
using Toggl.Droid.Extensions;
using Toggl.Droid.ViewHolders;

namespace Toggl.Droid.Activities
{
    public partial class YourPlanActivity
    {
        private TextView planName;
        private TextView moreInfo;
        private TextView featuresLabel;
        private TextView planExpirationDate;
        private RecyclerView featuresRecyclerView;

        private readonly SimpleAdapter<YourPlanViewModel.PlanFeature> adapter =
            new SimpleAdapter<YourPlanViewModel.PlanFeature>(
                Resource.Layout.YourPlanActivityItem,
                FeatureViewHolder.Create
            );

        protected override void InitializeViews()
        {
            planName = FindViewById<TextView>(Resource.Id.PlanName);
            planExpirationDate = FindViewById<TextView>(Resource.Id.PlanExpirationDate);

            featuresLabel = FindViewById<TextView>(Resource.Id.FeaturesLabel);
            featuresLabel.Text = Shared.Resources.Features;

            featuresRecyclerView = FindViewById<RecyclerView>(Resource.Id.FeaturesRecyclerView);
            featuresRecyclerView.SetAdapter(adapter);
            featuresRecyclerView.SetLayoutManager(new LinearLayoutManager(this));

            moreInfo = FindViewById<TextView>(Resource.Id.MoreInfoText);
            moreInfo.TextFormatted = formatMoreInfoText();

            SetupToolbar(title: Shared.Resources.YourWorkspacePlan);
        }

        private static ICharSequence formatMoreInfoText()
        {
            var rawString = Shared.Resources.LoginToYourAccountOnTogglToSeeMore;
            var togglDotCom = "toggl.com";
            var spannable = new SpannableString(rawString);
            var index = rawString.IndexOf(togglDotCom);
            var boldSpan = new StyleSpan(TypefaceStyle.Bold);
            var length = togglDotCom.Length;
            spannable.SetSpan(boldSpan, index, index + length, SpanTypes.ExclusiveInclusive);
            return spannable;
        }

        private class FeatureViewHolder : BaseRecyclerViewHolder<YourPlanViewModel.PlanFeature>
        {
            private TextView name;
            private ImageView check;

            public FeatureViewHolder(View itemView)
                : base(itemView)
            {
            }

            public FeatureViewHolder(IntPtr handle, JniHandleOwnership ownership)
                : base(handle, ownership)
            {
            }

            protected override void InitializeViews()
            {
                name = ItemView.FindViewById<TextView>(Resource.Id.FeatureName);
                check = ItemView.FindViewById<ImageView>(Resource.Id.FeatureChecked);
            }

            protected override void UpdateView()
            {
                name.Text = Item.Name;

                if (Item.Available)
                {
                    check.SetPadding(0, 0, 0, 0);
                    check.SetImageResource(Resource.Drawable.ic_check);
                    check.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#1AD180"));
                }
                else
                {
                    var padding = 8.DpToPixels(ItemView.Context);
                    check.SetPadding(padding, padding, padding, padding);
                    check.SetImageResource(Resource.Drawable.shape_dot);
                    check.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#B5BCC0"));
                }
            }

            public static FeatureViewHolder Create(View itemView)
                => new FeatureViewHolder(itemView);
        }
    }
}
