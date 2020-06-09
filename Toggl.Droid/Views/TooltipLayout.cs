using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Toggl.Droid.Views
{
    [Register("toggl.droid.views.TooltipLayout")]
    public class TooltipLayout : FrameLayout
    {
        private TextView label;

        public string Text
        {
            get { return label?.Text; }
            set {
                if (label == null)
                    return;

                label.Text = value;
            }
        }

        public TooltipLayout(Context context) : base(context)
        {
            initialize();
        }

        public TooltipLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            initialize();
        }

        public TooltipLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            initialize();
        }

        public TooltipLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            initialize();
        }

        protected TooltipLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        private void initialize()
        {
            LayoutInflater
                .From(Context)
                .Inflate(Resource.Layout.TooltipWithCenteredBottomArrow, this, true);

            label = FindViewById<TextView>(Resource.Id.TooltipText);
        }
    }
}
