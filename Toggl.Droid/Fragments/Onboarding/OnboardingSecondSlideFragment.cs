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
        private const float baseLeftOffset = 0.66496f;
        private const float baseTopOffset = 0.1265f;
        private const float g = 9.81f;
        private int maxPupilMovement;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.OnboardingSecondSlideFragment, container, false);
            InitializeViews(view);

            maxPupilMovement = 10.DpToPixels(Context);
            sensorManager = (SensorManager) Context.GetSystemService(Android.Content.Context.SensorService);
            sensor = sensorManager.GetDefaultSensor(SensorType.Accelerometer);

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
                float z = sensorEvent.Values[2];

                pupilView.UpdateMargin((int) (periscopeView.Width * baseLeftOffset) + (int) -(maxPupilMovement * x / g),
                    (int) (periscopeView.Height * baseTopOffset) + (int) (maxPupilMovement * y / g));
            }
        }
    }
}