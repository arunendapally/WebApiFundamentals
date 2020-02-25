using System.ComponentModel.DataAnnotations;

namespace TheCodeCamp.Models
{
    public class TalkModel
    {
        public int TalkId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [StringLength(maximumLength: 4096, MinimumLength = 100)]
        public string Abstract { get; set; }

        [Required]
        [Range(1, 100)]
        public int Level { get; set; }

        public SpeakerModel Speaker { get; set; }
    }
}