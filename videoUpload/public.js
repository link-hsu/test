$(function ($) {
  //--------------------Top------------------
  $(window).scroll(function () {
    var scroll = $(window).scrollTop();
    if (scroll >= 70) {
      $(".GoTop").fadeIn();
    } else {
      $(".GoTop").fadeOut();
    }
  });

  $('.GoTop').click(function () {
    $('html,body').animate({
      scrollTop: $('html').offset().top
    })
    return false;
  });

  $(".GoReserve").fadeIn();
  //--------------------WebFontSize 字體放大.縮小設定------------------
  if ($("#FontS").length > 0) {
    $("#FontS").bind("click", function () {
      SetFontSize("S");
      changeFontSize("S");
    });
  }
  if ($("#FontM").length > 0) {
    $("#FontM").bind("click", function () {
      SetFontSize("M");
      changeFontSize("M");
    });
  }
  if ($("#FontL").length > 0) {
    $("#FontL").bind("click", function () {
      SetFontSize("L");
      changeFontSize("L");
    });
  }
  var fontCookieStyle = GetFontSize();
  if (fontCookieStyle != "")
    changeFontSize(fontCookieStyle);
})

//--------------------WebFontSize Start------------------
var fontCookieKey = "WebSiteFontSize";
function SetFontSize(fontSizeStyle) {
  $.cookie(fontCookieKey, fontSizeStyle, { path: '/' });
}
function GetFontSize() {
  var fonCookie = $.cookie(fontCookieKey);
  if (fonCookie == null)
    return "";
  return fonCookie;
}
function changeFontSize(fontSizeStyle) {
  var divID = "panel-body";
  var fontSize = $("#" + divID).css("font-size");
  switch (fontSizeStyle) {
    case "L":
      $("#FontL").addClass("active");
      $("#FontM").removeClass("active");
      $("#FontS").removeClass("active");
      fontSize = "120%";
      break;
    case "M":
      fontSize = "100%";
      $("#FontM").addClass("active");
      $("#FontL").removeClass("active");
      $("#FontS").removeClass("active");
      break;
    case "S":
      fontSize = "80%";
      $("#FontS").addClass("active");
      $("#FontL").removeClass("active");
      $("#FontM").removeClass("active");
      break;
  }
  $("#" + divID).css("font-size", fontSize);
}
//--------------------WebFontSize End------------------

//-------------------JavaScript Extension Function------------------
if (!String.prototype.trim) {
  /**
 * IE9前的版本不支援String.trim()<br />
 * Running the following code before any other code will create String.trim if it's not natively available.
 * @see <a href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/Trim">Mozilla Developer Network</a>.
 */
  String.prototype.trim = function () {
    return this.replace(/^\s+|\s+$/g, '');
  };
}

Date.prototype.format = function (format) {
  //eg: new Date().format("yyyy-MM-dd HH:mm:ss:S")

  if (!format) {
    format = "yyyy-MM-dd hh:mm:ss";
  }

  var o = {
    "M+": this.getMonth() + 1,  // month
    "d+": this.getDate(),       // day
    "H+": this.getHours(),      // hour
    "h+": this.getHours(),      // hour
    "m+": this.getMinutes(),    // minute
    "s+": this.getSeconds(),    // second
    "q+": Math.floor((this.getMonth() + 3) / 3), // quarter
    "S": this.getMilliseconds()
  };

  if (/(y+)/.test(format)) {
    format = format.replace(RegExp.$1, (this.getFullYear() + "")
      .substr(4 - RegExp.$1.length));
  }

  for (var k in o) {
    if (new RegExp("(" + k + ")").test(format)) {
      format = format.replace(RegExp.$1, RegExp.$1.length == 1
        ? o[k]
        : ("00" + o[k]).substr(("" + o[k]).length));
    }
  }

  return format;
};

