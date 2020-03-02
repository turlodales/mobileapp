using Android.Graphics;
using Android.Media;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using Toggl.Droid.Extensions;

namespace Toggl.Droid.Activities
{
    public partial class OnboardingActivity : ISurfaceHolderCallback
    {
        private SurfaceView onboardingSurfaceView;
        private View continueWithGoogleButton;
        private Button continueWithEmailButton;
        private TextView onboardingMessage;
        private TextView loginGoogleLoginLabel;

        private MediaPlayer mediaPlayer;
        
        protected override void InitializeViews()
        {
            onboardingSurfaceView = FindViewById<SurfaceView>(Resource.Id.togglMan);
            continueWithGoogleButton = FindViewById(Resource.Id.continueWithGoogleButton);
            continueWithEmailButton = FindViewById<Button>(Resource.Id.continueWithEmailButton);
            onboardingMessage = FindViewById<TextView>(Resource.Id.message);
            loginGoogleLoginLabel = FindViewById<TextView>(Resource.Id.LoginGoogleLoginLabel);

            onboardingMessage.Text = Shared.Resources.OnboardingMessageVariantA;
            continueWithEmailButton.Text = Shared.Resources.ContinueWithEmail;
            loginGoogleLoginLabel.Text = Shared.Resources.ContinueWithGoogle;
            
            onboardingSurfaceView.Holder.AddCallback(this);
            continueWithEmailButton.FitBottomMarginInset(); 
        }

        public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
        {
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            mediaPlayer?.Release();
            mediaPlayer = MediaPlayer.Create(ApplicationContext, Resource.Raw.togglman);
            mediaPlayer.SetVideoScalingMode(VideoScalingMode.ScaleToFitWithCropping);
            mediaPlayer.SetDisplay(holder);
            mediaPlayer.Start();
            mediaPlayer.Looping = true;
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
        }

        protected override void OnPause()
        {
            base.OnPause();
            mediaPlayer?.Release();
            mediaPlayer = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mediaPlayer?.Release();
            onboardingSurfaceView?.Holder?.RemoveCallback(this);
            mediaPlayer = null;
        }
    }
}
