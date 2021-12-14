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
using System.Windows.Forms.Integration;
using AForge.Video.FFMPEG;

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
        private string selectedFilterName = "";
        private VideoFileWriter originWriter,transedWriter;
        private bool IsRecordingVideo = false;   //是否开始录像

        public MainPage()
        {
            InitializeComponent();
            SnackbarERROR.MessageQueue = new SnackbarMessageQueue();
            filterList.Add("反色");
            filterList.Add("模糊");
            filterList.Add("边缘强化");
            filterList.Add("像素化");
            filterList.Add("随机抖动");
            filterList.Add("Erosion3x3");
            filterList.Add("动态旋转");
            filterList.Add("纵向拉伸");
            filterList.Add("凸透镜");
            filterList.Add("凹透镜");
            filterList.Add("复合效果(十字分区)");
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
                var devideName = Tools.readSettings("videoDevice");
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
            originalSource.NewFrame += RecodeOriginVideo;
            //originalSource.Start();

            transSource = new AsyncVideoSource(originalSource);
            transSource.NewFrame += sourceFilterEvent;
            transSource.NewFrame += RecodeTransedVideo;
            //transSource.Start();

            vp1.VideoSource = transSource;
            vp2.VideoSource = originalSource;
            vp1.Start();

            vp2.Start();


            while (vp1.GetCurrentVideoFrame() == null)
            {
                Button_Disconnect_Cam_Click_(sender, e);
                originalSource = new VideoCaptureDevice(videoDevice.MonikerString);//连接摄像头
                originalSource.VideoResolution = originalSource.VideoCapabilities[0];
                originalSource.NewFrame += RecodeOriginVideo;
                //originalSource.Start();

                transSource = new AsyncVideoSource(originalSource);
                transSource.NewFrame += sourceFilterEvent;
                transSource.NewFrame += RecodeTransedVideo;
                //transSource.Start();

                vp1.VideoSource = transSource;
                vp2.VideoSource = originalSource;
                vp1.Start();

                vp2.Start();
                Thread.Sleep(2000);
            }


            //保存当前滤镜名
            Tools.saveSettings("lastFilterName", filterList[filterListBox.SelectedIndex]);
            //允许点击截图和录制
            buttonRecoder.IsEnabled = true;
            buttonSnapshot.IsEnabled = true;
        }

        private void sourceFilterEvent(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = eventArgs.Frame;
            switch (selectedFilterName)
            {
                case "反色":
                    {
                        Invert filter = new Invert();
                        filter.ApplyInPlace(bitmap);
                        break;
                    }

                case "模糊":
                    {
                        Blur filter = new Blur();
                        filter.ApplyInPlace(bitmap);
                        break;
                    }

                case "边缘强化":
                    {
                        Edges filter = new Edges();
                        filter.ApplyInPlace(bitmap);
                        break;
                    }

                case "像素化":
                    {
                        Pixellate pixellate = new Pixellate(50);
                        pixellate.ApplyInPlace(bitmap);
                        break;
                    }

                case "随机抖动":
                    {
                        Jitter pointed = new Jitter(500);
                        pointed.ApplyInPlace(bitmap);
                        break;
                    }

                case "Erosion3x3":
                    {
                        TestFilter testFilter = new TestFilter();
                        testFilter.ApplyInPlace(bitmap);
                        break;
                    }

                case "动态旋转":
                    {
                        DynamicRotateFilter dynamicRotateFilter = new DynamicRotateFilter();
                        dynamicRotateFilter.ApplyInPlace(bitmap);
                        break;
                    }

                case "纵向拉伸":
                    {
                        TransX transX = new TransX();
                        transX.ApplyInPlace(bitmap);
                        break;
                    }

                case "凸透镜":
                    {
                        ConvexFilter convex = new ConvexFilter();
                        convex.ApplyInPlace(bitmap);
                        break;
                    }
                case "凹透镜":
                    {
                        ConcaveFilter concave = new ConcaveFilter();
                        concave.ApplyInPlace(bitmap);
                        break;
                    }
                case "复合效果(十字分区)":
                    {
                        ComplexFilter complexFilter = new ComplexFilter();
                        complexFilter.ApplyInPlace(bitmap);
                        break;
                    }
            }

        }

        private void Button_Disconnect_Cam_Click_(object sender, RoutedEventArgs e)
        {
            vp2.Stop();
            vp1.Stop();
            //vp2.Dispose();
            //vp1.Dispose();
        }

        private void Button_Reconnect_Cam_Click(object sender, RoutedEventArgs e)
        {
            Button_Disconnect_Cam_Click_(sender, e);
            Button_Connect_Cam_Click(sender, e);
        }

        private void filterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedIndex = filterListBox.SelectedIndex;
            selectedFilterName = (string)filterListBox.SelectedItem;
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

        private void Button_Snapshot(object sender, RoutedEventArgs e)
        {
            Bitmap bitmapOrigin=vp2.GetCurrentVideoFrame();
            Bitmap bitmapTransed = vp1.GetCurrentVideoFrame();
            string path = Tools.readSettings("PictureSavePath");
            string fileName = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            string fileFormatName = "";
            string PictureSaveFormat = Tools.readSettings("PictureSaveFormat");
            if (PictureSaveFormat.Equals("BMP"))
            {
                fileFormatName = ".bmp";
                bitmapOrigin.Save(path + '\\' + fileName + "原始图片" + fileFormatName, System.Drawing.Imaging.ImageFormat.Bmp);
                bitmapTransed.Save(path + '\\' + fileName + "哈哈镜处理图片" + fileFormatName, System.Drawing.Imaging.ImageFormat.Bmp);

            }
            else
            {
                fileFormatName = ".jpg";
                bitmapOrigin.Save(path +'\\'+ fileName + "原始图片" + fileFormatName, System.Drawing.Imaging.ImageFormat.Jpeg);
                bitmapTransed.Save(path + '\\' + fileName + "哈哈镜处理图片" + fileFormatName, System.Drawing.Imaging.ImageFormat.Jpeg);

            }
        }

        private void Button_recoder(object sender, RoutedEventArgs e)
        {
            if (IsRecordingVideo)
            {
                IsRecordingVideo = false;
                RecoderStop();
                IsRecordingVideo = false;
                buttonRecoder.Content = "开始录制";

            }
            else
            {
                RecoderStart();
                IsRecordingVideo = true;
                buttonRecoder.Content = "停止录制";

            }
        }

        //停止录制
        private void RecoderStop()
        {
            this.originWriter.Close();
            this.transedWriter.Close();
        }
        //开始录制
        private void RecoderStart()
        {
            int width = originalSource.VideoResolution.FrameSize.Width;    //录制视频的宽度
            int height = originalSource.VideoResolution.FrameSize.Height;   //录制视频的高度
            int fps = originalSource.VideoResolution.AverageFrameRate;

            string path = Tools.readSettings("VideoSavePath");
            string fileName = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();

            this.originWriter = new VideoFileWriter();
            this.transedWriter = new VideoFileWriter();

            if (this.originalSource.IsRunning && this.transSource.IsRunning)
            {
                originWriter.Open(path + '\\' + fileName + "原始录像.avi", width, height, fps, VideoCodec.MPEG4);
                transedWriter.Open(path + '\\' + fileName + "哈哈镜处理录像.avi", width, height, fps, VideoCodec.MPEG4);

            }
            else
                MessageBox.Show("没有视频源输入，无法录制视频。", "错误");

        }
        //录制事件处理函数
        private void RecodeOriginVideo(object sender, NewFrameEventArgs eventArgs)
        {
            if (IsRecordingVideo)
            {
                Bitmap bitmap = eventArgs.Frame;    //获取到一帧图像
                originWriter.WriteVideoFrame(bitmap);
            }

        }
        private void RecodeTransedVideo(object sender, NewFrameEventArgs eventArgs)
        {
            if (IsRecordingVideo)
            {
                Bitmap bitmap = eventArgs.Frame;    //获取到一帧图像
                transedWriter.WriteVideoFrame(bitmap);
            }

        }
    }
}

