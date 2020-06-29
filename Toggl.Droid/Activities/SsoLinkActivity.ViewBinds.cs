using Android.Widget;
using Toggl.Droid.Extensions;

namespace Toggl.Droid.Activities
{
    public sealed partial class SsoLinkActivity
    {
        private Button linkButton;
        private TextView ssoLinkHeader;
        private TextView ssoLinkDescription;
        private ImageView closeButton;
        protected override void InitializeViews()
        {
            ssoLinkDescription = FindViewById<TextView>(Resource.Id.ssoLinkDescription);
            ssoLinkHeader = FindViewById<TextView>(Resource.Id.ssoLinkHeader);
            linkButton = FindViewById<Button>(Resource.Id.linkButton);
            closeButton = FindViewById<ImageView>(Resource.Id.close);

            ssoLinkHeader.Text = Shared.Resources.AccountWithEmailAlreadyExists;
            ssoLinkDescription.Text = Shared.Resources.ToEnableLoginWithSsoLoginWithExistingCredentials;
            linkButton.Text = Shared.Resources.LoginToEnableSso;

            linkButton.FitBottomMarginInset();
        }
    }
}
