using System;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Toggl.Droid.Extensions;

namespace Toggl.Droid.Views
{
    [Register("toggl.droid.views.TooltipLayout")]
    public class TooltipLayout : FrameLayout
    {
        private static Dictionary<int, int> arrowPositionToLayoutDictionary = new Dictionary<int, int>
        {
            { 0, Resource.Layout.TooltipWithRightBottomArrow },
            { 1, Resource.Layout.TooltipWithLeftBottomArrow},
            { 2, Resource.Layout.TooltipWithCenteredBottomArrow },
            { 3, Resource.Layout.TooltipWithLeftTopArrow },
            { 4, Resource.Layout.TooltipWithCenteredTopArrow }
        };

        private TextView title;
        private TextView label;
        private TextView button;

        public string Title
        {
            get => title?.Text;
            set
            {
                if (title == null)
                    return;

                title.Text = value;
                title.Visibility = (!string.IsNullOrEmpty(value)).ToVisibility();
            }
        }

        public string Text
        {
            get => label?.Text;
            set
            {
                if (label == null)
                    return;

                label.Text = value;
                label.Visibility = (!string.IsNullOrEmpty(value)).ToVisibility();
            }
        }

        public ICharSequence TextFormatted
        {
            get => label?.TextFormatted;
            set
            {
                if (label == null)
                    return;

                label.TextFormatted = value;
            }
        }

        public string ButtonText
        {
            get => button?.Text;
            set
            {
                if (button == null)
                    return;

                button.Text = value;
                button.Visibility = (!string.IsNullOrEmpty(value)).ToVisibility();
            }
        }

        public ICharSequence ButtonTextFormatted
        {
            get => button?.TextFormatted;
            set
            {
                if (button == null)
                    return;

                button.TextFormatted = value;
                button.Visibility = (!string.IsNullOrEmpty(value.ToString())).ToVisibility();
                button.MovementMethod = LinkMovementMethod.Instance;
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

            title = FindViewById<TextView>(Resource.Id.TooltipTitle);
            label = FindViewById<TextView>(Resource.Id.TooltipText);
            button = FindViewById<TextView>(Resource.Id.TooltipButton);
        }
    }
}
