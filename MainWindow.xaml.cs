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
using FunyCamNF.pages.main;

namespace FunyCamNF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        MainPage mainPage = new MainPage();
        public MainWindow()
        {
            InitializeComponent();
            mainFrame.Content = mainPage;
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
             mainPage.formsHostFiltered.Visibility = MenuToggleButton.IsChecked.Value? Visibility.Hidden : Visibility.Visible;
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

    }
}

