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
using System.Windows.Input;
using System.Windows.Media;
using System.Configuration;
using FunyCamNF.utils;
using AForge.Video.FFMPEG;

namespace FunyCamNF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> deviceList = new List<string>();
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice originalSource;
        private AsyncVideoSource transSource;

        private List<string> filterList = new List<string>();

        private int selectedIndex = 0;
        private VideoFileWriter writer;   
        private bool IsRecordVideo = false;   //是否开始录像

        public MainWindow()
        {
            InitializeComponent();
            filterList.Add("反色");
            filterList.Add("模糊");
            filterList.Add("edges");
            filterList.Add("pixellate");
            filterList.Add("Jitter");
            filterList.Add("Erosion3x3");
            filterList.Add("动态旋转");
            filterListBox.ItemsSource = filterList;
            getDevices();

            
            
        }

        private void getDevices()
        {
            try
            {
                // 枚举所有视频输入设备
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                foreach (FilterInfo device in videoDevices)
                {
                    deviceList.Add(device.Name);
                }
                camListBox.ItemsSource = deviceList;
                camListBox.SelectedIndex = 0;
                filterListBox.SelectedItem = 0;
                // 检查上次选择的相机设备
                string lastDeviceName = Tools.readSettings("lastDeviceName");
                for(int i = 0; i < deviceList.Count; i++)
                {
                    if (deviceList[i].Equals(lastDeviceName))
                    {
                        camListBox.SelectedIndex = i;
                        break;
                    }
                }
                //检查上次的滤镜选择
                string lastFilterName = Tools.readSettings("lastFilterName");
                for(int i=0;i< filterList.Count; i++)
                {
                    if (filterList[i].Equals(lastFilterName))
                    {
                        filterListBox.SelectedIndex = i;
                    }
                }


            }
            catch (ApplicationException)
            {
                camListBox.Items.Add("No local capture devices");
                videoDevices = null;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button_Click_1(sender, e);
            originalSource = new VideoCaptureDevice(videoDevices[camListBox.SelectedIndex].MonikerString);//连接摄像头
            originalSource.VideoResolution = originalSource.VideoCapabilities[camListBox.SelectedIndex];


            transSource = new AsyncVideoSource(originalSource);
            transSource.NewFrame += OriginalSource_NewFrame;
            transSource.Start();

            vp1.VideoSource = transSource;
            vp2.VideoSource = originalSource;
            vp2.Start();
            vp1.Start();
            //保存当前相机设备名
            Tools.saveSettings("lastDeviceName", deviceList[camListBox.SelectedIndex]);
            //保存当前滤镜名
            Tools.saveSettings("lastFilterName", filterList[filterListBox.SelectedIndex]);
        }

        private void OriginalSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = eventArgs.Frame;
            if (selectedIndex == 0)
            {
                Invert filter = new Invert();
                filter.ApplyInPlace(bitmap);
            }
            else if (selectedIndex == 1)
            {
                Blur filter = new Blur();
                filter.ApplyInPlace(bitmap);
            }
            else if (selectedIndex == 2)
            {
                Edges filter = new Edges();
                filter.ApplyInPlace(bitmap);
            }
            else if (selectedIndex == 3)
            {
                AForge.Imaging.Filters.Pixellate pixellate = new Pixellate(50);
                pixellate.ApplyInPlace(bitmap);
            }
            else if (selectedIndex == 4)
            {
                Jitter pointed = new Jitter(500);
                pointed.ApplyInPlace(bitmap);
            }
            else if (selectedIndex == 5)
            {
                TestFilter testFilter = new TestFilter();
                testFilter.ApplyInPlace(bitmap);
            }
            else if (selectedIndex == 6)
            {
                DynamicRotateFilter dynamicRotateFilter = new DynamicRotateFilter();
                dynamicRotateFilter.ApplyInPlace(bitmap);
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            vp2.WaitForStop();
            vp1.WaitForStop();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Button_Click_1(sender, e);
            Button_Click(sender, e);
        }

        private void filterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedIndex = filterListBox.SelectedIndex;
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {

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
            formHost1.Visibility = Visibility.Visible;
            formHost2.Visibility = Visibility.Visible;

        }

        private async void MenuPopupButton_OnClick(object sender, RoutedEventArgs e)
        {
            /*var sampleMessageDialog = new SampleMessageDialog
            {
                Message = { Text = ((ButtonBase)sender).Content.ToString() }
            };

            await DialogHost.Show(sampleMessageDialog, "RootDialog");*/
        }

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
            formHost1.Visibility = MenuToggleButton.IsChecked.Value? Visibility.Hidden : Visibility.Visible;
            formHost2.Visibility = MenuToggleButton.IsChecked.Value ? Visibility.Hidden : Visibility.Visible;
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

    }
}
}
