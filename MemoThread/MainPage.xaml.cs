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
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Audio;
using System.Windows.Threading;
using Microsoft.Xna.Framework;
using System.IO;
using System.Threading;
using System.Collections;
using Microsoft.Phone.Marketplace;

namespace MemoThread
{
	public partial class MainPage : PhoneApplicationPage
	{
		MemoStorage memoStorage;

		CameraCaptureTask cameraTask = new CameraCaptureTask ();
		bool isRecordStarted = false;
		DispatcherTimer micTimer = new DispatcherTimer ();

		Microphone mic = Microphone.Default;
		PhoneApplicationService phoneAppService = PhoneApplicationService.Current;

		MemoryStream recordStream;

		// 생성자
		public MainPage ()
		{
			InitializeComponent ();

			cameraTask.Completed += ( object sender, PhotoResult e ) =>
			{
				if ( e.TaskResult == TaskResult.Cancel ) return;

				BitmapImage bitmapImage = new BitmapImage ();
				bitmapImage.SetSource ( e.ChosenPhoto );
				DateTime dateTime = DateTime.Now;
				byte [] photoBuffer = new byte [ e.ChosenPhoto.Length ];

				e.ChosenPhoto.Position = 0;
				e.ChosenPhoto.Read ( photoBuffer, 0, ( int ) e.ChosenPhoto.Length );

				Memo memo = new Memo ( MemoType.Photo, photoBuffer, dateTime );
				memoStorage.MemoList.Add ( memo );
				AddItemToStackBottom ( new PhotoThread ( bitmapImage, dateTime ), memo.MemoID );
			};

			mic.BufferDuration = TimeSpan.FromSeconds ( 1 );
			mic.BufferReady += ( object sender, EventArgs e ) =>
			{
				byte [] buffer = new byte [ mic.GetSampleSizeInBytes ( mic.BufferDuration ) ];
				mic.GetData ( buffer, 0, buffer.Length );
				recordStream.Write ( buffer, 0, buffer.Length );
			};

			if ( new LicenseInformation ().IsTrial () )
			{
				//abbRecord.IsEnabled = false;
				( ApplicationBar.Buttons [ 2 ] as ApplicationBarIconButton ).IsEnabled = false;
			}
		}

		private void PhoneApplicationPage_Loaded ( object sender, RoutedEventArgs e )
		{
			micTimer.Interval = TimeSpan.FromTicks ( 333333 );
			micTimer.Tick += ( object s, EventArgs ee ) =>
			{
				FrameworkDispatcher.Update ();
			};
			FrameworkDispatcher.Update ();
			micTimer.Start ();

			new Thread ( () =>
			{
				if ( memoStorage != null ) return;
				memoStorage = new MemoStorage ( ( Memo memo ) =>
				{
					Dispatcher.BeginInvoke ( () =>
					{
						switch ( memo.MemoType )
						{
							case MemoType.Text:
								AddItemToStackTop ( new TextThread ( memo.MemoObject as string, memo.MemoDate ), memo.MemoID );
								break;
							case MemoType.Photo:
								BitmapImage bitmapImage = new BitmapImage ();
								bitmapImage.SetSource ( new MemoryStream ( memo.MemoObject as byte [] ) );
								AddItemToStackTop ( new PhotoThread ( bitmapImage, memo.MemoDate ), memo.MemoID );
								break;
							case MemoType.Voice:
								SoundEffectInstance soundEffect = new SoundEffect ( memo.MemoObject as byte [],
									mic.SampleRate, AudioChannels.Mono ).CreateInstance ();
								AddItemToStackTop ( new VoiceThread ( soundEffect, memo.MemoDate ), memo.MemoID );
								break;
						}
					} );
				} );
			} ).Start ();
		}

