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
        $("#tablePT > tbody").append(" <tr> <td><select name='PaymentTypes[" + count + "].Priority' class='form-control'>" +
            "<option value='0'  >0</option>" +
            "<option value='1' >1</option>" +
            "<option value='2'selected >2</option>" +
            "<option value='3' >3</option>" +
            "<option value='4' >4</option>" +
            "<option value='5' >5</option>" +
            "<option value='6' >6</option>" +
            "<option value='7' >7</option>" +
            "<option value='8' >8</option>" +
            "<option value='9' >9</option>" +
            "<option value='10' >10</option>" +
            "</select> </td> <td>" +
            "<input name='PaymentTypes[" + count + "].Name' class='form-control'>" +
            " </td> <td> </td> </tr>"
        );

    });
});