using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using QRCoder;
using UsersSubscriptions.Common;

namespace UsersSubscriptions.Services
{
    public class Qrcoder
    {
        public static Byte[] GenerateQRCode(string code)
        {
            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData qrCodeData = generator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(UsersConstants.qrCodeImageSize);
            return BitmapToBytes(qrCodeImage);
        }
        private static Byte[] BitmapToBytes(Bitmap img)
        {
            using  (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static void  CreateQrFile (string code)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
            qrCodeData.SaveRawData("wwwroot/qrr/file-" + code + ".qrr",
                QRCodeData.Compression.Uncompressed);

        }
        public static Byte[] GetQrFile(string code)
        {
            QRCodeData qrCodeData = new QRCodeData("wwwroot/qrr/file-" + code + ".qrr",
                QRCodeData.Compression.Uncompressed);
            QRCode qRCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qRCode.GetGraphic(UsersConstants.qrCodeImageSize);
            return BitmapToBytes(qrCodeImage);
        }
    }
}