var Dcn = Dcn || {};
(function (win, $) {
  'use strict';
  //--------------------JsonTool------------------
  var Json = function () { };

  Json.prototype.data = null;

  Json.prototype = {
    Add: function (json_key, json_value) {

      var json = null;

      if (Dcn.Json.data == null) {
        json = [];
      }
      else {
        json = Dcn.Json.data;
      }

      json.push({ key: json_key, value: json_value });

      Dcn.Json.data = json;
    }
  };

  win.Dcn.Json = win.Dcn.Json || new Json();
  //--------------------Ajax------------------
  var Ajax = function () { };

  Ajax.prototype.data = null;
  Ajax.prototype.error = null;

  Ajax.prototype = {
    jquery_upload_file_image: function (post_url, id) {
      console.log(post_url);
      var des_deny_file_type = "您上傳的檔案不符合圖片格式，請重新上傳！";
      var des_deny_file_size = "您上傳圖檔已超過系統限制 5MB ，請調整圖片檔案大小！";
      var accept_file_type = /(\.|\/)(gif|jpe?g|png)$/i;

      var upload_file_type = $(id).attr("accept");

      switch (upload_file_type) {
        case ".pdf":
          des_deny_file_type = "您上傳的檔案不符合 PDF 格式，請重新上傳！";
          des_deny_file_size = "您上傳已超過系統限制 5MB ，請調整檔案大小！";
          accept_file_type = /(\.|\/)(pdf)$/i;
          break;
      }

      $(id).fileupload({
        url: post_url,
        autoUpload: true,
        maxFileSize: 5242880,
        acceptFileTypes: accept_file_type,
        dataType: 'json',
        messages: {
          acceptFileTypes: des_deny_file_type,
          maxFileSize: des_deny_file_size
        },
        done: function (e, data) {
          if (data.result.result == false) {
            Dcn.Message.string("訊息", data.result.error_message);
            if (typeof console !== "undefined" && !!console) console.log("jquery_upload_file_image_done:" + data.result.error_message);
          }
          else {
            Dcn.Ajax.data = data;
            Dcn.Message.hide();
            if (typeof console !== "undefined" && !!console) console.log("jquery_upload_file_image_done:do");

            var now = (new Date()).getHours() + ":" + (new Date()).getMinutes() + ":" + (new Date()).getSeconds();;
            var div = $("<div>").attr("id", jQuery.now()).append(now + "(上傳成功：" + data.files[0].name + ")");
            $(id).after(div);
            div.fadeOut(10000);
            //debugger;
            var state = $(id + "_State").get(0);
            if (state) {
              state.innerText = "(上傳成功)";
              $(id + "_State").show();
            }

            var update_delete_button = $("button[btn-upload-id^='" + id.replace('#', '') + "']");
            if (update_delete_button) {
              update_delete_button.show();
            }
            location.reload();

          }
        },
        submit: function (e, data) {
          $(Dcn.Message.head_id).show();
          $(Dcn.Message.footer_id).show();

          var body = $("<div>");
          body.attr("title", "上傳中");
          body.append("<span class='loading' aria-hidden='true'></span>上傳中...");

          console.log(body);
          Dcn.Message.title = "訊息";
          Dcn.Message.body = body;
          Dcn.Message.show();

          if (typeof console !== "undefined" && !!console) console.log("jquery_upload_file_image_submit:do");
        },
        processfail: function (e, data) {
          if (data.files[0].error) {
            Dcn.Message.string("訊息", data.files[0].error);
          }

          if (typeof console !== "undefined" && !!console) console.log(data.files[0].error);
        },
        fail: function (e, data) {
          $("#modal-container").modal('hide');
        }
      });
    },
    jquery_get_file_image: function (url, post_url, id) {
      var post_data = { 'post_url': post_url };
      console.log(url, post_url, id);
      $.ajax({
        url: url,
        type: "post",
        cache: false,
        async: false,
        dataType: 'json',
        data: post_data,
        beforeSend: function () {
          Dcn.Ajax.data = null;
        },
        success: function (data) {
          Dcn.Ajax.data = data;
          $("#" + id).attr('src', 'data:image/png;base64,' + data);
          if (typeof console !== "undefined" && !!console) console.log('call_url_json_done:' + url);
        },
        error: function (error) {
          Dcn.Ajax.error = error.statusText;
          if (typeof console !== "undefined" && !!console) console.log('call_url_json_error');
          if (typeof console !== "undefined" && !!console) console.log(error);
        }
      });

    },
    jquery_upload_delete_file_image: function () {

      if (window.confirm('確定是否刪除')) {
        //debugger;
        var data = null;
        var id = $(this).attr('btn-upload-id');
        var action = $(this).attr('btn-upload-action');
        var file_extension = '';

        var __RequestVerificationToken = $("[name='__RequestVerificationToken']").val();
        var option = { "file_id": id, __RequestVerificationToken: __RequestVerificationToken, "action": action };

        Dcn.Ajax.call_url_json(post_delete_url, option);
        if (typeof (Dcn.Ajax.error) == "undefined" && typeof (Dcn.Ajax.data) != "undefined") {
          Dcn.ImageTool.data = Dcn.Ajax.data;
          data = Dcn.Ajax.data;

          if (data.result == true) {
            var set_link = '';
            switch (id) {
              case "DriverLicence1":
                set_link = '/Content/Image/Open/Sop/ImageUploadTemplate/DRI1.jpg'
                break
              case "DriverLicence2":
                set_link = '/Content/Image/Open/Sop/ImageUploadTemplate/DRI2.jpg'
                break
              case "HealthInsurance1":
                set_link = '/Content/Image/Open/Sop/ImageUploadTemplate/HID1.jpg'
                break
              case "InBank2":
              case "InBank3":
                set_link = '/Content/Image/Open/Sop/ImageUploadTemplate/BANK1.jpg'
                break;
              case "RealEstate2":
              case "RealEstate3":
                set_link = '/Content/Image/Open/Sop/ImageUploadTemplate/REALESTATE1.jpg';
                break;
              case "TradeRecord2":
                set_link = '';
                break;
              case "TradeRecord50":
                set_link = '#';
                break;
              case "ForeignInBank1":
              case "ForeignOutBank1":
                set_link = '';
              default:
                set_link = '#';
                break;
            }

            $("#" + id + "_State").hide();
            $(this).hide();

            switch (data.file_extension) {
              case '.pdf':
                file_extension = 'pdf';
                break;
              default:
                file_extension = 'image';
                break;
            }

            Dcn.ImageTool.set_url('#' + data.file_id + '_' + file_extension, set_link);
            Dcn.Message.string("訊息", '己刪除成功');
          }
        }
        else {
          Dcn.Message.string("錯誤訊息", Dcn.Ajax.error);
        }
      }
      else {
        alert('取消刪除');
        return false;
      }
    },
      jquery_upload_file_video: async function (video_record, start_click, end_click) {
          let mediaRecord;
          let recordChunks = [];

          try {
              const stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: "user" }, audio: true, });
              videoRecord.srcObject = stream;

              document.getElementById(start_click).addEventListener('click', () => {
                  recordChunks = [];
                  const options = MediaRecorder.isTypeSupported("video/mp4;codecs=avc1")
                      ? { mimeType: "video/mp4;codecs=avc1" }
                      : { mimeType: "video/webm;codecs=vp8" };
                  mediaRecord = new MediaRecorder(stream, options);

                  mediaRecord.ondataavailable = (event) => {
                      if (event.data.size > 0) {
                          recordChunks.push(event.data);
                      }
                  };
                  mediaRecord.onstop = async () => {
                      const blob = new Blob(recordChunks, { type: options.mimeType });
                      const formData = new FormData();
                      formData.append("videoFile", blob, "recording." + (options.mimeType.includes("mp4") ? "mp4" : "webm"));

                      const controller = new AbortController();
                      setTimeout(() => controller.abort(), 30000); // 30秒超時

                      try {
                          const response = await fetch("upload", {
                              method: "POST",
                              body: formData,
                              signal: controller.signal,
                          });
                          if (response.ok) {
                              console.log("錄製完成並成功上傳！");
                          } else {
                              console.log("上傳失敗！");
                          }
                      } catch (error) {
                          console.log("上傳失敗，請檢查網路或稍後重試！");
                      }
                  };
                  mediaRecord.start();

                  document.getElementById(start_click).disabled = true;
                  document.getElementById(end_click).disabled = false;
              });

              document.getElementById(end_click).addEventListener('click', () => {
                  mediaRecord.stop();
                  document.getElementById(start_click).disabled = false;
                  document.getElementById(end_click).disabled = true;
              });

          } catch (err) {
              console.log("Error: " + err);
              console.log("無法啟動攝像頭，請檢查設備設置！");
          }
    },
    //json in , json out
    call_url_json: function (url, post_data) {
      console.log(post_data);
      $.ajax({
        url: url,
        type: "post",
        cache: false,
        async: false,
        dataType: 'json',
        data: post_data,
        beforeSend: function () {
          Dcn.Ajax.data = null;
        },
        success: function (data) {
          Dcn.Ajax.data = data;
          if (typeof console !== "undefined" && !!console) console.log('call_url_json_done:' + url);
        },
        error: function (error) {
          Dcn.Ajax.error = error.statusText;
          if (typeof console !== "undefined" && !!console) console.log('call_url_json_error');
          if (typeof console !== "undefined" && !!console) console.log(error);
        }
      });
    },
    sales: function (option) {
      var result = null;
      var url = "/Option/Sales";

      Dcn.Ajax.call_url_json(url, option);

      if (typeof (Dcn.Ajax.error) == "undefined" && typeof (Dcn.Ajax.data) != "undefined") {
        result = Dcn.Ajax.data;

        $(option.id).empty();

        if (result.length > 0) {
          $.each(result, function (i, item) {
            $(option.id).append(
              $('<option>', {
                text: item.Text,
                value: item.Value,
                selected: item.Selected
              }
              ));
          });
        }
        var sales = localStorage.getItem("sales");
        if (sales != '') $('#SalesId').val(sales);
        localStorage.removeItem("comp");
        localStorage.removeItem("sales");
      }
      else {
        Dcn.Message.string("錯誤訊息", Dcn.Ajax.error);
      }
    },
    city_init_json: function (option) {

      Dcn.Ajax.city(option.city, option.city_selected);
      Dcn.Ajax.town(option.city, option.city_selected, option.town, option.town_selected);

      $(option.city).change(function () {
        Dcn.Ajax.town(option.city, $(option.city).val(), option.town, $(option.town).val());
      });

    },
    city: function (city, city_selected) {
      var result = null;
      var option = { "city": city, "city_selected": city_selected };
      var url = "/Option/City";

      Dcn.Ajax.call_url_json(url, option);

      if (typeof (Dcn.Ajax.error) == "undefined" && typeof (Dcn.Ajax.data) != "undefined") {
        result = Dcn.Ajax.data;

        $(city).empty();

        if (result.length > 0) {
          $.each(result, function (i, item) {
            $(city).append($('<option>', {
              text: item.Text,
              value: item.Value,
              selected: item.Selected
            }));

          });
        }
      }
      else {
        Dcn.Message.string("錯誤訊息", Dcn.Ajax.error);
      }
    },
    town: function (city, city_selected, town, town_selected) {
      var result = null;
      var option = { "city": city, "city_selected": city_selected, "town": town, "town_selected": town_selected };
      var url = "/Option/Town";

      Dcn.Ajax.call_url_json(url, option);

      if (typeof (Dcn.Ajax.error) == "undefined" && typeof (Dcn.Ajax.data) != "undefined") {
        result = Dcn.Ajax.data;

        $(town).empty();

        if (result.length > 0) {
          $.each(result, function (i, item) {
            $(town).append($('<option>', {
              text: item.Text,
              value: item.Value,
              selected: item.Selected
            }));
          });
        }
      }
      else {
        Dcn.Message.string("錯誤訊息", Dcn.Ajax.error);
      }
    }
  };
  win.Dcn.Ajax = win.Dcn.Ajax || new Ajax();

  //--------------------ImageTool------------------
  var ImageTool = function () { };
  ImageTool.prototype.data = null;
  ImageTool.prototype = {
    set_url: function (id, set_link) {
      var attr_type = "";
      var text_info = "";

      switch (Dcn.ImageTool.data.file_extension) {
        case ".pdf":
          attr_type = "href";
          $(id).text("檔案資訊");
          break;
        default:
          attr_type = "src";
          break;
      }

      $(id).attr(attr_type, set_link);

      switch (set_link) {
        case '':
          $(id).removeAttr(attr_type);
          break;
        case '#':
          $(id).text("無");
          $(id).removeAttr(attr_type);
          break;
      }

    },
    set_fileuploaddone_data_to_url: function (e, data) {
      //debugger;

      if (data.result.result == true) {
        Dcn.ImageTool.data = data.result;

        var file_extension = '';
        switch (data.result.file_extension) {
          case '.pdf':
            file_extension = 'pdf';
            break;
          default:
            file_extension = 'image';
            break;
        }

        Dcn.ImageTool.set_url('#' + data.result.file_id + '_' + file_extension, data.result.web_file_path);
      }
    }
  };
  win.Dcn.ImageTool = win.Dcn.ImageTool || new ImageTool();

  //--------------------Message------------------
  var Message = function () { };

  Message.prototype = {
    modal_content_id: "#modal-content",
    head_id: "#modal-header",
    id: "#modal-container",
    title_id: "#modal-header-title",
    body_id: "#modal-body-content",
    footer_id: "#modal-footer",
    header_btn_close_id: "#modal-footer-btn-close",
    footer_btn_close_id: "#modal-header-btn-close",
    title: "訊息",
    content: "處理中",
    title_close: "關閉",
    body: null,
    array: function (_title, message) {
      Dcn.Message.clear();

      var title = $("<div>");
      var body = $("<div>");

      $.each(message, function (index, value) {
        body.append($("<li>").text(value));
      });

      title.attr("class", "h4");
      title.text(_title);

      Dcn.Message.title = title;
      Dcn.Message.body = body;

      $(Dcn.Message.title_id).append(Dcn.Message.title);
      $(Dcn.Message.body_id).append(Dcn.Message.body);

      Dcn.Message.btn_close(1);
      Dcn.Message.show();
    },
    string: function (_title, content) {
      Dcn.Message.clear();

      var title = $("<div>");
      title.attr("class", "h4");
      title.text(_title);

      var body = $("<li>");
      body.text(content);

      Dcn.Message.title = title;
      Dcn.Message.body = body;

      Dcn.Message.btn_close(1);
      Dcn.Message.show();
    },
    Foucs: function () {
      $(Dcn.Message.id).modal({ backdrop: 'static', keyboard: false });
    },
    //--------------------接受：標題、完整內容(含div)-----------------//
    SendTitleContents: function (_title, content) {
      Dcn.Message.clear();

      var title = $("<div>");
      title.attr("class", "h4");
      title.text(_title);

      var body = $("<div>");
      body.append(content);

      Dcn.Message.title = title;
      Dcn.Message.body = body;

      Dcn.Message.btn_close(1);
      Dcn.Message.show();

    },
    FormSubmit: function (_title, _content) {

      if ($("form").data("validator") != null && $("form").data("validator").errorList.length > 0)
        return false;

      if (typeof ($("form").validate) === "function" && !$("form").validate().checkForm())
        return false;

      var title;
      var content;

      if (typeof (_title) == "undefined")
        _title = "訊息";

      if (typeof (_content) == "undefined")
        _content = this.content;

      var title = $("<div>");
      title.attr("class", "h4");
      title.text(_title);

      var body = $("<div>");
      body.append("<span class='loading' aria-hidden='true'></span>" + _content);
      //body.addClass("loading");
      //body.text(_content);

      Dcn.Message.title = title;
      Dcn.Message.body = body;

      $(Dcn.Message.head_id).show();
      $(Dcn.Message.footer_id).show();
      Dcn.Message.btn_close(0);
      Dcn.Message.show();
    },
    DialogDiv: function (div) {
      $(Dcn.Message.head_id).hide();
      $(Dcn.Message.footer_id).hide();

      $(Dcn.Message.body_id).append(div);

      $(Dcn.Message.id).modal("show");
    },
    DialogShowPage: function (action, controller, area) {
      $(Dcn.Message.head_id).hide();
      $(Dcn.Message.footer_id).hide();

      if (typeof (area) != "undefined") area += "/";
      else area = "";

      var url = "/" + area + controller + "/" + action;
      $(Dcn.Message.body_id).load(url, function () {
        $(Dcn.Message.id).modal("show");
      });
      //$(Dcn.Message.id).modal("show");
    },
    DialogEditPage: function (controller, action, id, area) {
      if (typeof (area) != "undefined") area += "/";
      else area = "";

      var url = "/" + area + controller + "/" + action + "/" + id;
      $(Dcn.Message.body_id).load(url, function () {
        $(Dcn.Message.id).modal("show");
      });
      $(Dcn.Message.head_id).hide();
      $(Dcn.Message.footer_id).hide();
      $(Dcn.Message.id).modal("show");
    },
    DialogImageUrl: function (url) {
      //if (typeof (url) == "undefined" || url == "" || url.indexOf(".") == -1) return false;

      Dcn.Message.clear();
      var image = $("<img>");
      image.attr({
        src: url,
        title: "圖片(點選圖片，另跳原始檔圖片大小)",
        alt: "圖片(點我，另跳原始檔圖片大小)",
        Class: "img-responsive img-thumbnail",
        onclick: "imgWindow('" + url + "')"
      });


      $("#modal-header-title").append("照片瀏覽 (點選圖片，另跳原始檔圖片大小)");
      $("#modal-body-content").append(image);

      Dcn.Message.btn_close(1);
      $(Dcn.Message.id).modal({ keyboard: true }, 'show');
    },
    show: function () {
      Dcn.Message.clear();

      $(Dcn.Message.title_id).append(Dcn.Message.title);
      $(Dcn.Message.body_id).append(Dcn.Message.body);
      $(Dcn.Message.header_btn_close_id).text(Dcn.Message.title_close);

      $(Dcn.Message.id).modal("show");
    },
    event: function (option) {
      $(Dcn.Message.id).on('hidden.bs.modal', function () {
        switch (option.action) {
          case "redirect":
            window.location = option.url;
            break;
        }
      })
    },
    hide: function () {
      $(Dcn.Message.id).modal("hide");
    },
    btn_close: function (show) {
      switch (show) {
        case 0:
          $(Dcn.Message.header_btn_close_id).hide();
          $(Dcn.Message.footer_btn_close_id).hide();
          break;
        case 1:
          $(Dcn.Message.header_btn_close_id).show();
          $(Dcn.Message.footer_btn_close_id).show();
          break;
      }
    },
    clear: function () {
      $(Dcn.Message.title_id).empty();
      $(Dcn.Message.body_id).empty();


      $(Dcn.Message.head_id).show();
      $(Dcn.Message.footer_id).show();
    }
  };



  win.Dcn.Message = win.Dcn.Message || new Message();

  //--------------------CheckBox Tool------------------
  //Dcn.CheckBox.GetSelectedValues("input[name=InformationSource]");
  var CheckBox = function () { };

  CheckBox.prototype = {
    one: function (id) {
      $(id).click(function () {
        var Selected = $(this).val();
        $(id).each(function (i) {
          if ($(this).val() == Selected) $(this).prop("checked", true);
          else $(this).prop("checked", false);
        });
      });
    },
    GetSelectedValues: function (id) {
      var result = [];

      $(id).each(function (i) {
        if (this.checked == true)
          result.push($(this).val());
      });

      return result;
    }
  };
  win.Dcn.CheckBox = win.Dcn.CheckBox || new CheckBox();
  //--------------------Radio Tool------------------
  //Dcn.Radio.GetSelectedValue("Input[Name=InfoSourceCode]");
  var Radio = function () { };

  Radio.prototype = {
    GetSelectedValue: function (id) {
      var result = "";
      $(id).each(function (i) {
        if (this.checked == true)
          result = $(this).val();
      });

      return result;
    }
  };
  win.Dcn.Radio = win.Dcn.Radio || new Radio();
  //--------------------FormUtil Tool------------------
  var FormUtil = function () { };

  FormUtil.prototype = {
    Required: function (id, msg) {
      //debugger;
      var error = "";

      if (msg)
        error = msg;
      else
        error = $(id).attr("data-val-required");

      $(id).rules('add', {
        "required": true,
        messages: {
          required: error
        }
      });
    },
    removeRequired: function (id) {
      debugger;
      $(id).rules("remove", "required");
    }
  };
  win.Dcn.FormUtil = win.Dcn.FormUtil || new FormUtil();
  //--------------------Function Util------------------
  var FunUtil = function () { };

  FunUtil.prototype = {
    //如何得知-連動:聚財網會員帳號:
    InfoSource: function () {
      //debugger;
      if (Dcn.Radio.GetSelectedValue("Input[Name=InfoSourceCode]") == "50") {
        $("#InfoSourceOther").attr('disabled', false);
      }
      else {
        $("#InfoSourceOther").val("");
        $("#InfoSourceOther").attr('disabled', true);
        Dcn.Validate.ErrorClear("#InfoSourceOther-error");
      }
    },
    //外籍人士(Click)
    Usa: function (e) {
      var UsaValue = $(e.target).val();
      if (UsaValue != "0") {
        Dcn.Message.string("訊息", "線上未開放外籍客戶，若要開戶請臨櫃辦理！");
        $("Input[Name=Usa]").get(0).checked = true;
      }
    },
    //是否有在其它券商開戶(Click)
    OtherSecuritiesAccountCredit: function (e) {
      var OtherSecuritiesAccountCreditValue = $(e.target).val();
      if (OtherSecuritiesAccountCreditValue != "1") {
        Dcn.Message.SendTitleContents("訊息", "未於其它券商開戶，無法線上信用開戶。請於以下連結申請「證券」開戶。<a href='https://customer.dcn.com.tw/StockOpen'>https://customer.dcn.com.tw/StockOpen</a>");
        $("Input[Name=OtherSecuritiesAccountCredit]").get(1).checked = true;
      }
    },
    //檢查申請信用額度(Click)
    MarginTradingShortSelling: function (e) {
      var MarginTradingShortSellingValue = $("Input[Name=MarginTradingShortSelling]").val();
      var ProofOfPropertyValue = $("Input[Name=ProofOfProperty]:checked").val();
      if (MarginTradingShortSellingValue > 50 && (ProofOfPropertyValue == 1 || ProofOfPropertyValue == 2)) {
        Dcn.Message.SendTitleContents("訊息", "申請信用額度超過50萬，財力證明需選擇「不動產」或「其它」，並上傳財力證明文件。");
        $("Input[Name=MarginTradingShortSelling]").val("");
      }
    },
    //同戶籍地址-連動.通訊地址(Click)
    SameResident: function (e) {
      //debugger;
      if ($("#SameResident").is(':checked')) {
        Dcn.Ajax.city('#CityCode', $('#ResidentCityCode').val());
        Dcn.Ajax.town('#CityCode', $('#ResidentCityCode').val(), '#TownCode', $('#ResidentTownCode').val());
        $('#Address').val($('#ResidentAddress').val());
        Dcn.Validate.ErrorClear("#CityCode-error");
        Dcn.Validate.ErrorClear("#TownCode-error");
        Dcn.Validate.ErrorClear("#Address-error");
      }
    },
    //同通訊地址-連動.居住地址(Click)
    SameCorrespondence: function (e) {
      //debugger;
      if ($("#SameCorrespondence").is(':checked')) {
        Dcn.Ajax.city('#CurrentCityCode', $('#CityCode').val());
        Dcn.Ajax.town('#CurrentCityCode', $('#CityCode').val(), '#CurrentTownCode', $('#TownCode').val());
        $('#CurrentAddress').val($('#Address').val());
        Dcn.Validate.ErrorClear("#CurrentCityCode-error");
        Dcn.Validate.ErrorClear("#CurrentTownCode-error");
        Dcn.Validate.ErrorClear("#CurrentAddress-error");
      }
    },
    //服務地址-連動.鄉鎮.地址(Change)
    CompanyCity: function () {
      //debugger;
      Dcn.Ajax.town('#CompanyCityCode', $('#CompanyCityCode').val(), '#CompanyTownCode', $('#CompanyTownCode').val());
    },
    //戶籍地址-連動.鄉鎮.地址(Change)
    ResidentCity: function () {
      //debugger;
      Dcn.Ajax.town('#ResidentCityCode', $('#ResidentCityCode').val(), '#ResidentTownCode', $('#ResidentTownCode').val());
    },
    //通訊地址-連動.鄉鎮.地址(Change)
    City: function () {
      //debugger;
      Dcn.Ajax.town('#CityCode', $('#CityCode').val(), '#TownCode', $('#TownCode').val());
    },
    CurrentCity: function () {
      //debugger;
      Dcn.Ajax.town('#CurrentCityCode', $('#CurrentCityCode').val(), '#CurrentTownCode', $('#CurrentTownCode').val());
    },
    //職業-連動:職稱.服務機構(Change)
    Occupation: function () {
      //debugger;
      var option = {
        CompanyName: "服務機構名稱",
        CompanyTelePhone: "服務機構電話",
        CompanyAddress: "服務機構地址",
      }

      var OccupationValue = $(this).val();
      switch (OccupationValue) {
        case "":
          $('#CompanyName').attr('disabled', true);
          $('#CompanyTelePhone').attr('disabled', true);
          $('#CompanyCityCode').attr('disabled', true);
          $('#CompanyTownCode').attr('disabled', true);
          $('#CompanyAddress').attr('disabled', true);
          break;
        case "19":
        case "20":
        case "21":
        case "22":
        case "23":
          $('#JobTitleCode').val("18");
          $('#CompanyCityCode').val("");
          $('#CompanyTownCode').val("");
          $('#CompanyName').attr('disabled', true);
          $('#CompanyTelePhone').attr('disabled', true);
          $('#CompanyCityCode').attr('disabled', true);
          $('#CompanyTownCode').attr('disabled', true);
          $('#CompanyAddress').attr('disabled', true);
          $('#CompanyName').val("");
          $('#CompanyTelePhone').val("");
          $('#CompanyAddress').val("");
          Dcn.Validate.ErrorClear("#JobTitleCode-error");
          Dcn.Validate.ErrorClear("#CompanyName-error");
          Dcn.Validate.ErrorClear("#CompanyTelePhone-error");
          Dcn.Validate.ErrorClear("#CompanyCityCode-error");
          Dcn.Validate.ErrorClear("#CompanyTownCode-error");
          Dcn.Validate.ErrorClear("#CompanyAddress-error");

          $('#CompanyName').data('bs.popover').options.content = "(非必填)" + option.CompanyName;
          $('#CompanyName').attr("placeholder", "(非必填) 請輸入「" + option.CompanyName + "」");
          $('#CompanyName').attr("title", "(非必填) 請輸入「" + option.CompanyName + "」");

          $('#CompanyTelePhone').data('bs.popover').options.content = "(非必填) (請輸入「數字」不含任何符號)";
          $('#CompanyTelePhone').attr("placeholder", "(非必填) 請輸入「" + option.CompanyTelePhone + "」");
          $('#CompanyTelePhone').attr("title", "(非必填) 請輸入「" + option.CompanyTelePhone + "」");

          $('#CompanyAddress').data('bs.popover').options.content = "(非必填)" + option.CompanyAddress;
          $('#CompanyAddress').attr("placeholder", "(非必填) 請輸入「" + option.CompanyAddress + "」");
          $('#CompanyAddress').attr("title", "(非必填) 請輸入「" + option.CompanyAddress + "」");

          break;
        default:
          $('#CompanyName').attr('disabled', false);
          $('#CompanyTelePhone').attr('disabled', false);
          $('#CompanyCityCode').attr('disabled', false);
          $('#CompanyTownCode').attr('disabled', false);
          $('#CompanyAddress').attr('disabled', false);

          $('#CompanyName').data('bs.popover').options.content = option.CompanyName;
          $('#CompanyName').attr("placeholder", "請輸入「" + option.CompanyName + "」");
          $('#CompanyName').attr("title", "服務機構名稱");

          $('#CompanyTelePhone').data('bs.popover').options.content = "(請輸入「數字」不含任何符號)";
          $('#CompanyTelePhone').attr("placeholder", "請輸入「" + option.CompanyTelePhone + "」");
          $('#CompanyTelePhone').attr("title", "請輸入「" + option.CompanyTelePhone + "」");

          $('#CompanyAddress').data('bs.popover').options.content = option.CompanyAddress;
          $('#CompanyAddress').attr("placeholder", "請輸入「" + option.CompanyAddress + "」");
          $('#CompanyAddress').attr("title", "請輸入「" + option.CompanyAddress + "」");
          break;
      }
    },
    //指定營業員-連動(Change)
    AssignSales: function (event) {
      //debugger;
      var BranchValue = $("Input[Name=BranchCode]:checked").val();
      var AssignValue = $("Input[Name=AssignSales]:checked").val();
      if (!BranchValue) {
        Dcn.Message.string("提示訊息", "請先選擇「開戶分公司別」");
        $("Input[Name=AssignSales]:checked").get(0).checked = false;
        return false;
      }

      switch (AssignValue) {
        case "0":
          Dcn.Ajax.sales({ id: "#SalesId", branch_selected: BranchValue });
          $('#SalesId').attr('disabled', true);
          Dcn.Validate.ErrorClear("#SalesId-error");
          break;
        case "1":
          if (BranchValue == "0") {
            Dcn.Message.string("提示訊息", "請先選擇「開戶分公司別」");
            $("Input[Name=AssignSales]").get(1).checked = true;
          }
          else if (BranchValue != "") {
            $('#SalesId').attr('disabled', false);
            Dcn.Ajax.sales({
              id: "#SalesId",
              branch_selected: BranchValue,
              license: event.data.license
            });
          }
          else {
            Dcn.Message.string("提示訊息", "請先選擇「開戶分公司別」");
            $('#SalesId option[value=""]').attr('selected', true);
            $("Input[Name=AssignSales]").get(0).checked = false;
          }
          break;
      }
    },
    //選分公司-連動:指定營業員.營業員(Change)
    Branch: function (event) {
      //debugger;
      var BranchValue = $(this).val();
      var SalesValue = "";

      if (BranchValue == "0") {
        $("Input[Name=AssignSales]").get(1).checked = true;
        Dcn.Ajax.sales({ id: "#SalesId", branch_selected: BranchValue });
      }
      else {
        $("Input[Name=AssignSales]").get(0).checked = true;
      }

      Dcn.Validate.ErrorClear("#AssignSales-error");

      $("Input[Name=AssignSales]").trigger("change", event.data.license)
    },
    //Back 狀態 change異動 (相關欄位：SendToCustomer)
    SendToCustomer: function (event) {
      //debugger;
      if ($("#StatusCode").val() === "3") {
        $("Input[Name=SendToCustomer]").prop('disabled', false);
        $("Input[Name=SendToCustomer]").prop('checked', true);
        $("Label[for=SendToCustomer]").show();
      }
      else {
        $("Input[Name=SendToCustomer]").prop('disabled', true);
        $("Label[for=SendToCustomer]").hide();
      }
    },
    //Back 證.期.複
    StatusCodeInit: function (event) {
      //debugger;
      var item = $("#trCancelCode");
      switch ($("#StatusCode").val()) {
        case "9":
          if (item) {
            item.show();
            $("Input[Name=CancelCode]").prop('disabled', false);
          }
          break;
        default:
          if (item) {
            item.hide();
            $("Input[Name=CancelCode]").prop('disabled', true);
          }
          break
      }
    },
    StatusCodeChange: function (event) {
      //debugger;

      switch ($(this).val()) {
        case "2":
          var bank = $("#BankAccountId");
          if (bank && $("#BankAccountId").val() == "") {
            alert("狀態為已完成交割戶時，請輸入客戶之【銀行交割帳戶】!");
          }
          break;
        case "3":
          var branch = $("#BranchCode");
          if (branch && branch.val() == "0") {
            alert("完成開戶前，請先指派客戶之【分公司】及【營業員】!!!");
          }
          break;
        case "9":
          alert("請註記【取消開戶原因】在備註欄位，及選擇【案件取消分類】!!");
          break;
      }
      Dcn.FunUtil.StatusCodeInit();
    },
    ApplyClick: function () {
      var pid = Dcn.Forge.Pid;
      var controller = Dcn.Forge.ControllerPost;
      if (Dcn.Forge.cert != null) {
        Dcn.Message.string("訊息", "已申請過「憑證」！");
        return false;
      }

      var apply_done = Dcn.Forge.ApplyCertificate(controller, pid);
      if (apply_done) {
        var apply_text = $("#Apply").text() + "(完成)";
        $("#Apply").attr('disabled', true);
        $("#Apply").text(apply_text);

        $("#Sign").removeAttr("disabled");
        Dcn.Message.string("「申請憑證」成功", "請點選「2.確認簽章」，可進行下一步！");
      }
      else {
        Dcn.Message.string("「申請憑證」失敗", "請重新整頁面，再點選「確認簽章」，若依舊有問題請連絡客服人員！");
      }
    },
    SignClick: function () {
      var pid = Dcn.Forge.Pid;
      var controller = Dcn.Forge.ControllerPost;
      if (Dcn.Forge.cert == null) {
        Dcn.Message.string("訊息", "請點選「申請憑證」！");
        return false;
      }

      if (Dcn.Forge.p7 != null) {
        Dcn.Message.string("訊息", "已「確認簽章」成功", "請可點選「下一步」！");
        return false;
      }

      var sign_done = Dcn.Forge.SignCertificate(controller, pid);
      if (sign_done) {
        var sign_text = $("#Sign").text() + "(完成)";
        $("#Sign").text(sign_text);
        $("#Sign").attr('disabled', true);

        $("#SerialNumber").text(Dcn.Forge.serialNumber);

        $("Input[Type=submit]").removeAttr('disabled', true);

        Dcn.Message.string("「確認簽章」成功，完成「電子憑證簽章」", "請點選「下一步」！");
      }
      else {
        Dcn.Message.string("錯誤", "簽章失敗！");
      }
    }
  };
  win.Dcn.FunUtil = win.Dcn.FunUtil || new FunUtil();
  //--------------------TextArea Tool------------------
  var TextArea = function () { };

  TextArea.prototype = {
    limit: function (id, number) {
      $(id).keypress(function () {
        var id_content_length = $(id).val().length;
        if (id_content_length > number) {
          $(id).val($(id).val().substring(0, number));
          Dcn.Message.string("訊息", "超過字數限制，多出的字將被截斷！");
        }
        var num = id_content_length;
      });
    }

  };
  win.Dcn.TextArea = win.Dcn.TextArea || new TextArea();

  //--------------------Utility Tool------------------
  var Utility = function () { };
  Utility.prototype.UserBorwser = null;

  Utility.prototype = {
    // 表單新增 Input 限制輸入長度(MaxLength)
    SetInputMaxLength: function () {
      $(document).ready
        (
          function () {
            $('input[data-val-length-max]').each(
              function (index) {
                $(this).attr('maxlength', $(this).attr('data-val-length-max'));
              });
            $('input[data-val-maxlength-max]').each(
              function (index) {
                $(this).attr('maxlength', $(this).attr('data-val-maxlength-max'));
              });
          });
    },
    // Value Not Empty Setting:Disabled, If Empty Setting:Enabel
    SetTextValueNotEmptyDisabled: function (id, value) {
      if (value != "") {
        $(id).attr('disabled', false);
      }
      else {
        $(id).attr('disabled', true);
        $(id).val('');
      }
    },
    // 補前置零
    SetFrontZero: function (iLen, val) {
      var val = val + "";
      var rtn = "";
      for (var i = 0; i < iLen; i++) {
        rtn += "0";
      }
      rtn += val;
      rtn = rtn.substring(val.length, rtn.length);

      return rtn;
    },
    // 加入最愛
    Favorite: function () {
      window.external.AddFavorite(location.href, document.title);
    },
    //抓 ie 版本
    GetIEVersion: function () {
      var userAgent = window.navigator.userAgent;
      var Idx = userAgent.indexOf("MSIE");
      var IsEdge = userAgent.indexOf("Edge") > -1;

      if (Idx > 0)
        return parseInt(userAgent.substring(Idx + 5, userAgent.indexOf(".", Idx)));
      else if (!!userAgent.match(/Trident\/7\./))
        return 11;
      else if (IsEdge)
        return 12; //Edge 
      else
        return -1; //It is not IE
    },
    //抓 Window Chrome 版本
    GetWindowChromeVersion: function () {
      var raw = navigator.userAgent.match(/Chrom(e|ium)\/([0-9]+)\./);

      return raw ? parseInt(raw[2], 10) : -1;
    },
    //抓 IOS Chrome 版本
    GetMacChromeVersion: function () {
      var raw = navigator.userAgent.match(/(?:Chrome|CriOS)\/(\d+)/);

      return raw ? parseInt(raw[1], 10) : -1;
    },
    //抓瀏覽器資訊
    GetBorwser: function () {
      if (Dcn.Utility.UserBorwser)
        return Dcn.Utility.UserBorwser;

      var result = { userAgent: '', mobile: false, kind: '', version: -1, ie: false, chrome: false, line: false, android: false, mac: false, system: '' };
      var userAgent = window.navigator.userAgent;

      var isIE = userAgent.search("MSIE") > -1;
      var isIE7 = userAgent.search("MSIE 7") > -1;
      var isFirefox = userAgent.search("Firefox") > -1;
      var isOpera = userAgent.search("Opera") > -1;
      var isChrome = userAgent.search("Chrome") > -1;
      var isIOSChrome = userAgent.search("CriOS") > -1;
      var isSafari = userAgent.search("Safari") > -1;
      var isLine = userAgent.search("Line") > -1;
      var isMobile = false;

      var isMac = userAgent.search("Mac") > -1
      var isAndroid = userAgent.search("Android") > -1
      var isLinux = userAgent.search("Linux") > -1

      if (this.GetIEVersion() > 0) {
        result.kind = 'IE';
        result.version = this.GetIEVersion();
        result.ie = true;
      }

      if (isFirefox) {
        result.kind = 'Firefox';
      }

      if (isOpera) {
        result.kind = 'Opera';
      }

      if (isIOSChrome) {
        result.kind = 'Chrome';
        result.version = this.GetMacChromeVersion();
        result.chrome = true;
        isSafari = false;
        result.mac = true;
        result.system = 'Mac';
      }

      if (isChrome) {
        result.kind = 'Chrome';
        result.version = this.GetWindowChromeVersion();
        result.chrome = true;
        isSafari = false;

        isSafari = false;
        result.mac = true;
        result.system = 'Mac';
      }

      if (isMac && isSafari) {
        result.kind = 'Safari';
        result.system = 'Mac';
        result.mac = true;
      }

      if (isAndroid && isLinux) {
        result.system = 'Android';
        result.android = true;
        if (isChrome)
          result.kind = 'Chrome';
      }

      if (isLine) {
        result.kind = 'Line';
        result.line = true;
        result.chrome = false;
      }

      if (/ipad/i.test(userAgent.toLowerCase())) {
        result.kind = "ipad";
        result.mobile = true;
        result.system = "mobile";
      }
      else {
        if (/iphone|ipod|android|blackberry|mini|windows\sce|palm/i.test(userAgent.toLowerCase())) {
          result.mobile = true;
          result.system = 'mobile';
        } else {
          result.system = 'pc';
        }
      }

      result.userAgent = userAgent;

      Dcn.Utility.UserBorwser = result;

      return result;
    },
    CheckIE8: function () {

      var result = false; // Default value assumes failure. 

      result = +(/MSIE\s(\d+)/.exec(navigator.userAgent) || 0)[1] < 9;

      return result;
    },
    //確認 User 瀏覽器是否符合規範
    CheckBorwser: function () {
      //debugger;
      var result = false;
      var borwser = Dcn.Utility.GetBorwser();

      Dcn.Message.clear();
      var title = $("<div>");
      title.attr("class", "h4");
      title.text("訊息：");

      var body = $("<div>");

      if (!borwser.chrome && borwser.mobile) {
        body.append($("<li class='list-unstyled'>").append("<div class='icon-chrome' title='建議使用「 Google Chrome 瀏覽器 」'>建議使用「 Google Chrome 瀏覽器 」</div>"));
        result = true;
      }

      if (borwser.line) {
        body.append($("<li>").text("您目前使用「Line」瀏覽頁面"));
        body.append($("<li>").text("請改使用 Chrome 瀏覽器"));
        result = true;
      }

      if (borwser.android && !borwser.chrome) {
        body.append($("<li>").append("<a href='https://play.google.com/store/apps/details?id=com.android.chrome&hl=zh_TW' title='另開新視窗' target='_blank'>Android Chrome(下載)</a>"));
        result = true;
      }

      if (this.GetBorwser().mac && !this.GetBorwser().chrome) {
        body.append($("<li>").append("<a href='https://itunes.apple.com/tw/app/chrome/id535886823?l=zh' title='另開新視窗' target='_blank'>MAC Chrome(下載)</a>"));
        result = true;
      }

      if (result) {
        Dcn.Message.title = title;
        Dcn.Message.body = body;

        Dcn.Message.btn_close(1);
        Dcn.Message.show();
      }

      if (borwser.ie && borwser.version <= 9) {
        alert('警告：您的 IE 瀏覽器版本，不支援 HTML5\r\n請改用 Chrome 瀏覽！');
        result = true;
      }

      if (borwser.chrome && borwser.version < 49) {
        alert('警告：您的 Chrome 瀏覽器版本，不支援 HTML5\r\n請更新 Chrome 版本！');
        result = true;
      }

      return result;
    },
    //確認 User Cookie是否啟用
    CheckCookie: function () {
      //debugger;
      if (!navigator.cookieEnabled) {
        //var title = $("<div>");
        //title.attr("class", "h4");
        //title.text("重要訊息：");

        //var body = $("<div>");
        //body.append($("<li>").append("已檢測您的瀏覽器 Cookie 設定並「沒啟用」，點選以下連線依照步驟設定！"));

        //body.append("<a href='https://play.google.com/store/apps/details?id=com.android.chrome&hl=zh_TW' class='btn-default' title='設定' target='_blank'>設定Cookie說明</a>");

        if (location.href.search("/Error/Cookie") == -1) {
          alert("偵測到您的瀏覽器 Cookie 設定尚未「啟用」，請設定完後才能執行頁面，即將自動導向「Cookie設定」說明！");
          Dcn.Utility.GoToUrl("/Error/Cookie");
        }
      }
      else {
        console.log("檢測 Cookie狀態：已啟用");
      }
    },
    //轉 true false
    ParseBool: function (str) {
      switch (String(str).toLowerCase()) {
        case "true":
        case "1":
        case "yes":
        case "y":
        case "on":
        case "success":
          return true;
        case "false":
        case "0":
        case "no":
        case "n":
        case "off":
        case "fail":
          return false;
        default:
          return false;
      }
    },
    //網站網址
    GetWebRootUrl: function (oAddPath) {
      var aProtocol = location.protocol,
        aHostname = location.hostname,
        aWebSiteName = location.pathname.split("/")[1],
        aAddPath = "";
      if (!oAddPath) {
        aAddPath = "";
      } else {
        if (oAddPath.indexOf('/') == 0) {
          aAddPath = oAddPath.substr(1);
        } else {
          aAddPath = oAddPath;
        }
      }
      return aProtocol + "//" + aHostname + "/" + aWebSiteName + "/" + aAddPath;

    },
    // 去字串前後空白
    Trim: function (str) {
      while (str.indexOf(" ") == 0) {
        str = str.substring(1, str.length);
      }
      while ((str.length > 0) && (str.indexOf(" ") == (str.length - 1))) {
        str = str.substring(0, str.length - 1);
      }
      return str;
    },
    // 去除字串end位置指定的一個符號
    TrimEndChar: function (str, sym) {
      if (str[str.length - 1] == sym) {
        return str.substring(0, str.length - 1);
      } else {
        return str;
      }
    },
    //依據所設定的長度，向「左」補所指定字元
    PadLeft: function (oValue, oLen, oPadValue) {
      var len = oValue.toString().length;
      while (len < oLen) {
        oValue = oPadValue + oValue;
        len++;
      }
      return oValue;

    },
    //依據所設定的長度，向「右」補所指定字元
    PadRight: function (oValue, oLen, oPadValue) {
      var len = oValue.toString().length;
      while (len < oLen) {
        oValue = oValue + oPadValue;
        len++;
      }
      return oValue;
    },
    //  偵測 Input Text 輸入每一個字元時只可以輸入數值
    SetInputTextOnlyNumber: function (oSelect, emptyDefaultValue) {
      var $Target = (typeof oSelect == 'object' && oSelect instanceof jQuery) ? oSelect : $(oSelect);

      if ($Target.length > 0 && $Target[0].nodeName.toLowerCase() == "input" && $Target[0].type.toLowerCase() == "text") {
        $Target.on({
          keypress: function (event) {
            if ((event.which < 48 || event.which > 57) && event.which != 46) {
              event.preventDefault();
            }
          },
          blur: function () {
            var $this = $(this);
            var oldstr = $this.val();
            var len = oldstr.length;
            var newstr = '';
            for (var i = 0; i < len; i++) {
              var achar = oldstr[i];
              if (((i == 0 && achar != '0') || i > 0 || len == 1) && achar != ' ') {
                newstr += achar;
              }
            }
            if (newstr == '' && emptyDefaultValue) {
              newstr = emptyDefaultValue;
            }
            $this.val(newstr);
          }
        });
      }
    },
    //  偵測 Input text 輸入每一個字元時只可以輸入正整數
    SetInputTextOnlyInteger: function (oSelect, emptyDefaultValue) {
      var $Target = (typeof oSelect == 'object' && oSelect instanceof jQuery) ? oSelect : $(oSelect);
      if ($Target.length > 0 && $Target[0].nodeName.toLowerCase() == "input" && $Target[0].type.toLowerCase() == "text") {
        $Target.on({
          keypress: function (event) {
            if (event.which < 48 || event.which > 57) {
              event.preventDefault();
            }
          },
          blur: function () {
            var $this = $(this);
            var oldstr = $this.val();
            var len = oldstr.length;
            var newstr = '';
            for (var i = 0; i < len; i++) {
              var achar = oldstr[i];
              if (((i == 0 && achar != '0') || i > 0 || len == 1) && achar != ' ') {
                newstr += achar;
              }
            }
            if (newstr == '' && emptyDefaultValue) {
              newstr = emptyDefaultValue;
            }
            $this.val(newstr);
          }
        });
      }
    },
    // 金錢格式-三位一撇
    MoneyFormat: function (str) {
      if (str.length <= 3) return str;
      else return MoneyFormat(str.substr(0, str.length - 3)) + "," + (str.substr(str.length - 3));
    },
    GoToUrl: function (oUrl) {
      if (!oUrl || oUrl == "") {
        oUrl = this.GetWebRootUrl()
      }
      if (oUrl && oUrl.toLowerCase().indexOf("http://") == -1 && oUrl.toLowerCase().indexOf("https://") == -1 && oUrl.toLowerCase().indexOf("~") == 0) {
        oUrl = oUrl.replace("~", "");
        oUrl = this.GetWebRootUrl(oUrl)
      }
      window.top.location.href = oUrl
    },
    GoHome: function () {
      this.GoToUrl(this.GetWebRootUrl());
    },
    // 回上一頁
    ToOneBack: function () {
      this.ToBack(-1);
    },
    //回上n頁
    ToBack: function (n) {
      window.history.back(n);
    },
    //列印
    PrintDiv: function (id) {
      var content = "<div class='container'>" + document.all.item(id).innerHTML + "</div>";
      var old_content = document.body.innerHTML;
      document.body.innerHTML = content;
      window.print();
      document.body.innerHTML = old_content;
      return false;
    },
    //取 Url 參數值
    GetUrlParam: function (name) {
      var results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(window.location.href);
      if (results == null) {
        return null;
      }
      else {
        return decodeURI(results[1]) || 0;
      };
    }
  };
  win.Dcn.Utility = win.Dcn.Utility || new Utility();

  //--------------------Even Tool------------------
  var Even = function () { };

  Even.prototype = {
    ImgThumbnailClick: function () {
      var url = $(this).attr("src");

      if (Dcn.Validate.Image(url)) //修改註記: 確認圖片格式是否為JPEG | JPG | PNG，但解密後已經轉為base64，修正為不判斷
        Dcn.Message.DialogImageUrl(url);
      else
        return false;
    },
    DisableEnter: function () {
      if (event.keyCode == 13) { return false; }
    }
  };
  win.Dcn.Even = win.Dcn.Even || new Even();

  //--------------------Validate Tool------------------
  var Validate = function () { };
  Validate.prototype.Form = null;
  Validate.prototype.Valid = null;
  Validate.prototype = {
    Image: function (url) {
      //return /\.(gif|png|jpe?g)$/i.test(url) ? true : false;
      return true;
    },
    ErrorClear: function (id) {
      //debugger;
      this.GetForm();

      var o = Validate.Form.find(id);
      if (o.length) {
        Validate.Valid.settings.success(o);
      }
    },
    GetForm: function (id) {
      Validate.Form = $("form");
      Validate.Valid = Validate.Form.validate();
    }
  };
  win.Dcn.Validate = win.Dcn.Validate || new Validate();
  //--------------------AutoComplete Tool------------------
  var JqueryAutoComplete = function () { };
  JqueryAutoComplete.prototype = {
    ID: "",
    DataFilter: "",
    DataFilterValue: null,
    Set: function (ID, ParentID, FilterValue) {
      $(ID).autocomplete({
        delay: 0,
        maxShowItems: 10,
        source: function (request, response) {
          // request物件只有一個term屬性，對應使用者輸入的文字
          // response在你自行處理並獲取資料後，將JSON資料交給該函式處理，以便於autocomplete根據資料顯示列表

          console.log(ID);
          console.log(ParentID);
          console.log(FilterValue);

          if (FilterValue)
            Dcn.JqueryAutoComplete.DataFilterValue = $(FilterValue).val();
          else
            Dcn.JqueryAutoComplete.DataFilterValue = null;

          Dcn.JqueryAutoComplete.GetData(Dcn.JqueryAutoComplete.DataFilterValue);

          response(
            $.ui.autocomplete.filter(Dcn.JqueryAutoComplete.DataFilter, request.term)
          );
        },
        select: function (event, ui) {

          var selected_value = ui.item.label.split(",")[0];
          var selected_name = ui.item.label.split(",")[1];

          if (ParentID) {
            $(ParentID).val(selected_name);
            this.value = selected_value;
          }
          else
            this.value = selected_name;

          return false;
        }
      });
    },
    GetData: function (select_value) {

      Dcn.JqueryAutoComplete.DataFilter = Bank.filter(function (item, number) {
        if (select_value)
          return item.bank_value == select_value;
        else
          return item.branch_value == select_value;
      });
    }
  };
  win.Dcn.JqueryAutoComplete = win.Dcn.JqueryAutoComplete || new JqueryAutoComplete();
  //--------------------Forge Tool------------------
  var Forge = function () { };

  Forge.prototype.Certificate = null;
  Forge.prototype.ControllerPost = null;
  Forge.prototype.Pid = null;
  Forge.prototype.csr = null;
  Forge.prototype.csrkey = null;
  Forge.prototype.csrpublicKeyToPem = null;
  Forge.prototype.csrprivateKeyToPem = null;
  Forge.prototype.cert = null;
  Forge.prototype.serialNumber = null;
  Forge.prototype.notAfter = null;
  Forge.prototype.notBefore = null;
  Forge.prototype.p7 = null;

  Forge.prototype = {
    CreateCsrRequest: function (pid) {
      if (typeof console !== "undefined" && !!console) console.log('CreateCsrRequest Start');
      Dcn.Forge.csrkey = forge.pki.rsa.generateKeyPair(2048);
      Dcn.Forge.csrpublicKeyToPem = forge.pki.publicKeyToPem(Dcn.Forge.csrkey.publicKey).replace(/\r\n|\n/g, "");
      Dcn.Forge.csrprivateKeyToPem = forge.pki.privateKeyToPem(Dcn.Forge.csrkey.privateKey).replace(/\r\n|\n/g, "");

      var csr = forge.pki.createCertificationRequest();
      csr.publicKey = Dcn.Forge.csrkey.publicKey;

      var attrs = [
        { shortName: 'CN', value: pid },
        { shortName: 'OU', value: 'Dah Chang Security Co., Ltd.' },
        { shortName: 'O', value: 'Public Certification Authority' },
        { shortName: 'C', value: 'TW' }
      ];

      csr.setSubject(attrs);
      csr.sign(Dcn.Forge.csrkey.privateKey, forge.md.sha1.create());

      var csr_pem_original = forge.pki.certificationRequestToPem(csr);
      Dcn.Forge.csr = csr_pem_original.replace('-----BEGIN CERTIFICATE REQUEST-----', '').replace('-----END CERTIFICATE REQUEST-----', '').replace(/\r\n|\n/g, "").replace(/\s+/g, "");
      if (typeof console !== "undefined" && !!console) console.log('CreateCsrRequest End');
    },
    CertToSignP7: function (pid) {
      var result;
      var cert = forge.pki.certificateFromPem(Dcn.Forge.cert);
      Dcn.Forge.serialNumber = cert.serialNumber;
      Dcn.Forge.notAfter = cert.validity.notAfter.format("yyyy/MM/dd hh:mm:ss");
      Dcn.Forge.notBefore = cert.validity.notBefore.format("yyyy/MM/dd hh:mm:ss");

      var privatekey = forge.pki.privateKeyFromPem(Dcn.Forge.csrprivateKeyToPem);

      var p7 = forge.pkcs7.createSignedData();
      p7.content = forge.util.createBuffer(pid, 'utf8');
      p7.addCertificate(Dcn.Forge.cert);
      p7.addSigner({
        key: privatekey,
        certificate: Dcn.Forge.cert,
        digestAlgorithm: forge.pki.oids.sha1,
        authenticatedAttributes: []
      });
      p7.sign();
      var p7asn = p7.toAsn1();
      var p7der = forge.asn1.toDer(p7asn);
      var p7b64 = forge.util.encode64(p7der.getBytes());

      //多 begin 
      //p7pem = forge.pkcs7.messageToPem(p7);
      result = p7b64;

      return result;
    },
    ApplyCertificate: function (open_type, pid) {
      if (Dcn.Forge.csr == null) {
        Dcn.Forge.CallBack(pid, Dcn.Forge.CreateCsrRequest);
      }

      var option = { "open_type": open_type, "pid": pid, "csr": Dcn.Forge.csr, "csrprivateKeyToPem": Dcn.Forge.csrprivateKeyToPem, "csrpublicKeyToPem": Dcn.Forge.csrpublicKeyToPem };
      var url = "/Option/ApplyCertificate";

      console.log('url:' + url);
      console.log('option:' + option);
      Dcn.Ajax.call_url_json(url, option);

      if (typeof (Dcn.Ajax.data) != "undefined" && Dcn.Ajax.data.result == "true") {
        var certPem = "-----BEGIN CERTIFICATE-----\r\n" + Dcn.Ajax.data.cert + "\r\n-----END CERTIFICATE-----\r\n";
        Dcn.Forge.cert = certPem;
        Dcn.Forge.SetCertificate(Dcn.Forge.cert);
        return true;
      }
      else {
        return false;
      }
    },
    SignCertificate: function (open_type, pid) {
      if (Dcn.Forge.p7 != null) {
        Dcn.Message.string("訊息", "已簽署通過，請選點「下一步」！");
        return true;
      }

      var p7 = Dcn.Forge.CallBack(pid, Dcn.Forge.CertToSignP7);

      var result = null;
      var option = { "open_type": open_type, "pid": pid, "p7": p7, "serial_number": Dcn.Forge.serialNumber, "not_after": Dcn.Forge.notAfter, "not_before": Dcn.Forge.notBefore };
      var url = "/Option/SignCertificate";
      Dcn.Ajax.call_url_json(url, option);

      if (typeof (Dcn.Ajax.data) != "undefined" && Dcn.Ajax.data !== null && Dcn.Ajax.data.result == "true") {
        Dcn.Forge.p7 = p7;
        return true;
      }
      else {
        Dcn.Message.string("錯誤訊息", Dcn.Ajax.error);
        return false;
      }
    },
    SetCertificate: function (_cert) {
      var cert_info = '';
      if (localStorage) {
        if (_cert != undefined || _cert !== 'undefined' && _cert.length > 50) {
          localStorage.setItem("certPem", _cert)
          Dcn.Forge.Certificate = forge.pki.certificateFromPem(_cert);

          cert_info = 'localStorage OK! \r\n'
            + 'serialNumber：' + Dcn.Forge.Certificate.serialNumber + '\r\n'
            + Dcn.Forge.Certificate.validity.notBefore.format("yyyy/MM/dd hh:mm:ss") + '\r\n'
            + Dcn.Forge.Certificate.validity.notAfter.format("yyyy/MM/dd hh:mm:ss") + '\r\n';
        }
        else {
          localStorage.removeItem("certPem");
        }
      }
      else {
        return "瀏覽器不支援 localStorage";
      }

      return cert_info;
    },
    CallBack: function (pid, callback) {
      var result = null;

      if (typeof console !== "undefined" && !!console) console.log('CallBack Start');
      result = callback(pid);
      if (typeof console !== "undefined" && !!console) console.log('CallBack End');

      return result;
    }
  };
  win.Dcn.Forge = win.Dcn.Forge || new Forge();

  //-------------------- Init ------------------
  var Init = function () { };
  Init.prototype = {
    Start: function (url) {
      //debugger;
      Dcn.Utility.CheckCookie();
    }
  };
  win.Dcn.Init = win.Dcn.Init || new Init();
})(window, jQuery);
Dcn.Init.Start();
//--------------------jQuery.Extensions.js 倒數計時器 Start------------------
jQuery.fn.countDown = function (settings, to) {
  settings = jQuery.extend({
    startFontSize: '1.750em', //開始計時 字型大小
    endFontSize: '1.750em', //結束計時 字型大小
    duration: 1000, //一秒一次
    startNumber: 10, //開始數字
    endNumber: 0, //結束數字
    callBack: function () { }
  }, settings);

  return this.each(function () {

    if (!to && to != settings.endNumber) {
      to = settings.startNumber;
    }

    $(this).text(to).css('fontSize', settings.startFontSize);

    $(this).animate({
      'fontSize': settings.endFontSize
    }, settings.duration, '', function () {
      if (to > settings.endNumber + 1) {
        $(this).css('fontSize', settings.startFontSize).text(to - 1).countDown(settings, to - 1);
      } else {
        settings.callBack(this);
      }
    });

  });
};
//--------------------jQuery.Extensions.js  End------------------

function imgWindow(url) {

  let image = new Image();
  image.src = url;
  image.style = 'width:95%;max-height:842;';
  var newTab = window.open();
  newTab.document.body.innerHTML = image.outerHTML;
}
