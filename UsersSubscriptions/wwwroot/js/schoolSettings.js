$(function () {
    $("#modalUpdate").click(function () {
        $("#btnSubmit").click();
    });
});
function removeTabbleRow(ident) {
    $("#" + ident).addClass("d-none");
    $("#id" + ident).val("delete");
}
$(function () {
    $("#addNewPayType").click(function () {
        let count = parseInt($("#paymentTypesCount").val());
        $("#paymentTypesCount").val(count + 1);
        var select = " <tr> <td><select name='PaymentTypes[" + count + "].Priority' class='form-control'>";
        for (let i = 0; i < 11; i++) {
            if (i == 2) {
                select += "<option selected value='" + i + "'  >" + i + "</option>";
            } else {
            select += "<option value='" + i + "'  >" + i + "</option>";
            }
        }
        select += "</select> </td> <td>" +
            "<input name='PaymentTypes[" + count + "].Name' class='form-control'>" +
            " </td> <td> </td> </tr>";
        $("#tablePT > tbody").append(select

        );

    });
});