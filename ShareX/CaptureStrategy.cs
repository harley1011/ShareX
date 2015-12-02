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
using System;
using System.Drawing;
using System.Threading;

namespace ShareX
{
    public abstract class CaptureStrategy
    {
        protected delegate Image ScreenCaptureDelegate();
        protected MainForm mainForm { get; set; }
        protected enum LastRegionCaptureType { Surface, Light, Transparent, Annotate }
        protected LastRegionCaptureType lastRegionCaptureType = LastRegionCaptureType.Surface;

        public CaptureStrategy(MainForm mainForm)
        {
            this.mainForm = mainForm;
        }

        abstract public void Capture(CaptureType captureType, TaskSettings taskSettings, bool autoHideForm = true);

        protected void DoCapture(ScreenCaptureDelegate capture, CaptureType captureType, TaskSettings taskSettings = null, bool autoHideForm = true)
        {
            if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();

            if (taskSettings.CaptureSettings.IsDelayScreenshot && taskSettings.CaptureSettings.DelayScreenshot > 0)
            {
                TaskEx.Run(() =>
                {
                    int sleep = (int)(taskSettings.CaptureSettings.DelayScreenshot * 1000);
                    Thread.Sleep(sleep);
                },
                () =>
                {
                    DoCaptureWork(capture, captureType, taskSettings, autoHideForm);
                });
            }
            else
            {
                DoCaptureWork(capture, captureType, taskSettings, autoHideForm);
            }
        }

        protected void DoCaptureWork(ScreenCaptureDelegate capture, CaptureType captureType, TaskSettings taskSettings, bool autoHideForm = true)
        {
            if (autoHideForm)
            {
                mainForm.Hide();
                Thread.Sleep(250);
            }

            Image img = null;

            try
            {
                Screenshot.CaptureCursor = taskSettings.CaptureSettings.ShowCursor;
                Screenshot.CaptureShadow = taskSettings.CaptureSettings.CaptureShadow;
                Screenshot.ShadowOffset = taskSettings.CaptureSettings.CaptureShadowOffset;
                Screenshot.CaptureClientArea = taskSettings.CaptureSettings.CaptureClientArea;
                Screenshot.AutoHideTaskbar = taskSettings.CaptureSettings.CaptureAutoHideTaskbar;

                img = capture();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteException(ex);
            }
            finally
            {
                if (autoHideForm)
                {
                    mainForm.ShowActivate();
                }

                AfterCapture(img, captureType, taskSettings);
            }
        }

        protected void AfterCapture(Image img, CaptureType captureType, TaskSettings taskSettings)
        {
            if (img != null)
            {
                if (taskSettings.GeneralSettings.PlaySoundAfterCapture)
                {
                    TaskHelpers.PlayCaptureSound(taskSettings);
                }

                if (taskSettings.ImageSettings.ImageEffectOnlyRegionCapture && !IsRegionCapture(captureType))
                {
                    taskSettings.AfterCaptureJob = taskSettings.AfterCaptureJob.Remove(AfterCaptureTasks.AddImageEffects);
                }

                string customFileName;

                if (TaskHelpers.ShowAfterCaptureForm(taskSettings, out customFileName, img))
                {
                    UploadManager.RunImageTask(img, taskSettings, customFileName);
                }
            }
        }

        protected bool IsRegionCapture(CaptureType captureType)
        {
            return captureType.HasFlagAny(CaptureType.RectangleWindow, CaptureType.Rectangle, CaptureType.Polygon, CaptureType.Freehand, CaptureType.LastRegion);
        }
    }
}