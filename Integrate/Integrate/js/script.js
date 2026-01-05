var areaOption={
    "query":"q",
    "detail":"d"
}

var apiRootUrl="http://localhost:5191/api/"; // 請確認此 Port 與後端一致
var state="";

var stateOption={
    "add":"add",
    "update":"update"
}

var defauleBookStatusId="A";

$(function () {
    
    registerRegularComponent();

    var validator = $("#book_detail_area").kendoValidator({
        rules:{
            //日期必填驗證
            dateCheckRule: function(input){
                if (input.is(".date_picker")) {
                    var selector=$("#"+$(input).prop("id"));
                    return selector.data("kendoDatePicker").value();
                }
                return true;
            }
        },
        messages: { 
            //日期驗證訊息
            dateCheckRule: function(input){ return input.attr("data-message_prefix")+"格式有誤";}
          }
        }).data("kendoValidator");


    $("#book_detail_area").kendoWindow({
        width: "1200px",
        title: "新增書籍",
        visible: false,
        modal: true,
        actions: [
            "Close"
        ],
        close: onBookWindowClose
    }).data("kendoWindow").center();

    $("#book_record_area").kendoWindow({
        width: "700px",
        title: "借閱紀錄",
        visible: false,
        modal: true,
        actions: [
            "Close"
        ]
    }).data("kendoWindow").center();
    

    $("#btn_add_book").click(function (e) {
        e.preventDefault();
        state=stateOption.add;

        enableBookDetail(true);
        clear(areaOption.detail);
        setStatusKeepRelation(state);

        $("#btn-save").css("display","");        
        $("#book_detail_area").data("kendoWindow").title("新增書籍");
        $("#book_detail_area").data("kendoWindow").open();
    });


    $("#btn_query").click(function (e) {
        e.preventDefault();
        
        var grid=getBooGrid();
        grid.dataSource.read();
    });

    $("#btn_clear").click(function (e) {
        e.preventDefault();

        clear(areaOption.query);
        //TODO: 清空後重新查詢
        var grid = getBooGrid();
        grid.dataSource.read();
    });

    $("#btn-save").click(function (e) {
        e.preventDefault();
        if (validator.validate()) {
            switch (state) {
                case "add":
                    addBook();
                    break;
                case "update":
                    updateBook($("#book_id_d").val());
                break;
                default:
                    break;
            }
        }        
    });

    $("#book_grid").kendoGrid({
        dataSource: {
            transport: {
                read: {
                  url: apiRootUrl+"bookmaintain/querybook",
                  dataType: "json",
                  type: "post",
                  data: function(){
                    return {
                        "BookName":$("#book_name_q").val(),
                        //TODO: 補齊傳入參數
                        "BookClassId":$("#book_class_q").data("kendoDropDownList").value(),
                        "BookKeeperId":$("#book_keeper_q").data("kendoDropDownList").value(),
                        "BookStatusId":$("#book_status_q").data("kendoDropDownList").value()
                    }
                  }
                }
            },
            schema: {
                 model: {
                    fields: {
                        bookId: { type: "int" },
                        bookClassName: { type: "string" },
                        bookName: { type: "string" },
                        bookBoughtDate: { type: "string" },
                        bookStatusName: { type: "string" },
                        bookKeeperCname: { type: "string" }
                    }
                },
                // 處理資料來源 Schema
                data: function (response) {
                    if (Array.isArray(response)) return response;
                    if (response.data) return response.data;
                    if (response.Data) return response.Data;
                    return [];
                }
            },
            serverPaging: false,
            pageSize: 20,
        },
        height: 550,
        sortable: true,
        pageable: {
            input: true,
            numeric: false
        },
        columns: [
            { field: "bookId", title: "書籍編號", width: "10%" },
            { field: "bookClassName", title: "圖書類別", width: "15%" },
            { field: "bookName", title: "書名", width: "30%" ,
              template: "<a style='cursor:pointer; color:blue' onclick='showBookForDetail(event,#:bookId #)'>#: bookName #</a>"
            },
            { field: "bookBoughtDate", title: "購書日期", width: "15%" },
            { field: "bookStatusName", title: "借閱狀態", width: "15%" },
            { field: "bookKeeperCname", title: "借閱人", width: "15%" },
            { command: { text: "借閱紀錄", click: showBookLendRecord }, title: " ", width: "120px" },
            { command: { text: "修改", click: showBookForUpdate }, title: " ", width: "100px" },
            { command: { text: "刪除", click: deleteBook }, title: " ", width: "100px" }
        ]

    });

    $("#book_record_grid").kendoGrid({
        dataSource: {
            data: [],
            schema: {
                model: {
                    fields: {
                        lendDate: { type: "string" },
                        bookKeeperId: { type: "string" },
                        bookKeeperEname: { type: "string" },
                        bookKeeperCname: { type: "string" }
                    }
                }
            },
            pageSize: 20,
        },
        height: 250,
        sortable: true,
        pageable: {
            input: true,
            numeric: false
        },
        columns: [
            { field: "lendDate", title: "借閱日期", width: "25%" },
            { field: "bookKeeperId", title: "借閱人編號", width: "15%" },
            { field: "bookKeeperEname", title: "借閱人英文姓名", width: "30%" },
            { field: "bookKeeperCname", title: "借閱人中文姓名", width: "30%" },
        ]
    });

})

