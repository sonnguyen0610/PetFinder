using System;
namespace PetFinderAPI.Models
{
    public class ImageDto
    {
        public int Id { get; set; }

        public String Url { get; set; }

        public ImageDto(Image image)
        {
            Id = image.Id;
            Url = image.Url;
        }

        public ImageDto()
        {

        }
    }
}
