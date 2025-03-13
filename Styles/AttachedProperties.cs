using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace AutoScanFQCTest.Styles
{
    public enum ImageButtonState
    {
        Normal = 0,
        Hover,
        Pressed,
        Disabled,
        Selected
    }

    public static class AttachedProperties
    {
        public static readonly DependencyProperty ImageButtonStateProperty =
            DependencyProperty.RegisterAttached("ImageButtonState", typeof(ImageButtonState), typeof(AttachedProperties), new FrameworkPropertyMetadata(ImageButtonState.Normal, FrameworkPropertyMetadataOptions.Inherits));

        public static ImageButtonState GetImageButtonState(UIElement element)
        {
            return (ImageButtonState)element.GetValue(ImageButtonStateProperty);
        }

        public static void SetImageButtonState(UIElement element, ImageButtonState value)
        {
            element.SetValue(ImageButtonStateProperty, value);
        }



        public static readonly DependencyProperty NormalImageProperty =
            DependencyProperty.RegisterAttached(
                "NormalImage",
                typeof(ImageSource),
                typeof(AttachedProperties),
                new PropertyMetadata(null, OnNormalImageChanged));

        public static void SetNormalImage(UIElement element, ImageSource value)
        {
            element.SetValue(NormalImageProperty, value);
        }

        public static ImageSource GetNormalImage(UIElement element)
        {
            return (ImageSource)element.GetValue(NormalImageProperty);
        }

        private static void OnNormalImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }



        public static readonly DependencyProperty HoverImageProperty =
            DependencyProperty.RegisterAttached(
                "HoverImage",
                typeof(ImageSource),
                typeof(AttachedProperties),
                new PropertyMetadata(null, OnHoverImageChanged));

        public static void SetHoverImage(UIElement element, ImageSource value)
        {
            element.SetValue(HoverImageProperty, value);
        }

        public static ImageSource GetHoverImage(UIElement element)
        {
            return (ImageSource)element.GetValue(HoverImageProperty);
        }

        private static void OnHoverImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }



        public static readonly DependencyProperty SelectionImageProperty =
            DependencyProperty.RegisterAttached(
                "SelectionImage",
                typeof(ImageSource),
                typeof(AttachedProperties),
                new PropertyMetadata(null, OnSelectionImageChanged));

        public static void SetSelectionImage(UIElement element, ImageSource value)
        {
            element.SetValue(SelectionImageProperty, value);
        }

        public static ImageSource GetSelectionImage(UIElement element)
        {
            return (ImageSource)element.GetValue(SelectionImageProperty);
        }

        private static void OnSelectionImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
