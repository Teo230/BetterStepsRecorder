using BetterStepsRecorder.WPF.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Automation;
using static BetterStepsRecorder.WPF.Utilities.WindowHelper;

namespace BetterStepsRecorder.WPF.Services
{
    internal class ScreenCaptureService : IScreenCaptureService
    {
        private IntPtr _hookID = IntPtr.Zero;
        private bool _isCapturing = false;
        private LowLevelMouseProc _proc;

        public event EventHandler<ScreenshotInfo> OnScreenshotCaptured;

        /// <summary>
        /// Delegate for the low-level mouse hook callback
        /// </summary>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The message identifier</param>
        /// <param name="lParam">A pointer to the message data</param>
        /// <returns>The result of the hook processing</returns>
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// P/Invoke declaration for the SetWindowsHookEx function
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        public ScreenCaptureService()
        {
            _proc = HookCallback;
        }

        public void StartCapturing()
        {
            _hookID = SetHook(_proc);
            _isCapturing = true;
        }

        public void StopCapturing()
        {
            UnhookWindowsHookEx(_hookID);
            _isCapturing = false;
        }

        /// <summary>
        /// Sets up the Windows hook for capturing mouse events
        /// </summary>
        /// <param name="proc">The callback procedure for the hook</param>
        /// <returns>A handle to the hook</returns>
        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule? curModule = curProcess.MainModule)
            {
                if (curModule != null)
                {
                    return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                }
                else
                {
                    // Handle the case where MainModule is null
                    throw new InvalidOperationException("The process does not have a main module.");
                }
            }
        }

        /// <summary>
        /// Callback function for processing mouse events
        /// </summary>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The message identifier</param>
        /// <param name="lParam">A pointer to the message data</param>
        /// <returns>The result of the hook processing</returns>
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (!_isCapturing)
                return CallNextHookEx(_hookID, nCode, wParam, lParam);

            if (nCode < 0 || MouseMessages.WM_LBUTTONDOWN != (MouseMessages)wParam)
                return CallNextHookEx(_hookID, nCode, wParam, lParam);

            if (!GetCursorPos(out POINT cursorPos))
                return CallNextHookEx(_hookID, nCode, wParam, lParam);

            IntPtr hwnd = WindowFromPoint(cursorPos);
            if (hwnd != IntPtr.Zero)
            {
                string? windowTitle = GetTopLevelWindowTitle(hwnd);
                string? applicationName = GetApplicationName(hwnd);

                // Get UI Element coordinates and size
                GetWindowRect(hwnd, out RECT UIrect);
                int UIWidth = UIrect.Right - UIrect.Left;
                int UIHeight = UIrect.Bottom - UIrect.Top;

                // Get window coordinates and size
                RECT rect = GetTopLevelWindowRect(hwnd);
                int windowWidth = rect.Right - rect.Left;
                int windowHeight = rect.Bottom - rect.Top;

                // Get UI element under cursor using FlaUI
                AutomationElement? element = GetElementFromPoint(new System.Drawing.Point(cursorPos.X, cursorPos.Y));
                string? elementName = null;
                string? elementType = null;

                if (element != null)
                {
                    elementName = element.Current.Name;
                    elementType = element.Current.ControlType.ToString();
                }

                // Take screenshot of the window
                string? screenshotb64 = SaveScreenRegionScreenshot(rect.Left, rect.Top, windowWidth, windowHeight);

                // Send screenshot event
                if (!string.IsNullOrEmpty(screenshotb64))
                {
                    var screenshotInfo = new ScreenshotInfo
                    {
                        WindowTitle = windowTitle,
                        ApplicationName = applicationName,
                        ElementName = elementName,
                        Description = $"Captured at {DateTime.Now:HH:mm:ss}",
                        ElementType = elementType,
                        UIWidth = UIWidth,
                        UIHeight = UIHeight,
                        WindowWidth = windowWidth,
                        WindowHeight = windowHeight,
                        MousePosition = new POINT { X = cursorPos.X, Y = cursorPos.Y },
                        ScreenshotBase64 = screenshotb64
                    };
                    OnScreenshotCaptured?.Invoke(this, screenshotInfo);
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Captures a screenshot of a specific region of the screen and returns it as a Base64 string
        /// </summary>
        /// <param name="x">X coordinate of the top-left corner</param>
        /// <param name="y">Y coordinate of the top-left corner</param>
        /// <param name="width">Width of the region to capture</param>
        /// <param name="height">Height of the region to capture</param>
        /// <returns>Base64 string representation of the screenshot, or null if capture failed</returns>
        private string? SaveScreenRegionScreenshot(int x, int y, int width, int height)
        {
            try
            {
                // Create a bitmap of the specified size
                Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                // Create graphics object from the bitmap
                using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    // Copy the specified screen area to the bitmap
                    gfx.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);

                    // Draw an arrow pointing at the cursor
                    //DrawArrowAtCursor(gfx, width, height, x, y);
                    DrawRedDot(gfx, width, height, x, y);
                    DrawCursor(gfx, width, height, x, y);
                }

                // Convert the bitmap to a memory stream
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    byte[] imageBytes = ms.ToArray();

                    // Convert byte array to Base64 string
                    string base64String = Convert.ToBase64String(imageBytes);

                    // Dispose of the bitmap
                    bmp.Dispose();

                    // Return the Base64 string
                    return base64String;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to capture screenshot: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Draws an arrow pointing to the current cursor position on the given graphics object
        /// </summary>
        /// <param name="gfx">Graphics object to draw on</param>
        /// <param name="width">Width of the bitmap</param>
        /// <param name="height">Height of the bitmap</param>
        /// <param name="offsetX">X offset of the bitmap</param>
        /// <param name="offsetY">Y offset of the bitmap</param>
        private void DrawArrowAtCursor(Graphics gfx, int width, int height, int offsetX, int offsetY)
        {
            // Define the arrow properties
            Pen arrowPen = new Pen(Color.Magenta, 5);
            arrowPen.EndCap = System.Drawing.Drawing2D.LineCap.Custom;
            arrowPen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5); // Bigger arrow head

            // Define the length of the arrow
            int arrowLength = 200;

            // Get the current cursor position
            POINT cursorPos;
            GetCursorPos(out cursorPos);

            // Convert the screen coordinates to bitmap coordinates
            int cursorX = cursorPos.X - offsetX;
            int cursorY = cursorPos.Y - offsetY;

            // Determine arrow direction: down if in top half, up if in bottom half
            int endX, endY;
            if (cursorY < height / 2)
            {
                // Cursor is in the top half, arrow points down
                endX = cursorX;
                endY = cursorY + arrowLength;
            }
            else
            {
                // Cursor is in the bottom half, arrow points up
                endX = cursorX;
                endY = cursorY - arrowLength;
            }

            // Draw the arrow
            gfx.DrawLine(arrowPen, endX, endY, cursorX, cursorY);
        }

        private void DrawRedDot(Graphics gfx, int width, int height, int offsetX, int offsetY)
        {
            // Get the current cursor position in screen coordinates
            if (!GetCursorPos(out POINT cursorPos))
                return;

            // Convert screen coordinates to bitmap coordinates
            int cursorX = cursorPos.X - offsetX;
            int cursorY = cursorPos.Y - offsetY;

            // Define the size of the red dot
            int dotDiameter = 12;
            int dotRadius = dotDiameter / 2;

            // Optionally, draw a black border around the dot for visibility
            using (Pen borderPen = new Pen(Color.Red, 2))
            {
                gfx.DrawEllipse(borderPen, cursorX - dotRadius, cursorY - dotRadius, dotDiameter, dotDiameter);
            }

            gfx.FillEllipse(Brushes.Red, cursorX - dotRadius, cursorY - dotRadius, dotDiameter, dotDiameter);
        }

        private void DrawCursor(Graphics gfx, int width, int height, int offsetX, int offsetY)
        {
            // Get the current cursor position in screen coordinates
            if (!GetCursorPos(out POINT cursorPos))
                return;

            // Convert screen coordinates to bitmap coordinates
            int cursorX = cursorPos.X - offsetX;
            int cursorY = cursorPos.Y - offsetY;

            // Capture the cursor
            CURSORINFO cursorInfo = new CURSORINFO();
            cursorInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            if (GetCursorInfo(out cursorInfo) && cursorInfo.flags == 1) // 1 means cursor is visible
            {
                IntPtr hdc = gfx.GetHdc();
                try
                {
                    // Draw the cursor at the correct position relative to the bitmap
                    DrawIcon(hdc, cursorX, cursorY, cursorInfo.hCursor);
                }
                finally
                {
                    gfx.ReleaseHdc(hdc);
                }
            }
        }
    }

    internal interface IScreenCaptureService
    {
        void StartCapturing();
        void StopCapturing();
        event EventHandler<ScreenshotInfo> OnScreenshotCaptured;
    }

    internal class ScreenshotInfo()
    {
        internal Guid ID { get; } = Guid.NewGuid();
        internal string? ScreenshotBase64 { get; set; }
        internal int Step { get; set; }

        [Category("Application")]
        public string? WindowTitle { get; internal set; }
        [Category("Application")]
        public string? ApplicationName { get; internal set; }
        [Category("Detail"), DisplayName("1. Name")]
        public string? ElementName { get; set; }
        [Category("Detail"), DisplayName("2. Description")]
        public string? Description { get; set; }
        [Category("Application")]
        internal string? ElementType { get; set; }
        [Category("Application")]
        public int UIWidth { get; internal set; }
        [Category("Application")]
        public int UIHeight { get; internal set; }
        [Category("Application")]
        public int WindowWidth { get; internal set; }
        [Category("Application")]
        public int WindowHeight { get; internal set; }
        [Category("Application")]
        public POINT MousePosition { get; internal set; }
    }
}
