

using AForge.Video.DirectShow;
using FunyCamNF.utils;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace FunyCamNF.pages.setting
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        private List<string> deviceList = new List<string>();
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice originalSource;

        private List<string> filterList = new List<string>();

        private int selectedIndex = 0;
        private bool IsRecordVideo = false;   //是否开始录像
        public SettingPage()
        {
            InitializeComponent();
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
                // 读取选择的相机设备
                string lastDeviceName = Tools.readSettings("lastDeviceName");
                for (int i = 0; i < deviceList.Count; i++)
                {
                    if (deviceList[i].Equals(lastDeviceName))
                    {
                        camListBox.SelectedIndex = i;
                        break;
                    }
                }
            }
            catch (ApplicationException)
            {
                camListBox.Items.Add("No local capture devices");
                videoDevices = null;
            }
        }

        private void Button_Setting_sace_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
