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

namespace MemoThread
{
	public partial class PhotoThread : UserControl
	{
		public new object Content { get { return imgMemo.Source; } set { imgMemo.Source = value as ImageSource; } }
		public DateTime Date { get { return DateTime.Parse ( txtDate.Text ); } set { txtDate.Text = Convert.ToString ( value ); } }

		public PhotoThread ( ImageSource src, DateTime dateTime )
		{
			InitializeComponent ();

			Content = src;
			Date = dateTime;
		}

		private void imgMemo_Tap ( object sender, GestureEventArgs e )
		{
			ForPhotoView.imageSource = imgMemo.Source;
			( ( ( ( ( Parent as StackPanel ).Parent as ScrollViewer ).Parent as Grid ).Parent as Grid ).Parent as Page ).
				NavigationService.Navigate ( new Uri ( "/PhotoViewPage.xaml", UriKind.RelativeOrAbsolute ) );
		}
	}
}
