using Android.App;
using Android.Graphics;
using Android.Media;
using Android.Views;
using Android.Widget;

namespace Toggl.Droid.Fragments.Onboarding
{
    public partial class OnboardingFirstSlideFragment : ISurfaceHolderCallback
    {
        private TextView onboardingMessage;
        private SurfaceView onboardingSurfaceView;

        private MediaPlayer mediaPlayer;


        protected override void InitializeViews(View view)
        {
            onboardingMessage = view.FindViewById<TextView>(Resource.Id.message);
            onboardingSurfaceView = view.FindViewById<SurfaceView>(Resource.Id.togglMan);

            onboardingSurfaceView.Holder.AddCallback(this);
            onboardingMessage.Text = Shared.Resources.OnboardingMessagePage1;
        }
        public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
        {
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            mediaPlayer?.Release();
            mediaPlayer = MediaPlayer.Create(Application.Context, Resource.Raw.togglman);
            mediaPlayer.SetVideoScalingMode(VideoScalingMode.ScaleToFitWithCropping);
            mediaPlayer.SetDisplay(holder);
            mediaPlayer.Start();
            mediaPlayer.Looping = true;
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            mediaPlayer?.Release();
            onboardingSurfaceView?.Holder?.RemoveCallback(this);
            mediaPlayer = null;
        }
    }
}