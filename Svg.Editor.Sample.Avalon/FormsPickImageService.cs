//using System.Threading.Tasks;
//using Plugin.Media;
//using Plugin.Media.Abstractions;
//using Svg.Editor.Interfaces;
//using Svg.Interfaces;

//namespace Svg.Editor.Sample.Forms
//{
//    public class FormsPickImageService : IPickImageService
//    {
//        public async Task<string> PickImagePathAsync(int maxPixelDimension)
//        {
//	        using (var inStream = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
//		        {CompressionQuality = 80, PhotoSize = PhotoSize.MaxWidthHeight, MaxWidthHeight = maxPixelDimension}))
//            {
//                if (inStream == null) return null;

//                var fs = SvgEngine.Resolve<IFileSystem>();
//	            var path = "background.png";

//				var fullPath = fs.PathCombine(fs.GetDefaultStoragePath(), path);

//                if (fs.FileExists(fullPath))
//                    fs.DeleteFile(fullPath);

//                using (var outStream = fs.OpenWrite(fullPath))
//                {
//                    inStream.GetStream().CopyTo(outStream);
//                }
//                return path;
//            }
//        }
//    }
//}