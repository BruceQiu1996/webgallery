$(document).ready(function () {
    if ($("#currentpage").val() == "1") {
        $("#prelink").removeAttr("href");
        $("#prelink").addClass("link-disabled");
    }
    if ($("#currentpage").val() == $("#totalpage").val() || $("#totalpage").val() == "0") {
        $("#nextlink").removeAttr("href");
        $("#nextlink").addClass("link-disabled");
    }
})