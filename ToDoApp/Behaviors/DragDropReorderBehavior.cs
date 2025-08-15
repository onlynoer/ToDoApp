using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ToDoApp.Behaviors
{
    /// <summary>
    /// Attach to any ItemsControl to allow drag-reorder of its items.
    /// Works with StackPanel/VirtualizingStackPanel and WrapPanel.
    /// For "Add New" placeholder scenarios, set KeepLastItemFixed=true so the last item won't be moved past.
    /// </summary>
    public static class DragDropReorderBehavior
    {
        public static readonly DependencyProperty EnableReorderProperty =
            DependencyProperty.RegisterAttached(
                "EnableReorder",
                typeof(bool),
                typeof(DragDropReorderBehavior),
                new PropertyMetadata(false, OnEnableReorderChanged));

        public static void SetEnableReorder(DependencyObject element, bool value) => element.SetValue(EnableReorderProperty, value);
        public static bool GetEnableReorder(DependencyObject element) => (bool)element.GetValue(EnableReorderProperty);

        public static readonly DependencyProperty KeepLastItemFixedProperty =
            DependencyProperty.RegisterAttached(
                "KeepLastItemFixed",
                typeof(bool),
                typeof(DragDropReorderBehavior),
                new PropertyMetadata(false));

        public static void SetKeepLastItemFixed(DependencyObject element, bool value) => element.SetValue(KeepLastItemFixedProperty, value);
        public static bool GetKeepLastItemFixed(DependencyObject element) => (bool)element.GetValue(KeepLastItemFixedProperty);

        private static Point _dragStartPoint;
        private static object _draggedData;

        private static void OnEnableReorderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ItemsControl ic) return;

            if ((bool)e.NewValue)
            {
                ic.AllowDrop = true;
                ic.PreviewMouseLeftButtonDown += Ic_PreviewMouseLeftButtonDown;
                ic.MouseMove += Ic_MouseMove;
                ic.DragOver += Ic_DragOver;
                ic.Drop += Ic_Drop;
            }
            else
            {
                ic.PreviewMouseLeftButtonDown -= Ic_PreviewMouseLeftButtonDown;
                ic.MouseMove -= Ic_MouseMove;
                ic.DragOver -= Ic_DragOver;
                ic.Drop -= Ic_Drop;
            }
        }

        private static void Ic_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private static void Ic_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not ItemsControl ic) return;
            if (e.LeftButton != MouseButtonState.Pressed) return;

            var diff = _dragStartPoint - e.GetPosition(null);
            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            var container = GetContainerUnderMouse(ic, e);
            if (container == null) return;

            var data = ic.ItemContainerGenerator.ItemFromContainer(container);
            if (data == DependencyProperty.UnsetValue) return;

            _draggedData = data;
            DragDrop.DoDragDrop(ic, data, DragDropEffects.Move);
        }

        private static void Ic_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private static void Ic_Drop(object sender, DragEventArgs e)
        {
            if (sender is not ItemsControl ic) return;
            if (_draggedData == null) return;

            var items = ic.ItemsSource as IList ?? ic.Items;
            if (items == null) return;

            // Determine target index
            var container = GetContainerUnderMouse(ic, e);
            int newIndex;
            if (container != null)
            {
                newIndex = ic.ItemContainerGenerator.IndexFromContainer(container);
            }
            else
            {
                // drop to end
                newIndex = items.Count - 1;
            }

            // Handle "keep last item fixed" (e.g., AddGroup placeholder)
            if (GetKeepLastItemFixed(ic))
            {
                newIndex = Math.Min(newIndex, items.Count - 2); // last index reserved
            }

            int oldIndex = IndexOf(items, _draggedData);
            if (oldIndex < 0) { _draggedData = null; return; }

            if (newIndex < 0) newIndex = 0;
            if (newIndex > items.Count - 1) newIndex = items.Count - 1;

            if (oldIndex != newIndex)
            {
                Move(items, oldIndex, newIndex);
            }

            _draggedData = null;
        }

        private static int IndexOf(IList list, object item)
        {
            for (int i = 0; i < list.Count; i++)
                if (Equals(list[i], item)) return i;
            return -1;
        }

        private static void Move(IList list, int oldIndex, int newIndex)
        {
            var item = list[oldIndex];
            list.RemoveAt(oldIndex);
            // adjust index after removal
            if (newIndex > oldIndex) newIndex--;
            list.Insert(newIndex, item);
        }

        private static DependencyObject GetContainerUnderMouse(ItemsControl ic, DragEventArgs e)
        {
            var pos = e.GetPosition(ic);
            return GetContainerAtPoint(ic, pos);
        }

        private static DependencyObject GetContainerUnderMouse(ItemsControl ic, MouseEventArgs e)
        {
            var pos = e.GetPosition(ic);
            return GetContainerAtPoint(ic, pos);
        }

        private static DependencyObject GetContainerAtPoint(ItemsControl ic, Point point)
        {
            var element = ic.InputHitTest(point) as DependencyObject;
            while (element != null)
            {
                if (element is ContentPresenter || element is ListBoxItem || element is ListViewItem)
                {
                    // Is this an item container?
                    var item = ic.ItemContainerGenerator.ItemFromContainer(element);
                    if (item != DependencyProperty.UnsetValue)
                        return element;
                }
                element = VisualTreeHelper.GetParent(element);
            }
            return null;
        }
    }
}
