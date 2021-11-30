

using AForge.Video.DirectShow;
using FunyCamNF.utils;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Forms;

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
            //初始化设置界面相关选项
            this.VideoSaveFormatListBox.Items.Add("MP4");
            this.VideoSaveFormatListBox.Items.Add("AVI");
            this.VideoSaveFormatListBox.Items.Add("MKV");
            PictureSaveFormatListBox.Items.Add("JPEG");
            PictureSaveFormatListBox.Items.Add("BMP");
            readSetting();
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

        private void readSetting()
        {
            string videoDevice = Tools.readSettings("videoDevice");
            string videoSavePath = Tools.readSettings("videoSavePath");
            string videoSaveFormat = Tools.readSettings("videoSaveFormat");
            string PictureSavePath = Tools.readSettings("PictureSavePath");
            string PictureSaveFormat = Tools.readSettings("PictureSaveFormat");
            //camListBox.SelectedItem = videoDevice;
            videoSavePathText.Text = videoSavePath;
            VideoSaveFormatListBox.SelectedItem = videoSaveFormat;
            this.PictureSavePath.Text = PictureSavePath;
            this.PictureSaveFormatListBox.SelectedItem = PictureSaveFormat;

        }

        private void Button_Setting_save_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Tools.saveSettings("videoDevice", (string)camListBox.SelectedItem);
            Tools.saveSettings("videoSavePath", videoSavePathText.Text);
            Tools.saveSettings("videoSaveFormat", (string)VideoSaveFormatListBox.SelectedItem);
            Tools.saveSettings("PictureSavePath", PictureSavePath.Text);
            Tools.saveSettings("PictureSaveFormat", (string)PictureSaveFormatListBox.SelectedItem);
        }

        private void chooseVideoSavePathButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string m_Dir = chooseFolder();
            this.videoSavePathText.Text = m_Dir;
        }

        private void choosePictureSavePathButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            
            string m_Dir = chooseFolder();
            this.PictureSavePath.Text = m_Dir;
        }

        private string chooseFolder()
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return null;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            return m_Dir;
        }
    }
}
