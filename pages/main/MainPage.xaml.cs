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
using System.Windows.Forms.Integration;

namespace FunyCamNF.pages.main
{
    /// <summary>
    /// MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : Page
    {
        public WindowsFormsHost formsHostFiltered,formsHostOrigin;
        private List<string> deviceList = new List<string>();
        private FilterInfo videoDevice;
        private VideoCaptureDevice originalSource;
        private AsyncVideoSource transSource;

        private List<string> filterList = new List<string>();

        private int selectedIndex = 0;
        private VideoFileWriter writer;
        private bool IsRecordVideo = false;   //是否开始录像

        public MainPage()
        {
            InitializeComponent();
            SnackbarERROR.MessageQueue = new SnackbarMessageQueue();
            filterList.Add("反色");
            filterList.Add("模糊");
            filterList.Add("edges");
            filterList.Add("pixellate");
            filterList.Add("Jitter");
            filterList.Add("Erosion3x3");
            filterList.Add("动态旋转");
            filterList.Add("拉伸1");
            filterList.Add("凸透镜");
            filterList.Add("凹透镜");
            filterListBox.ItemsSource = filterList;
            getDevices();
            formsHostFiltered = formHost1;
            formsHostOrigin = formHost2;
        }

        private void getDevices()
        {
            try
            {
                // 枚举所有视频输入设备
                var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                    throw new ApplicationException();
                var devideName = Tools.readSettings("lastDeviceName");
                foreach (FilterInfo device in videoDevices)
                {
                    if (device.Name == devideName)
                    {
                        videoDevice = device;
                    }
                }
                if (videoDevice == null)
                {

                    var message = new SnackbarMessage
                    {
                        Content = "没有读取到视频设备，请检查设置是否正确！",
                        Margin = new Thickness(0, 0, 0, 0),
                    };
                    SnackbarERROR.MessageQueue.Enqueue(message);
                }
                filterListBox.SelectedItem = 0;
                //检查上次的滤镜选择
                string lastFilterName = Tools.readSettings("lastFilterName");
                for (int i = 0; i < filterList.Count; i++)
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
                videoDevice = null;
            }
        }

        private void Button_Connect_Cam_Click(object sender, RoutedEventArgs e)
        {
            //展示图像窗口
            formsHostFiltered.Visibility = Visibility.Visible;
            formsHostOrigin.Visibility = Visibility.Visible;
            // 断开连接
            Button_Disconnect_Cam_Click_(sender, e);

            originalSource = new VideoCaptureDevice(videoDevice.MonikerString);//连接摄像头
            originalSource.VideoResolution = originalSource.VideoCapabilities[0];

            transSource = new AsyncVideoSource(originalSource);
            transSource.NewFrame += OriginalSource_NewFrame;
            transSource.Start();

            vp1.VideoSource = transSource;
            vp2.VideoSource = originalSource;
            vp2.Start();
            vp1.Start();
            //保存当前滤镜名
            Tools.saveSettings("lastFilterName", filterList[filterListBox.SelectedIndex]);
        }

        private void OriginalSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = eventArgs.Frame;

            switch (selectedIndex)
            {
                case 0:
                    {
                        Invert filter = new Invert();
                        filter.ApplyInPlace(bitmap);
                        break;
                    }

                case 1:
                    {
                        Blur filter = new Blur();
                        filter.ApplyInPlace(bitmap);
                        break;
                    }

                case 2:
                    {
                        Edges filter = new Edges();
                        filter.ApplyInPlace(bitmap);
                        break;
                    }

                case 3:
                    {
                        Pixellate pixellate = new Pixellate(50);
                        pixellate.ApplyInPlace(bitmap);
                        break;
                    }

                case 4:
                    {
                        Jitter pointed = new Jitter(500);
                        pointed.ApplyInPlace(bitmap);
                        break;
                    }

                case 5:
                    {
                        TestFilter testFilter = new TestFilter();
                        testFilter.ApplyInPlace(bitmap);
                        break;
                    }

                case 6:
                    {
                        DynamicRotateFilter dynamicRotateFilter = new DynamicRotateFilter();
                        dynamicRotateFilter.ApplyInPlace(bitmap);
                        break;
                    }

                case 7:
                    {
                        TransX transX = new TransX();
                        transX.ApplyInPlace(bitmap);
                        break;
                    }

                case 8:
                    {
                        ConvexFilter convex = new ConvexFilter();
                        convex.ApplyInPlace(bitmap);
                        break;
                    }
                case 9:
                    {
                        ConcaveFilter concave = new ConcaveFilter();
                        concave.ApplyInPlace(bitmap);
                        break;
                    }
            }

        }

        private void Button_Disconnect_Cam_Click_(object sender, RoutedEventArgs e)
        {
            vp2.WaitForStop();
            vp1.WaitForStop();
        }

        private void Button_Reconnect_Cam_Click(object sender, RoutedEventArgs e)
        {
            Button_Disconnect_Cam_Click_(sender, e);
            Button_Connect_Cam_Click(sender, e);
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

           /* MenuToggleButton.IsChecked = false;*/
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
          /*  DemoItemsSearchBox.Focus();*/
            hideForm();
        }

        private static void ModifyTheme(bool isDarkTheme)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            theme.SetBaseTheme(isDarkTheme ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);
        }

        private void hideForm()
        {
            /*formHost1.Visibility = MenuToggleButton.IsChecked.Value ? Visibility.Hidden : Visibility.Visible;
            formHost2.Visibility = MenuToggleButton.IsChecked.Value ? Visibility.Hidden : Visibility.Visible;*/
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

        private void SnackbarERRORMessage_ActionClick(object sender, RoutedEventArgs e)
        {
            SnackbarERROR.IsActive = false;
        }

        private void Button_recoder(object sender, RoutedEventArgs e)
        {
        }
    }
}

