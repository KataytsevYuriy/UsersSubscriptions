$('.phone-input').mask("+38(000)000-00-00", { placeholder: "+38(___)___-__-__" });


var userIdent, userName;

function sendPhone() {
    $("#cameraVideo").addClass("d-none");
    if (scanner != undefined) {
        scanner.stop();
    }
    var dataSend = $("#phoneInput").val().toString();
    $("#teacherName").empty();
    dataSend = dataSend.replace(/[^\d]/g, '');
    $.post("/Owner/GetUserByPhone/" + dataSend, function (data) {
        if (data == "") {
            $("#teacherName").append("Користувача не знайдено");
            $("#modalAdd").prop("disabled", true);
        } else {
            userIdent = data;
            userName = data.name;
            $("#teacherName").append(userName);
            $("#modalAdd").prop("disabled", false);
        }
    }, "json");
};
function sendId(content) {
    $.post("/Owner/GetUserById/" + content, function (data) {
        if (data == "") {
            $("#teacherName").append("Користувача не знайдено");
            $("#modalAdd").prop("disabled", true);
        } else {
            userIdent = data;
            userName = data.name;
            $("#teacherName").append(userName);
            $("#modalAdd").prop("disabled", false);
        }
    }, "json");
};
function addTeacher() {
    updateUser(userIdent);
    $("#modalCancel").click();
    $("#modalAdd").prop("disabled", true);
    $("#teacherName").empty();
}


var getUserInfoUrl = '@Url.Action("GetUserById")';
var cameras;
var scanner;

$(function () {
    $("#scanQR").click(function () {
        $("#cameraVideo").removeClass("d-none");
        $.getScript("/js/instascan.min.js", function () {
            scanner = new Instascan.Scanner({ video: document.getElementById('preview'), mirror: false });
            scanner.addListener('scan', function (content) {
                sendId(content);
                $("#cameraVideo").addClass("d-none");
                scanner.stop();
            });
            Instascan.Camera.getCameras().then(function (camerasList) {
                cameras = camerasList;
                if (cameras.length > 0) {
                    if (cameras.length > 1) {
                        var camerasArea = $("#area-cameras-list");
                        for (var i = 0; i < cameras.length; i++) {
                            if (i == cameras.length - 1) {
                                camerasArea.append("<a class='list-group-item active' href='#' data-camera-id='" + cameras[i].id + "'>" + cameras[i].name + "</a><br/>")
                            }
                            else {
                                camerasArea.append("<a class='list-group-item' href='#' data-camera-id='" + cameras[i].id + "'>" + cameras[i].name + "</a><br/>")
                            }
                        }
                    }
                    scanner.start(cameras[cameras.length - 1]);
                } else {
                    console.error('No cameras found.');
                }
            }).catch(function (e) {
                console.error(e);
            });
        });

        $("body").on('click', '#area-cameras-list a', function () {
            $(".list-group-item").removeClass("active");
            $(this).addClass('active');
            var cameraId = $(this).attr('data-camera-id');
            var activeCamera = cameras.find(x => x.id === cameraId);
            scanner.start(activeCamera);
        });
    });
});