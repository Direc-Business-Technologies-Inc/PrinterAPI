using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using ZXing;

namespace PrinterAPI.Helpers
{
	public class QRHelper
	{
		public byte[] bargenerator(string textname, int otype, int dimension)
		{
			byte[] retbytes;
			var barcodeWriter = new BarcodeWriter();
            if (otype == 2048)
            {
				barcodeWriter.Options = new ZXing.Common.EncodingOptions { Height = dimension, Width = dimension, Margin = 0 };
				barcodeWriter.Format = BarcodeFormat.QR_CODE;
			}
			else if (otype == 16)
			{
				int _height = Convert.ToInt32(dimension / 2.5);
				barcodeWriter.Options = new ZXing.Common.EncodingOptions { Height = _height, Width = dimension, Margin = 0 };
				barcodeWriter.Format = BarcodeFormat.CODE_128;
			}

			var result = barcodeWriter.Write(textname);
			var barcodeBitmap = new Bitmap(result);
			using (MemoryStream memory = new MemoryStream())
			{
				barcodeBitmap.Save(memory, ImageFormat.Jpeg);
				retbytes = memory.ToArray();
			}
			return retbytes;
		}

		public string GenerateQRCode(string textname, int otype, int dimension)
		{
			string folderPath = "\\Images";
			string imagePath = "\\Images\\QrCode" + DateTime.Now.ToString("YYYYMMdd_hhmmss") + ".jpg";
			// If the directory doesn't exist then create it.
			if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + folderPath))
			{
				Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + folderPath);
			}

			var barcodeWriter = new BarcodeWriter();
			if (otype == 2048)
			{
				barcodeWriter.Options = new ZXing.Common.EncodingOptions { Height = dimension, Width = dimension, Margin = 0 };
				barcodeWriter.Format = BarcodeFormat.QR_CODE;
			}
			else if (otype == 16)
			{
				int _height = Convert.ToInt32(dimension / 1.5);
				//barcodeWriter.Options = new ZXing.Common.EncodingOptions { Height = _height , Width = dimension };
				barcodeWriter.Format = BarcodeFormat.CODE_128;
			}

			var result = barcodeWriter.Write(textname);
			string barcodePath = AppDomain.CurrentDomain.BaseDirectory + imagePath;
			var barcodeBitmap = new Bitmap(result);
			using (MemoryStream memory = new MemoryStream())
			{
				using (FileStream fs = new FileStream(barcodePath, FileMode.Create, FileAccess.ReadWrite))
				{
					barcodeBitmap.Save(memory, ImageFormat.Jpeg);
					byte[] bytes = memory.ToArray();
					fs.Write(bytes, 0, bytes.Length);
				}
			}
			return AppDomain.CurrentDomain.BaseDirectory + imagePath;
		}
	}
}