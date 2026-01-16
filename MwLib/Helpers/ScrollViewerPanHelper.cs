using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MwLib.Helpers;

public static class ScrollViewerPanHelper
{
    // ScrollViewer ごとの状態を保持
    private sealed class PanState
    {
        public bool IsDragging;
        public Point StartPoint;
        public double StartHOffset;
        public double StartVOffset;
    }

    private static readonly ConditionalWeakTable<ScrollViewer, PanState> _states
        = [];

    public static void AttachMiddleButtonPan(ScrollViewer sv)
    {
        ArgumentNullException.ThrowIfNull(sv);

        var state = _states.GetOrCreateValue(sv);

        sv.PreviewMouseDown += (s, e) =>
        {
            if (e.ChangedButton != MouseButton.Middle)
                return;

            state.IsDragging = true;
            state.StartPoint = e.GetPosition(sv);
            state.StartHOffset = sv.HorizontalOffset;
            state.StartVOffset = sv.VerticalOffset;

            sv.CaptureMouse();
            e.Handled = true;
        };

        sv.PreviewMouseMove += (s, e) =>
        {
            if (!state.IsDragging)
                return;

            Point p = e.GetPosition(sv);
            Vector delta = p - state.StartPoint;

            sv.ScrollToHorizontalOffset(state.StartHOffset - delta.X);
            sv.ScrollToVerticalOffset(state.StartVOffset - delta.Y);

            e.Handled = true;
        };

        sv.PreviewMouseUp += (s, e) =>
        {
            if (e.ChangedButton != MouseButton.Middle)
                return;

            EndPan(sv, state);
            e.Handled = true;
        };

        sv.LostMouseCapture += (s, e) =>
        {
            EndPan(sv, state);
        };
    }

    private static void EndPan(ScrollViewer sv, PanState state)
    {
        if (!state.IsDragging)
            return;

        state.IsDragging = false;
        sv.ReleaseMouseCapture();
    }
}
