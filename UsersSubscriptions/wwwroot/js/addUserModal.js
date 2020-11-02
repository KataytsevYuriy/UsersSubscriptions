$('.phone-input').mask("+38(000)000-00-00", { placeholder: "+38(___)___-__-__" });


var userIdent, userName, usersIdents, userId;

function sendPhone() {
    $("#cameraVideo").addClass("d-none");
    if (scanner != undefined) {
        scanner.stop();
    }
    //$("#info").show();
    //$("#info").empty();
    clearUserName();
    $("#selectUser").empty();
    $("#selectUser").addClass("d-none");
    var findByName = $("#nameInput").val().toString();
    if (findByName.length > 0) {
        $.post("/Owner/GetUserByName/" + findByName, function (data) {
            usersIdents = JSON.parse(data);
            if (usersIdents.length > 0) {
                $('#listUsers').removeClass("display-none");
                var usersListDiv = document.getElementById("listUsers");
                $("#listUsers").empty();
                for (let i = 0; i < usersIdents.length; i++) {
                    var userInList = document.createElement("div");
                    userInList.classList.add("padding-5");
                    userInList.classList.add("pointer");
                    userInList.classList.add("my-1");
                    userInList.classList.add("alert-info");
                    userInList.innerText = usersIdents[i].Name;
                    userInList.setAttribute("onclick", "selectUser(" + i + ")");
                    usersListDiv.appendChild(userInList);
                }
            }
        }, "json")
            .fail(function () {
                $("#info").append("Користувача не знайдено");
                //$("#info").addClass("alert-danger");
                $('#info').removeClass("display-none");
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
    $("#modalAdd").prop("disabled", true);
    $("#teacherName").empty();
    $("#phoneInput").val("");
    $("#nameInput").val("");
    clearUserName();
    //$("#info").empty();
}
function clearUserName() {
    $("#info").empty();
    $("#info").addClass('display-none');

    $('#listUsers').empty();
    $('#listUsers').addClass('display-none');
}

function selectUser(listUserNumber) {
    debugger;
    if (listUserNumber < usersIdents.length) {
        userIdent = new Object;
        userIdent.Id = usersIdents[listUserNumber].Id;
        userIdent.Name = usersIdents[listUserNumber].Name;
        userName = usersIdents[listUserNumber].Name;
    }

    $(".full-name").val(usersIdents[listUserNumber].Name);
    $("#info").append(userName);
    $("#info").addClass("alert-info");
    $("#selectUser").addClass("d-none");
    $("#modalAdd").prop("disabled", false);
    addTeacher();
};

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