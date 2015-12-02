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

namespace ShareX
{
    internal class CaptureFactory
    {
        //Singleton: one instance of factory
        private static CaptureFactory instance;

        private CaptureFactory()
        {
        }

        public static CaptureFactory getInstance()
        {
            if (instance == null)
            {
                instance = new CaptureFactory();
            }
            return instance;
        }

        public CaptureStrategy GetStrategy(CaptureType captureType, MainForm mainForm)
        {
            CaptureStrategy strategy = null;

            switch (captureType)
            {
                case CaptureType.Screen:
                    strategy = new CaptureScreen(mainForm);
                    break;
                case CaptureType.ActiveWindow:
                    strategy = new CaptureActiveWindow(mainForm);
                    break;
                case CaptureType.ActiveMonitor:
                    strategy = new CaptureActiveMonitor(mainForm);
                    break;
                case CaptureType.Rectangle:
                case CaptureType.RectangleWindow:
                case CaptureType.Polygon:
                case CaptureType.Freehand:
                    strategy = new CaptureRegion(mainForm);
                    break;
                case CaptureType.CustomRegion:
                    strategy = new CaptureCustomRegion(mainForm);
                    break;
                case CaptureType.LastRegion:
                    strategy = new CaptureLastRegion(mainForm);
                    break;
            }
            return strategy;
        }
    }
}