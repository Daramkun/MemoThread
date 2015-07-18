using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Marketplace;

namespace MemoThread
{
	public partial class PhotoViewPage : PhoneApplicationPage
	{
		float zoomTotal = 1;

		public PhotoViewPage ()
		{
			InitializeComponent ();

			imgPhoto.Source = ForPhotoView.imageSource;
			imgPhoto.Width = ( ForPhotoView.imageSource as BitmapImage ).PixelWidth;
			imgPhoto.Height = ( ForPhotoView.imageSource as BitmapImage ).PixelHeight;
			
			if ( new LicenseInformation ().IsTrial () )
			{
				TouchPanel.EnabledGestures = GestureType.Pinch;
				Touch.FrameReported += ( object sender, TouchFrameEventArgs e ) =>
				{
					while ( TouchPanel.IsGestureAvailable )
					{
						GestureSample gestureSample = TouchPanel.ReadGesture ();

						switch ( gestureSample.GestureType )
						{
							case GestureType.Pinch:
								Vector2 FirstFingerCurrentPosition = gestureSample.Position;
								Vector2 SecondFingerCurrentPosition = gestureSample.Position2;
								Vector2 FirstFingerPreviousPosition = FirstFingerCurrentPosition - gestureSample.Delta;
								Vector2 SecondFingerPreviousPosition = SecondFingerCurrentPosition - gestureSample.Delta2;
								float CurentPositionFingerDistance = Vector2.Distance (
									FirstFingerCurrentPosition, SecondFingerCurrentPosition );
								float PreviousPositionFingerDistance = Vector2.Distance (
									FirstFingerPreviousPosition, SecondFingerPreviousPosition );
								float zoomDelta = ( CurentPositionFingerDistance -
									PreviousPositionFingerDistance ) * .003f;

								if ( zoomTotal + zoomDelta >= 0.2f && zoomTotal + zoomDelta <= 3.0f )
									zoomTotal += zoomDelta;

								imgPhoto.Width = ( ForPhotoView.imageSource as BitmapImage ).PixelWidth * zoomTotal;
								imgPhoto.Height = ( ForPhotoView.imageSource as BitmapImage ).PixelHeight * zoomTotal;
								break;
						}
						UpdateLayout ();
					}
				};
			}
		}
	}
}