using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Numerics;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.UserProfile;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Playback;
using Windows.Data.Pdf;
using Windows.Web.Http;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace 文件格式转换工厂
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();        
        }

        public StorageFile storageFile, storageFile2, storageFile3, storageFile4 = null; 
        string[] filegroup = new string[] { ".mp3", ".wma" , ".wav" , ".flac", ".m4a", ".alac" };
        public void AddAudio()
        {
            ConvertTo.Items.Add("Alac");
            ConvertTo.Items.Add("Flac");
            ConvertTo.Items.Add("M4a");
            ConvertTo.Items.Add("Mp3");
            ConvertTo.Items.Add("Wav");
            ConvertTo.Items.Add("Wma");

        }
        private async void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var select = new FileOpenPicker();
            select.SuggestedStartLocation = PickerLocationId.Desktop;

            select.FileTypeFilter.Add(".mp4");
            select.FileTypeFilter.Add(".avi");
            select.FileTypeFilter.Add(".flv");
            select.FileTypeFilter.Add(".wmv");

            select.FileTypeFilter.Add(".mp3");
            select.FileTypeFilter.Add(".wma");
            select.FileTypeFilter.Add(".wav");
            select.FileTypeFilter.Add(".flac");
            select.FileTypeFilter.Add(".m4a");
            select.FileTypeFilter.Add(".alac");
            select.FileTypeFilter.Add("*");

            storageFile = await select.PickSingleFileAsync();

            if (storageFile != null)
            {
                ConvertTo.Items.Clear();
                FileQuat.Items.Clear();

                FilePath.Text = storageFile.Path;

                var result = from aaa in filegroup where aaa== storageFile.FileType.ToLower() select aaa;
                if(result.Count()>0)
                {
                    AddAudio();
                }
                else
                {
                    ConvertTo.Items.Add("MP4");
                    ConvertTo.Items.Add("AVI");
                    ConvertTo.Items.Add("HEVC");
                    ConvertTo.Items.Add("WMV");


                    AddAudio();

                }
            }
            else
            {
                FilePath.Text = "";
            }
        }
        string[] filetypegroup = new string[] { "MP4", "AVI", "HEVC", "WMV"};
        private void ConvertTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ConvertTo.SelectedIndex<0)
            {
                return;
            }

            FileQuat.Items.Clear();
            FileQuat.SelectedIndex = -1;

            var result = from aaa in filetypegroup where aaa == ConvertTo.SelectedItem.ToString() select aaa;
            if(result.Count()>0)
            {
                FileQuat.Items.Add("1080p");
                FileQuat.Items.Add("720p");
                FileQuat.Items.Add("Wvga");
                FileQuat.Items.Add("Ntsc");
                FileQuat.Items.Add("Pal");
                FileQuat.Items.Add("Vga");
                FileQuat.Items.Add("Qvga");
                FileQuat.Items.Add("2160p");
                FileQuat.Items.Add("4320p");
            }
            else
            {
                FileQuat.Items.Add("High");
                FileQuat.Items.Add("Medium");
                FileQuat.Items.Add("Low");
            }
        }

        private async void SaveFileBtn_Click(object sender, RoutedEventArgs e)
        {
            if (storageFile != null)
            {
                if(ConvertTo.SelectedIndex<0)
                {
                    FilePath2.Text = DisplayCustomLang("请先选择目标文件的格式！", "請先選擇目標文件的格式！", "Please select a filetype first!", "Пожалуйста, сначала выберите тип файла!");
                }
                else
                {
                    var select = new FileSavePicker();
                    select.SuggestedStartLocation = PickerLocationId.Desktop;
                    select.FileTypeChoices.Add("", new List<string>() { "."+ConvertTo.SelectedItem.ToString() });
                    select.SuggestedFileName = storageFile.DisplayName;
                    storageFile2 = await select.PickSaveFileAsync();

                    if (storageFile2 != null)
                    {
                        FilePath2.Text = storageFile2.Path;
                    }
                    else
                    {

                    }
                }
            }
            else
            {
                FilePath.Text = DisplayCustomLang("请先选择源文件！", "請先選擇源文件！", "Please select a file!", "Пожалуйста, выберите файл!");
            }

        }

        MediaTranscoder transcoder = new MediaTranscoder();
        private async void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (storageFile == null || storageFile2 == null)
            {
                await new MessageDialog(DisplayCustomLang("您没有选择源文件或未选择保存位置！", "您沒有選擇源文件或未選擇保存位置！",
                    "You didn't select source file or the save location!", "Вы не выбрали исходный файл или место для сохранения!")).ShowAsync();
                return;
            }
            else if (ConvertTo.SelectedIndex < 0 | FileQuat.SelectedIndex < 0)
            {
                await new MessageDialog(DisplayCustomLang("您还没有选择文件的质量！", "您還沒有選擇文件的質量！",
                    "You have not selected the quality of the file!", "Вы не выбрали качество файла!")).ShowAsync();
                return;
            }

            StartBtn.IsEnabled = false;

            IProgress<double> progress = new Progress<double>((p) => { ConvertProcess.Value = p; });
            PrepareTranscodeResult transcodeResult = null;


            switch(ConvertTo.SelectedItem.ToString())
            {
                case "MP4":

                    ViedoEditFunc();
                    transcodeResult = await transcoder.PrepareFileTranscodeAsync(storageFile, storageFile2, MediaEncodingProfile.CreateMp4((VideoEncodingQuality)FileQuat.SelectedIndex + 1));

                    break;
                case "AVI":

                    ViedoEditFunc();
                    transcodeResult = await transcoder.PrepareFileTranscodeAsync(storageFile, storageFile2, MediaEncodingProfile.CreateAvi((VideoEncodingQuality)FileQuat.SelectedIndex + 1));

                    break;
                case "HEVC":

                    ViedoEditFunc();
                    transcodeResult = await transcoder.PrepareFileTranscodeAsync(storageFile, storageFile2, MediaEncodingProfile.CreateHevc((VideoEncodingQuality)FileQuat.SelectedIndex + 1));

                    break;
                case "WMV":

                    ViedoEditFunc();
                    transcodeResult = await transcoder.PrepareFileTranscodeAsync(storageFile, storageFile2, MediaEncodingProfile.CreateWmv((VideoEncodingQuality)FileQuat.SelectedIndex + 1));
                 
                    break;

                case "Alac":

                    transcodeResult = await transcoder.PrepareFileTranscodeAsync(storageFile, storageFile2, MediaEncodingProfile.CreateAlac((AudioEncodingQuality)FileQuat.SelectedIndex + 1));

                    break;
                case "Flac":

                    transcodeResult = await transcoder.PrepareFileTranscodeAsync(storageFile, storageFile2, MediaEncodingProfile.CreateFlac((AudioEncodingQuality)FileQuat.SelectedIndex + 1));

                    break;
                case "M4a":

                    transcodeResult = await transcoder.PrepareFileTranscodeAsync(storageFile, storageFile2, MediaEncodingProfile.CreateM4a((AudioEncodingQuality)FileQuat.SelectedIndex + 1));

                    break;
                case "Mp3":

                    transcodeResult = await transcoder.PrepareFileTranscodeAsync(storageFile, storageFile2, MediaEncodingProfile.CreateMp3((AudioEncodingQuality)FileQuat.SelectedIndex + 1));

                    break;
                case "Wav":

                    transcodeResult = await transcoder.PrepareFileTranscodeAsync(storageFile, storageFile2, MediaEncodingProfile.CreateWav((AudioEncodingQuality)FileQuat.SelectedIndex + 1));

                    break;
                case "Wma":

                    transcodeResult = await transcoder.PrepareFileTranscodeAsync(storageFile, storageFile2, MediaEncodingProfile.CreateWma((AudioEncodingQuality)FileQuat.SelectedIndex + 1));

                    break;
            }


            if (transcodeResult.CanTranscode)
            {
                await transcodeResult.TranscodeAsync().AsTask(progress);
                await Launcher.LaunchFileAsync(storageFile2);
            }
            else
            {
                await new MessageDialog(DisplayCustomLang("此转换不支持,可能是您的文件无法转换到对应的格式", "此轉換不支持,可能是您的文件無法轉換到對應的格式",
                    "The conversion is not supported, it may be that your selected file cannot be converted to selected format.", 
                    "Преобразование не поддерживается, возможно, выбранный вами файл не может быть преобразован в выбранный формат.")).ShowAsync();
            }

            StartBtn.IsEnabled = true;
        }

        private void ConvertProcess_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ConvertSucc.Visibility = (Visibility)1;

            if (ConvertProcess.Value == 100)
            {
                ConvertSucc.Visibility = 0;
            }

        }

        private async void SelectIMG_Click(object sender, RoutedEventArgs e)
        {
            var select = new FileOpenPicker();
            select.SuggestedStartLocation = PickerLocationId.Desktop;
            select.FileTypeFilter.Add(".jpg");
            select.FileTypeFilter.Add(".png");
            select.FileTypeFilter.Add(".jpeg");
            select.FileTypeFilter.Add("*");

            storageFile3 = await select.PickSingleFileAsync();

            if (storageFile3 != null)
            {
                IMGPath.Text = storageFile3.Path;
                IMGConvertTo.SelectedIndex = -1;
                storageFile4 = null;
                IMGPath2.Text = "";
            }
            else
            {
                IMGPath.Text = "";
            }
        }

        private async void SaveIMGBtn_Click(object sender, RoutedEventArgs e)
        {   
            if(UseWebIMG.IsChecked==false)
            {
                if (storageFile3 == null)
                {
                    IMGPath.Text = DisplayCustomLang("请先选择源文件！", "請先選擇源文件！", "Please select a file!", "Пожалуйста, выберите файл!");
                    return;
                }
            }
            if(IMGConvertTo.SelectedIndex<0)
            {
                IMGPath2.Text = DisplayCustomLang("请先选择目标文件的格式！", "請先選擇目標文件的格式！", "Please select a filetype first!", "Пожалуйста, сначала выберите тип файла!");
                return;
            }          

            var select = new FileSavePicker();
            select.SuggestedStartLocation = PickerLocationId.Desktop;

            switch(IMGConvertTo.SelectedIndex)
            {
                case 0:
                    select.FileTypeChoices.Add("PNG", new List<string>() { ".png" });
                    break;
                case 1:
                    select.FileTypeChoices.Add("BMP", new List<string>() { ".bmp" });
                    break;
                case 2:
                    select.FileTypeChoices.Add("JPEG", new List<string>() { ".jpeg" });
                    break;
                case 3:
                    select.FileTypeChoices.Add("JPEG-XR", new List<string>() { ".jxr" });
                    break;
                case 4:
                    select.FileTypeChoices.Add("TIFF", new List<string>() { ".tiff" });
                    break;
            }

            if (storageFile3 != null)
            {
                select.SuggestedFileName = storageFile3.DisplayName;
            }

            storageFile4 = await select.PickSaveFileAsync();

            if (storageFile4 != null)
            {
                IMGPath2.Text = storageFile4.Path;
            }


        }
        IRandomAccessStream streamFile = null;
        private async void IMGStartBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] downloaded = null;
            IMGSucc.Visibility = Visibility.Collapsed;

            if(UseWebIMG.IsChecked==false)
            {
                if(storageFile3 == null||storageFile4==null)
                {
                    await new MessageDialog(DisplayCustomLang("您没有选择源文件或未选择保存位置！", "您沒有選擇源文件或未選擇保存位置！",
                    "You didn't select source file or the save location!", "Вы не выбрали исходный файл или место для сохранения!")).ShowAsync();
                    return;
                }


                try
                {
                    using (streamFile = await storageFile3.OpenReadAsync())
                    {
                        StartWrite(downloaded);
                    }
                }
                catch(Exception)
                {

                }

            }
            else
            {
                if (string.IsNullOrWhiteSpace(IMGURL.Text) || storageFile4 == null)
                {
                    await new MessageDialog(DisplayCustomLang("您输入的URL格式有误或您没有选择文件的保存位置！", "您輸入的URL格式有誤或您沒有選擇文件的保存位置！",
                        "The format of the URL you entered is incorrect, or you didn't select the file save location.", "Вы ввели неправильный формат URL или не выбрали место для сохранения файла.")).ShowAsync();
                    return;
                }

                downloaded = await DownloadIMG();
                if(downloaded==null)
                {
                    await new MessageDialog(DisplayCustomLang("图片下载失败，请检查您的链接是否有误或稍后重试！", "圖片下載失敗，請檢查您的鏈接是否有誤或稍後重試！", 
                        "Download your image failed, please check your URL again and try download again.", "Не удалось загрузить изображение, проверьте URL еще раз и попробуйте загрузить еще раз.")).ShowAsync();
                    return;
                }


                StartWrite(downloaded);
            }



        }
        BitmapDecoder bitmapDecoder;
        public async void StartWrite(byte[] filebyte)
        {       
            if(UseWebIMG.IsChecked==false)
            {
                try
                {
                    bitmapDecoder = await BitmapDecoder.CreateAsync(streamFile);
                }
                catch(Exception)
                {
                    await new MessageDialog(DisplayCustomLang("获取位图失败，请重新选择一个位图！", "獲取位圖失敗，請重新選擇一個位圖！",
    "Failed to get the selected bitmap, please select a new image file!",
    "Не удалось получить выбранное растровое изображение, выберите новый файл изображения!")).ShowAsync();
                    return;
                }
            }

            PixelDataProvider pixelData = await bitmapDecoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, new BitmapTransform(), ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);
            filebyte = pixelData.DetachPixelData();

            try
            {
                using (IRandomAccessStream streamFile2 = await storageFile4.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder bitmapEncoder = null;

                    switch(IMGConvertTo.SelectedIndex)
                    {
                        case 0:
                            bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, streamFile2);
                            break;
                        case 1:
                            bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, streamFile2);
                            break;
                        case 2:
                            bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, streamFile2);
                            break;
                        case 3:
                            bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegXREncoderId, streamFile2);
                            break;
                        case 4:
                            bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.TiffEncoderId, streamFile2);
                            break;
                    }

                    bitmapEncoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, bitmapDecoder.PixelWidth, bitmapDecoder.PixelHeight, bitmapDecoder.DpiX, bitmapDecoder.DpiY, filebyte);
                    await bitmapEncoder.FlushAsync();
                    IMGSucc.Visibility = Visibility.Visible;

                    await Launcher.LaunchFileAsync(storageFile4);

                }
            }
            catch (Exception)
            {
                await new MessageDialog(DisplayCustomLang("此转换不支持,可能是您的文件无法转换到对应的格式", "此轉換不支持,可能是您的文件無法轉換到對應的格式",
                    "The conversion is not supported, it may be that your selected file cannot be converted to selected format.",
                    "Преобразование не поддерживается, возможно, выбранный вами файл не может быть преобразован в выбранный формат.")).ShowAsync();
            }
        }
        public async void ViedoEditFunc()
        {
            VideoTransformEffectDefinition videoEffect = new VideoTransformEffectDefinition();
            transcoder.ClearEffects();
            var activatableClassId = videoEffect.ActivatableClassId;
            transcoder.AddVideoEffect(activatableClassId, true, videoEffect.Properties);

            try
            {
                if (CB1.IsChecked == true)
                {
                    videoEffect.OutputSize = new Size(Convert.ToDouble(VideoWidth.Text), Convert.ToDouble(VideoHeight.Text));
                }
                if (CB2.IsChecked == true)
                {
                    if (MirrorMode.SelectedIndex == 0)
                    {
                        videoEffect.Mirror = MediaMirroringOptions.Horizontal;
                    }
                    if (MirrorMode.SelectedIndex == 1)
                    {
                        videoEffect.Mirror = MediaMirroringOptions.Vertical;
                    }
                    if (MirrorMode.SelectedIndex == 2)
                    {
                        videoEffect.Mirror = MediaMirroringOptions.None;
                    }
                }
                if (CB3.IsChecked == true)
                {
                    videoEffect.CropRectangle = new Rect(Convert.ToDouble(DrawXCoord.Text), Convert.ToDouble(DrawYCoord.Text), Convert.ToDouble(DrawWidth.Text), Convert.ToDouble(DrawHeight.Text));
                }
                if (CB4.IsChecked == true)
                {
                    if (VideoAlg.SelectedIndex == 0)
                    {
                        videoEffect.ProcessingAlgorithm = MediaVideoProcessingAlgorithm.Default;
                    }
                    if (VideoAlg.SelectedIndex == 1)
                    {
                        videoEffect.ProcessingAlgorithm = MediaVideoProcessingAlgorithm.MrfCrf444;
                    }
                }
                if (CB5.IsChecked == true)
                {
                    if (QMEffect.SelectedIndex == 0)
                    {
                        videoEffect.SphericalProjection.ProjectionMode = SphericalVideoProjectionMode.Flat;
                    }
                    if (QMEffect.SelectedIndex == 1)
                    {
                        videoEffect.SphericalProjection.ProjectionMode = SphericalVideoProjectionMode.Spherical;
                        if (QMMode.SelectedIndex == 0)
                        {
                            videoEffect.SphericalProjection.FrameFormat = SphericalVideoFrameFormat.Equirectangular;
                        }
                        if (QMMode.SelectedIndex == 1)
                        {
                            videoEffect.SphericalProjection.FrameFormat = SphericalVideoFrameFormat.None;
                        }
                        if (QMMode.SelectedIndex == 2)
                        {
                            videoEffect.SphericalProjection.FrameFormat = SphericalVideoFrameFormat.Unsupported;
                        }
                    }
                }
                if (CB6.IsChecked == true)
                {
                    videoEffect.PaddingColor = Color.FromArgb(Convert.ToByte(CA.Text), Convert.ToByte(CR.Text), Convert.ToByte(CG.Text), Convert.ToByte(CCB.Text));
                }
                if (CB7.IsChecked == true)
                {
                    if (VideoRotation.SelectedIndex == 0)
                    {
                        videoEffect.Rotation = MediaRotation.Clockwise180Degrees;
                    }
                    if (VideoRotation.SelectedIndex == 1)
                    {
                        videoEffect.Rotation = MediaRotation.Clockwise270Degrees;
                    }
                    if (VideoRotation.SelectedIndex == 2)
                    {
                        videoEffect.Rotation = MediaRotation.Clockwise90Degrees;
                    }
                    if (VideoRotation.SelectedIndex == 3)
                    {
                        videoEffect.Rotation = MediaRotation.None;
                    }
                }
            }
            catch(Exception)
            {
                await new MessageDialog(DisplayCustomLang("出现错误，请检查您所输入的值是否有误。", "出現錯誤，請檢查您所輸入的值是否有誤。","An error occurred. Please check your entered values carefully.",
                    "Произошла ошибка. Пожалуйста, внимательно проверьте введенные вами значения.")).ShowAsync();
            }


        }
        private void UseWebIMG_Click(object sender, RoutedEventArgs e)
        {
            if (UseWebIMG.IsChecked == true)
            {
                SelectIMG.IsEnabled = false;
            }
            else
            {
                SelectIMG.IsEnabled = true;
            }
        }
        private async Task<byte[]> DownloadIMG()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage responseMessage = await client.GetAsync(new Uri(IMGURL.Text));

                    using (InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream())
                    {
                        await responseMessage.Content.WriteToStreamAsync(memoryStream);
                        bitmapDecoder = await BitmapDecoder.CreateAsync(memoryStream);
                        memoryStream.Seek(0);

                        byte[] downloadedIMG = new byte[memoryStream.Size];
                        var reader = new DataReader(memoryStream);
                        await reader.LoadAsync((uint)memoryStream.Size);
                        reader.ReadBytes(downloadedIMG);

                        return downloadedIMG;
                    }
                }
                catch(Exception)
                {

                }
            }

            return null;
        }
        public string DisplayCustomLang(string cn, string tw, string en, string ru)
        {
            if (ApplicationLanguages.PrimaryLanguageOverride.Contains("zh-Hans") || ApplicationLanguages.PrimaryLanguageOverride.Contains("zh-CN"))
            {
                return cn;
            }
            else if (ApplicationLanguages.PrimaryLanguageOverride.Contains("zh-Hant"))
            {
                return tw;
            }
            else if (ApplicationLanguages.PrimaryLanguageOverride == "en")
            {
                return en;
            }
            else if (ApplicationLanguages.PrimaryLanguageOverride == "ru")
            {
                return ru;
            }

            return en;
        }
        private async void SettingBtn_Click(object sender, RoutedEventArgs e)
        {
            await SettingDialog.ShowAsync();
        }

        private void SettingDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }
        private void SaveLang_Click(object sender, RoutedEventArgs e)
        {
            RestartPanel.Visibility = 0;

            switch (LanguageCtrl.SelectedIndex)
            {
                case 0:
                    ApplicationLanguages.PrimaryLanguageOverride = GlobalizationPreferences.Languages[0];
                    break;
                case 1:
                    ApplicationLanguages.PrimaryLanguageOverride = "zh-Hans";
                    break;
                case 2:
                    ApplicationLanguages.PrimaryLanguageOverride = "zh-Hant";
                    break;
                case 3:
                    ApplicationLanguages.PrimaryLanguageOverride = "en";
                    break;
                case 4:
                    ApplicationLanguages.PrimaryLanguageOverride = "ru";
                    break;
            }

        }

        private async void RestartBtn_Click(object sender, RoutedEventArgs e)
        {
            RestartErr.Visibility = Visibility.Collapsed;
            LangRing.IsActive = true;
            LangRing.Visibility = Visibility.Visible;
            try
            {
                await RestartFunc();
            }
            catch (Exception)
            {
                RestartErr.Visibility = Visibility.Visible;
            }

            LangRing.IsActive = false;
            LangRing.Visibility = Visibility.Collapsed;
        }
        private async Task RestartFunc()
        {
            await CoreApplication.RequestRestartAsync(string.Empty);
        }


    }
}
