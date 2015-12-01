/* *
 * Copyright (C) 2015 Christopher Herrick
 * 
 * This file is part of the FluxLib library.
 *
 * The FluxLib library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.

 * The FluxLib library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public License
 * along with the FluxLib library.  If not, see <http://www.gnu.org/licenses/>.
 * */

using System;
using Microsoft.Win32;
using Windows.Graphics.Display;

namespace FluxLib.Microsoft.Win32;
{
   public class DeviceOrientationMonitor : IDisposable
   {
      #region Events
      #region Delegates
      public delegate void DeviceOrientationChangedDelegate( DeviceOrientationMonitor sender, DeviceOrientationChangedEventArgs args );
      public delegate void ScreenOrientationChangedDelegate( DeviceOrientationMonitor sender, ScreenOrientationChangedEventArgs args );
      #endregion

      public event DeviceOrientationChangedDelegate DeviceOrientationChanged;
      public event ScreenOrientationChangedDelegate ScreenOrientationChanged;
      #endregion

      public DeviceOrientationMonitor( Boolean beginMonitoring = true )
      {
         IsMonitoring = false;

         CurrentDeviceOrientation = GetDeviceOrientation( DisplayProperties.CurrentOrientation );

         DeviceOrientationChanged += DeviceOrientationMonitor_DeviceOrientationChanged;
         SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

         IsMonitoring = beginMonitoring;
      }

      private void SystemEvents_DisplaySettingsChanged( Object sender, EventArgs e )
      {
         if( IsMonitoring )
         {
            DeviceOrientations previousDeviceOrientation = CurrentDeviceOrientation;
            DeviceOrientations currentDeviceOrientation = GetDeviceOrientation( DisplayProperties.CurrentOrientation );

            CurrentDeviceOrientation = currentDeviceOrientation;

            if( previousDeviceOrientation != currentDeviceOrientation && DeviceOrientationChanged != null )
               DeviceOrientationChanged.Invoke( this, new DeviceOrientationChangedEventArgs( previousDeviceOrientation, currentDeviceOrientation ) );
         }
      }

      private void DeviceOrientationMonitor_DeviceOrientationChanged( DeviceOrientationMonitor sender, DeviceOrientationChangedEventArgs args )
      {
         if( IsMonitoring )
         {
            ScreenOrientations previousOrientation = args.PreviousScreenOrientation;
            ScreenOrientations currentOrientation = args.CurrentScreenOrientation;

            if( previousOrientation != currentOrientation && ScreenOrientationChanged != null )
               ScreenOrientationChanged.Invoke( sender, new ScreenOrientationChangedEventArgs( previousOrientation, currentOrientation ) );
         }
      }

      protected readonly Object IsMonitoringLock = new Object();

      private Boolean isMonitoring;

      public Boolean IsMonitoring
      {
         get
         {
            lock( IsMonitoringLock )
               return isMonitoring;
         }

         protected set
         {
            lock( IsMonitoringLock )
               isMonitoring = value;
         }
      }

      protected readonly Object CurrentDeviceOrientationLock = new Object();

      private DeviceOrientations currentDeviceOrientation;

      public DeviceOrientations CurrentDeviceOrientation
      {
         get
         {
            lock( CurrentDeviceOrientationLock )
               return currentDeviceOrientation;
         }

         protected set
         {
            lock( CurrentDeviceOrientationLock )
               currentDeviceOrientation = value;
         }
      }

      protected static DeviceOrientations GetDeviceOrientation( DisplayOrientations orientation )
      {
         DeviceOrientations result;

         switch( orientation )
         {
            case DisplayOrientations.LandscapeFlipped:
               result = DeviceOrientations.Landscape_Reversed;
               break;
            case DisplayOrientations.Portrait:
               result = DeviceOrientations.Portrait_Upright;
               break;
            case DisplayOrientations.PortraitFlipped:
               result = DeviceOrientations.Portrait_Reversed;
               break;
            case DisplayOrientations.Landscape:
               result = DeviceOrientations.Landscape_Upright;
               break;
            case DisplayOrientations.None:
            default:
               throw new NotSupportedException( String.Format( "Display orientation '{0}' is not supported.", orientation ) );
         }

         return result;
      }

