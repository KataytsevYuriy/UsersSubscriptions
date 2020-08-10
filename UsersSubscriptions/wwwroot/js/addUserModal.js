$('.phone-input').mask("+38(000)000-00-00", { placeholder: "+38(___)___-__-__" });


var userIdent, userName, usersIdents;

function sendPhone() {
    $("#cameraVideo").addClass("d-none");
    if (scanner != undefined) {
        scanner.stop();
    }
    $("#info").show();
    $("#info").empty();
    $("#selectUser").empty();
    $("#selectUser").addClass("d-none");
    var findByName = $("#nameInput").val().toString();
    if (findByName.length > 0) {
        $.post("/Owner/GetUserByName/" + findByName, function (data) {
            usersIdents = JSON.parse(data);
            if (usersIdents.length == 1) {
                userIdent = usersIdents[0];
                userName = usersIdents[0].Name;
                $("#info").append(userName);
                $("#info").addClass("alert-info");
                $("#modalAdd").prop("disabled", false);
            } else {
                var selectUser = document.getElementById("selectUser");
                if (usersIdents.length > 5) {
                    selectUser.size = 5;
                } else {
                    selectUser.size = usersIdents.length;
                }
                for (let i = 0; i < usersIdents.length; i++) {
                    var selectOption = document.createElement("option");
                    selectOption.text = usersIdents[i].Name;
                    selectOption.value = usersIdents[i].Id;
                    selectOption.classList.add("alert-success");
                    selectUser.add(selectOption, selectUser[i]);
                }
                $("#selectUser").removeClass("d-none");
                userName = data.name;
                $("#info").append(userName);
                $("#info").addClass("alert-info");
                $("#info").removeClass("alert-danger");
                $("#modalAdd").prop("disabled", true);
            }
        }, "json")
            .fail(function () {
                $("#info").append("Користувача не знайдено");
                $("#info").addClass("alert-danger");
                $("#modalAdd").prop("disabled", true);
            });
    } else {
        var dataSend = $("#phoneInput").val().toString();
        $("#info").empty();
        $("#info").removeClass();
        dataSend = dataSend.replace(/[^\d]/g, '');
        $.post("/Owner/GetUserByPhone/" + dataSend, function (data) {
            userIdent = data;
            userName = data.name;
            $("#info").append(userName);
            $("#info").addClass("alert-info");
            $("#modalAdd").prop("disabled", false);
        }, "json")
            .fail(function () {
                $("#info").append("Користувача не знайдено");
                $("#info").addClass("alert-danger");
                $("#modalAdd").prop("disabled", true);
            });
    }
};
function sendId(content) {
    $("#info").empty();
    $("#info").removeClass();
    $.post("/Owner/GetUserById/" + content, function (data) {
        userIdent = data;
        userName = data.name;
        $("#info").append(userName);
        $("#info").addClass("alert-info");
        $("#modalAdd").prop("disabled", false);
    }, "json")
        .fail(function () {
            $("#info").append("Користувача не знайдено");
            $("#info").addClass("alert-danger");
            $("#modalAdd").prop("disabled", true);
        });
};
function addTeacher() {
    updateUser(userIdent);
    $("#addPhoneModal").modal("hide");
    //$("#modalCancel").click();
    $("#modalAdd").prop("disabled", true);
    $("#teacherName").empty();
    $("#phoneInput").val("");
    $("#nameInput").val("");
    $("#info").empty();
}
function clearUserName() {
    $("#info").empty();
}

$(function () {
    $("#selectUser").change(function () {
        var selectedUserId = $(this).val();
        for (let i = 0; i < usersIdents.length; i++) {
            if (usersIdents[i].Id == selectedUserId) {
                userIdent = new Object;
                userIdent.Id = selectedUserId;
                userIdent.Name = usersIdents[i].Name;
                userName = usersIdents[i].Name;
                break;
            }
        }
        $("#info").empty();
        $("#info").append(userName);
        $("#info").addClass("alert-info");
        $("#selectUser").addClass("d-none");
        $("#modalAdd").prop("disabled", false);
        addTeacher();
    });
});

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