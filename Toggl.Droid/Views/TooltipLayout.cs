using System;
using System.Collections.Generic;
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
        private static Dictionary<int, int> arrowPositionToLayoutDictionary = new Dictionary<int, int>
        {
            { 0, Resource.Layout.TooltipWithCenteredBottomArrow },
            { 1, Resource.Layout.TooltipWithRightBottomArrow }
        };

        private TextView label;

        public string Text
        {
            get => label?.Text;
            set
            {
                if (label == null)
                    return;

                label.Text = value;
            }
        }

        public TooltipLayout(Context context)
            : base(context)
        {
            initialize();
        }

        public TooltipLayout(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            initialize(attrs);
        }

        public TooltipLayout(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
            initialize(attrs, defStyleAttr, 0);
        }

        public TooltipLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
            : base(context, attrs, defStyleAttr, defStyleRes)
        {
            initialize(attrs, defStyleAttr, defStyleRes);
        }

        protected TooltipLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        private void initialize(IAttributeSet attrs = null, int defStyleAttrs = 0, int defStyleRes = 0)
        {
            var indexOfLayoutToInflate = 0;

            if (attrs != null)
            {
                using var customsAttrs = Context.ObtainStyledAttributes(
                    attrs, Resource.Styleable.TooltipLayout, defStyleAttrs, defStyleRes);

                indexOfLayoutToInflate = customsAttrs.GetInt(Resource.Styleable.TooltipLayout_arrowDirection, 0);
            }

            var layoutToInflate = arrowPositionToLayoutDictionary[indexOfLayoutToInflate];

            LayoutInflater.From(Context).Inflate(layoutToInflate, this, true);

            label = FindViewById<TextView>(Resource.Id.TooltipText);
        }
    }
}
