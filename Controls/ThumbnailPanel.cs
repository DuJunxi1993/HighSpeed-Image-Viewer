using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using HighSpeedImageViewer.Models;

namespace HighSpeedImageViewer.Controls;

public class ThumbnailPanel : ItemsControl
{
    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(ThumbnailPanel),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsRender, OnSelectedIndexChanged));

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ThumbnailPanel panel)
        {
            panel.UpdateSelectionIndicator();
        }
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public event Action<int>? ItemClicked;

    protected override bool IsItemItsOwnContainerOverride(object item) => item is FileListItem;

    protected override DependencyObject GetContainerForItemOverride() => new FileListItem { Owner = this };

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is FileListItem fi && item is ImageItem ii)
        {
            fi.ImageItem = ii;
            fi.Owner = this;
        }
    }

    public void NotifyItemClicked(int index)
    {
        ItemClicked?.Invoke(index);
    }

    public void UpdateSelectionIndicator()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            var container = ItemContainerGenerator.ContainerFromIndex(i) as FileListItem;
            if (container != null)
            {
                container.IsSelected = (i == SelectedIndex);
            }
        }
    }

    private int _lastScrollIndex = -1;

    public void ScrollIntoView(int index)
    {
        if (index < 0 || index >= Items.Count) return;

        var scrollViewer = FindParentScrollViewer(this);
        if (scrollViewer == null) return;

        Dispatcher.BeginInvoke(new Action(() =>
        {
            var container = ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
            if (container == null)
            {
                Dispatcher.BeginInvoke(new Action(() => ScrollIntoView(index)), DispatcherPriority.Loaded);
                return;
            }

            var itemHeight = container.ActualHeight;
            if (itemHeight <= 0) itemHeight = 40;

            var viewportHeight = scrollViewer.ViewportHeight;
            var currentOffset = scrollViewer.VerticalOffset;

            var itemTop = index * itemHeight;
            var itemBottom = (index + 1) * itemHeight;

            var itemTopVisible = itemTop - currentOffset;
            var itemBottomVisible = itemBottom - currentOffset;

            bool shouldScroll = false;
            double targetOffset = 0;

            if (_lastScrollIndex < index)
            {
                if (itemBottomVisible > viewportHeight)
                {
                    targetOffset = (index + 1) * itemHeight - viewportHeight;
                    shouldScroll = true;
                }
            }
            else if (_lastScrollIndex > index)
            {
                if (itemTopVisible < 0)
                {
                    targetOffset = index * itemHeight;
                    shouldScroll = true;
                }
            }

            _lastScrollIndex = index;

            if (!shouldScroll) return;

            var maxOffset = scrollViewer.ScrollableHeight;
            if (targetOffset > maxOffset) targetOffset = maxOffset;
            if (targetOffset < 0) targetOffset = 0;

            scrollViewer.ScrollToVerticalOffset(targetOffset);
        }), DispatcherPriority.Loaded);
    }

    private static ScrollViewer? FindParentScrollViewer(DependencyObject child)
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent != null)
        {
            if (parent is ScrollViewer sv) return sv;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }
}

public class FileListItem : ContentControl
{
    internal ThumbnailPanel? Owner { get; set; }

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(FileListItem),
            new PropertyMetadata(false, OnIsSelectedChanged));

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FileListItem item)
        {
            item.UpdateIndicator();
        }
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public ImageItem? ImageItem
    {
        get => DataContext as ImageItem;
        set => DataContext = value;
    }

    private static readonly Brush SelectedIndicatorBrush = new SolidColorBrush(Color.FromRgb(0, 103, 192));
    private static readonly Brush HoverBackground = new SolidColorBrush(Color.FromRgb(51, 51, 51));

    public FileListItem()
    {
        Background = Brushes.Transparent;
        Cursor = System.Windows.Input.Cursors.Hand;
    }

    private void UpdateIndicator()
    {
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (!IsSelected)
            Background = HoverBackground;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (!IsSelected)
            Background = Brushes.Transparent;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (ImageItem != null && Owner != null)
        {
            var idx = Owner.Items.IndexOf(ImageItem);
            if (idx >= 0) Owner.NotifyItemClicked(idx);
        }
        e.Handled = true;
    }
}
