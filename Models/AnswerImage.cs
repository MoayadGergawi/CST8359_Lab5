using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Lab5.Models
{
    public class AnswerImage
    {
        public int AnswerImageId { 
            get; 
            set; 
        }
        
        [Required]
        [StringLength(200, MinimumLength = 1)]
        [DisplayName("File Name")]
        public String FileName { 
            get; 
            set; 
        }
        
        [Required]
        [Url]
        public String Url { 
            get; 
            set; 
        }
        
        [Required]
        public Question Question { 
            get; 
            set; 
        }
    }

    public enum Question
    {
        Earth,
        Computer
    }
}