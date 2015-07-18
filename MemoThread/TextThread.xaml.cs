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
	public partial class TextThread : UserControl
	{
		public new object Content { get { return txtMemo.Text; } set { txtMemo.Text = value as string; } }
		public DateTime Date { get { return DateTime.Parse ( txtDate.Text ); } set { txtDate.Text = Convert.ToString ( value ); } }

		public TextThread ( string text, DateTime dateTime )
		{
			InitializeComponent ();

			Content = text;
			Date = dateTime;
		}
	}
}
