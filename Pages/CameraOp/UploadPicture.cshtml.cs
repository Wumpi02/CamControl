using CamControl.Models;
using CamControl.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetVips;
using System.IO;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace CamControl.Pages.CameraOp
{
   
    public class UploadPictureModel : PageModel
    {
        private readonly ICameraService _cameraService;
        [BindProperty]
        public IFormFile UploadImage { get; set; }
        [BindProperty(SupportsGet =true)]
        public Guid PresetGuid { get; set; }
        [BindProperty()]
        public string ImageUrl { get;set;  }

        public UploadPictureModel(ICameraService cameraService)
        {
            _cameraService = cameraService;
        }

        public void OnGet(Guid presetguid)
        {
            ImageUrl = _cameraService.GetImagePath() + presetguid.ToString() + ".png";
        }

        public async Task<IActionResult> OnPostAsync(Guid presetguid)
        {
            string ext;
            Guid cameraguid;
            if (UploadImage != null && UploadImage.Length > 0)
            { ext = System.IO.Path.GetExtension(UploadImage.FileName);
                var filePath = Path.ChangeExtension(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Presets", presetguid.ToString() + ".png"), ext);
                var filePathTemp = filePath + ".tmp";
               if (System.IO.File.Exists(filePathTemp) ) { System.IO.File.Delete(filePathTemp); }
                if (System.IO.File.Exists(filePath)) { System.IO.File.Delete(filePath); }
                using (var fileStream = new FileStream(filePathTemp, FileMode.Create))
                {
                    await UploadImage.CopyToAsync(fileStream);
                    NetVips.Image _image = NetVips.Image.NewFromFile(filePathTemp);
                    using NetVips.Image image = _image.ThumbnailImage(256);
                    filePath = Path.ChangeExtension(filePath, ".png");
                    image.WriteToFile(filePath);
                }

                if (System.IO.File.Exists(filePathTemp)) { System.IO.File.Delete(filePathTemp); }
              cameraguid = _cameraService.GetCameraList().Find(a=> a.Camera_Guid ==  a.Presets.Find(b => b.Preset_Guid == presetguid).Camera_Guid).Camera_Guid;
                return RedirectToPage("/CameraOp/AddEditPreset", new { cameraguid = cameraguid, presetguid = presetguid });
            }
            return Page();
            
        }
    }

}
