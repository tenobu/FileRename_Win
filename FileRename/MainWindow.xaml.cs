using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using Microsoft.Win32;
using win = System.Windows.Forms;

namespace FileRename
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		private string str_Folder = "";

		private long long_FolderCount = 0, long_FileCount, long_Length = 0;

	
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// 設定から読み取る。
			this.Left = Properties.Settings.Default.WindowLeft;
			this.Top = Properties.Settings.Default.WindowTop;
			this.Width = Properties.Settings.Default.WindowWidth;
			this.Height = Properties.Settings.Default.WindowHeight;

			this.label_Folder.Content = Properties.Settings.Default.Folder;

			// FromFolderが何も無かったら、
			if (label_Folder.Content.Equals(""))
			{
				// FromFolderに初期値を送る
				label_Folder.Content = "無し";
			}

			str_Folder = (string)label_Folder.Content;

			if (Directory.Exists(str_Folder))
			{
				long_FolderCount = long_FileCount = long_Length = 0;

				// タスクを分けて、メインからサブにタスクを移す。
				var task = Task.Factory.StartNew(() =>
				{
					CheckFolder(str_Folder);
				});
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.WindowState == WindowState.Normal)
			{
				// 設定に書き込む。
				Properties.Settings.Default.WindowLeft = this.Left;
				Properties.Settings.Default.WindowTop = this.Top;
				Properties.Settings.Default.WindowWidth = this.Width;
				Properties.Settings.Default.WindowHeight = this.Height;
			}

			// 設定にフォルダ名を書き込む。
			Properties.Settings.Default.Folder = str_Folder;

			// 設定に送ったものをセーブ。
			Properties.Settings.Default.Save();
		}

		private void image_Folder_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				if (Directory.Exists(files[0]))
				{
					e.Handled = true;
				}
				else
				{
					e.Handled = false;
				}
			}
			else
			{
				e.Handled = false;
			}
		}

		private void image_Folder_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				// Note that you can have more than one file.
				var files = (string[])e.Data.GetData(DataFormats.FileDrop);

				if (Directory.Exists(files[0]) == false) return;

				label_Folder.Content = files[0];
				str_Folder = (string)label_Folder.Content;

				long_FolderCount = long_FileCount = long_Length = 0;

				CheckFolder(str_Folder);
			}
		}

		private void label_Folder_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				// Note that you can have more than one file.
				var files = (string[])e.Data.GetData(DataFormats.FileDrop);

				if (Directory.Exists(files[0]) == false) return;

				label_Folder.Content = files[0];
				str_Folder = (string)label_Folder.Content;

				long_FolderCount = long_FileCount = long_Length = 0;

				CheckFolder(str_Folder);
			}
		}

		private void button_Folder_Click(object sender, RoutedEventArgs e)
		{
			{
				// フォルダブラウザーダイアログ
				win.FolderBrowserDialog fbd = new win.FolderBrowserDialog();

				fbd.Description = "検索するフォルダを指定してください。";

				fbd.RootFolder = Environment.SpecialFolder.Desktop;

				fbd.SelectedPath = (string)label_Folder.Content;

				fbd.ShowNewFolderButton = true;

				win.DialogResult result = fbd.ShowDialog();

				if (result == win.DialogResult.OK)
				{
					label_Folder.Content = fbd.SelectedPath;
					str_Folder = (string)label_Folder.Content;

					long_FolderCount = long_FileCount = long_Length = 0;

					CheckFolder(str_Folder);
				}
			}
		}

		private void CheckFolder(string folder_path)
		{
			foreach (var file_path in Directory.GetFiles(folder_path))
			{
				long_FileCount++;
				long_Length += new FileInfo(file_path).Length;

				button_Check.Dispatcher.BeginInvoke(
					new Action(() =>
					{
						label_FilesCount.Content = long_FileCount;
						label_FilesSize .Content = long_Length.ToString("###,##0 Byte");
					}));
			}

			foreach (var c_folder_path in Directory.GetDirectories(folder_path))
			{
				long_FolderCount++;

				button_Check.Dispatcher.BeginInvoke(
					new Action(() =>
					{
						label_FoldersCount.Content = long_FolderCount;
					}));

				CheckFolder(c_folder_path);
			}
		}

		private void button_Check_Click(object sender, RoutedEventArgs e)
		{
			long_FolderCount = long_FileCount = long_Length = 0;

			// タスクを分けて、メインからサブにタスクを移す。
			var task = Task.Factory.StartNew(() =>
			{
				CheckFolder(str_Folder);
			});
		}
	}
}
