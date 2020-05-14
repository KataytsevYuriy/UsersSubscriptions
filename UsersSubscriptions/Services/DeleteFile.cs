using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Services
{
    public class DeleteFile
    {
        public void Delete(string patch)
        {
            var fullPatch = Path.Combine(Directory.GetCurrentDirectory(), patch);
            if (File.Exists(fullPatch))
            {
                File.Delete(fullPatch);
            }
        }
    }
}