      public ScreenOrientations CurrentScreenOrientation
      {
         get
         {
            return DeviceOrientationUtilities.GetScreenOrientation( CurrentDeviceOrientation );
         }
      }

      #region IDisposable Members
      public void Dispose()
      {
         IsMonitoring = false;

         SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
         DeviceOrientationChanged -= DeviceOrientationMonitor_DeviceOrientationChanged;
      }
      #endregion
   }

   public enum ScreenOrientations
   {
      Landscape,
      Portrait,
   }

   // The order here matters (used when computing the difference).
   // Order adapted from: https://msdn.microsoft.com/en-us/library/windows/apps/windows.graphics.display.displayorientations.aspx
   public enum DeviceOrientations : byte
   {
      Landscape_Upright = 0,
      Portrait_Upright,
      Landscape_Reversed,
      Portrait_Reversed,
   }

   // The order here matters (used when computing the difference).
   // Order adapted from: https://msdn.microsoft.com/en-us/library/windows/apps/windows.graphics.display.displayorientations.aspx
   public enum DeviceOrientationDifference : byte
   {
      None = 0,
      CounterclockwiseRotation,
      Inverted,
      ClockwiseRotation,
   }

   public static class DeviceOrientationUtilities
   {
      public static ScreenOrientations GetScreenOrientation( DeviceOrientations orientation )
      {
         ScreenOrientations result;

         if( orientation == DeviceOrientations.Landscape_Reversed || orientation == DeviceOrientations.Landscape_Upright )
            result = ScreenOrientations.Landscape;
         else // if( orientation == DeviceOrientations.Portrait_Reversed || orientation == DeviceOrientations.Portrait_Upright )
            result = ScreenOrientations.Portrait;

         return result;
      }

      public static Boolean OrientationIsPortrait( DeviceOrientations deviceOrientation )
      {
         return GetScreenOrientation( deviceOrientation ) == ScreenOrientations.Portrait;
      }

      public static Boolean OrientationIsLandscape( DeviceOrientations deviceOrientation )
      {
         return GetScreenOrientation( deviceOrientation ) == ScreenOrientations.Landscape;
      }

      public static DeviceOrientationDifference ComputeOrientationDifference( DeviceOrientations referenceDeviceOrientation, DeviceOrientations targetDeviceOrientation )
      {
         Int32 orientationCount = Enum.GetNames( typeof( DeviceOrientations ) ).Length;

         byte difference = ( byte ) ( ( ( int ) targetDeviceOrientation + orientationCount - ( int ) referenceDeviceOrientation ) % orientationCount );

         DeviceOrientationDifference result = ( DeviceOrientationDifference ) difference;

         return result;
      }
   }

   public class DeviceOrientationChangedEventArgs : EventArgs
   {
      public DeviceOrientationChangedEventArgs( DeviceOrientations previousOrientation, DeviceOrientations currentOrientation )
      {
         PreviousDeviceOrientation = previousOrientation;
         CurrentDeviceOrientation = currentOrientation;
      }

      public DeviceOrientations PreviousDeviceOrientation
      {
         get;
         protected set;
      }

      public DeviceOrientations CurrentDeviceOrientation
      {
         get;
         protected set;
      }

      public DeviceOrientationDifference DeviceOrientationDifference
      {
         get
         {
            return DeviceOrientationUtilities.ComputeOrientationDifference( PreviousDeviceOrientation, CurrentDeviceOrientation );
         }
      }

      public ScreenOrientations PreviousScreenOrientation
      {
         get
         {
            return DeviceOrientationUtilities.GetScreenOrientation( PreviousDeviceOrientation );
         }
      }

      public ScreenOrientations CurrentScreenOrientation
      {
         get
         {
            return DeviceOrientationUtilities.GetScreenOrientation( CurrentDeviceOrientation );
         }
      }
   }

   public class ScreenOrientationChangedEventArgs : EventArgs
   {
      public ScreenOrientationChangedEventArgs( ScreenOrientations previousOrientation, ScreenOrientations currentOrientation )
      {
         PreviousOrientation = previousOrientation;
         CurrentOrientation = currentOrientation;
      }

      public ScreenOrientations PreviousOrientation
      {
         get;
         protected set;
      }

      public ScreenOrientations CurrentOrientation
      {
         get;
         protected set;
      }
   }
}
