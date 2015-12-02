#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2015 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using ShareX.HelpersLib;
using ShareX.ScreenCaptureLib;
using System.Drawing;
using System.Windows.Forms;

namespace ShareX
{
    public class CaptureRegion : CaptureStrategy
    {
        public CaptureRegion(MainForm mainForm)
            : base(mainForm)
        { }

        public override void Capture(CaptureType captureType, TaskSettings taskSettings, bool autoHideForm = true)
        {
            Surface surface;

            switch (captureType)
            {
                default:
                case CaptureType.Rectangle:
                    surface = new RectangleRegion();
                    break;
                case CaptureType.RectangleWindow:
                    RectangleRegion rectangleRegion = new RectangleRegion();
                    rectangleRegion.AreaManager.WindowCaptureMode = true;
                    rectangleRegion.AreaManager.IncludeControls = true;
                    surface = rectangleRegion;
                    break;
                case CaptureType.Polygon:
                    surface = new PolygonRegion();
                    break;
                case CaptureType.Freehand:
                    surface = new FreeHandRegion();
                    break;
            }

            DoCapture(() =>
            {
                Image img = null;
                Image screenshot = Screenshot.CaptureFullscreen();

                try
                {
                    surface.Config = taskSettings.CaptureSettingsReference.SurfaceOptions;
                    surface.SurfaceImage = screenshot;
                    surface.Prepare();
                    surface.ShowDialog();

                    if (surface.Result == SurfaceResult.Region)
                    {
                        using (screenshot)
                        {
                            img = surface.GetRegionImage();
                        }
                    }
                    else if (surface.Result == SurfaceResult.Fullscreen)
                    {
                        img = screenshot;
                    }
                    else if (surface.Result == SurfaceResult.Monitor)
                    {
                        Screen[] screens = Screen.AllScreens;

                        if (surface.MonitorIndex < screens.Length)
                        {
                            Screen screen = screens[surface.MonitorIndex];
                            Rectangle screenRect = CaptureHelpers.ScreenToClient(screen.Bounds);

                            using (screenshot)
                            {
                                img = ImageHelpers.CropImage(screenshot, screenRect);
                            }
                        }
                    }
                    else if (surface.Result == SurfaceResult.ActiveMonitor)
                    {
                        Rectangle activeScreenRect = CaptureHelpers.GetActiveScreenBounds0Based();

                        using (screenshot)
                        {
                            img = ImageHelpers.CropImage(screenshot, activeScreenRect);
                        }
                    }

                    if (img != null)
                    {
                        lastRegionCaptureType = LastRegionCaptureType.Surface;
                    }
                }
                finally
                {
                    surface.Dispose();
                }

                return img;
            }, captureType, taskSettings, autoHideForm);
        }
    }
}