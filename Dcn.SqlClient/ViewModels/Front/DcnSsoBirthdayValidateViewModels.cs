using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Dcn.SqlClient.ValidateAttribute;

namespace Dcn.SqlClient.ViewModels.Front
{
    public class DcnSsoBirthdayValidateViewModels
    {
        [Required]
        [StringLength(10, ErrorMessage = "{0} 的長度至少必須為 {2} 個字元。", MinimumLength = 10)]
        [Pid(ErrorMessage = "身分證錯誤，請照身分證上字號輸入！")]
        [Display(Name = "帳號")]
        public string Account { get; set; }

        [Required]
        [MaxLength(20)]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "出生日期")]
        public DateTime? Birthday { get; set; }
    }
}
