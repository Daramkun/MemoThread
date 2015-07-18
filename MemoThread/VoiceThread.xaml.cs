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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Phone.Shell;

namespace MemoThread
{
	public partial class VoiceThread : UserControl
	{
		SoundEffectInstance soundEffect;
		PhoneApplicationService phoneAppService = PhoneApplicationService.Current;

		public new object Content { get { return soundEffect; } set { soundEffect = value as SoundEffectInstance; } }
		public DateTime Date { get { return DateTime.Parse ( txtDate.Text ); } set { txtDate.Text = Convert.ToString ( value ); } }

		public VoiceThread ( SoundEffectInstance soundEffect, DateTime dateTime )
		{
			InitializeComponent ();

			Content = soundEffect;
			Date = dateTime;
		}

		private void btnPlay_Click ( object sender, RoutedEventArgs e )
		{
			soundEffect.Play ();
			phoneAppService.UserIdleDetectionMode = IdleDetectionMode.Disabled;
		}

		private void btnStop_Click ( object sender, RoutedEventArgs e )
		{
			soundEffect.Stop ();
			phoneAppService.UserIdleDetectionMode = IdleDetectionMode.Enabled;
		}
	}
}
