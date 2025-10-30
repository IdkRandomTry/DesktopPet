using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Pet
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private const int SM_CXSCREEN = 0; // Width of the screen of the primary display monitor

        private Point[] _tailPoints;
        private const int MaxTailLength = 25;
        private DispatcherTimer _timer;
        private const double CloseButtonThreshold = 0.9; 

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the array
            _tailPoints = new Point[MaxTailLength];
            for (int i = 0; i < MaxTailLength; i++)
            {
                _tailPoints[i] = new Point(0, 0);
            }

            // Set window to be topmost so it appears above other windows
            this.Topmost = true;

            // Allow the window to be dragged by clicking anywhere on it
            this.MouseLeftButtonDown += (sender, e) => this.DragMove();

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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Get the current mouse position relative to the screen
            if (GetCursorPos(out POINT cursorPos))
            {
                // Get actual physical screen width (not DPI-scaled)
                int screenWidth = GetSystemMetrics(SM_CXSCREEN);
                double thresholdX = screenWidth * CloseButtonThreshold;

                // Show/hide close button based on mouse X position
                closeButton.Visibility = cursorPos.X > thresholdX ? Visibility.Visible : Visibility.Collapsed;

                // Convert screen coordinates to WPF device-independent pixels
                Point screenPoint = new Point(cursorPos.X + 5, cursorPos.Y + 5);

                // Convert to window coordinates
                Point mousePosition = this.PointFromScreen(screenPoint);

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
                mouseTail.Points = new PointCollection(_tailPoints);
            }
        }
    }
}
