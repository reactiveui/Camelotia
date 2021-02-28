using System.IO;
using System.Threading.Tasks;
using Android;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Camelotia.Services.Interfaces;
using Plugin.FilePicker;

namespace Camelotia.Presentation.Xamarin.Droid.Services
{
    public sealed class AndroidFileManager : IFileManager
    {
        private readonly MainActivity _activity;

        public AndroidFileManager(MainActivity activity) => _activity = activity;

        public async Task<(string Name, Stream Stream)> OpenRead()
        {
            CheckAppPermissions();
            var file = await CrossFilePicker.Current.PickFile().ConfigureAwait(false);
            if (file == null) return (null, null);

            var fileName = file.FileName;
            var stream = file.GetStream();
            return (fileName, stream);
        }

        public Task<Stream> OpenWrite(string name) => Task.Run(() =>
        {
            CheckAppPermissions();
            name = name.Replace(' ', '_');
            var downloads = Environment.DirectoryDownloads;
            var path = Environment.GetExternalStoragePublicDirectory(downloads).AbsolutePath;
            var filePath = Path.Combine(path, name);

            var stream = (Stream)File.Create(filePath);
            _activity.RunOnUiThread(() =>
            {
                var toast = $"File {name} was downloaded";
                Toast.MakeText(_activity, toast, ToastLength.Long).Show();
            });

            return stream;
        });

        private void CheckAppPermissions()
        {
            if ((int)Build.VERSION.SdkInt < 23) return;

            var manager = _activity.PackageManager;
            var packageName = _activity.PackageName;

            var read = Manifest.Permission.ReadExternalStorage;
            var write = Manifest.Permission.WriteExternalStorage;
            if (manager.CheckPermission(read, packageName) != Permission.Granted &&
                manager.CheckPermission(write, packageName) != Permission.Granted)
            {
                _activity.RequestPermissions(
                    new string[]
                    {
                        Manifest.Permission.ReadExternalStorage,
                        Manifest.Permission.WriteExternalStorage
                    },
                    1);
            }
        }
    }
}
