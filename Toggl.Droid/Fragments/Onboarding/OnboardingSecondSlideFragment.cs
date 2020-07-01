using System;
using Android.Hardware;
using Android.OS;
using Android.Views;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Extensions;

namespace Toggl.Droid.Fragments.Onboarding
{
    public partial class OnboardingSecondSlideFragment : ReactiveTabFragment<OnboardingViewModel>, ISensorEventListener
    {
        private SensorManager sensorManager;
        private Sensor sensor;
        private const int periscopeMdpiWidth = 203;
        private const int periscopeMdpiHeight = 205;
        private int baseLeftPosition = 0;
        private int baseTopPosition = 0;
        private const float baseLeftOffset = 0.6551724138f;
        private const float baseTopOffset = 0.1219512195f;
        private const float g = 9.81f;
        private int maxPupilMovement;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.OnboardingSecondSlideFragment, container, false);
            InitializeViews(view);


            sensorManager = (SensorManager) Context.GetSystemService(Android.Content.Context.SensorService);
            sensor = sensorManager.GetDefaultSensor(SensorType.Accelerometer);

            periscopeView.Post(() =>
            {
                baseLeftPosition = periscopeView.Left;
                baseTopPosition = periscopeView.Top;
                var periscopeFullWidth = periscopeMdpiWidth.DpToPixels(Context);
                var periscopeFullHeight = periscopeMdpiHeight.DpToPixels(Context);

                var scalingX = (float) periscopeView.Width / periscopeFullWidth;
                var scalingY = (float) periscopeView.Height / periscopeFullHeight;
                var scalingFactor = Math.Min(scalingX, scalingY);

                maxPupilMovement = (int) (5.DpToPixels(Context) * scalingFactor);
                baseLeftPosition -= (int) ( (pupilView.Width - pupilView.Width * scalingFactor) / 2f);
                baseTopPosition -= (int) ( (pupilView.Height - pupilView.Height * scalingFactor) / 2f);

                pupilView.ScaleX = scalingFactor;
                pupilView.ScaleY = scalingFactor;

                updateMarginOnPupilView(0, 0);
            });

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
            sensorManager.RegisterListener(this, sensor, SensorDelay.Ui);
        }

        public override void OnPause()
        {
            base.OnPause();
            sensorManager.UnregisterListener(this);
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent sensorEvent)
        {
            if (sensorEvent.Sensor.Type == SensorType.Accelerometer)
            {
                float x = sensorEvent.Values[0];
                float y = sensorEvent.Values[1];

                updateMarginOnPupilView(x, y);
            }
        }

        private void updateMarginOnPupilView(float x, float y)
        {
            pupilView.UpdateMargin(
                baseLeftPosition + (int) (periscopeView.Width * baseLeftOffset) + (int) -(maxPupilMovement * x / g),
                baseTopPosition + (int) (periscopeView.Height * baseTopOffset) + (int) (maxPupilMovement * y / g));
        }
    }
}