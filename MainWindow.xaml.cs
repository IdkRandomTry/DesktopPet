using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Markup.Xaml;

namespace Pet
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private Point[] _tailPoints;
        private const int MaxTailLength = 25;
        private DispatcherTimer _timer;
        private Polyline? mouseTail;

        public MainWindow()
        {
            InitializeComponent();
            
            // Find the mouseTail control
            mouseTail = this.FindControl<Polyline>("mouseTail");

            // Initialize the array
            _tailPoints = new Point[MaxTailLength];
            for (int i = 0; i < MaxTailLength; i++)
            {
                _tailPoints[i] = new Point(0, 0);
            }

            // Set window to be topmost (already set in XAML)
            this.Topmost = true;

            // Allow the window to be dragged by clicking anywhere on it
            this.PointerPressed += (sender, e) =>
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    this.BeginMoveDrag(e);
                }
            };

            // Close the window when Escape key is pressed
            this.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    this.Close();
                }
            };

            // Set up a timer to track mouse position globally
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Platform-specific cursor tracking
            Point mousePosition;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows-specific cursor tracking
                if (GetCursorPos(out POINT cursorPos))
                {
                    // Convert screen coordinates to window coordinates
                    var screenPoint = new PixelPoint(cursorPos.X + 5, cursorPos.Y + 5);
                    mousePosition = this.PointToClient(screenPoint);
                }
                else
                {
                    return;
                }
            }
            else
            {
                // For Linux/macOS, use Avalonia's pointer position
                // This is a fallback - you might need platform-specific implementations
                var pointerPos = this.PointToClient(PointerPosition);
                mousePosition = pointerPos;
            }

            // Update each point to follow the previous one
            // First point follows the mouse
            _tailPoints[0] = new Point(
                _tailPoints[0].X + (mousePosition.X - _tailPoints[0].X) * 0.5,
                _tailPoints[0].Y + (mousePosition.Y - _tailPoints[0].Y) * 0.5
            );

            // Each subsequent point follows the previous point
            for (int i = 1; i < MaxTailLength; i++)
            {
                _tailPoints[i] = new Point(
                    _tailPoints[i].X + (_tailPoints[i - 1].X - _tailPoints[i].X) * 0.5,
                    _tailPoints[i].Y + (_tailPoints[i - 1].Y - _tailPoints[i].Y) * 0.5
                );
            }

            // Update the Polyline element
            if (mouseTail != null)
            {
                mouseTail.Points = new Avalonia.Collections.AvaloniaList<Point>(_tailPoints);
            }
        }

        private PixelPoint PointerPosition
        {
            get
            {
                // Get the current pointer position relative to screen
                // This is a simplified version - might need refinement for non-Windows platforms
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && GetCursorPos(out POINT pt))
                {
                    return new PixelPoint(pt.X, pt.Y);
                }
                return new PixelPoint(0, 0);
            }
        }
    }
}
