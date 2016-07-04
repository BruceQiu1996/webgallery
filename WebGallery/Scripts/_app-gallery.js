$(document).ready(function () {
    if ($("#currentpage").val() == "1") {
        $("#prelink").removeAttr("href");
        $("#prelink").addClass("link-disabled");
    }
    if ($("#currentpage").val() == $("#totalpage").val() || $("#totalpage").val() == "0") {
        $("#nextlink").removeAttr("href");
        $("#nextlink").addClass("link-disabled");
    }

    $(".header div select").change(function () {
        $(this).parent().parent().submit();
    });
    if (localStorage['page'] == document.URL) {
        $(document).scrollTop(localStorage['scrollTop']);
    }
})

$(document).scroll(function () {
    localStorage['page'] = document.URL;
    localStorage['scrollTop'] = $(document).scrollTop();
});