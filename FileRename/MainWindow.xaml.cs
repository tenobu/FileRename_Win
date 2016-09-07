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
using System.Collections.ObjectModel;

namespace FileRename
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		private string str_Folder = "";

		private long long_FolderCount = 0, long_FileCount, long_Length = 0;

		private Folder_data folders_Data = null;

		private List<FolderFile_data> list_FolderFiles = null;
		//private bool bool_ListFlag = false;

		private Dictionary<string, List<FolderFile_data>> dic_FolderFiles = null;

		//private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer(); 


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
				CheckFolder();
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

				CheckFolder();
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

				CheckFolder();
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

					CheckFolder();
				}
			}
		}

		private void button_Check_Click(object sender, RoutedEventArgs e)
		{
			CheckFolder();
		}

		/*private void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			if (list_FolderFiles != null)
			{
				//listBox_FolderFiles.ItemsSource = list_FolderFiles;
				listBox_FolderFiles.Items.Add(list_FolderFiles);
			}
		}*/
		
		private void CheckFolder()
		{
			folders_Data = new Folder_data(str_Folder);

			list_FolderFiles = null;
			dic_FolderFiles  = new Dictionary<string, List<FolderFile_data>>();

			//listBox_FolderFiles.Items.Clear();

			long_FolderCount = long_FileCount = long_Length = 0;

			//bool_ListFlag = false;

			/*timer.Tick += new EventHandler(dispatcherTimer_Tick);
			timer.Interval = new TimeSpan(0, 0, 1);
			timer.Start();*/

			// タスクを分けて、メインからサブにタスクを移す。
			//var task = Task.Factory.StartNew(() =>
			//{
				GetFolder(str_Folder, folders_Data);

				treeView_Folder.ItemsSource = null;
				treeView_Folder.ItemsSource = folders_Data;
			
			//timer.Stop();
			//});
		}

		private void GetFolder(string folder_path, Folder_data fd)
		{
			var di = new DirectoryInfo(folder_path);

			fd.image_Icon  = new BitmapImage(new Uri(@"Images\folder.png", UriKind.Relative));
			fd.str_FPath   = folder_path;
			fd.str_FName   = new DirectoryInfo(folder_path).Name;
	
			var list_ff = new List<FolderFile_data>();

			dic_FolderFiles.Add(folder_path, list_ff);

			var bool_ListFlag = false;

			if (folder_path.Equals(str_Folder))
			{
				bool_ListFlag = true;

				list_FolderFiles = list_ff;
			}
			else
			{
				bool_ListFlag = false;
			}

			var ch = fd.fd_Children;

			foreach (var c_folder_path in Directory.GetDirectories(folder_path))
			{
				var ch_a = new Folder_data(c_folder_path);

				treeView_Folder.ItemsSource = null;
				treeView_Folder.ItemsSource = folders_Data;

				AddFolderFilesList(list_ff, bool_ListFlag, "Directory", c_folder_path);

				long_FolderCount++;

				button_Check.Dispatcher.BeginInvoke(
					new Action(() =>
					{
						label_Count.Content =
							long_FolderCount.ToString("フォルダ数 ###,##0") + "、" +
							long_FileCount  .ToString("ファイル数 ###,##0");
					}));

				GetFolder(c_folder_path, ch_a);

				ch.Add(ch_a);
			}

			foreach (var file_path in Directory.GetFiles(folder_path))
			{
				AddFolderFilesList(list_ff, bool_ListFlag, "File", file_path);

				long_FileCount++;
				long_Length += new FileInfo(file_path).Length;

				button_Check.Dispatcher.BeginInvoke(
					new Action(() =>
					{
						label_Count.Content =
							long_FolderCount.ToString("フォルダ数 ###,##0") + "、" +
							long_FileCount  .ToString("ファイル数 ###,##0");
						label_Size .Content =
							long_Length     .ToString("サイズ ###,##0 Byte");
					}));
			}
		}

		private void AddFolderFilesList(List<FolderFile_data> list_ff, bool bool_ListFlag, string str_ff, string str_path)
		{
			var ffd = new FolderFile_data();

			ffd.str_Type = str_ff;
			ffd.str_FFPath = str_path;

			if (str_ff.Equals("Directory"))
			{
				var di = new DirectoryInfo(str_path);

				ffd.image_Icon = new BitmapImage(new Uri(@"Images\folder.png", UriKind.Relative));
				ffd.str_FFName = di.Name;
				ffd.long_Size = 0;
				ffd.str_SizeS = "";

				list_ff.Add(ffd);

				if (bool_ListFlag)
				{
					listBox_FolderFiles.Dispatcher.BeginInvoke(
						new Action(() =>
						{
							listBox_FolderFiles.ItemsSource = list_ff;
						}));
				}
			}
			else if (
				str_ff.Equals("File"))
			{
				var fi = new FileInfo(str_path);

				if (fi.Extension.ToLower().Equals(".bmp" ) ||
					fi.Extension.ToLower().Equals(".gif" ) ||
					fi.Extension.ToLower().Equals(".jpg" ) ||
					fi.Extension.ToLower().Equals(".jpeg") ||
					fi.Extension.ToLower().Equals(".png" ) ||
					fi.Extension.ToLower().Equals(".tiff") ||
					fi.Extension.ToLower().Equals(".ico" ))
				{
					ffd.image_Icon = new BitmapImage(new Uri(fi.FullName));
				}
				else
				{
					ffd.image_Icon = SHFileInfo.GetBitmap(fi.FullName);
				}

				ffd.str_FFName = fi.Name;
				ffd.long_Size = fi.Length;
				ffd.str_SizeS = fi.Length.ToString("###,##0 Byte");

				list_ff.Add(ffd);

				if (bool_ListFlag)
				{
					listBox_FolderFiles.Dispatcher.BeginInvoke(
						new Action(() =>
						{
							listBox_FolderFiles.ItemsSource = list_ff;
						}));
				}
			}
		}

		private void listBox_FolderFiles_MouseUp(object sender, MouseButtonEventArgs e)
		{

		}

		private void listBox_FolderFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{

		}

		private void treeView_Folder_MouseUp(object sender, MouseButtonEventArgs e)
		{

		}

		private void treeView_Folder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{

		}
	}

	public class Folder_data : ObservableCollection<Folder_data>
	{
		public Folder_data()
		{
			//fd_Children = new Folder_data();
		}

		public Folder_data(string path)
		{
			str_FPath   = path;
			fd_Children = new Folder_data();
		}

		public BitmapImage image_Icon  { get; set; }
		public string      str_FPath   { get; set; }
		public string      str_FName   { get; set; }
		public Folder_data fd_Children { get; set; }
	}

	/*public class Folder_data
	{
		public BitmapImage image_Icon { get; set; }
		public string      str_FFPath { get; set; }
		public string      str_FFName { get; set; }
	}*/

	public class FolderFile_data
	{
		public string      str_Type   { get; set; }
		public BitmapImage image_Icon { get; set; }
		public string      str_FFName { get; set; }
		public string      str_FFPath { get; set; }
		public long        long_Size  { get; set; }
		public string      str_SizeS  { get; set; }
	}
}
