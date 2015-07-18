using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;
using System.Diagnostics;

namespace MemoThread
{
	public enum MemoType
	{
		Text = 1,
		Photo = 2,
		Voice = 3,
	}

	public class Memo
	{
		internal static long memoIdGenerator = 0;

		long memoId;
		MemoType memoType;
		object memoObject;
		DateTime memoDate;

		public Memo () { }

		public Memo ( MemoType memoType, object memoObject, DateTime memoDate )
		{
			MemoType = memoType;
			MemoObject = memoObject;
			MemoDate = memoDate;

			memoIdGenerator++;
		}

		public long MemoID { get { return memoId; } set { memoId = value; } }
		public MemoType MemoType { get { return memoType; } set { memoType = value; } }
		public object MemoObject { get { return memoObject; } set { memoObject = value; } }
		public DateTime MemoDate { get { return memoDate; } set { memoDate = value; } }
	}

	public class MemoStorage : IDisposable
	{
		List<Memo> memoList = new List<Memo> ();
		Action<Memo> loadedOne;

		public List<Memo> MemoList { get { return memoList; } }
		public Action<Memo> LoadedOne { get { return loadedOne; } set { loadedOne = value; } }

		public MemoStorage (Action<Memo> loadedOne)
		{
			LoadedOne = loadedOne;

			try
			{
				using ( IsolatedStorageFileStream fs = new IsolatedStorageFileStream ( "memo.mem", FileMode.Open,
					IsolatedStorageFile.GetUserStoreForApplication () ) )
				{
					BinaryReader br = new BinaryReader ( fs );
					int count = br.ReadInt32 ();
					Memo.memoIdGenerator = br.ReadInt64 ();
					for ( int i = 0; i < count; i++ )
					{
						Memo memo = new Memo ();
						memo.MemoID = br.ReadInt64 ();
						memo.MemoType = ( MemoType ) br.ReadByte ();
						switch ( memo.MemoType )
						{
							case MemoType.Text:
								memo.MemoObject = br.ReadString ();
								break;
							case MemoType.Photo:
							case MemoType.Voice:
								int len = br.ReadInt32 ();
								memo.MemoObject = br.ReadBytes ( len );
								break;
						}
						memo.MemoDate = DateTime.Parse ( br.ReadString () );

						memoList.Add ( memo );
						if ( loadedOne != null )
							loadedOne ( memo );
					}
					memoList.Reverse ();
				}
			}
			catch ( Exception e ) { Debug.WriteLine ( e.StackTrace ); }
		}

		public void Save ()
		{
			try
			{
				using ( IsolatedStorageFileStream fs = new IsolatedStorageFileStream ( "memo.mem", FileMode.Create,
					IsolatedStorageFile.GetUserStoreForApplication () ) )
				{
					BinaryWriter bw = new BinaryWriter ( fs );
					bw.Write ( memoList.Count );
					bw.Write ( Memo.memoIdGenerator );

					memoList.Reverse ();
					foreach ( Memo memo in memoList )
					{
						bw.Write ( memo.MemoID );
						bw.Write ( ( byte ) memo.MemoType );
						switch ( memo.MemoType )
						{
							case MemoType.Text:
								bw.Write ( memo.MemoObject as string );
								break;
							case MemoType.Photo:
							case MemoType.Voice:
								bw.Write ( ( memo.MemoObject as byte [] ).Length );
								bw.Write ( memo.MemoObject as byte [] );
								break;
						}
						bw.Write ( Convert.ToString ( memo.MemoDate ) );
					}
					memoList.Reverse ();
				}
			}
			catch ( Exception e ) { Debug.WriteLine ( e.StackTrace ); }
		}

		public void Dispose ()
		{
			Save ();
		}
	}
}
