#if IOS || ANDROID || MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#elif WINDOWS
using Microsoft.Maui.Graphics.Win2D;
#endif
using CommunityToolkit.Maui.Storage;
using System.Reflection;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace PuzzleCaptureMauiApp
{
    public partial class MainPage : ContentPage
    {
        string folderpath = string.Empty;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            var photoStream = await cameraView.TakePhotoAsync(Camera.MAUI.ImageFormat.JPEG);

            if (folderpath == string.Empty)
            {
                var result = await FolderPicker.PickAsync(default);

                if (result.IsSuccessful)
                {
                    folderpath = result.Folder.Path;
                }
                else
                {
                    Message.Text = result.Exception.Message;
                    return;
                }
            }

            var puzzlePrefix = string.IsNullOrEmpty(puzzlePrefixText.Text) ? "puzzle" : puzzlePrefixText.Text;
            int puzzleNumber = string.IsNullOrEmpty(puzzleNumberText.Text) ? 1 : int.Parse(puzzleNumberText.Text);
            var puzzlefileName = $"{puzzlePrefix}_{puzzleNumber:00000}.jpg";
            var puzzlePath = Path.Join(folderpath, puzzlefileName);

            var saveResult = await FileSaver.Default.SaveAsync(folderpath, puzzlefileName, photoStream, default);
            if (saveResult.IsSuccessful)
            {
                puzzleNumberText.Text = (puzzleNumber + 1).ToString();
            }
        }

        private void cameraView_CamerasLoaded(object sender, EventArgs e)
        {
            try
            {
                cameraView.Camera = cameraView.Cameras.First();

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await cameraView.StopCameraAsync();
                    await cameraView.StartCameraAsync();
                });
            }
            catch (Exception ex)
            {

            }
        }
    }
}