		private void SetMenu ( UIElement obj, long id )
		{
			ContextMenu menu = new ContextMenu ();
			MenuItem deleteMenuItem = new MenuItem () { Header = "삭제" };
			deleteMenuItem.Tap += ( object sender, System.Windows.Input.GestureEventArgs e ) =>
			{
				stkThreads.Children.Remove ( obj );
				IEnumerable<Memo> finded = from m in memoStorage.MemoList where m.MemoID == id select m;
				Memo mmm = null;
				foreach ( Memo mm in finded )
					mmm = mm;
				if(mmm != null)
					memoStorage.MemoList.Remove ( mmm );
			};
			menu.Items.Add ( deleteMenuItem );
			ContextMenuService.SetContextMenu ( obj, menu );
		}

		private void AddItemToStackBottom ( UIElement obj, long id )
		{
			SetMenu ( obj, id );
			stkThreads.Children.Add ( obj );

			srlThreads.UpdateLayout ();
			srlThreads.ScrollToVerticalOffset ( stkThreads.ActualHeight );

			Microsoft.Devices.VibrateController.Default.Start ( TimeSpan.FromMilliseconds ( 40 ) );

			memoStorage.Save ();
		}

		private void AddItemToStackTop ( UIElement obj, long id )
		{
			SetMenu ( obj, id );
			stkThreads.Children.Insert ( 0, obj );

			srlThreads.UpdateLayout ();
			srlThreads.ScrollToVerticalOffset ( stkThreads.ActualHeight );
		}

		private void abbSave_Click ( object sender, EventArgs e )
		{
			if ( txtMemo.Text.Trim () == "" )
			{
				MessageBox.Show ( "텍스트가 비어 있습니다!", "메모스레드", MessageBoxButton.OK );
				return;
			}

			DateTime dateTime = DateTime.Now;
			Memo memo = new Memo ( MemoType.Text, txtMemo.Text, dateTime );
			memoStorage.MemoList.Add ( memo );
			AddItemToStackBottom ( new TextThread ( txtMemo.Text, dateTime ), memo.MemoID );

			this.Focus ();
			txtMemo.Text = "";
			txtMemo.Focus ();
		}

		private void abbPhoto_Click ( object sender, EventArgs e )
		{
			try
			{
				cameraTask.Show ();
			}
			catch
			{
				MessageBox.Show ( "카메라를 사용할 수 없습니다!", "메모스레드", MessageBoxButton.OK );
			}
		}

		private void abbRecord_Click ( object sender, EventArgs e )
		{
			switch ( isRecordStarted )
			{
				case false:
					{
						isRecordStarted = true;
						SystemTray.ProgressIndicator.IsVisible = true;
						recordStream = new MemoryStream ();
						mic.Start ();

						phoneAppService.UserIdleDetectionMode = IdleDetectionMode.Disabled;
					}
					break;
				case true:
					{
						isRecordStarted = false;
						SystemTray.ProgressIndicator.IsVisible = false;
						mic.Stop ();

						SoundEffectInstance soundEffect = new SoundEffect ( recordStream.ToArray (),
							mic.SampleRate, AudioChannels.Mono ).CreateInstance ();

						DateTime dateTime = DateTime.Now;
						Memo memo = new Memo ( MemoType.Voice, recordStream.ToArray (), dateTime );
						memoStorage.MemoList.Add ( memo );
						AddItemToStackBottom ( new VoiceThread ( soundEffect, dateTime ), memo.MemoID );
						
						soundEffect = null;

						recordStream.Dispose ();
						recordStream = null;

						phoneAppService.UserIdleDetectionMode = IdleDetectionMode.Enabled;
					}
					break;
			}
		}

		private void PhoneApplicationPage_BackKeyPress ( object sender, System.ComponentModel.CancelEventArgs e )
		{
			memoStorage.Dispose ();
		}

		private void ApplicationBarMenuItem_Click ( object sender, EventArgs e )
		{
			if ( MessageBox.Show ( "기록된 모든 메모를 삭제하시겠습니까?", "메모스레드", MessageBoxButton.OKCancel ) == MessageBoxResult.OK )
			{
				stkThreads.Children.Clear ();
				memoStorage.MemoList.Clear ();
			}
		}
	}
}