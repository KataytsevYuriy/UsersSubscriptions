$(function () {
    $('#SaveSelectPTModal').click(function () {
        var schoolId = $("#schoolId").val();
        var courseId = $("#courseId").val();
        var pTypes = $('#CheckboxesSelectPT input:checked').map(function () {
            return $(this).val();
        }).toArray();
        $.ajax({
            type: 'POST',
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            data: { schoolId: schoolId, courseId: courseId, pTypes: pTypes },
            url: "/Owner/SetCoursePayTypes",
            cache: false,
            success: function (resp) {
                if (resp == "true") {
                    $("#selectPaymentTypeCancel").click();
                }
                if (resp != "true") {
                    $("#flashMessage").text('<p class=\"alert alert-danger\">' + resp + '</p>');
                }
            }
        });
    });
});