/**
 * 當圖書類別改變時,置換圖片
 */
function onClassChange() {
    var dropdown = $("#book_class_d").data("kendoDropDownList");
    var selectedValue = "";
    if (dropdown) {
        selectedValue = dropdown.value();
    }

    if(selectedValue===""){
        $("#book_image_d").attr("src", "image/optional.jpg");
    }else{
        $("#book_image_d").attr("src", "image/" + selectedValue + ".jpg");
    }
}

/**
 * 當 BookWindow 關閉後要處理的作業
 */
function onBookWindowClose() {
    //清空表單內容
    clear(areaOption.detail);
}

function addBook() { 

    var book = {
        //TODO: 補齊欄位值
        "BookName": $("#book_name_d").val(),
        
        // ↓↓↓ 修正重點：必須使用 data("kendoDropDownList").value() 取得選單值
        "BookClassId": $("#book_class_d").data("kendoDropDownList").value(),
        "BookClassName": $("#book_class_d").data("kendoDropDownList").text(),
        
        "BookBoughtDate": kendo.toString($("#book_bought_date_d").data("kendoDatePicker").value(), "yyyy-MM-dd"),
        
        "BookStatusId": $("#book_status_d").data("kendoDropDownList").value(),
        "BookStatusName": $("#book_status_d").data("kendoDropDownList").text(),
        
        // 借閱人如果是空的，就送出空字串 (配合後端移除 Required 後即可通過)
        "BookKeeperId": $("#book_keeper_d").data("kendoDropDownList").value() || "",
        "BookKeeperCname": $("#book_keeper_d").data("kendoDropDownList").text(),
        
        "BookAuthor": $("#book_author_d").val(),
        "BookPublisher": $("#book_publisher_d").val(),
        "BookNote": $("#book_note_d").val()
    }

    // 檢查一下是否有必填欄位是空的 (可以在這裡 console.log(book) 查看)
    console.log("送出的資料:", book);

    $.ajax({
        type: "post",
        url: apiRootUrl + "bookmaintain/addbook",
        data: JSON.stringify(book),
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            if (response.status === true || response.Status === true) {
                alert("新增成功");
                $("#book_detail_area").data("kendoWindow").close();
                getBooGrid().dataSource.read();
            } else {
                alert("新增失敗：" + (response.message || response.Message));
            }
        },
        error: function(xhr) {
            // 這裡會顯示詳細的錯誤訊息，例如 "BookClassId 不可空白"
            console.log(xhr.responseText);
            alert("新增發生錯誤，請查看 Console (F12) 獲取詳細資訊");
        }
    });
    }


