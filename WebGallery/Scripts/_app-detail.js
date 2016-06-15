$(document).ready(function () {
    var page = 1;
    var imageurls = new Array();
    imageurls = $("#screenurls").val().trim().split(" ");
    if (imageurls.length > 0) {
        $("#screenshot").attr("src", imageurls[0]);
    }
    if (page >= imageurls.length) {
        $("#next-btn").hide();
    }
    if (page <= 1) {
        $("#pre-btn").hide();
    }
    $("#next-btn").click(function () {
        $("#screenshot").attr("src", imageurls[++page - 1]);
        if (page >= imageurls.length) {
            $(this).hide();
        }
        if (page > 1) {
            $("#pre-btn").show();
        }
    })
    $("#pre-btn").click(function () {
        $("#screenshot").attr("src", imageurls[--page - 1]);
        if (page <= 1) {
            $(this).hide();
        }
        if (page < imageurls.length) {
            $("#next-btn").show();
        }
    })
    $("#screenshothref").click(function () {
        if (imageurls.length > 0) {
            $("#screenshot-block").show();
        }
    })
})