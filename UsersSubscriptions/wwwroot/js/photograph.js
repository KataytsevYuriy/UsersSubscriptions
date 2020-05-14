//you need add 'id=userId' to teg input whitn userId
$(function () {
    $(window).on("shown.bs.modal", function () {

        var video = document.getElementById('videoAvatar');
        var videoCam;
        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            navigator.mediaDevices.getUserMedia({ video: true }).then(function (stream) {
                video.srcObject = stream;
                video.play();
                videoCam = stream;
            });
        }
        var canvas = document.getElementById('canvas');

        document.getElementById("snap").addEventListener("click", function () {
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            var context = canvas.getContext('2d').drawImage(video, 0, 0);
            $('#videoAvatar').addClass('d-none');
            $('#canvas').removeClass('d-none');
            $("#modalAdd").prop("disabled", false);
            videoCam.getVideoTracks()[0].stop();
        });

        $(function () {
            $('#modalAdd').click(async function () {
                var userId = $('#userId').val();
                if (userId != null && userId.length > 0) {
                    var UrlAction = "/Teacher/AddAvatar";
                    var canvas = document.getElementById('canvas');
                    let imageBlob = await new Promise(resolve => canvas.toBlob(resolve, 'image/jpeg'));
                    let formData = new FormData();
                    formData.append('userId', userId);
                    formData.append('imageData', imageBlob, 'image/jpeg');
                    let response = await fetch(UrlAction, {
                        method: 'POST',
                        body: formData,
                    });
                    let result = await response.json();
                    if (result == true) {
                        location.reload(true);
                    }
                    console.log(result);
                }
            });
        });



    });
    $('#addAvatarModal').on("hidden.bs.modal", function () {
        if ($('#videoAvatar').hasClass('d-none')) {
            $('#videoAvatar').removeClass('d-none');
            $('#canvas').addClass('d-none');
        }
        console.log("closing");
    });

});