function updateBook(bookId){
    
    //TODO: 取得畫面上相關書籍資料
    var book = {
        "BookId": bookId,
        "BookName": $("#book_name_d").val(),
        "BookClassId": $("#book_class_d").data("kendoDropDownList").value(),
        "BookBoughtDate": kendo.toString($("#book_bought_date_d").data("kendoDatePicker").value(), "yyyy-MM-dd"),
        "BookStatusId": $("#book_status_d").data("kendoDropDownList").value(),
        "BookKeeperId": $("#book_keeper_d").data("kendoDropDownList").value() || "",
        "BookAuthor": $("#book_author_d").val(),
        "BookPublisher": $("#book_publisher_d").val(),
        "BookNote": $("#book_note_d").val()
    }

    $.ajax({
        type: "post",
        url: apiRootUrl+"bookmaintain/updatebook",
        data: JSON.stringify(book),
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            if (response.status === true || response.Status === true) {
                alert("修改成功");
                $("#book_detail_area").data("kendoWindow").close();
                getBooGrid().dataSource.read();
            } else {
                alert("修改失敗：" + (response.message || response.Message));
            }
        },
        error: function(xhr) {
            alert("修改發生錯誤");
        }
    });
   
 }

function deleteBook(e) {
    e.preventDefault();
    var grid = getBooGrid();
    var row = grid.dataItem(e.target.closest("tr"));

    if (confirm("確定刪除")) {
        
        $.ajax({
            type: "post",
            url: apiRootUrl+"bookmaintain/deletebook",
            data: JSON.stringify(row.bookId),
            contentType: "application/json",
            dataType: "json",
            success: function (response) {
                if(!response.status && !response.Status){
                    alert(response.message || response.Message);
                }else{
                    grid.dataSource.read();
                    alert("刪除成功");
                }
            },
            error: function(xhr) {
                 alert("刪除失敗");
            }
        });
    }
}

/**
 * 顯示圖書明細-for 修改
 * @param {*} e 
 */
function showBookForUpdate(e) {
    e.preventDefault();

    state=stateOption.update;
    $("#book_detail_area").data("kendoWindow").title("修改書籍");
    //顯示存檔按鈕
    $("#btn-save").css("display","");

    //取得點選該筆的 bookId
    var grid = getBooGrid();
    var bookId = grid.dataItem(e.target.closest("tr")).bookId;

    //設定畫面唯讀與否
    enableBookDetail(true);

    //綁定資料
    bindBook(bookId);
    
    //設定借閱狀態與借閱人關聯
    setStatusKeepRelation();

    //開啟 Window
    $("#book_detail_area").data("kendoWindow").open();
}

/**
 * 顯示圖書明細-for 明細(點選Grid書名超連結)
 * @param {*} e 
 */
function showBookForDetail(e,bookId) {
    e.preventDefault();

    state=stateOption.update;
    $("#book_detail_area").data("kendoWindow").title("書籍明細");

    //隱藏存檔按鈕
    $("#btn-save").css("display","none");

    //綁定資料
    bindBook(bookId);
    
    //設定借閱狀態與借閱人關聯
    setStatusKeepRelation();

    //設定畫面唯讀與否
    enableBookDetail(false);
    $("#book_detail_area").data("kendoWindow").open();
}

/**
 * 設定書籍明細畫面唯讀與否
 * @param {*} enable 
 */
