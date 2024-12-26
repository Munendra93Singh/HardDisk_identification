using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace WMIAccess
{
    class program
    {
        public class types
        {
            public string DeviceId { set; get; }
            public string DeviceType { set; get; }
            public string Description { set; get; }
        }
        static void Main(string[] args)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"\\.\root\microsoft\windows\storage", "SELECT * FROM MSFT_PhysicalDisk");
            string type = "";
            var result = searcher.Get();
            List<types> _types = new List<types>();

            foreach (ManagementObject queryObj in result)
            {
                switch (Convert.ToInt16(queryObj["MediaType"]))
                {
                    case 1:
                        type = "Unspecified";
                        break;
                    case 3:
                        type = "HDD";
                        break;
                    case 4:
                        type = "SSD";
                        break;
                    case 5:
                        type = "SCM";
                        break;
                    default:
                        type = "Unspecified";
                        break;
                }
                var Description = Convert.ToString(queryObj.Properties["Description"].Value);


                var MediaType = Convert.ToString(queryObj.Properties["MediaType"].Value); // SSD or HDD --Diskdrive C(0).Diskdisk E(1,2)
                var diskDriveName = Convert.ToString(queryObj.Properties["DeviceId"].Value); // 12345678


                string devid = Convert.ToString(queryObj.Properties["DeviceId"].Value);
                _types.Add(new types { Description = MediaType, DeviceType = type, DeviceId = devid });
            }

            var driveQuery = new ManagementObjectSearcher("select * from Win32_DiskDrive");
            foreach (ManagementObject d in driveQuery.Get())
            {
                var Description = d.Properties["Description"].Value;
                var partitionQueryText = string.Format("associators of {{{0}}} where AssocClass = Win32_DiskDriveToDiskPartition", d.Path.RelativePath);
                var partitionQuery = new ManagementObjectSearcher(partitionQueryText);
                foreach (ManagementObject p in partitionQuery.Get())
                {
                    var logicalDriveQueryText = string.Format("associators of {{{0}}} where AssocClass = Win32_LogicalDiskToPartition", p.Path.RelativePath);
                    var logicalDriveQuery = new ManagementObjectSearcher(logicalDriveQueryText);
                    foreach (ManagementObject ld in logicalDriveQuery.Get())
                    {
                        var driveId = Convert.ToString(ld.Properties["DeviceId"].Value); // C:
                        var totalSpace = Convert.ToUInt64(ld.Properties["Size"].Value); // in bytes
                        var driveIdd = Convert.ToString(d.Properties["index"].Value); // C:

                        Console.WriteLine("DriveId: {0}", driveId);

                        string mt = _types.Where(l => l.DeviceId.Equals(driveIdd)).FirstOrDefault().DeviceType;
                        Console.WriteLine("MediaType: {0}", mt);

                        Console.WriteLine($"Total Size :{(Convert.ToSingle(totalSpace) / 1024 / 1024 / 1024):#00} GB");
                        Console.WriteLine(new string('-', 70));
                    }
                }
            }
            Console.ReadKey();
        }
    }
}