function enableBookDetail(enable) { 

    $("#book_id_d").prop('readonly', !enable);
    $("#book_name_d").prop('readonly', !enable);
    $("#book_author_d").prop('readonly', !enable);
    $("#book_publisher_d").prop('readonly', !enable);
    $("#book_note_d").prop('readonly', !enable);

    if(enable){    
        $("#book_status_d").data("kendoDropDownList").enable(true);
        $("#book_class_d").data("kendoDropDownList").enable(true);
        $("#book_keeper_d").data("kendoDropDownList").enable(true);
        $("#book_bought_date_d").data("kendoDatePicker").enable(true);
    }else{
        $("#book_status_d").data("kendoDropDownList").readonly();
        $("#book_class_d").data("kendoDropDownList").readonly();
        $("#book_keeper_d").data("kendoDropDownList").readonly();
        $("#book_bought_date_d").data("kendoDatePicker").readonly();
    }
 }

 /**
  * 繫結書及明細畫面資料
  * @param {*} bookId 
  */
function bindBook(bookId){

    $.ajax({
        type: "post",
        url: apiRootUrl+"bookmaintain/loadbook",
        data:JSON.stringify(bookId),
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            var book = response.data || response.Data;
            
            $("#book_id_d").val(book.bookId);
            $("#book_name_d").val(book.bookName);
            $("#book_class_d").data("kendoDropDownList").value(book.bookClassId);
            $("#book_author_d").val(book.bookAuthor);
            $("#book_bought_date_d").data("kendoDatePicker").value(book.bookBoughtDate);
            $("#book_publisher_d").val(book.bookPublisher);
            $("#book_note_d").val(book.bookNote);
            $("#book_status_d").data("kendoDropDownList").value(book.bookStatusId);
            $("#book_keeper_d").data("kendoDropDownList").value(book.bookKeeperId);

            onClassChange();
            setStatusKeepRelation(); 
        },error:function(xhr){
            alert("載入書籍失敗");
        }
    });    


}

function showBookLendRecord(e) {
    e.preventDefault();
    
    var grid = getBooGrid();
    var row = grid.dataItem(e.target.closest("tr"));
    var bookId = row.bookId;
    var bookName = row.bookName;

    //row.bookId
    //TODO: 完成發 AJAX 和處理後續動作
    $.ajax({
        type: "post",
        url: apiRootUrl + "bookmaintain/lendrecord",
        data: JSON.stringify(bookId),
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            var data = response;
            if(response.data || response.Data) {
                 data = response.data || response.Data;
            }
            $("#book_record_grid").data("kendoGrid").dataSource.data(data);
            $("#book_record_area").data("kendoWindow").title(bookName+" 借閱紀錄").open();
        }
    });    
}

function clear(area) {
    //TODO:補齊要清空的欄位
    switch (area) {
        case "q":
            $("#book_name_q").val("");
            $("#book_class_q").data("kendoDropDownList").select(0);
            $("#book_keeper_q").data("kendoDropDownList").select(0);
            $("#book_status_q").data("kendoDropDownList").select(0);
            break;
    
        case "d":
            $("#book_id_d").val("");
            $("#book_name_d").val("");
            $("#book_class_d").data("kendoDropDownList").select(0);
            $("#book_bought_date_d").data("kendoDatePicker").value(new Date());
            $("#book_status_d").data("kendoDropDownList").value("A");
            $("#book_keeper_d").data("kendoDropDownList").select(0);
            $("#book_author_d").val("");
            $("#book_publisher_d").val("");
            $("#book_note_d").val("");
            onClassChange();
            //清除驗證訊息
            $("#book_detail_area").kendoValidator().data("kendoValidator").reset();
            break;
        default:
            break;
    }
}
                      
function setStatusKeepRelation() { 
    // TODO: 確認選項關聯呈現方式
    switch (state) {
        case "add":
            $("#book_status_d_col").css("display","none");
            $("#book_keeper_d_col").css("display","none");
        
            $("#book_status_d").prop('required',false);
            $("#book_keeper_d").prop('required',false);            
            break;
        case "update":
            $("#book_status_d_col").css("display","");
            $("#book_keeper_d_col").css("display","");
            $("#book_status_d").prop('required',true);

            var bookStatusId=
                $("#book_status_d").data("kendoDropDownList").value();

            if(bookStatusId=="A" || bookStatusId=="U"){
                $("#book_keeper_d").prop('required',false);
                $("#book_keeper_d").data("kendoDropDownList").value(""); 
                $("#book_keeper_d").data("kendoDropDownList").enable(false);

                $("#book_detail_area").data("kendoValidator").validateInput($("#book_keeper_d"));

                $("#book_keeper_d_label").removeClass("required");
                
            }else{
                $("#book_keeper_d").data("kendoDropDownList").enable(true);
                $("#book_keeper_d").prop('required',true);
                $("#book_keeper_d_label").addClass("required");
            }
            break;
        default:
            break;
    }
    
 }

 /**
  * 生成畫面上的 Kendo 控制項
  */
function registerRegularComponent(){
    
    // 解決 Schema data 相容性問題
    var safeSchemaData = function(response) {
        if (response === null || response === undefined) return [];
        if (Array.isArray(response)) return response;
        if (response.hasOwnProperty('data')) return response.data;
        if (response.hasOwnProperty('Data')) return response.Data;
        return [];
    };

    var commonDataSourceConfig = {
        transport: {
            read: {
                dataType: "json",
                type: "post"
            }
        },
        schema: {
            data: safeSchemaData
        }
    };

    $("#book_status_q").kendoDropDownList({
        dataTextField: "text",
        dataValueField: "value",
        optionLabel: "請選擇",
        index: 0,        
        dataSource: $.extend(true, {}, commonDataSourceConfig, {
            transport: { read: { url: apiRootUrl + "code/bookstatus" } }
        })
    });

    $("#book_status_d").kendoDropDownList({
        dataTextField: "text",
        dataValueField: "value",
        optionLabel: "請選擇",
        index: 0,        
        change: setStatusKeepRelation,
        dataSource: $.extend(true, {}, commonDataSourceConfig, {
            transport: { read: { url: apiRootUrl + "code/bookstatus" } }
        })
    });
    //TODO: 其他的下拉選單
    
    // 查詢 - 圖書類別
    $("#book_class_q").kendoDropDownList({
        dataTextField: "text",
        dataValueField: "value",
        optionLabel: "請選擇",
        index: 0,
        dataSource: $.extend(true, {}, commonDataSourceConfig, {
            transport: { read: { url: apiRootUrl + "code/bookclass" } }
        })
    });

    // 明細 - 圖書類別
    $("#book_class_d").kendoDropDownList({
        dataTextField: "text",
        dataValueField: "value",
        optionLabel: "請選擇",
        index: 0,
        change: onClassChange,
        dataSource: $.extend(true, {}, commonDataSourceConfig, {
            transport: { read: { url: apiRootUrl + "code/bookclass" } }
        })
    });

    // 查詢 - 借閱人
    $("#book_keeper_q").kendoDropDownList({
        dataTextField: "text",
        dataValueField: "value",
        optionLabel: "請選擇",
        index: 0,
        dataSource: $.extend(true, {}, commonDataSourceConfig, {
            transport: { read: { url: apiRootUrl + "code/bookkeeper" } }
        })
    });

    // 明細 - 借閱人
    $("#book_keeper_d").kendoDropDownList({
        dataTextField: "text",
        dataValueField: "value",
        optionLabel: "請選擇",
        index: 0,
        dataSource: $.extend(true, {}, commonDataSourceConfig, {
            transport: { read: { url: apiRootUrl + "code/bookkeeper" } }
        })
    });


    $("#book_bought_date_d").kendoDatePicker({
        format: "yyyy-MM-dd",
        value: new Date(),
        dateInput: true
    });
}

/**
 * * @returns 取得畫面上的 book grid
 */
function getBooGrid(){
    return $("#book_grid").data("kendoGrid");
}